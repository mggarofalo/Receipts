using System.Text.RegularExpressions;

namespace Infrastructure.Services;

/// <summary>
/// Shared OCR text correction logic used by all OCR engine implementations.
/// Fixes common OCR character confusions (S→$, O→0, l→1, I→1) and normalizes whitespace.
/// </summary>
internal static partial class OcrCorrectionHelper
{
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
}
