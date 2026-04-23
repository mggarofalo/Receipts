using Application.Interfaces.Services;
using Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenCvSharp;
using Sdcb.PaddleOCR;
using Sdcb.PaddleOCR.Models;
using Sdcb.PaddleOCR.Models.Local;

namespace Infrastructure.Services;

/// <summary>
/// PaddleOCR engine using PP-OCRv4 mobile models via the Sdcb.PaddleOCR library.
/// Provides higher accuracy than Tesseract on messy/skewed receipts at the cost of
/// ~200 MB RAM and slightly longer inference time (~1-2 s per image on CPU).
/// </summary>
public sealed class PaddleOcrEngine : IOcrEngine, IDisposable
{
	private const int DefaultTimeoutSeconds = 60; // PaddleOCR is slower than Tesseract
	private const long DefaultMaxImageBytes = 10 * 1024 * 1024; // 10 MB

	private readonly PaddleOcrAll _engine;
	private readonly SemaphoreSlim _semaphore = new(1, 1);
	private readonly ILogger<PaddleOcrEngine> _logger;
	private readonly TimeSpan _processTimeout;
	private readonly long _maxImageBytes;
	private bool _disposed;

	public PaddleOcrEngine(IConfiguration configuration, ILogger<PaddleOcrEngine> logger)
	{
		_logger = logger;

		int timeoutSeconds = int.TryParse(configuration[ConfigurationVariables.OcrTimeoutSeconds], out int ts)
			? ts
			: DefaultTimeoutSeconds;
		_processTimeout = TimeSpan.FromSeconds(timeoutSeconds);

		_maxImageBytes = long.TryParse(configuration[ConfigurationVariables.OcrMaxImageBytes], out long mb)
			? mb
			: DefaultMaxImageBytes;

		// Use bundled PP-OCRv4 mobile models (English)
		FullOcrModel model = LocalFullModels.EnglishV4;
		_engine = new PaddleOcrAll(model)
		{
			AllowRotateDetection = true,
			Enable180Classification = true,
		};

		// Raise the detector's max edge size above Paddle's 960 default. Receipt
		// photos are often 3000+ px — downscaling that aggressively to fit 960
		// destroys small-font text (receipt totals, UPC digits) that the
		// recognizer can no longer read. 1536 keeps detail while staying below
		// the memory ceiling that justified the 3G container limit in RECEIPTS-606.
		_engine.Detector.MaxSize = 1536;

		_logger.LogInformation("PaddleOCR engine initialized with PP-OCRv4 English mobile model (CPU mode)");
	}

	/// <summary>
	/// Internal constructor for unit testing — accepts a pre-built <see cref="PaddleOcrAll"/> instance.
	/// </summary>
	internal PaddleOcrEngine(
		PaddleOcrAll engine,
		ILogger<PaddleOcrEngine> logger,
		TimeSpan processTimeout,
		long maxImageBytes)
	{
		_engine = engine ?? throw new ArgumentNullException(nameof(engine));
		_logger = logger;
		_processTimeout = processTimeout;
		_maxImageBytes = maxImageBytes;
	}

	public async Task<OcrResult> ExtractTextAsync(byte[] imageBytes, CancellationToken ct)
	{
		ct.ThrowIfCancellationRequested();

		if (imageBytes.Length > _maxImageBytes)
		{
			throw new ArgumentException(
				$"Image size ({imageBytes.Length:N0} bytes) exceeds the maximum allowed ({_maxImageBytes:N0} bytes).",
				nameof(imageBytes));
		}

		await _semaphore.WaitAsync(ct);
		try
		{
			using CancellationTokenSource timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
			timeoutCts.CancelAfter(_processTimeout);

			CancellationToken linked = timeoutCts.Token;
			linked.ThrowIfCancellationRequested();

			// PaddleOCR Run is synchronous and requires OpenCvSharp.Mat;
			// wrap in Task.Run to avoid blocking the caller.
			PaddleOcrResult result = await Task.Run(() =>
			{
				using Mat mat = Cv2.ImDecode(imageBytes, ImreadModes.Color);
				if (mat.Empty())
				{
					throw new ArgumentException(
						"Failed to decode image: data is corrupt or the format is not supported.",
						nameof(imageBytes));
				}

				return _engine.Run(mat);
			}, linked);

			if (linked.IsCancellationRequested)
			{
				_logger.LogWarning(
					"PaddleOCR processing was cancelled or timed out after Run() completed");
				linked.ThrowIfCancellationRequested();
			}

			string rawText = ExtractText(result);
			float confidence = ComputeAverageConfidence(result);

			string correctedText = OcrCorrectionHelper.ApplyOcrCorrections(rawText);

			_logger.LogDebug(
				"PaddleOCR completed: {RegionCount} regions, {CharCount} chars, confidence {Confidence:P1}",
				result.Regions.Length,
				correctedText.Length,
				confidence);

			return new OcrResult(correctedText, confidence);
		}
		finally
		{
			_semaphore.Release();
		}
	}

