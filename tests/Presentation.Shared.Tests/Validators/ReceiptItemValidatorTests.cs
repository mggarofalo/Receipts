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
			Category = "Test Category",
			Subcategory = string.Empty
		};

		// Act
		FluentValidation.Results.ValidationResult result = _validator.Validate(receiptItem);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.ErrorMessage == ReceiptItemValidator.SubcategoryIsRequired);
	}

	[Fact]
	public void Should_Pass_When_DateIsInThePast()
	{
		// Arrange
		DateOnly pastDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-1));
		ReceiptVM receipt = new()
		{
			Description = "Test Receipt",
			Location = "Test Location",
			Date = pastDate
		};

		// Act
		FluentValidation.Results.ValidationResult result = new ReceiptValidator().Validate(receipt);

		// Assert
		Assert.True(result.IsValid);
	}

	[Fact]
	public void Should_Pass_When_DateIsToday()
	{
		// Arrange
		DateOnly today = DateOnly.FromDateTime(DateTime.Today);
		ReceiptVM receipt = new()
		{
			Description = "Test Receipt",
			Location = "Test Location",
			Date = today
		};

		// Act
		FluentValidation.Results.ValidationResult result = new ReceiptValidator().Validate(receipt);

		// Assert
		Assert.True(result.IsValid);
	}

	[Fact]
	public void Should_Fail_When_DateIsInTheFuture()
	{
		// Arrange
		DateOnly futureDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1));
		ReceiptVM receipt = new()
		{
			Description = "Test Receipt",
			Location = "Test Location",
			Date = futureDate
		};

		// Act
		FluentValidation.Results.ValidationResult result = new ReceiptValidator().Validate(receipt);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.ErrorMessage == ReceiptValidator.DateMustBePriorToCurrentDate);
	}

	[Fact]
	public void Should_Fail_When_DateIsNull()
	{
		// Arrange
		ReceiptVM receipt = new()
		{
			Description = "Test Receipt",
			Location = "Test Location",
			Date = null
		};

		// Act
		FluentValidation.Results.ValidationResult result = new ReceiptValidator().Validate(receipt);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.ErrorMessage == ReceiptValidator.DateMustBePriorToCurrentDate);
	}
}
