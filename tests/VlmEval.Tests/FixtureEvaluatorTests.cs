using Application.Interfaces.Services;
using Application.Models.Ocr;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace VlmEval.Tests;

/// <summary>
/// Tests for <see cref="FixtureEvaluator"/> scoring helpers and the end-to-end
/// <see cref="FixtureEvaluator.EvaluateAsync(Fixture, CancellationToken)"/> flow.
///
/// These tests document <em>current</em> behavior. Where current behavior is known to be
/// buggy (e.g., money tolerance off-by-one — RECEIPTS-634), the test pins the existing
/// behavior with a TODO comment so the fix in the follow-up issue will turn red and
/// force the test to be flipped at the same time.
/// </summary>
public class FixtureEvaluatorTests
{
	#region DiffStore

	[Fact]
	public void DiffStore_SubstringMatch_ReturnsPass()
	{
		FieldDiff diff = FixtureEvaluator.DiffStore("Walmart", FieldConfidence<string>.High("Walmart Supercenter #1234"));

		diff.Field.Should().Be("store");
		diff.Status.Should().Be(DiffStatus.Pass);
		diff.Detail.Should().BeNull();
	}

	[Fact]
	public void DiffStore_CaseInsensitiveMatch_ReturnsPass()
	{
		FieldDiff diff = FixtureEvaluator.DiffStore("walmart", FieldConfidence<string>.High("WALMART SUPERCENTER"));

		diff.Status.Should().Be(DiffStatus.Pass);
	}

	[Fact]
	public void DiffStore_NoSubstringMatch_ReturnsFail()
	{
		FieldDiff diff = FixtureEvaluator.DiffStore("Walmart", FieldConfidence<string>.High("Target Store"));

		diff.Status.Should().Be(DiffStatus.Fail);
		diff.Expected.Should().Be("Walmart");
		diff.Actual.Should().Be("Target Store");
		diff.Detail.Should().Contain("does not contain expected substring");
	}

	[Fact]
	public void DiffStore_MissingActualValue_ReturnsFail()
	{
		FieldDiff diff = FixtureEvaluator.DiffStore("Walmart", FieldConfidence<string>.None());

		diff.Status.Should().Be(DiffStatus.Fail);
		diff.Expected.Should().Be("Walmart");
		diff.Actual.Should().BeNull();
		diff.Detail.Should().Be("VLM did not extract store name");
	}

	[Fact]
	public void DiffStore_WhitespaceActualValue_ReturnsFail()
	{
		FieldDiff diff = FixtureEvaluator.DiffStore("Walmart", FieldConfidence<string>.High("   "));

		diff.Status.Should().Be(DiffStatus.Fail);
		diff.Detail.Should().Be("VLM did not extract store name");
	}

	[Fact]
	public void DiffStore_ExpectedNull_ReturnsNotDeclared()
	{
		FieldDiff diff = FixtureEvaluator.DiffStore(null, FieldConfidence<string>.High("Walmart"));

		diff.Status.Should().Be(DiffStatus.NotDeclared);
		diff.Expected.Should().BeNull();
		diff.Actual.Should().Be("Walmart");
	}

	#endregion

	#region DiffDate

	[Fact]
	public void DiffDate_ExactMatch_ReturnsPass()
	{
		DateOnly expected = new(2026, 1, 14);

		FieldDiff diff = FixtureEvaluator.DiffDate(expected, FieldConfidence<DateOnly>.High(expected));

		diff.Field.Should().Be("date");
		diff.Status.Should().Be(DiffStatus.Pass);
		diff.Expected.Should().Be("2026-01-14");
		diff.Actual.Should().Be("2026-01-14");
	}

	[Fact]
	public void DiffDate_Mismatch_ReturnsFail()
	{
		DateOnly expected = new(2026, 1, 14);
		DateOnly actual = new(2026, 1, 15);

		FieldDiff diff = FixtureEvaluator.DiffDate(expected, FieldConfidence<DateOnly>.High(actual));

		diff.Status.Should().Be(DiffStatus.Fail);
		diff.Expected.Should().Be("2026-01-14");
		diff.Actual.Should().Be("2026-01-15");
	}

