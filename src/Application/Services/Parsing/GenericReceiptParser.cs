using System.Globalization;
using System.Text.RegularExpressions;
using Application.Interfaces.Services;
using Application.Models.Ocr;

namespace Application.Services.Parsing;

public partial class GenericReceiptParser : IReceiptParser
{
	public bool CanParse(string ocrText) => true;

	public ParsedReceipt Parse(string ocrText)
	{
		string[] lines = ocrText.Split('\n', StringSplitOptions.TrimEntries);

		FieldConfidence<string> storeName = ExtractStoreName(lines);
		FieldConfidence<DateOnly> date = ExtractDate(ocrText);
		List<ParsedReceiptItem> items = ExtractItems(lines);
		FieldConfidence<decimal> subtotal = ExtractSubtotal(lines);
		List<ParsedTaxLine> taxLines = ExtractTaxLines(lines);
		FieldConfidence<decimal> total = ExtractTotal(lines);
		FieldConfidence<string?> paymentMethod = ExtractPaymentMethod(ocrText);

		return new ParsedReceipt(storeName, date, items, subtotal, taxLines, total, paymentMethod);
	}

	private static FieldConfidence<string> ExtractStoreName(string[] lines)
	{
		foreach (string line in lines)
		{
			if (string.IsNullOrWhiteSpace(line))
			{
				continue;
			}

			if (DatePattern().IsMatch(line) || PricePattern().IsMatch(line))
			{
				continue;
			}

			return FieldConfidence<string>.Low(line.Trim());
		}

		return FieldConfidence<string>.None();
	}

