using System.Text.RegularExpressions;
using Application.Interfaces.Services;
using Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Tesseract;

namespace Infrastructure.Services;

public sealed partial class TesseractOcrEngine : IOcrEngine, IDisposable
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
			using Page page = _engine.Process(pix, PageSegMode.SingleBlock);

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
	{
		if (string.IsNullOrEmpty(raw))
		{
			return raw;
		}

		// 1. Dollar sign: S followed by a number in price context (at start or after whitespace).
		//    Must run before digit-confusion fixes so "S1O.5O" becomes "$1O.5O" first,
		//    allowing the numeric token regex to match "1O.5O" (not preceded by a letter).
		string result = DollarSignRegex().Replace(raw, "$$");

		// 2. Fix numeric tokens: find tokens composed of digits, O, l, I, and periods
		//    that contain at least one real digit, then fix character confusions within them.
		//    O→0, l→1, I→1 within numeric context.
		result = NumericTokenRegex().Replace(result, FixNumericToken);

		// 3. Line normalization: trim each line, collapse 3+ consecutive blank lines to 2
		string[] lines = result.Split('\n');
		for (int i = 0; i < lines.Length; i++)
		{
			lines[i] = lines[i].TrimEnd();
		}

		result = string.Join('\n', lines);
		result = CollapseBlankLinesRegex().Replace(result, "\n\n");

		return result;
	}

	private static string FixNumericToken(Match match)
	{
		string token = match.Value;

		// Only apply corrections if the token contains at least one real digit
		bool hasDigit = false;
		foreach (char c in token)
		{
			if (char.IsAsciiDigit(c))
			{
				hasDigit = true;
				break;
			}
		}

		if (!hasDigit)
		{
			return token;
		}

		return OcrDigitConfusionRegex().Replace(token, m => m.Value switch
		{
			"O" => "0",
			"l" => "1",
			"I" => "1",
			_ => m.Value
		});
	}

	// Matches tokens that look numeric: composed of digits, O, l, I, and at most one decimal point.
	// The token must be at a word boundary (not adjacent to letters other than the confusable ones).
	// Two alternatives: with decimal point, or integer-only.
	[GeneratedRegex(@"(?<![A-Za-z])[\dOlI]+\.[\dOlI]+(?![A-Za-z])|(?<![A-Za-z])[\dOlI]+(?![A-Za-z])")]
	private static partial Regex NumericTokenRegex();

	// Matches O, l, or I — the characters commonly confused with digits in OCR output.
	[GeneratedRegex(@"[OlI]")]
	private static partial Regex OcrDigitConfusionRegex();

	// S at start or after whitespace, followed by what looks like a price
	// (digits/OCR confusables, period, digits/OCR confusables).
	[GeneratedRegex(@"(?<=\s|^)S(?=[\dOlI]+\.[\dOlI])", RegexOptions.Multiline)]
	private static partial Regex DollarSignRegex();

	[GeneratedRegex(@"\n{3,}")]
	private static partial Regex CollapseBlankLinesRegex();

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