	/// <summary>
	/// Extracts concatenated text from PaddleOCR regions, reconstructing lines
	/// by grouping regions whose centers share a Y-coordinate within a tolerance.
	/// Paddle's detector often segments per phrase/word rather than per line, so
	/// naïvely joining <c>r.Text</c> with <c>\n</c> produces many fragment-lines
	/// that break downstream parsers (they expect <c>DESC UPC FLAG PRICE</c> on
	/// one line, not four). Grouping by Y reassembles the visual rows.
	/// </summary>
	internal static string ExtractText(PaddleOcrResult result)
	{
		if (result.Regions.Length == 0)
		{
			return string.Empty;
		}

		return ReconstructLines(
			result.Regions.Select(r => (r.Rect.Center.X, r.Rect.Center.Y, r.Rect.Size.Height, r.Text)));
	}

	/// <summary>
	/// Core line-reconstruction implementation split out for testability.
	/// Groups regions into lines using <c>median(height) * 0.5</c> as the Y-tolerance.
	/// </summary>
	internal static string ReconstructLines(
		IEnumerable<(float X, float Y, float Height, string Text)> regions)
	{
		(float X, float Y, float Height, string Text)[] ordered = regions
			.Where(r => !string.IsNullOrEmpty(r.Text))
			.ToArray();

		if (ordered.Length == 0)
		{
			return string.Empty;
		}

		// Y-tolerance = half the median region height. The median is robust to
		// outlier giant/tiny regions.
		float[] heights = ordered.Select(r => r.Height).OrderBy(h => h).ToArray();
		float medianHeight = heights.Length % 2 == 0
			? (heights[heights.Length / 2 - 1] + heights[heights.Length / 2]) / 2f
			: heights[heights.Length / 2];
		float tolerance = medianHeight * 0.5f;

		// Sort top-to-bottom by Y, then walk through and start a new line
		// whenever the Y jump exceeds the tolerance.
		(float X, float Y, float Height, string Text)[] byY = [.. ordered.OrderBy(r => r.Y)];

		List<List<(float X, string Text)>> lines = [];
		List<(float X, string Text)> currentLine = [(byY[0].X, byY[0].Text)];
		float currentLineY = byY[0].Y;

		for (int i = 1; i < byY.Length; i++)
		{
			if (byY[i].Y - currentLineY > tolerance)
			{
				lines.Add(currentLine);
				currentLine = [(byY[i].X, byY[i].Text)];
				currentLineY = byY[i].Y;
			}
			else
			{
				currentLine.Add((byY[i].X, byY[i].Text));
			}
		}
		lines.Add(currentLine);

		// Within each line, sort left-to-right by X and join with space.
		return string.Join('\n', lines.Select(line =>
			string.Join(' ', line.OrderBy(w => w.X).Select(w => w.Text))));
	}

	/// <summary>
	/// Computes a weighted average confidence across all detected regions.
	/// Each region's score is weighted by its text length. Regions with NaN or
	/// non-finite scores are skipped so the aggregate cannot poison downstream
	/// JSON serialization, which rejects NaN/Infinity.
	/// </summary>
	internal static float ComputeAverageConfidence(PaddleOcrResult result)
		=> ComputeWeightedConfidence(result.Regions.Select(r => (r.Score, r.Text.Length)));

	/// <summary>
	/// Core weighted-average implementation split out for testability — skips
	/// NaN/Infinity scores and clamps the result to [0, 1].
	/// </summary>
	internal static float ComputeWeightedConfidence(IEnumerable<(float Score, int Length)> regions)
	{
		float totalWeightedScore = 0f;
		int totalLength = 0;

		foreach ((float score, int length) in regions)
		{
			if (!float.IsFinite(score))
			{
				continue;
			}

			totalWeightedScore += score * length;
			totalLength += length;
		}

		return totalLength > 0 ? Math.Clamp(totalWeightedScore / totalLength, 0f, 1f) : 0f;
	}

	public void Dispose()
	{
		if (!_disposed)
		{
			_engine.Dispose();
			_semaphore.Dispose();
			_disposed = true;
		}
	}
}