	private static FieldConfidence<DateOnly> ExtractDate(string ocrText)
	{
		Match isoMatch = IsoDatePattern().Match(ocrText);
		if (isoMatch.Success && DateOnly.TryParseExact(isoMatch.Value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly isoDate))
		{
			return FieldConfidence<DateOnly>.High(isoDate);
		}

		Match usMatch = UsDatePattern().Match(ocrText);
		if (usMatch.Success && DateOnly.TryParseExact(usMatch.Value, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly usDate))
		{
			return FieldConfidence<DateOnly>.Medium(usDate);
		}

		Match shortUsMatch = ShortUsDatePattern().Match(ocrText);
		if (shortUsMatch.Success && DateOnly.TryParseExact(shortUsMatch.Value, "MM/dd/yy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly shortUsDate))
		{
			return FieldConfidence<DateOnly>.Medium(shortUsDate);
		}

		Match longDateMatch = LongDatePattern().Match(ocrText);
		if (longDateMatch.Success && DateOnly.TryParse(longDateMatch.Value, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly longDate))
		{
			return FieldConfidence<DateOnly>.Medium(longDate);
		}

		return FieldConfidence<DateOnly>.None();
	}

	private static FieldConfidence<decimal> ExtractTotal(string[] lines)
	{
		for (int i = lines.Length - 1; i >= 0; i--)
		{
			string upper = lines[i].ToUpperInvariant();
			if (!TotalLinePattern().IsMatch(upper) || SubtotalLinePattern().IsMatch(upper))
			{
				continue;
			}

			Match priceMatch = PricePattern().Match(lines[i]);
			if (priceMatch.Success && decimal.TryParse(priceMatch.Value, CultureInfo.InvariantCulture, out decimal totalAmount))
			{
				return FieldConfidence<decimal>.High(totalAmount);
			}
		}

		return FieldConfidence<decimal>.None();
	}

	private static FieldConfidence<decimal> ExtractSubtotal(string[] lines)
	{
		for (int i = lines.Length - 1; i >= 0; i--)
		{
			string upper = lines[i].ToUpperInvariant();
			if (!SubtotalLinePattern().IsMatch(upper))
			{
				continue;
			}

			Match priceMatch = PricePattern().Match(lines[i]);
			if (priceMatch.Success && decimal.TryParse(priceMatch.Value, CultureInfo.InvariantCulture, out decimal subtotalAmount))
			{
				return FieldConfidence<decimal>.High(subtotalAmount);
			}
		}

		return FieldConfidence<decimal>.None();
	}

	private static List<ParsedTaxLine> ExtractTaxLines(string[] lines)
	{
		List<ParsedTaxLine> taxLines = [];

		foreach (string line in lines)
		{
			string upper = line.ToUpperInvariant();
			if (!TaxLinePattern().IsMatch(upper))
			{
				continue;
			}

			Match priceMatch = PricePattern().Match(line);
			if (!priceMatch.Success || !decimal.TryParse(priceMatch.Value, CultureInfo.InvariantCulture, out decimal taxAmount))
			{
				continue;
			}

			string label = PricePattern().Replace(line, "").Trim();
			taxLines.Add(new ParsedTaxLine(
				FieldConfidence<string>.Medium(label),
				FieldConfidence<decimal>.Medium(taxAmount)
			));
		}

		return taxLines;
	}

	private static FieldConfidence<string?> ExtractPaymentMethod(string ocrText)
	{
		string upper = ocrText.ToUpperInvariant();

		if (upper.Contains("VISA"))
		{
			return FieldConfidence<string?>.High("VISA");
		}

		if (upper.Contains("MASTERCARD"))
		{
			return FieldConfidence<string?>.High("MASTERCARD");
		}

		if (upper.Contains("AMEX") || upper.Contains("AMERICAN EXPRESS"))
		{
			return FieldConfidence<string?>.High("AMEX");
		}

		if (upper.Contains("DEBIT"))
		{
			return FieldConfidence<string?>.Medium("DEBIT");
		}

		if (upper.Contains("CREDIT"))
		{
			return FieldConfidence<string?>.Medium("CREDIT");
		}

		if (upper.Contains("CASH"))
		{
			return FieldConfidence<string?>.Medium("CASH");
		}

		return FieldConfidence<string?>.None();
	}

	protected static List<ParsedReceiptItem> ExtractItems(string[] lines)
	{
		List<ParsedReceiptItem> items = [];

		foreach (string line in lines)
		{
			string upper = line.ToUpperInvariant();
			if (TotalLinePattern().IsMatch(upper) ||
				SubtotalLinePattern().IsMatch(upper) ||
				TaxLinePattern().IsMatch(upper) ||
				ChangeLinePattern().IsMatch(upper) ||
				string.IsNullOrWhiteSpace(line))
			{
				continue;
			}

			Match priceMatch = PricePattern().Match(line);
			if (!priceMatch.Success || !decimal.TryParse(priceMatch.Value, CultureInfo.InvariantCulture, out decimal price))
			{
				continue;
			}

			string description = line[..priceMatch.Index].Trim();
			if (string.IsNullOrWhiteSpace(description))
			{
				continue;
			}

			items.Add(new ParsedReceiptItem(
				Code: FieldConfidence<string?>.None(),
				Description: FieldConfidence<string>.Medium(description),
				Quantity: FieldConfidence<decimal>.Low(1m),
				UnitPrice: FieldConfidence<decimal>.Medium(price),
				TotalPrice: FieldConfidence<decimal>.Medium(price)
			));
		}

		return items;
	}

	[GeneratedRegex(@"\d{4}-\d{2}-\d{2}")]
	private static partial Regex IsoDatePattern();

	[GeneratedRegex(@"\d{2}/\d{2}/\d{4}")]
	private static partial Regex UsDatePattern();

	[GeneratedRegex(@"\d{2}/\d{2}/\d{2}")]
	private static partial Regex ShortUsDatePattern();

	[GeneratedRegex(@"(?:January|February|March|April|May|June|July|August|September|October|November|December)\s+\d{1,2},?\s+\d{4}", RegexOptions.IgnoreCase)]
	private static partial Regex LongDatePattern();

	[GeneratedRegex(@"\d+\.\d{2}")]
	protected static partial Regex PricePattern();

	[GeneratedRegex(@"\d{2}[/-]\d{2}[/-]\d{2,4}")]
	private static partial Regex DatePattern();

	[GeneratedRegex(@"\b(?:GRAND\s+)?TOTAL\b")]
	protected static partial Regex TotalLinePattern();

	[GeneratedRegex(@"\bSUB[\s-]?TOTAL\b")]
	protected static partial Regex SubtotalLinePattern();

	[GeneratedRegex(@"\b(?:TAX|GST|HST|VAT|PST)\b")]
	protected static partial Regex TaxLinePattern();

	[GeneratedRegex(@"\bCHANGE\b")]
	private static partial Regex ChangeLinePattern();
}
