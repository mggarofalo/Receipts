using Common;
using Domain;
using Domain.Aggregates;
using Domain.Core;

namespace Domain.Tests.Aggregates;

public class ReceiptWithItemsWarningTests
{
	private static ReceiptWithItems CreateReceiptWithItems(
		decimal taxAmount = 1.00m,
		decimal itemUnitPrice = 10.00m,
		List<Adjustment>? adjustments = null)
	{
		Receipt receipt = new(
			Guid.NewGuid(),
			"Test Store",
			DateOnly.FromDateTime(DateTime.Now),
			new Money(taxAmount));

		ReceiptItem item = new(
			Guid.NewGuid(),
			"ITEM001",
			"Test Item",
			1,
			new Money(itemUnitPrice),
			new Money(itemUnitPrice),
			"Category",
			"Subcategory");

		return new ReceiptWithItems
		{
			Receipt = receipt,
			Items = [item],
			Adjustments = adjustments ?? []
		};
	}

	[Fact]
	public void Subtotal_ComputedFromItems()
	{
		// Arrange
		ReceiptWithItems rwi = CreateReceiptWithItems(itemUnitPrice: 10.00m);

		// Assert
		Assert.Equal(10.00m, rwi.Subtotal.Amount);
	}

	[Fact]
	public void AdjustmentTotal_ComputedFromAdjustments()
	{
		// Arrange
		Adjustment tip = new(Guid.NewGuid(), AdjustmentType.Tip, new Money(2.00m));
		Adjustment discount = new(Guid.NewGuid(), AdjustmentType.Discount, new Money(-1.00m));
		ReceiptWithItems rwi = CreateReceiptWithItems(adjustments: [tip, discount]);

		// Assert
		Assert.Equal(1.00m, rwi.AdjustmentTotal.Amount);
	}

	[Fact]
	public void ExpectedTotal_IsSubtotalPlusTaxPlusAdjustments()
	{
		// Arrange: item=$10, tax=$1, tip=$2 → expected=$13
		Adjustment tip = new(Guid.NewGuid(), AdjustmentType.Tip, new Money(2.00m));
		ReceiptWithItems rwi = CreateReceiptWithItems(
			taxAmount: 1.00m,
			itemUnitPrice: 10.00m,
			adjustments: [tip]);

		// Assert
		Assert.Equal(13.00m, rwi.ExpectedTotal.Amount);
	}

	[Fact]
	public void GetWarnings_NormalTaxRate_NoWarnings()
	{
		// Arrange: 10% tax rate is within 0-25%
		ReceiptWithItems rwi = CreateReceiptWithItems(taxAmount: 1.00m, itemUnitPrice: 10.00m);

		// Act
		List<ValidationWarning> warnings = rwi.GetWarnings();

		// Assert
		Assert.Empty(warnings);
	}

	[Fact]
	public void GetWarnings_HighTaxRate_ReturnsWarning()
	{
		// Arrange: 30% tax rate exceeds 25%
		ReceiptWithItems rwi = CreateReceiptWithItems(taxAmount: 3.00m, itemUnitPrice: 10.00m);

		// Act
		List<ValidationWarning> warnings = rwi.GetWarnings();

		// Assert
		Assert.Single(warnings);
		Assert.Equal(nameof(Receipt.TaxAmount), warnings[0].Property);
		Assert.Equal(ValidationWarningSeverity.Warning, warnings[0].Severity);
	}

	[Fact]
	public void GetWarnings_ZeroTaxRate_NoWarning()
	{
		// Arrange: 0% tax is within the range
		ReceiptWithItems rwi = CreateReceiptWithItems(taxAmount: 0m, itemUnitPrice: 10.00m);

		// Act
		List<ValidationWarning> warnings = rwi.GetWarnings();

		// Assert
		Assert.Empty(warnings);
	}

	[Fact]
	public void GetWarnings_ExactlyTwentyFivePercentTax_NoWarning()
	{
		// Arrange: 25% tax is boundary (not exceeding)
		ReceiptWithItems rwi = CreateReceiptWithItems(taxAmount: 2.50m, itemUnitPrice: 10.00m);

		// Act
		List<ValidationWarning> warnings = rwi.GetWarnings();

		// Assert
		Assert.Empty(warnings);
	}

	[Fact]
	public void GetWarnings_NegativeTaxRate_ReturnsWarning()
	{
		// Arrange: negative tax is outside 0-25%
		ReceiptWithItems rwi = CreateReceiptWithItems(taxAmount: -1.00m, itemUnitPrice: 10.00m);

		// Act
		List<ValidationWarning> warnings = rwi.GetWarnings();

		// Assert
		Assert.Single(warnings);
		Assert.Equal(nameof(Receipt.TaxAmount), warnings[0].Property);
	}

	[Fact]
	public void GetWarnings_HighAdjustmentRate_ReturnsWarning()
	{
		// Arrange: adjustment is >10% of subtotal
		Adjustment bigTip = new(Guid.NewGuid(), AdjustmentType.Tip, new Money(2.00m));
		ReceiptWithItems rwi = CreateReceiptWithItems(
			taxAmount: 1.00m,
			itemUnitPrice: 10.00m,
			adjustments: [bigTip]);

		// Act
		List<ValidationWarning> warnings = rwi.GetWarnings();

		// Assert: 2/10 = 20% > 10%
		Assert.Contains(warnings, w => w.Property == nameof(ReceiptWithItems.AdjustmentTotal));
	}

	[Fact]
	public void GetWarnings_SmallAdjustmentRate_NoAdjustmentWarning()
	{
		// Arrange: adjustment is 5% of subtotal
		Adjustment smallTip = new(Guid.NewGuid(), AdjustmentType.Tip, new Money(0.50m));
		ReceiptWithItems rwi = CreateReceiptWithItems(
			taxAmount: 1.00m,
			itemUnitPrice: 10.00m,
			adjustments: [smallTip]);

		// Act
		List<ValidationWarning> warnings = rwi.GetWarnings();

		// Assert: 0.5/10 = 5% < 10%, no adjustment warning
		Assert.DoesNotContain(warnings, w => w.Property == nameof(ReceiptWithItems.AdjustmentTotal));
	}

	[Fact]
	public void GetWarnings_ZeroSubtotal_NoWarnings()
	{
		// Arrange: zero subtotal skips all rate checks
		Receipt receipt = new(
			Guid.NewGuid(),
			"Test Store",
			DateOnly.FromDateTime(DateTime.Now),
			new Money(5.00m));

		ReceiptWithItems rwi = new()
		{
			Receipt = receipt,
			Items = [],
			Adjustments = []
		};

		// Act
		List<ValidationWarning> warnings = rwi.GetWarnings();

		// Assert
		Assert.Empty(warnings);
	}

	[Fact]
	public void GetWarnings_NoAdjustments_NoAdjustmentWarning()
	{
		// Arrange
		ReceiptWithItems rwi = CreateReceiptWithItems(taxAmount: 1.00m, itemUnitPrice: 10.00m);

		// Act
		List<ValidationWarning> warnings = rwi.GetWarnings();

		// Assert
		Assert.DoesNotContain(warnings, w => w.Property == nameof(ReceiptWithItems.AdjustmentTotal));
	}
}
