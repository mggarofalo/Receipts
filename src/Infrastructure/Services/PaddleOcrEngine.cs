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
	/// Extracts concatenated text from PaddleOCR regions, preserving line structure.
	/// </summary>
	internal static string ExtractText(PaddleOcrResult result)
	{
		if (result.Regions.Length == 0)
		{
			return string.Empty;
		}

		return string.Join('\n', result.Regions.Select(r => r.Text));
	}

	/// <summary>
	/// Computes a weighted average confidence across all detected regions.
	/// Each region's score is weighted by its text length.
	/// </summary>
	internal static float ComputeAverageConfidence(PaddleOcrResult result)
	{
		if (result.Regions.Length == 0)
		{
			return 0f;
		}

		float totalWeightedScore = 0f;
		int totalLength = 0;

		foreach (PaddleOcrResultRegion region in result.Regions)
		{
			int length = region.Text.Length;
			totalWeightedScore += region.Score * length;
			totalLength += length;
		}

		return totalLength > 0 ? totalWeightedScore / totalLength : 0f;
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
