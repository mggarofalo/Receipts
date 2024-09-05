using Shared.Validators;
using Shared.ViewModels.Core;

namespace Presentation.Shared.Tests.Validators;

public class ReceiptItemValidatorTests
{
	private readonly ReceiptItemValidator _validator = new();

	[Fact]
	public void Should_Pass_When_ValidReceiptItem()
	{
		// Arrange
		ReceiptItemVM receiptItem = new()
		{
			ReceiptItemCode = "ITEM123",
			Description = "Test Item",
			Quantity = 1,
			UnitPrice = 100,
			TotalAmount = 100,
			Category = "Test Category",
			Subcategory = "Test Subcategory"
		};

		// Act
		FluentValidation.Results.ValidationResult result = _validator.Validate(receiptItem);

		// Assert
		Assert.True(result.IsValid);
	}

	[Fact]
	public void Should_Fail_When_ReceiptItemCodeIsEmpty()
	{
		// Arrange
		ReceiptItemVM receiptItem = new()
		{
			ReceiptItemCode = string.Empty,
			Description = "Test Item",
			Quantity = 1,
			UnitPrice = 100,
			TotalAmount = 100,
			Category = "Test Category",
			Subcategory = "Test Subcategory"
		};

		// Act
		FluentValidation.Results.ValidationResult result = _validator.Validate(receiptItem);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.ErrorMessage == ReceiptItemValidator.ReceiptItemCodeIsRequired);
	}

	[Fact]
	public void Should_Fail_When_DescriptionIsEmpty()
	{
		// Arrange
		ReceiptItemVM receiptItem = new()
		{
			ReceiptItemCode = "ITEM123",
			Description = string.Empty,
			Quantity = 1,
			UnitPrice = 100,
			TotalAmount = 100,
			Category = "Test Category",
			Subcategory = "Test Subcategory"
		};

		// Act
		FluentValidation.Results.ValidationResult result = _validator.Validate(receiptItem);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.ErrorMessage == ReceiptItemValidator.DescriptionIsRequired);
	}

	[Fact]
	public void Should_Fail_When_CategoryIsEmpty()
	{
		// Arrange
		ReceiptItemVM receiptItem = new()
		{
			ReceiptItemCode = "ITEM123",
			Description = "Test Item",
			Quantity = 1,
			UnitPrice = 100,
			TotalAmount = 100,
			Category = string.Empty,
			Subcategory = "Test Subcategory"
		};

		// Act
		FluentValidation.Results.ValidationResult result = _validator.Validate(receiptItem);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.ErrorMessage == ReceiptItemValidator.CategoryIsRequired);
	}

	[Fact]
	public void Should_Fail_When_SubcategoryIsEmpty()
	{
		// Arrange
		ReceiptItemVM receiptItem = new()
		{
			ReceiptItemCode = "ITEM123",
			Description = "Test Item",
			Quantity = 1,
			UnitPrice = 100,
			TotalAmount = 100,
			Category = "Test Category",
			Subcategory = string.Empty
		};

		// Act
		FluentValidation.Results.ValidationResult result = _validator.Validate(receiptItem);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.ErrorMessage == ReceiptItemValidator.SubcategoryIsRequired);
	}

	[Theory]
	[InlineData(1, 100, 100)]
	[InlineData(2, 50, 100)]
	[InlineData(3, 33.33, 99.99)]
	[InlineData(3.423, 1.99, 6.81)]
	[InlineData(3.422, 1.99, 6.81)]
	public void Should_Pass_When_TotalAmountIsEqualQuantityUnitPrice(decimal quantity, decimal unitPrice, decimal totalAmount)
	{
		// Arrange
		ReceiptItemVM receiptItem = new()
		{
			ReceiptItemCode = "ITEM123",
			Description = "Test Item",
			Quantity = quantity,
			UnitPrice = unitPrice,
			TotalAmount = totalAmount,
			Category = "Test Category",
			Subcategory = "Test Subcategory"
		};

		// Act
		FluentValidation.Results.ValidationResult result = _validator.Validate(receiptItem);

		// Assert
		Assert.True(result.IsValid);
	}

	[Theory]
	[InlineData(1, 100, 105)]
	[InlineData(2, 50, 105)]
	[InlineData(3, 33.33, 100)]
	public void Should_Fail_When_TotalAmountIsNotEqualQuantityUnitPrice(decimal quantity, decimal unitPrice, decimal totalAmount)
	{
		// Arrange
		ReceiptItemVM receiptItem = new()
		{
			ReceiptItemCode = "ITEM123",
			Description = "Test Item",
			Quantity = quantity,
			UnitPrice = unitPrice,
			TotalAmount = totalAmount,
			Category = "Test Category",
			Subcategory = "Test Subcategory"
		};

		// Act
		FluentValidation.Results.ValidationResult result = _validator.Validate(receiptItem);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.ErrorMessage == ReceiptItemValidator.TotalAmountErrorMessage);
	}
}