	[Fact]
	public void DiffDate_LowConfidenceActual_ReturnsFail()
	{
		DateOnly expected = new(2026, 1, 14);

		FieldDiff diff = FixtureEvaluator.DiffDate(expected, FieldConfidence<DateOnly>.None());

		diff.Status.Should().Be(DiffStatus.Fail);
		diff.Expected.Should().Be("2026-01-14");
		diff.Actual.Should().BeNull();
		diff.Detail.Should().Be("VLM did not extract date");
	}

	[Fact]
	public void DiffDate_ExpectedNull_ReturnsNotDeclared()
	{
		DateOnly actual = new(2026, 1, 14);

		FieldDiff diff = FixtureEvaluator.DiffDate(null, FieldConfidence<DateOnly>.High(actual));

		diff.Status.Should().Be(DiffStatus.NotDeclared);
		diff.Expected.Should().BeNull();
		diff.Actual.Should().Be("1/14/2026");
	}

	#endregion

	#region DiffMoney

	[Fact]
	public void DiffMoney_ExactMatch_ReturnsPass()
	{
		FieldDiff diff = FixtureEvaluator.DiffMoney("total", 70.43m, FieldConfidence<decimal>.High(70.43m));

		diff.Field.Should().Be("total");
		diff.Status.Should().Be(DiffStatus.Pass);
		diff.Expected.Should().Be("70.43");
		diff.Actual.Should().Be("70.43");
	}

	[Fact]
	public void DiffMoney_DeltaZero_ReturnsPass()
	{
		// delta = 0.00 < tolerance (0.01) → pass
		FieldDiff diff = FixtureEvaluator.DiffMoney("subtotal", 1.00m, FieldConfidence<decimal>.High(1.00m));

		diff.Status.Should().Be(DiffStatus.Pass);
	}

	[Fact]
	public void DiffMoney_DeltaUnderHalfCent_ReturnsPass()
	{
		// delta = 0.005 < tolerance (0.01) → pass
		FieldDiff diff = FixtureEvaluator.DiffMoney("total", 1.000m, FieldConfidence<decimal>.High(1.005m));

		diff.Status.Should().Be(DiffStatus.Pass);
	}

	[Fact]
	public void DiffMoney_DeltaJustUnderTolerance_ReturnsPass()
	{
		// delta = 0.009 < tolerance (0.01) → pass
		FieldDiff diff = FixtureEvaluator.DiffMoney("total", 1.000m, FieldConfidence<decimal>.High(1.009m));

		diff.Status.Should().Be(DiffStatus.Pass);
	}

	[Fact]
	public void DiffMoney_DeltaExactlyOneCent_ReturnsFail_BugDocumentedRECEIPTS634()
	{
		// TODO RECEIPTS-634: This test pins the off-by-one bug — line 113 of FixtureEvaluator
		// uses `<` instead of `<=`, so a delta of EXACTLY $0.01 fails even though the README
		// says "within $0.01 of expected". When RECEIPTS-634 fixes the comparison to `<=`,
		// flip this assertion to `DiffStatus.Pass` and remove this comment.
		FieldDiff diff = FixtureEvaluator.DiffMoney("total", 1.00m, FieldConfidence<decimal>.High(1.01m));

		diff.Status.Should().Be(DiffStatus.Fail);
		diff.Detail.Should().Contain("delta=$0.01");
	}

	[Fact]
	public void DiffMoney_DeltaOverTolerance_ReturnsFail()
	{
		// delta = 0.011 > tolerance (0.01) → fail (this fails today AND after the fix)
		FieldDiff diff = FixtureEvaluator.DiffMoney("total", 1.000m, FieldConfidence<decimal>.High(1.011m));

		diff.Status.Should().Be(DiffStatus.Fail);
		diff.Expected.Should().Be("1.00");
		diff.Actual.Should().Be("1.01");
	}

	[Fact]
	public void DiffMoney_NegativeDelta_UsesAbsoluteValue()
	{
		// actual is below expected — Math.Abs ensures it's still a small delta → pass
		FieldDiff diff = FixtureEvaluator.DiffMoney("subtotal", 1.000m, FieldConfidence<decimal>.High(0.995m));

		diff.Status.Should().Be(DiffStatus.Pass);
	}

