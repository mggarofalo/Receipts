using System.Globalization;
using System.Text.RegularExpressions;
using Application.Interfaces.Services;
using Application.Models.Ocr;

namespace Application.Services.Parsing;

public partial class KrogerReceiptParser : IReceiptParser
{
	public bool CanParse(string ocrText)
	{
		string header = ocrText.Length > 500 ? ocrText[..500] : ocrText;
		return KrogerIdentifier().IsMatch(header);
	}

	public ParsedReceipt Parse(string ocrText)
	{
		string[] lines = ocrText.Split('\n', StringSplitOptions.TrimEntries);

		FieldConfidence<string> storeName = FieldConfidence<string>.High("Kroger");
		FieldConfidence<DateOnly> date = ExtractDate(lines);
		List<ParsedReceiptItem> items = ExtractItems(lines);
		FieldConfidence<decimal> subtotal = ExtractLabeled(lines, SubtotalPattern());
		List<ParsedTaxLine> taxLines = ExtractTaxLines(lines);
		FieldConfidence<decimal> total = ExtractLabeled(lines, TotalPattern());
		FieldConfidence<string?> paymentMethod = ExtractPaymentMethod(lines);

		return new ParsedReceipt(storeName, date, items, subtotal, taxLines, total, paymentMethod);
	}

	private static FieldConfidence<DateOnly> ExtractDate(string[] lines)
	{
		foreach (string line in lines)
		{
			Match match = DatePattern().Match(line);
			if (match.Success && DateOnly.TryParseExact(match.Value, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly date))
			{
				return FieldConfidence<DateOnly>.High(date);
			}
		}

		return FieldConfidence<DateOnly>.None();
	}

	private static List<ParsedReceiptItem> ExtractItems(string[] lines)
	{
		List<ParsedReceiptItem> items = [];

		foreach (string line in lines)
		{
			string upper = line.ToUpperInvariant();
			if (TotalPattern().IsMatch(upper) ||
				SubtotalPattern().IsMatch(upper) ||
				TaxPattern().IsMatch(upper) ||
				FuelPointsPattern().IsMatch(upper) ||
				DigitalCouponPattern().IsMatch(upper) ||
				string.IsNullOrWhiteSpace(line))
			{
				continue;
			}

			Match itemMatch = KrogerItemPattern().Match(line);
			if (!itemMatch.Success)
			{
				continue;
			}

			string description = itemMatch.Groups["desc"].Value.Trim();
			string priceStr = itemMatch.Groups["price"].Value;

			if (!decimal.TryParse(priceStr, CultureInfo.InvariantCulture, out decimal price))
			{
				continue;
			}

			items.Add(new ParsedReceiptItem(
				Code: FieldConfidence<string?>.None(),
				Description: FieldConfidence<string>.High(description),
				Quantity: FieldConfidence<decimal>.Medium(1m),
				UnitPrice: FieldConfidence<decimal>.High(price),
				TotalPrice: FieldConfidence<decimal>.High(price)
			));
		}

		return items;
	}

	private static FieldConfidence<decimal> ExtractLabeled(string[] lines, Regex pattern)
	{
		for (int i = lines.Length - 1; i >= 0; i--)
		{
			string upper = lines[i].ToUpperInvariant();
			if (!pattern.IsMatch(upper))
			{
				continue;
			}

			Match priceMatch = PricePattern().Match(lines[i]);
			if (priceMatch.Success && decimal.TryParse(priceMatch.Value, CultureInfo.InvariantCulture, out decimal amount))
			{
				return FieldConfidence<decimal>.High(amount);
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
			if (!TaxPattern().IsMatch(upper))
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
				FieldConfidence<string>.High(label),
				FieldConfidence<decimal>.High(taxAmount)
			));
		}

		return taxLines;
	}

	private static FieldConfidence<string?> ExtractPaymentMethod(string[] lines)
	{
		foreach (string line in lines)
		{
			string upper = line.ToUpperInvariant();
			if (upper.Contains("VISA"))
			{
				return FieldConfidence<string?>.High("VISA");
			}

			if (upper.Contains("MASTERCARD"))
			{
				return FieldConfidence<string?>.High("MASTERCARD");
			}

			if (upper.Contains("DEBIT"))
			{
				return FieldConfidence<string?>.Medium("DEBIT");
			}

			if (upper.Contains("CREDIT"))
			{
				return FieldConfidence<string?>.Medium("CREDIT");
			}
		}

		return FieldConfidence<string?>.None();
	}

	[GeneratedRegex(@"\bKROGER\b|\bFRED\s+MEYER\b", RegexOptions.IgnoreCase)]
	private static partial Regex KrogerIdentifier();

	[GeneratedRegex(@"\d{2}/\d{2}/\d{4}")]
	private static partial Regex DatePattern();

	[GeneratedRegex(@"^(?<desc>.+?)\s+(?<price>\d+\.\d{2})\s*$")]
	private static partial Regex KrogerItemPattern();

	[GeneratedRegex(@"\d+\.\d{2}")]
	private static partial Regex PricePattern();

	[GeneratedRegex(@"\b(?:GRAND\s+)?TOTAL\b")]
	private static partial Regex TotalPattern();

	[GeneratedRegex(@"\bSUB[\s-]?TOTAL\b")]
	private static partial Regex SubtotalPattern();

	[GeneratedRegex(@"\bTAX\b")]
	private static partial Regex TaxPattern();

	[GeneratedRegex(@"\bFUEL\s+POINTS?\b")]
	private static partial Regex FuelPointsPattern();

	[GeneratedRegex(@"\bDIGITAL\s+COUPON\b")]
	private static partial Regex DigitalCouponPattern();
}
