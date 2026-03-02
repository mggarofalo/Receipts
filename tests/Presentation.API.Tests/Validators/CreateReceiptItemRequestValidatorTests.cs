using API.Generated.Dtos;
using API.Validators;

namespace Presentation.API.Tests.Validators;

public class CreateReceiptItemRequestValidatorTests
{
	private readonly CreateReceiptItemRequestValidator _validator = new();

	[Fact]
	public void Should_Pass_When_UnitPriceIsPositive()
	{
		// Arrange
		CreateReceiptItemRequest request = new() { UnitPrice = 9.99 };

		// Act
		FluentValidation.Results.ValidationResult result = _validator.Validate(request);

		// Assert
		Assert.True(result.IsValid);
	}

	[Fact]
	public void Should_Fail_When_UnitPriceIsZero()
	{
		// Arrange
		CreateReceiptItemRequest request = new() { UnitPrice = 0 };

		// Act
		FluentValidation.Results.ValidationResult result = _validator.Validate(request);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.ErrorMessage == CreateReceiptItemRequestValidator.UnitPriceMustBePositive);
	}

	[Fact]
	public void Should_Fail_When_UnitPriceIsNegative()
	{
		// Arrange
		CreateReceiptItemRequest request = new() { UnitPrice = -5.00 };

		// Act
		FluentValidation.Results.ValidationResult result = _validator.Validate(request);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.ErrorMessage == CreateReceiptItemRequestValidator.UnitPriceMustBePositive);
	}
}