	[Fact]
	public void DiffMoney_LowConfidenceActual_ReturnsFail()
	{
		FieldDiff diff = FixtureEvaluator.DiffMoney("total", 70.43m, FieldConfidence<decimal>.None());

		diff.Status.Should().Be(DiffStatus.Fail);
		diff.Expected.Should().Be("70.43");
		diff.Actual.Should().BeNull();
		diff.Detail.Should().Be("VLM did not extract total");
	}

	[Fact]
	public void DiffMoney_ExpectedNull_ReturnsNotDeclared_WithFormattedActual()
	{
		FieldDiff diff = FixtureEvaluator.DiffMoney("total", null, FieldConfidence<decimal>.High(70.43m));

		diff.Status.Should().Be(DiffStatus.NotDeclared);
		diff.Expected.Should().BeNull();
		diff.Actual.Should().Be("70.43");
	}

	[Fact]
	public void DiffMoney_ExpectedNull_LowConfidenceActual_FormatsActualAsNull()
	{
		FieldDiff diff = FixtureEvaluator.DiffMoney("total", null, FieldConfidence<decimal>.None());

		diff.Status.Should().Be(DiffStatus.NotDeclared);
		diff.Actual.Should().BeNull();
	}

	#endregion

	#region DiffTaxLines

	[Fact]
	public void DiffTaxLines_SingleMatch_ReturnsPass()
	{
		List<ExpectedTaxLine> expected = [new ExpectedTaxLine { Amount = 0.75m }];
		List<ParsedTaxLine> actual =
		[
			new ParsedTaxLine(FieldConfidence<string>.High("Tax"), FieldConfidence<decimal>.High(0.75m))
		];

		FieldDiff diff = FixtureEvaluator.DiffTaxLines(expected, actual);

		diff.Field.Should().Be("taxLines");
		diff.Status.Should().Be(DiffStatus.Pass);
		diff.Detail.Should().BeNull();
	}

	[Fact]
	public void DiffTaxLines_MultipleAmounts_AllMatched_ReturnsPass()
	{
		List<ExpectedTaxLine> expected =
		[
			new ExpectedTaxLine { Amount = 0.75m },
			new ExpectedTaxLine { Amount = 1.20m },
		];
		List<ParsedTaxLine> actual =
		[
			new ParsedTaxLine(FieldConfidence<string>.High("State"), FieldConfidence<decimal>.High(0.75m)),
			new ParsedTaxLine(FieldConfidence<string>.High("County"), FieldConfidence<decimal>.High(1.20m)),
		];

		FieldDiff diff = FixtureEvaluator.DiffTaxLines(expected, actual);

		diff.Status.Should().Be(DiffStatus.Pass);
	}

	[Fact]
	public void DiffTaxLines_MissingActualLine_ReturnsFail()
	{
		List<ExpectedTaxLine> expected = [new ExpectedTaxLine { Amount = 0.75m }];
		List<ParsedTaxLine> actual = [];

		FieldDiff diff = FixtureEvaluator.DiffTaxLines(expected, actual);

		diff.Status.Should().Be(DiffStatus.Fail);
		diff.Detail.Should().Contain("no tax line within $0.01 of $0.75");
	}

	[Fact]
	public void DiffTaxLines_ActualHasMoreLines_StillPasses()
	{
		// README: "actual may contain more lines"
		List<ExpectedTaxLine> expected = [new ExpectedTaxLine { Amount = 0.75m }];
		List<ParsedTaxLine> actual =
		[
			new ParsedTaxLine(FieldConfidence<string>.High("Tax"), FieldConfidence<decimal>.High(0.75m)),
			new ParsedTaxLine(FieldConfidence<string>.High("Other"), FieldConfidence<decimal>.High(2.50m)),
		];

		FieldDiff diff = FixtureEvaluator.DiffTaxLines(expected, actual);

		diff.Status.Should().Be(DiffStatus.Pass);
	}

	[Fact]
	public void DiffTaxLines_DuplicateExpectedAmounts_ConsumesEachActualOnce()
	{
		// Expected has two 0.75 lines; actual has two 0.75 lines → both should match.
		List<ExpectedTaxLine> expected =
		[
			new ExpectedTaxLine { Amount = 0.75m },
			new ExpectedTaxLine { Amount = 0.75m },
		];
		List<ParsedTaxLine> actual =
		[
			new ParsedTaxLine(FieldConfidence<string>.High("Tax 1"), FieldConfidence<decimal>.High(0.75m)),
			new ParsedTaxLine(FieldConfidence<string>.High("Tax 2"), FieldConfidence<decimal>.High(0.75m)),
		];

		FieldDiff diff = FixtureEvaluator.DiffTaxLines(expected, actual);

		diff.Status.Should().Be(DiffStatus.Pass);
	}

