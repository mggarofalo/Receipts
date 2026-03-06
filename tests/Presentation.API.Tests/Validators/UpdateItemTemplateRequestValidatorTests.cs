using API.Generated.Dtos;
using API.Validators;

namespace Presentation.API.Tests.Validators;

public class UpdateItemTemplateRequestValidatorTests
{
	private readonly UpdateItemTemplateRequestValidator _validator = new();

	[Fact]
	public void Should_Pass_When_AllFieldsValid()
	{
		// Arrange
		UpdateItemTemplateRequest request = new() { Id = Guid.NewGuid(), Name = "Template" };

		// Act
		FluentValidation.Results.ValidationResult result = _validator.Validate(request);

		// Assert
		Assert.True(result.IsValid);
	}

	[Fact]
	public void Should_Fail_When_IdIsEmpty()
	{
		// Arrange
		UpdateItemTemplateRequest request = new() { Id = Guid.Empty, Name = "Template" };

		// Act
		FluentValidation.Results.ValidationResult result = _validator.Validate(request);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.ErrorMessage == UpdateItemTemplateRequestValidator.IdMustNotBeEmpty);
	}

	[Fact]
	public void Should_Fail_When_NameIsEmpty()
	{
		// Arrange
		UpdateItemTemplateRequest request = new() { Id = Guid.NewGuid(), Name = "" };

		// Act
		FluentValidation.Results.ValidationResult result = _validator.Validate(request);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.ErrorMessage == UpdateItemTemplateRequestValidator.NameMustNotBeEmpty);
	}

	[Fact]
	public void Should_Fail_When_DefaultPricingModeIsInvalid()
	{
		// Arrange
		UpdateItemTemplateRequest request = new() { Id = Guid.NewGuid(), Name = "Template", DefaultPricingMode = "invalid" };

		// Act
		FluentValidation.Results.ValidationResult result = _validator.Validate(request);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.ErrorMessage == UpdateItemTemplateRequestValidator.DefaultPricingModeInvalid);
	}

	[Fact]
	public void Should_Pass_When_DefaultPricingModeIsQuantity()
	{
		// Arrange
		UpdateItemTemplateRequest request = new() { Id = Guid.NewGuid(), Name = "Template", DefaultPricingMode = "quantity" };

		// Act
		FluentValidation.Results.ValidationResult result = _validator.Validate(request);

		// Assert
		Assert.True(result.IsValid);
	}

	[Fact]
	public void Should_Pass_When_DefaultPricingModeIsFlat()
	{
		// Arrange
		UpdateItemTemplateRequest request = new() { Id = Guid.NewGuid(), Name = "Template", DefaultPricingMode = "flat" };

		// Act
		FluentValidation.Results.ValidationResult result = _validator.Validate(request);

		// Assert
		Assert.True(result.IsValid);
	}

	[Fact]
	public void Should_Pass_When_DefaultPricingModeIsNull()
	{
		// Arrange
		UpdateItemTemplateRequest request = new() { Id = Guid.NewGuid(), Name = "Template", DefaultPricingMode = null };

		// Act
		FluentValidation.Results.ValidationResult result = _validator.Validate(request);

		// Assert
		Assert.True(result.IsValid);
	}

	[Fact]
	public void Should_Fail_When_DefaultUnitPriceIsNegative()
	{
		// Arrange
		UpdateItemTemplateRequest request = new() { Id = Guid.NewGuid(), Name = "Template", DefaultUnitPrice = -1 };

		// Act
		FluentValidation.Results.ValidationResult result = _validator.Validate(request);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.ErrorMessage == UpdateItemTemplateRequestValidator.DefaultUnitPriceMustBePositive);
	}

	[Fact]
	public void Should_Fail_When_DefaultUnitPriceIsZero()
	{
		// Arrange
		UpdateItemTemplateRequest request = new() { Id = Guid.NewGuid(), Name = "Template", DefaultUnitPrice = 0 };

		// Act
		FluentValidation.Results.ValidationResult result = _validator.Validate(request);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.ErrorMessage == UpdateItemTemplateRequestValidator.DefaultUnitPriceMustBePositive);
	}

	[Fact]
	public void Should_Pass_When_DefaultUnitPriceIsPositive()
	{
		// Arrange
		UpdateItemTemplateRequest request = new() { Id = Guid.NewGuid(), Name = "Template", DefaultUnitPrice = 5.0 };

		// Act
		FluentValidation.Results.ValidationResult result = _validator.Validate(request);

		// Assert
		Assert.True(result.IsValid);
	}

	[Fact]
	public void Should_Pass_When_DefaultUnitPriceIsNull()
	{
		// Arrange
		UpdateItemTemplateRequest request = new() { Id = Guid.NewGuid(), Name = "Template", DefaultUnitPrice = null };

		// Act
		FluentValidation.Results.ValidationResult result = _validator.Validate(request);

		// Assert
		Assert.True(result.IsValid);
	}
}
