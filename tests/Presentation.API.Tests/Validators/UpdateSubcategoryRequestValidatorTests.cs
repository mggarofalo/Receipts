using API.Generated.Dtos;
using API.Validators;

namespace Presentation.API.Tests.Validators;

public class UpdateSubcategoryRequestValidatorTests
{
	private readonly UpdateSubcategoryRequestValidator _validator = new();

	[Fact]
	public void Should_Pass_When_AllFieldsValid()
	{
		// Arrange
		UpdateSubcategoryRequest request = new() { Id = Guid.NewGuid(), Name = "Fruits", CategoryId = Guid.NewGuid() };

		// Act
		FluentValidation.Results.ValidationResult result = _validator.Validate(request);

		// Assert
		Assert.True(result.IsValid);
	}

	[Fact]
	public void Should_Fail_When_IdIsEmpty()
	{
		// Arrange
		UpdateSubcategoryRequest request = new() { Id = Guid.Empty, Name = "Fruits", CategoryId = Guid.NewGuid() };

		// Act
		FluentValidation.Results.ValidationResult result = _validator.Validate(request);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.ErrorMessage == UpdateSubcategoryRequestValidator.IdMustNotBeEmpty);
	}

	[Fact]
	public void Should_Fail_When_NameIsEmpty()
	{
		// Arrange
		UpdateSubcategoryRequest request = new() { Id = Guid.NewGuid(), Name = "", CategoryId = Guid.NewGuid() };

		// Act
		FluentValidation.Results.ValidationResult result = _validator.Validate(request);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.ErrorMessage == UpdateSubcategoryRequestValidator.NameMustNotBeEmpty);
	}

	[Fact]
	public void Should_Fail_When_CategoryIdIsEmpty()
	{
		// Arrange
		UpdateSubcategoryRequest request = new() { Id = Guid.NewGuid(), Name = "Fruits", CategoryId = Guid.Empty };

		// Act
		FluentValidation.Results.ValidationResult result = _validator.Validate(request);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.ErrorMessage == UpdateSubcategoryRequestValidator.CategoryIdMustNotBeEmpty);
	}
}