	[Fact]
	public void DiffTaxLines_DuplicateExpectedAmounts_OnlyOneActual_FailsForSecond()
	{
		// Two expected 0.75 lines but only one actual 0.75 line → second consumes index -1 → fail.
		List<ExpectedTaxLine> expected =
		[
			new ExpectedTaxLine { Amount = 0.75m },
			new ExpectedTaxLine { Amount = 0.75m },
		];
		List<ParsedTaxLine> actual =
		[
			new ParsedTaxLine(FieldConfidence<string>.High("Tax"), FieldConfidence<decimal>.High(0.75m)),
		];

		FieldDiff diff = FixtureEvaluator.DiffTaxLines(expected, actual);

		diff.Status.Should().Be(DiffStatus.Fail);
		diff.Detail.Should().Contain("no tax line within $0.01 of $0.75");
	}

	[Fact]
	public void DiffTaxLines_LowConfidenceActualAmounts_AreIgnored()
	{
		// A low-confidence actual amount should not be available to match against.
		List<ExpectedTaxLine> expected = [new ExpectedTaxLine { Amount = 0.75m }];
		List<ParsedTaxLine> actual =
		[
			new ParsedTaxLine(FieldConfidence<string>.High("Tax"), FieldConfidence<decimal>.None()),
		];

		FieldDiff diff = FixtureEvaluator.DiffTaxLines(expected, actual);

		diff.Status.Should().Be(DiffStatus.Fail);
	}

	[Fact]
	public void DiffTaxLines_ExpectedNull_ReturnsNotDeclared()
	{
		List<ParsedTaxLine> actual =
		[
			new ParsedTaxLine(FieldConfidence<string>.High("Tax"), FieldConfidence<decimal>.High(0.75m)),
		];

		FieldDiff diff = FixtureEvaluator.DiffTaxLines(null, actual);

		diff.Status.Should().Be(DiffStatus.NotDeclared);
		diff.Actual.Should().Be("actual=1");
	}

	[Fact]
	public void DiffTaxLines_ExpectedEmpty_ReturnsNotDeclared()
	{
		FieldDiff diff = FixtureEvaluator.DiffTaxLines([], []);

		diff.Status.Should().Be(DiffStatus.NotDeclared);
	}

	[Fact]
	public void DiffTaxLines_ExpectedAmountIsNull_IsSkipped()
	{
		// Lines with a null amount don't constitute an assertion → pass.
		List<ExpectedTaxLine> expected = [new ExpectedTaxLine { Label = "Sales", Amount = null }];
		List<ParsedTaxLine> actual = [];

		FieldDiff diff = FixtureEvaluator.DiffTaxLines(expected, actual);

		diff.Status.Should().Be(DiffStatus.Pass);
	}

	#endregion

	#region DiffPaymentMethod

	[Fact]
	public void DiffPaymentMethod_SubstringMatch_ReturnsPass()
	{
		// README: "MASTERCARD" matches "MasterCard ****1234"
		FieldDiff diff = FixtureEvaluator.DiffPaymentMethod("MASTERCARD", FieldConfidence<string?>.High("MasterCard ****1234"));

		diff.Field.Should().Be("paymentMethod");
		diff.Status.Should().Be(DiffStatus.Pass);
	}

	[Fact]
	public void DiffPaymentMethod_NoMatch_ReturnsFail()
	{
		FieldDiff diff = FixtureEvaluator.DiffPaymentMethod("VISA", FieldConfidence<string?>.High("Cash"));

		diff.Status.Should().Be(DiffStatus.Fail);
		diff.Detail.Should().Contain("does not contain expected substring");
	}

	[Fact]
	public void DiffPaymentMethod_MissingActual_ReturnsFail()
	{
		FieldDiff diff = FixtureEvaluator.DiffPaymentMethod("VISA", FieldConfidence<string?>.None());

		diff.Status.Should().Be(DiffStatus.Fail);
		diff.Detail.Should().Be("VLM did not extract paymentMethod");
	}

