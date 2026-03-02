using Common;
using Domain.Core;

namespace Domain.Tests.Core;

public class AdjustmentTests
{
	[Fact]
	public void Constructor_ValidInput_CreatesAdjustment()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		AdjustmentType type = AdjustmentType.Tip;
		Money amount = new(5.00m);

		// Act
		Adjustment adjustment = new(id, type, amount);

		// Assert
		Assert.Equal(id, adjustment.Id);
		Assert.Equal(type, adjustment.Type);
		Assert.Equal(amount, adjustment.Amount);
		Assert.Null(adjustment.Description);
	}

	[Fact]
	public void Constructor_EmptyId_CreatesAdjustmentWithEmptyId()
	{
		// Arrange
		Money amount = new(5.00m);

		// Act
		Adjustment adjustment = new(Guid.Empty, AdjustmentType.Discount, amount);

		// Assert
		Assert.Equal(Guid.Empty, adjustment.Id);
	}

	[Fact]
	public void Constructor_WithDescription_SetsDescription()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		Money amount = new(3.00m);

		// Act
		Adjustment adjustment = new(id, AdjustmentType.Other, amount, "Custom discount");

		// Assert
		Assert.Equal("Custom discount", adjustment.Description);
	}

	[Fact]
	public void Constructor_ZeroAmount_ThrowsArgumentException()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		Money amount = new(0m);

		// Act & Assert
		ArgumentException exception = Assert.Throws<ArgumentException>(
			() => new Adjustment(id, AdjustmentType.Tip, amount));
		Assert.StartsWith(Adjustment.AmountMustBeNonZero, exception.Message);
	}

	[Fact]
	public void Constructor_NegativeAmount_Succeeds()
	{
		// Arrange (discounts are negative)
		Guid id = Guid.NewGuid();
		Money amount = new(-5.00m);

		// Act
		Adjustment adjustment = new(id, AdjustmentType.Discount, amount);

		// Assert
		Assert.Equal(-5.00m, adjustment.Amount.Amount);
	}

	[Fact]
	public void Constructor_OtherType_NullDescription_ThrowsArgumentException()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		Money amount = new(2.00m);

		// Act & Assert
		ArgumentException exception = Assert.Throws<ArgumentException>(
			() => new Adjustment(id, AdjustmentType.Other, amount, null));
		Assert.StartsWith(Adjustment.DescriptionRequiredForOtherType, exception.Message);
	}

	[Fact]
	public void Constructor_OtherType_EmptyDescription_ThrowsArgumentException()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		Money amount = new(2.00m);

		// Act & Assert
		ArgumentException exception = Assert.Throws<ArgumentException>(
			() => new Adjustment(id, AdjustmentType.Other, amount, ""));
		Assert.StartsWith(Adjustment.DescriptionRequiredForOtherType, exception.Message);
	}

	[Fact]
	public void Constructor_OtherType_WhitespaceDescription_ThrowsArgumentException()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		Money amount = new(2.00m);

		// Act & Assert
		ArgumentException exception = Assert.Throws<ArgumentException>(
			() => new Adjustment(id, AdjustmentType.Other, amount, "   "));
		Assert.StartsWith(Adjustment.DescriptionRequiredForOtherType, exception.Message);
	}

	[Fact]
	public void Constructor_OtherType_ValidDescription_Succeeds()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		Money amount = new(1.50m);

		// Act
		Adjustment adjustment = new(id, AdjustmentType.Other, amount, "Employee discount");

		// Assert
		Assert.Equal(AdjustmentType.Other, adjustment.Type);
		Assert.Equal("Employee discount", adjustment.Description);
	}

	[Theory]
	[InlineData(AdjustmentType.Tip)]
	[InlineData(AdjustmentType.Discount)]
	[InlineData(AdjustmentType.Rounding)]
	[InlineData(AdjustmentType.LoyaltyRedemption)]
	[InlineData(AdjustmentType.Coupon)]
	[InlineData(AdjustmentType.StoreCredit)]
	public void Constructor_NonOtherType_NullDescription_Succeeds(AdjustmentType type)
	{
		// Arrange
		Guid id = Guid.NewGuid();
		Money amount = new(5.00m);

		// Act
		Adjustment adjustment = new(id, type, amount);

		// Assert
		Assert.Equal(type, adjustment.Type);
		Assert.Null(adjustment.Description);
	}
}
