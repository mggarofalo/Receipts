using API.Generated.Dtos;
using API.Validators;

namespace Presentation.API.Tests.Validators;

public class CreateSubcategoryRequestValidatorTests
{
	private readonly CreateSubcategoryRequestValidator _validator = new();

	[Fact]
	public void Should_Pass_When_AllFieldsValid()
	{
		// Arrange
		CreateSubcategoryRequest request = new() { Name = "Fruits", CategoryId = Guid.NewGuid() };

		// Act
		FluentValidation.Results.ValidationResult result = _validator.Validate(request);

		// Assert
		Assert.True(result.IsValid);
	}

	[Fact]
	public void Should_Fail_When_NameIsEmpty()
	{
		// Arrange
		CreateSubcategoryRequest request = new() { Name = "", CategoryId = Guid.NewGuid() };

		// Act
		FluentValidation.Results.ValidationResult result = _validator.Validate(request);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.ErrorMessage == CreateSubcategoryRequestValidator.NameMustNotBeEmpty);
	}

	[Fact]
	public void Should_Fail_When_CategoryIdIsEmpty()
	{
		// Arrange
		CreateSubcategoryRequest request = new() { Name = "Fruits", CategoryId = Guid.Empty };

		// Act
		FluentValidation.Results.ValidationResult result = _validator.Validate(request);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.ErrorMessage == CreateSubcategoryRequestValidator.CategoryIdMustNotBeEmpty);
	}
}