	[Fact]
	public void DiffPaymentMethod_ExpectedNull_ReturnsNotDeclared()
	{
		FieldDiff diff = FixtureEvaluator.DiffPaymentMethod(null, FieldConfidence<string?>.High("VISA"));

		diff.Status.Should().Be(DiffStatus.NotDeclared);
	}

	#endregion

	#region DiffMinItemCount

	[Fact]
	public void DiffMinItemCount_ActualMeetsThreshold_ReturnsPass()
	{
		List<ParsedReceiptItem> items =
		[
			MakeItem("Apple", 1.00m),
			MakeItem("Bread", 2.00m),
			MakeItem("Cheese", 3.00m),
		];

		FieldDiff diff = FixtureEvaluator.DiffMinItemCount(3, items);

		diff.Status.Should().Be(DiffStatus.Pass);
		diff.Expected.Should().Be(">=3");
		diff.Actual.Should().Be("3");
	}

	[Fact]
	public void DiffMinItemCount_ActualExceedsThreshold_ReturnsPass()
	{
		List<ParsedReceiptItem> items =
		[
			MakeItem("Apple", 1.00m),
			MakeItem("Bread", 2.00m),
		];

		FieldDiff diff = FixtureEvaluator.DiffMinItemCount(1, items);

		diff.Status.Should().Be(DiffStatus.Pass);
	}

	[Fact]
	public void DiffMinItemCount_ActualBelowThreshold_ReturnsFail()
	{
		List<ParsedReceiptItem> items = [MakeItem("Apple", 1.00m)];

		FieldDiff diff = FixtureEvaluator.DiffMinItemCount(3, items);

		diff.Status.Should().Be(DiffStatus.Fail);
		diff.Detail.Should().Contain("expected at least 3 items, got 1");
	}

	[Fact]
	public void DiffMinItemCount_ExpectedNull_ReturnsNotDeclared()
	{
		List<ParsedReceiptItem> items = [MakeItem("Apple", 1.00m)];

		FieldDiff diff = FixtureEvaluator.DiffMinItemCount(null, items);

		diff.Status.Should().Be(DiffStatus.NotDeclared);
		diff.Actual.Should().Be("1");
	}

	#endregion

	#region DiffItems

	[Fact]
	public void DiffItems_ExpectedNull_ReturnsEmpty()
	{
		List<FieldDiff> diffs = FixtureEvaluator.DiffItems(null, [MakeItem("Apple", 1.00m)]);

		diffs.Should().BeEmpty();
	}

	[Fact]
	public void DiffItems_ExpectedEmpty_ReturnsEmpty()
	{
		List<FieldDiff> diffs = FixtureEvaluator.DiffItems([], [MakeItem("Apple", 1.00m)]);

		diffs.Should().BeEmpty();
	}

	[Fact]
	public void DiffItems_PriceAndDescriptionBothNull_ReturnsNotDeclared()
	{
		List<ExpectedItem> expected = [new ExpectedItem()];
		List<ParsedReceiptItem> actual = [MakeItem("Apple", 1.00m)];

		List<FieldDiff> diffs = FixtureEvaluator.DiffItems(expected, actual);

		diffs.Should().HaveCount(1);
		diffs[0].Status.Should().Be(DiffStatus.NotDeclared);
		diffs[0].Field.Should().Be("items[0]");
	}

	[Fact]
	public void DiffItems_PriceMatch_ReturnsPass()
	{
		List<ExpectedItem> expected = [new ExpectedItem { Description = "Apple", TotalPrice = 1.00m }];
		List<ParsedReceiptItem> actual = [MakeItem("Apple", 1.00m)];

		List<FieldDiff> diffs = FixtureEvaluator.DiffItems(expected, actual);

		diffs.Should().HaveCount(1);
		diffs[0].Status.Should().Be(DiffStatus.Pass);
	}

