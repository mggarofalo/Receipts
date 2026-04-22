using Application.Interfaces.Services;
using Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Tesseract;

namespace Infrastructure.Services;

public sealed class TesseractOcrEngine : IOcrEngine, IDisposable
{
	private const int DefaultTimeoutSeconds = 30;
	private const long DefaultMaxImageBytes = 10 * 1024 * 1024; // 10 MB

	private readonly TesseractEngine _engine;
	private readonly SemaphoreSlim _semaphore = new(1, 1);
	private readonly ILogger<TesseractOcrEngine> _logger;
	private readonly TimeSpan _processTimeout;
	private readonly long _maxImageBytes;
	private bool _disposed;

	public TesseractOcrEngine(IConfiguration configuration, ILogger<TesseractOcrEngine> logger)
	{
		_logger = logger;

		int timeoutSeconds = int.TryParse(configuration[ConfigurationVariables.OcrTimeoutSeconds], out int ts)
			? ts
			: DefaultTimeoutSeconds;
		_processTimeout = TimeSpan.FromSeconds(timeoutSeconds);

		_maxImageBytes = long.TryParse(configuration[ConfigurationVariables.OcrMaxImageBytes], out long mb)
			? mb
			: DefaultMaxImageBytes;

		string tessdataPath = configuration[ConfigurationVariables.TessdataPath]
			?? Path.Combine(AppContext.BaseDirectory, "Models", "Tessdata");

		if (!Directory.Exists(tessdataPath))
		{
			throw new FileNotFoundException(
				$"Tessdata directory not found at '{tessdataPath}'. " +
				"Ensure the eng.traineddata file is present in the Tessdata directory.");
		}

		string trainedDataFile = Path.Combine(tessdataPath, "eng.traineddata");
		if (!File.Exists(trainedDataFile))
		{
			throw new FileNotFoundException(
				$"Tesseract trained data file not found at '{trainedDataFile}'. " +
				"Download eng.traineddata from tessdata_best and place it in the Tessdata directory.");
		}

		_engine = new TesseractEngine(tessdataPath, "eng", EngineMode.Default);
		_logger.LogInformation("Tesseract OCR engine initialized with tessdata from {TessdataPath}", tessdataPath);
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

			using Pix pix = Pix.LoadFromMemory(imageBytes);
			// Auto (PSM 3) runs full automatic page segmentation without OSD. On real
			// receipt photos this preserves vertical line breaks more reliably than
			// SingleColumn (PSM 4) or SingleBlock (PSM 6), both of which can collapse
			// rows on photos with tight line spacing and break line-based parsers in
			// Application.Services.Parsing.
			using Page page = _engine.Process(pix, PageSegMode.Auto);

			// Check timeout/cancellation after native call returns
			if (linked.IsCancellationRequested)
			{
				_logger.LogWarning(
					"OCR processing was cancelled or timed out after native Process() completed");
				linked.ThrowIfCancellationRequested();
			}

			string rawText = page.GetText();
			float confidence = page.GetMeanConfidence();

			string correctedText = ApplyOcrCorrections(rawText);

			_logger.LogDebug(
				"OCR completed: {CharCount} chars, confidence {Confidence:P1}",
				correctedText.Length,
				confidence);

			return new OcrResult(correctedText, confidence);
		}
		finally
		{
			_semaphore.Release();
		}
	}

	internal static string ApplyOcrCorrections(string raw)
		=> OcrCorrectionHelper.ApplyOcrCorrections(raw);

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
