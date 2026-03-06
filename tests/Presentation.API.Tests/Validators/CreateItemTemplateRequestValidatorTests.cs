using API.Generated.Dtos;
using API.Validators;

namespace Presentation.API.Tests.Validators;

public class CreateItemTemplateRequestValidatorTests
{
	private readonly CreateItemTemplateRequestValidator _validator = new();

	[Fact]
	public void Should_Pass_When_NameIsValid()
	{
		// Arrange
		CreateItemTemplateRequest request = new() { Name = "Template" };

		// Act
		FluentValidation.Results.ValidationResult result = _validator.Validate(request);

		// Assert
		Assert.True(result.IsValid);
	}

	[Fact]
	public void Should_Fail_When_NameIsEmpty()
	{
		// Arrange
		CreateItemTemplateRequest request = new() { Name = "" };

		// Act
		FluentValidation.Results.ValidationResult result = _validator.Validate(request);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.ErrorMessage == CreateItemTemplateRequestValidator.NameMustNotBeEmpty);
	}

	[Fact]
	public void Should_Fail_When_DefaultPricingModeIsInvalid()
	{
		// Arrange
		CreateItemTemplateRequest request = new() { Name = "Template", DefaultPricingMode = "invalid" };

		// Act
		FluentValidation.Results.ValidationResult result = _validator.Validate(request);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.ErrorMessage == CreateItemTemplateRequestValidator.DefaultPricingModeInvalid);
	}

	[Fact]
	public void Should_Pass_When_DefaultPricingModeIsQuantity()
	{
		// Arrange
		CreateItemTemplateRequest request = new() { Name = "Template", DefaultPricingMode = "quantity" };

		// Act
		FluentValidation.Results.ValidationResult result = _validator.Validate(request);

		// Assert
		Assert.True(result.IsValid);
	}

	[Fact]
	public void Should_Pass_When_DefaultPricingModeIsFlat()
	{
		// Arrange
		CreateItemTemplateRequest request = new() { Name = "Template", DefaultPricingMode = "flat" };

		// Act
		FluentValidation.Results.ValidationResult result = _validator.Validate(request);

		// Assert
		Assert.True(result.IsValid);
	}

	[Fact]
	public void Should_Pass_When_DefaultPricingModeIsNull()
	{
		// Arrange
		CreateItemTemplateRequest request = new() { Name = "Template", DefaultPricingMode = null };

		// Act
		FluentValidation.Results.ValidationResult result = _validator.Validate(request);

		// Assert
		Assert.True(result.IsValid);
	}

	[Fact]
	public void Should_Fail_When_DefaultUnitPriceIsNegative()
	{
		// Arrange
		CreateItemTemplateRequest request = new() { Name = "Template", DefaultUnitPrice = -1 };

		// Act
		FluentValidation.Results.ValidationResult result = _validator.Validate(request);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.ErrorMessage == CreateItemTemplateRequestValidator.DefaultUnitPriceMustBePositive);
	}

	[Fact]
	public void Should_Fail_When_DefaultUnitPriceIsZero()
	{
		// Arrange
		CreateItemTemplateRequest request = new() { Name = "Template", DefaultUnitPrice = 0 };

		// Act
		FluentValidation.Results.ValidationResult result = _validator.Validate(request);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.ErrorMessage == CreateItemTemplateRequestValidator.DefaultUnitPriceMustBePositive);
	}

	[Fact]
	public void Should_Pass_When_DefaultUnitPriceIsPositive()
	{
		// Arrange
		CreateItemTemplateRequest request = new() { Name = "Template", DefaultUnitPrice = 5.0 };

		// Act
		FluentValidation.Results.ValidationResult result = _validator.Validate(request);

		// Assert
		Assert.True(result.IsValid);
	}

	[Fact]
	public void Should_Pass_When_DefaultUnitPriceIsNull()
	{
		// Arrange
		CreateItemTemplateRequest request = new() { Name = "Template", DefaultUnitPrice = null };

		// Act
		FluentValidation.Results.ValidationResult result = _validator.Validate(request);

		// Assert
		Assert.True(result.IsValid);
	}
}