	[Fact]
	public void DiffItems_PriceMatchPicksClosestPrice()
	{
		// Two candidate prices: 0.99 and 1.05. With expected=1.00, neither is within $0.01
		// (1.05 delta = 0.05; 0.99 delta = 0.01 — the boundary value, currently rejected).
		// Bring 0.99 within tolerance (delta=0.005) and confirm it wins over 1.05.
		List<ExpectedItem> expected = [new ExpectedItem { Description = "Apple", TotalPrice = 1.000m }];
		List<ParsedReceiptItem> actual =
		[
			MakeItem("Bread", 1.05m),
			MakeItem("Apple", 0.995m),
		];

		List<FieldDiff> diffs = FixtureEvaluator.DiffItems(expected, actual);

		diffs[0].Status.Should().Be(DiffStatus.Pass);
		diffs[0].Actual.Should().Contain("Apple");
	}

	[Fact]
	public void DiffItems_DescriptionOnlyFallback_ReturnsPass()
	{
		// No price declared → falls back to description-only substring match.
		List<ExpectedItem> expected = [new ExpectedItem { Description = "milk" }];
		List<ParsedReceiptItem> actual =
		[
			MakeItem("Whole Milk Gallon", 4.99m),
		];

		List<FieldDiff> diffs = FixtureEvaluator.DiffItems(expected, actual);

		diffs[0].Status.Should().Be(DiffStatus.Pass);
	}

	[Fact]
	public void DiffItems_PriceMismatch_FallsBackToDescription_ReturnsPass()
	{
		// Price doesn't match any item, but description does → falls back and matches.
		List<ExpectedItem> expected = [new ExpectedItem { Description = "milk", TotalPrice = 99.99m }];
		List<ParsedReceiptItem> actual =
		[
			MakeItem("Whole Milk Gallon", 4.99m),
		];

		List<FieldDiff> diffs = FixtureEvaluator.DiffItems(expected, actual);

		// Match is found via description fallback, but the price diff still fails the line.
		diffs[0].Status.Should().Be(DiffStatus.Fail);
		diffs[0].Detail.Should().Contain("totalPrice mismatch");
	}

	[Fact]
	public void DiffItems_NoMatch_ReturnsFail()
	{
		List<ExpectedItem> expected = [new ExpectedItem { Description = "Apple", TotalPrice = 1.00m }];
		List<ParsedReceiptItem> actual = [MakeItem("Bread", 5.00m)];

		List<FieldDiff> diffs = FixtureEvaluator.DiffItems(expected, actual);

		diffs[0].Status.Should().Be(DiffStatus.Fail);
		diffs[0].Detail.Should().Be("no matching line in VLM output");
	}

	[Fact]
	public void DiffItems_DescriptionMismatch_ReturnsFail()
	{
		// Price matches but description doesn't → reports description mismatch.
		List<ExpectedItem> expected = [new ExpectedItem { Description = "Apple", TotalPrice = 1.000m }];
		List<ParsedReceiptItem> actual = [MakeItem("Bread", 1.000m)];

		List<FieldDiff> diffs = FixtureEvaluator.DiffItems(expected, actual);

		diffs[0].Status.Should().Be(DiffStatus.Fail);
		diffs[0].Detail.Should().Contain("description mismatch");
	}

	[Fact]
	public void DiffItems_LowConfidenceTotalPrice_NotMatchedByPrice()
	{
		// Items with low-confidence prices are skipped by the price matcher.
		// The fallback description matcher then finds them by description.
		List<ExpectedItem> expected = [new ExpectedItem { Description = "Apple", TotalPrice = 1.000m }];
		List<ParsedReceiptItem> actual =
		[
			new ParsedReceiptItem(
				Code: FieldConfidence<string?>.None(),
				Description: FieldConfidence<string>.High("Apple"),
				Quantity: FieldConfidence<decimal>.High(1m),
				UnitPrice: FieldConfidence<decimal>.None(),
				TotalPrice: FieldConfidence<decimal>.None()),
		];

		List<FieldDiff> diffs = FixtureEvaluator.DiffItems(expected, actual);

		// Price matcher skips the item, description fallback finds it, but then the
		// "is the matched item's totalPrice low confidence?" check reports a missing price.
		diffs[0].Status.Should().Be(DiffStatus.Fail);
		diffs[0].Detail.Should().Contain("missing totalPrice");
	}

