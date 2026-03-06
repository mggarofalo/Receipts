using API.Generated.Dtos;
using API.Validators;

namespace Presentation.API.Tests.Validators;

public class CreateAdjustmentRequestValidatorTests
{
	private readonly CreateAdjustmentRequestValidator _validator = new();

	[Fact]
	public void Should_Pass_When_AllFieldsValid()
	{
		// Arrange
		CreateAdjustmentRequest request = new() { Type = "Tip", Amount = 5.0 };

		// Act
		FluentValidation.Results.ValidationResult result = _validator.Validate(request);

		// Assert
		Assert.True(result.IsValid);
	}

	[Fact]
	public void Should_Fail_When_TypeIsEmpty()
	{
		// Arrange
		CreateAdjustmentRequest request = new() { Type = "", Amount = 5.0 };

		// Act
		FluentValidation.Results.ValidationResult result = _validator.Validate(request);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.ErrorMessage == CreateAdjustmentRequestValidator.TypeMustNotBeEmpty);
	}

	[Fact]
	public void Should_Fail_When_TypeIsInvalid()
	{
		// Arrange
		CreateAdjustmentRequest request = new() { Type = "Invalid", Amount = 5.0 };

		// Act
		FluentValidation.Results.ValidationResult result = _validator.Validate(request);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.ErrorMessage == CreateAdjustmentRequestValidator.TypeMustBeValid);
	}

	[Fact]
	public void Should_Fail_When_AmountIsZero()
	{
		// Arrange
		CreateAdjustmentRequest request = new() { Type = "Tip", Amount = 0 };

		// Act
		FluentValidation.Results.ValidationResult result = _validator.Validate(request);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.ErrorMessage == CreateAdjustmentRequestValidator.AmountMustBeNonZero);
	}

	[Fact]
	public void Should_Fail_When_TypeIsOtherAndDescriptionIsNull()
	{
		// Arrange
		CreateAdjustmentRequest request = new() { Type = "Other", Amount = 5.0, Description = null };

		// Act
		FluentValidation.Results.ValidationResult result = _validator.Validate(request);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.ErrorMessage == CreateAdjustmentRequestValidator.DescriptionRequiredForOtherType);
	}

	[Fact]
	public void Should_Pass_When_TypeIsOtherAndDescriptionIsProvided()
	{
		// Arrange
		CreateAdjustmentRequest request = new() { Type = "Other", Amount = 5.0, Description = "Reason" };

		// Act
		FluentValidation.Results.ValidationResult result = _validator.Validate(request);

		// Assert
		Assert.True(result.IsValid);
	}

	[Fact]
	public void Should_Pass_When_TypeIsTipAndDescriptionIsNull()
	{
		// Arrange
		CreateAdjustmentRequest request = new() { Type = "Tip", Amount = 5.0, Description = null };

		// Act
		FluentValidation.Results.ValidationResult result = _validator.Validate(request);

		// Assert
		Assert.True(result.IsValid);
	}
}
