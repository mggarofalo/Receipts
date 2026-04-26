using System.Diagnostics;
using System.Globalization;
using Application.Interfaces.Services;
using Application.Models.Ocr;
using Microsoft.Extensions.Logging;

namespace VlmEval;

public sealed class FixtureEvaluator(
	IReceiptExtractionService extractionService,
	ILogger<FixtureEvaluator> logger)
{
	private const decimal MoneyTolerance = 0.01m;

	public async Task<FixtureResult> EvaluateAsync(Fixture fixture, CancellationToken cancellationToken)
	{
		Stopwatch stopwatch = Stopwatch.StartNew();

		byte[] bytes;
		try
		{
			bytes = await File.ReadAllBytesAsync(fixture.FilePath, cancellationToken);
		}
		catch (Exception ex)
		{
			return new FixtureResult(fixture.Name, false, stopwatch.Elapsed, [], $"Failed to read fixture file: {ex.Message}");
		}

		ParsedReceipt parsed;
		try
		{
			parsed = await extractionService.ExtractAsync(bytes, fixture.ContentType, cancellationToken);
		}
		catch (Exception ex)
		{
			stopwatch.Stop();
			logger.LogError(ex, "VLM call failed for {Fixture}", fixture.Name);
			return new FixtureResult(fixture.Name, false, stopwatch.Elapsed, [], $"VLM call failed: {ex.GetType().Name}: {ex.Message}");
		}

		stopwatch.Stop();

		List<FieldDiff> diffs = [];
		diffs.Add(DiffStore(fixture.Expected.Store, parsed.StoreName));
		diffs.Add(DiffDate(fixture.Expected.Date, parsed.Date));
		diffs.Add(DiffMoney("subtotal", fixture.Expected.Subtotal, parsed.Subtotal));
		diffs.Add(DiffMoney("total", fixture.Expected.Total, parsed.Total));
		diffs.Add(DiffTaxLines(fixture.Expected.TaxLines, parsed.TaxLines));
		diffs.Add(DiffPaymentMethod(fixture.Expected.PaymentMethod, parsed.PaymentMethod));
		diffs.Add(DiffMinItemCount(fixture.Expected.MinItemCount, parsed.Items));
		diffs.AddRange(DiffItems(fixture.Expected.Items, parsed.Items));

		bool allDeclaredPassed = diffs.All(d => d.Status != DiffStatus.Fail);

		return new FixtureResult(fixture.Name, allDeclaredPassed, stopwatch.Elapsed, diffs, Error: null);
	}

	internal static FieldDiff DiffStore(string? expected, FieldConfidence<string> actual)
	{
		if (expected is null)
		{
			return new FieldDiff("store", DiffStatus.NotDeclared, null, actual.Value, null);
		}

		if (!actual.IsPresent || string.IsNullOrWhiteSpace(actual.Value))
		{
			return new FieldDiff("store", DiffStatus.Fail, expected, null, "VLM did not extract store name");
		}

		bool match = actual.Value.Contains(expected, StringComparison.OrdinalIgnoreCase);
		return new FieldDiff(
			"store",
			match ? DiffStatus.Pass : DiffStatus.Fail,
			expected,
			actual.Value,
			match ? null : "store name does not contain expected substring");
	}

	internal static FieldDiff DiffDate(DateOnly? expected, FieldConfidence<DateOnly> actual)
	{
		if (expected is null)
		{
			return new FieldDiff("date", DiffStatus.NotDeclared, null, actual.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), null);
		}

		if (!actual.IsPresent)
		{
			return new FieldDiff("date", DiffStatus.Fail, expected.Value.ToString("yyyy-MM-dd"), null, "VLM did not extract date");
		}

		bool match = actual.Value == expected.Value;
		return new FieldDiff(
			"date",
			match ? DiffStatus.Pass : DiffStatus.Fail,
			expected.Value.ToString("yyyy-MM-dd"),
			actual.Value.ToString("yyyy-MM-dd"),
			null);
	}

	internal static FieldDiff DiffMoney(string field, decimal? expected, FieldConfidence<decimal> actual)
	{
		if (expected is null)
		{
			return new FieldDiff(field, DiffStatus.NotDeclared, null, Format(actual), null);
		}

		if (!actual.IsPresent)
		{
			return new FieldDiff(field, DiffStatus.Fail, expected.Value.ToString("0.00", CultureInfo.InvariantCulture), null, $"VLM did not extract {field}");
		}

		decimal delta = Math.Abs(actual.Value - expected.Value);
		bool match = delta < MoneyTolerance;
		return new FieldDiff(
			field,
			match ? DiffStatus.Pass : DiffStatus.Fail,
			expected.Value.ToString("0.00", CultureInfo.InvariantCulture),
			actual.Value.ToString("0.00", CultureInfo.InvariantCulture),
			match ? null : $"delta=${delta.ToString("0.00", CultureInfo.InvariantCulture)}");

		static string? Format(FieldConfidence<decimal> f) =>
			f.IsPresent ? f.Value.ToString("0.00", CultureInfo.InvariantCulture) : null;
	}

	internal static FieldDiff DiffTaxLines(List<ExpectedTaxLine>? expected, List<ParsedTaxLine> actual)
	{
		if (expected is null || expected.Count == 0)
		{
			return new FieldDiff("taxLines", DiffStatus.NotDeclared, null, $"actual={actual.Count}", null);
		}

		List<decimal> actualAmounts = [.. actual
			.Where(t => t.Amount.IsPresent)
			.Select(t => t.Amount.Value)];

		List<string> failures = [];
		List<string> matched = [];
		foreach (ExpectedTaxLine declared in expected)
		{
			if (declared.Amount is null)
			{
				continue;
			}

			decimal wanted = declared.Amount.Value;
			int idx = FindClosest(actualAmounts, wanted, MoneyTolerance);
			if (idx < 0)
			{
				failures.Add($"no tax line within ${MoneyTolerance:0.00} of ${wanted:0.00}");
				continue;
			}

			matched.Add(actualAmounts[idx].ToString("0.00", CultureInfo.InvariantCulture));
			actualAmounts.RemoveAt(idx);
		}

		bool pass = failures.Count == 0;
		return new FieldDiff(
			"taxLines",
			pass ? DiffStatus.Pass : DiffStatus.Fail,
			string.Join(", ", expected.Where(t => t.Amount is not null).Select(t => t.Amount!.Value.ToString("0.00", CultureInfo.InvariantCulture))),
			string.Join(", ", actual.Where(t => t.Amount.IsPresent).Select(t => t.Amount.Value.ToString("0.00", CultureInfo.InvariantCulture))),
			pass ? null : string.Join("; ", failures));
	}

	private static int FindClosest(IList<decimal> pool, decimal target, decimal tolerance)
	{
		int bestIndex = -1;
		decimal bestDelta = decimal.MaxValue;
		for (int i = 0; i < pool.Count; i++)
		{
			decimal delta = Math.Abs(pool[i] - target);
			if (delta < tolerance && delta < bestDelta)
			{
				bestDelta = delta;
				bestIndex = i;
			}
		}
		return bestIndex;
	}

	internal static FieldDiff DiffPaymentMethod(string? expected, FieldConfidence<string?> actual)
	{
		if (expected is null)
		{
			return new FieldDiff("paymentMethod", DiffStatus.NotDeclared, null, actual.Value, null);
		}

		if (!actual.IsPresent || string.IsNullOrWhiteSpace(actual.Value))
		{
			return new FieldDiff("paymentMethod", DiffStatus.Fail, expected, null, "VLM did not extract paymentMethod");
		}

		bool match = actual.Value.Contains(expected, StringComparison.OrdinalIgnoreCase);
		return new FieldDiff(
			"paymentMethod",
			match ? DiffStatus.Pass : DiffStatus.Fail,
			expected,
			actual.Value,
			match ? null : "payment method does not contain expected substring");
	}

	internal static FieldDiff DiffMinItemCount(int? expected, List<ParsedReceiptItem> actual)
	{
		if (expected is null)
		{
			return new FieldDiff("minItemCount", DiffStatus.NotDeclared, null, actual.Count.ToString(CultureInfo.InvariantCulture), null);
		}

		bool match = actual.Count >= expected.Value;
		return new FieldDiff(
			"minItemCount",
			match ? DiffStatus.Pass : DiffStatus.Fail,
			$">={expected.Value}",
			actual.Count.ToString(CultureInfo.InvariantCulture),
			match ? null : $"expected at least {expected.Value} items, got {actual.Count}");
	}

	internal static List<FieldDiff> DiffItems(List<ExpectedItem>? expected, List<ParsedReceiptItem> actual)
	{
		if (expected is null || expected.Count == 0)
		{
			return [];
		}

		List<ParsedReceiptItem> pool = [.. actual];
		List<FieldDiff> results = [];

		for (int i = 0; i < expected.Count; i++)
		{
			ExpectedItem item = expected[i];
			string fieldName = $"items[{i}]";

			if (item.TotalPrice is null && item.Description is null)
			{
				results.Add(new FieldDiff(fieldName, DiffStatus.NotDeclared, null, null, null));
				continue;
			}

			int matchedIndex = -1;
			if (item.TotalPrice is not null)
			{
				decimal target = item.TotalPrice.Value;
				decimal best = decimal.MaxValue;
				for (int p = 0; p < pool.Count; p++)
				{
					if (!pool[p].TotalPrice.IsPresent)
					{
						continue;
					}
					decimal delta = Math.Abs(pool[p].TotalPrice.Value - target);
					if (delta < MoneyTolerance && delta < best)
					{
						best = delta;
						matchedIndex = p;
					}
				}
			}

			if (matchedIndex < 0 && item.Description is not null)
			{
				for (int p = 0; p < pool.Count; p++)
				{
					if (!string.IsNullOrWhiteSpace(pool[p].Description.Value)
						&& pool[p].Description.Value!.Contains(item.Description, StringComparison.OrdinalIgnoreCase))
					{
						matchedIndex = p;
						break;
					}
				}
			}

			if (matchedIndex < 0)
			{
				results.Add(new FieldDiff(
					fieldName,
					DiffStatus.Fail,
					FormatExpectedItem(item),
					null,
					"no matching line in VLM output"));
				continue;
			}

			ParsedReceiptItem matched = pool[matchedIndex];
			pool.RemoveAt(matchedIndex);

			List<string> issues = [];
			if (item.Description is not null
				&& (matched.Description.Value is null
					|| !matched.Description.Value.Contains(item.Description, StringComparison.OrdinalIgnoreCase)))
			{
				issues.Add($"description mismatch (expected='{item.Description}', actual='{matched.Description.Value}')");
			}

			if (item.TotalPrice is not null)
			{
				if (!matched.TotalPrice.IsPresent)
				{
					issues.Add("missing totalPrice");
				}
				else if (Math.Abs(matched.TotalPrice.Value - item.TotalPrice.Value) >= MoneyTolerance)
				{
					issues.Add($"totalPrice mismatch (expected={item.TotalPrice.Value:0.00}, actual={matched.TotalPrice.Value:0.00})");
				}
			}

			results.Add(new FieldDiff(
				fieldName,
				issues.Count == 0 ? DiffStatus.Pass : DiffStatus.Fail,
				FormatExpectedItem(item),
				FormatActualItem(matched),
				issues.Count == 0 ? null : string.Join("; ", issues)));
		}

		return results;
	}

	private static string FormatExpectedItem(ExpectedItem item)
	{
		string desc = item.Description ?? "*";
		string price = item.TotalPrice?.ToString("0.00", CultureInfo.InvariantCulture) ?? "*";
		return $"{desc} @ {price}";
	}

	private static string FormatActualItem(ParsedReceiptItem item)
	{
		string desc = item.Description.Value ?? "?";
		string price = item.TotalPrice.IsPresent
			? item.TotalPrice.Value.ToString("0.00", CultureInfo.InvariantCulture)
			: "?";
		return $"{desc} @ {price}";
	}
}