	[Fact]
	public void DiffItems_PoolItemConsumedOnFirstMatch_ReturnsExpectedDiffs()
	{
		// Two expected lines both expecting Apple/1.00. Actual has two Apples at 1.00.
		// First should consume actual[0]; second should consume actual[1].
		List<ExpectedItem> expected =
		[
			new ExpectedItem { Description = "Apple", TotalPrice = 1.000m },
			new ExpectedItem { Description = "Apple", TotalPrice = 1.000m },
		];
		List<ParsedReceiptItem> actual =
		[
			MakeItem("Apple A", 1.000m),
			MakeItem("Apple B", 1.000m),
		];

		List<FieldDiff> diffs = FixtureEvaluator.DiffItems(expected, actual);

		diffs.Should().HaveCount(2);
		diffs[0].Status.Should().Be(DiffStatus.Pass);
		diffs[1].Status.Should().Be(DiffStatus.Pass);
	}

	[Fact]
	public void DiffItems_PoolExhausted_SecondExpectedFails()
	{
		// Two expected, only one actual.
		List<ExpectedItem> expected =
		[
			new ExpectedItem { Description = "Apple", TotalPrice = 1.000m },
			new ExpectedItem { Description = "Apple", TotalPrice = 1.000m },
		];
		List<ParsedReceiptItem> actual = [MakeItem("Apple", 1.000m)];

		List<FieldDiff> diffs = FixtureEvaluator.DiffItems(expected, actual);

		diffs.Should().HaveCount(2);
		diffs[0].Status.Should().Be(DiffStatus.Pass);
		diffs[1].Status.Should().Be(DiffStatus.Fail);
		diffs[1].Detail.Should().Be("no matching line in VLM output");
	}

	#endregion

	#region EvaluateAsync end-to-end

	[Fact]
	public async Task EvaluateAsync_FileMissing_ReturnsFailWithReadError()
	{
		Mock<IReceiptExtractionService> service = new();
		FixtureEvaluator evaluator = new(service.Object, NullLogger<FixtureEvaluator>.Instance);

		Fixture fixture = new(
			Name: "missing.jpg",
			FilePath: Path.Combine(Path.GetTempPath(), Guid.NewGuid() + "-does-not-exist.jpg"),
			ContentType: "image/jpeg",
			Expected: new ExpectedReceipt());

		FixtureResult result = await evaluator.EvaluateAsync(fixture, CancellationToken.None);

		result.Passed.Should().BeFalse();
		result.Error.Should().StartWith("Failed to read fixture file");
		service.VerifyNoOtherCalls();
	}

