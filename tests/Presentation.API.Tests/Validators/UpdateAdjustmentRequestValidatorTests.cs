using API.Generated.Dtos;
using API.Validators;

namespace Presentation.API.Tests.Validators;

public class UpdateAdjustmentRequestValidatorTests
{
	private readonly UpdateAdjustmentRequestValidator _validator = new();

	[Fact]
	public void Should_Pass_When_AllFieldsValid()
	{
		// Arrange
		UpdateAdjustmentRequest request = new() { Id = Guid.NewGuid(), Type = "Tip", Amount = 5.0 };

		// Act
		FluentValidation.Results.ValidationResult result = _validator.Validate(request);

		// Assert
		Assert.True(result.IsValid);
	}

	[Fact]
	public void Should_Fail_When_IdIsEmpty()
	{
		// Arrange
		UpdateAdjustmentRequest request = new() { Id = Guid.Empty, Type = "Tip", Amount = 5.0 };

		// Act
		FluentValidation.Results.ValidationResult result = _validator.Validate(request);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.ErrorMessage == UpdateAdjustmentRequestValidator.IdMustNotBeEmpty);
	}

	[Fact]
	public void Should_Fail_When_TypeIsEmpty()
	{
		// Arrange
		UpdateAdjustmentRequest request = new() { Id = Guid.NewGuid(), Type = "", Amount = 5.0 };

		// Act
		FluentValidation.Results.ValidationResult result = _validator.Validate(request);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.ErrorMessage == UpdateAdjustmentRequestValidator.TypeMustNotBeEmpty);
	}

	[Fact]
	public void Should_Fail_When_TypeIsInvalid()
	{
		// Arrange
		UpdateAdjustmentRequest request = new() { Id = Guid.NewGuid(), Type = "Invalid", Amount = 5.0 };

		// Act
		FluentValidation.Results.ValidationResult result = _validator.Validate(request);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.ErrorMessage == UpdateAdjustmentRequestValidator.TypeMustBeValid);
	}

	[Fact]
	public void Should_Fail_When_AmountIsZero()
	{
		// Arrange
		UpdateAdjustmentRequest request = new() { Id = Guid.NewGuid(), Type = "Tip", Amount = 0 };

		// Act
		FluentValidation.Results.ValidationResult result = _validator.Validate(request);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.ErrorMessage == UpdateAdjustmentRequestValidator.AmountMustBeNonZero);
	}

	[Fact]
	public void Should_Fail_When_TypeIsOtherAndDescriptionIsNull()
	{
		// Arrange
		UpdateAdjustmentRequest request = new() { Id = Guid.NewGuid(), Type = "Other", Amount = 5.0, Description = null };

		// Act
		FluentValidation.Results.ValidationResult result = _validator.Validate(request);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.ErrorMessage == UpdateAdjustmentRequestValidator.DescriptionRequiredForOtherType);
	}

	[Fact]
	public void Should_Pass_When_TypeIsOtherAndDescriptionIsProvided()
	{
		// Arrange
		UpdateAdjustmentRequest request = new() { Id = Guid.NewGuid(), Type = "Other", Amount = 5.0, Description = "Reason" };

		// Act
		FluentValidation.Results.ValidationResult result = _validator.Validate(request);

		// Assert
		Assert.True(result.IsValid);
	}

	[Fact]
	public void Should_Pass_When_TypeIsTipAndDescriptionIsNull()
	{
		// Arrange
		UpdateAdjustmentRequest request = new() { Id = Guid.NewGuid(), Type = "Tip", Amount = 5.0, Description = null };

		// Act
		FluentValidation.Results.ValidationResult result = _validator.Validate(request);

		// Assert
		Assert.True(result.IsValid);
	}
}