	[Fact]
	public async Task EvaluateAsync_ExtractionThrows_ReturnsFailWithVlmError()
	{
		string tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".jpg");
		await File.WriteAllBytesAsync(tempFile, [0x01, 0x02, 0x03]);
		try
		{
			Mock<IReceiptExtractionService> service = new();
			service
				.Setup(s => s.ExtractAsync(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
				.ThrowsAsync(new InvalidOperationException("VLM down"));

			FixtureEvaluator evaluator = new(service.Object, NullLogger<FixtureEvaluator>.Instance);

			Fixture fixture = new("test.jpg", tempFile, "image/jpeg", new ExpectedReceipt());

			FixtureResult result = await evaluator.EvaluateAsync(fixture, CancellationToken.None);

			result.Passed.Should().BeFalse();
			result.Error.Should().Contain("VLM call failed");
			result.Error.Should().Contain("InvalidOperationException");
		}
		finally
		{
			File.Delete(tempFile);
		}
	}

	[Fact]
	public async Task EvaluateAsync_AllAssertionsPass_ReturnsPassed()
	{
		string tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".jpg");
		await File.WriteAllBytesAsync(tempFile, [0x01, 0x02, 0x03]);
		try
		{
			ParsedReceipt parsed = new(
				StoreName: FieldConfidence<string>.High("Walmart Supercenter"),
				Date: FieldConfidence<DateOnly>.High(new DateOnly(2026, 1, 14)),
				Items: [MakeItem("Apple", 1.00m)],
				Subtotal: FieldConfidence<decimal>.High(1.00m),
				TaxLines: [new ParsedTaxLine(FieldConfidence<string>.High("State"), FieldConfidence<decimal>.High(0.08m))],
				Total: FieldConfidence<decimal>.High(1.08m),
				PaymentMethod: FieldConfidence<string?>.High("VISA ****1111"));

			Mock<IReceiptExtractionService> service = new();
			service
				.Setup(s => s.ExtractAsync(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(parsed);

			FixtureEvaluator evaluator = new(service.Object, NullLogger<FixtureEvaluator>.Instance);

			Fixture fixture = new(
				Name: "test.jpg",
				FilePath: tempFile,
				ContentType: "image/jpeg",
				Expected: new ExpectedReceipt
				{
					Store = "Walmart",
					Date = new DateOnly(2026, 1, 14),
					Subtotal = 1.00m,
					Total = 1.08m,
					TaxLines = [new ExpectedTaxLine { Amount = 0.08m }],
					PaymentMethod = "VISA",
					MinItemCount = 1,
					Items = [new ExpectedItem { Description = "Apple", TotalPrice = 1.00m }],
				});

			FixtureResult result = await evaluator.EvaluateAsync(fixture, CancellationToken.None);

			result.Passed.Should().BeTrue();
			result.Error.Should().BeNull();
			result.FieldDiffs.Should().NotBeEmpty();
			result.FieldDiffs.Should().NotContain(d => d.Status == DiffStatus.Fail);
		}
		finally
		{
			File.Delete(tempFile);
		}
	}

	[Fact]
	public async Task EvaluateAsync_OnlyUndeclaredFields_ReturnsPassed_AllNotDeclared()
	{
		// An empty ExpectedReceipt (nothing declared) should pass — every diff is NotDeclared.
		string tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".jpg");
		await File.WriteAllBytesAsync(tempFile, [0x01, 0x02, 0x03]);
		try
		{
			ParsedReceipt parsed = new(
				StoreName: FieldConfidence<string>.None(),
				Date: FieldConfidence<DateOnly>.None(),
				Items: [],
				Subtotal: FieldConfidence<decimal>.None(),
				TaxLines: [],
				Total: FieldConfidence<decimal>.None(),
				PaymentMethod: FieldConfidence<string?>.None());

			Mock<IReceiptExtractionService> service = new();
			service
				.Setup(s => s.ExtractAsync(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(parsed);

			FixtureEvaluator evaluator = new(service.Object, NullLogger<FixtureEvaluator>.Instance);

			Fixture fixture = new("test.jpg", tempFile, "image/jpeg", new ExpectedReceipt());

			FixtureResult result = await evaluator.EvaluateAsync(fixture, CancellationToken.None);

			result.Passed.Should().BeTrue();
			result.FieldDiffs.Should().OnlyContain(d => d.Status == DiffStatus.NotDeclared);
		}
		finally
		{
			File.Delete(tempFile);
		}
	}

	[Fact]
	public async Task EvaluateAsync_OneAssertionFails_OverallResultFails()
	{
		string tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".jpg");
		await File.WriteAllBytesAsync(tempFile, [0x01, 0x02, 0x03]);
		try
		{
			ParsedReceipt parsed = new(
				StoreName: FieldConfidence<string>.High("Target"), // expected Walmart → fail
				Date: FieldConfidence<DateOnly>.None(),
				Items: [],
				Subtotal: FieldConfidence<decimal>.None(),
				TaxLines: [],
				Total: FieldConfidence<decimal>.None(),
				PaymentMethod: FieldConfidence<string?>.None());

			Mock<IReceiptExtractionService> service = new();
			service
				.Setup(s => s.ExtractAsync(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(parsed);

			FixtureEvaluator evaluator = new(service.Object, NullLogger<FixtureEvaluator>.Instance);

			Fixture fixture = new(
				"test.jpg",
				tempFile,
				"image/jpeg",
				new ExpectedReceipt { Store = "Walmart" });

			FixtureResult result = await evaluator.EvaluateAsync(fixture, CancellationToken.None);

			result.Passed.Should().BeFalse();
			result.FieldDiffs.Should().Contain(d => d.Field == "store" && d.Status == DiffStatus.Fail);
		}
		finally
		{
			File.Delete(tempFile);
		}
	}

	#endregion

	private static ParsedReceiptItem MakeItem(string description, decimal totalPrice) =>
		new(
			Code: FieldConfidence<string?>.None(),
			Description: FieldConfidence<string>.High(description),
			Quantity: FieldConfidence<decimal>.High(1m),
			UnitPrice: FieldConfidence<decimal>.High(totalPrice),
			TotalPrice: FieldConfidence<decimal>.High(totalPrice));
}
