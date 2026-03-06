using API.Generated.Dtos;
using API.Validators;

namespace Presentation.API.Tests.Validators;

public class UpdateCategoryRequestValidatorTests
{
	private readonly UpdateCategoryRequestValidator _validator = new();

	[Fact]
	public void Should_Pass_When_AllFieldsValid()
	{
		// Arrange
		UpdateCategoryRequest request = new() { Id = Guid.NewGuid(), Name = "Food" };

		// Act
		FluentValidation.Results.ValidationResult result = _validator.Validate(request);

		// Assert
		Assert.True(result.IsValid);
	}

	[Fact]
	public void Should_Fail_When_IdIsEmpty()
	{
		// Arrange
		UpdateCategoryRequest request = new() { Id = Guid.Empty, Name = "Food" };

		// Act
		FluentValidation.Results.ValidationResult result = _validator.Validate(request);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.ErrorMessage == UpdateCategoryRequestValidator.IdMustNotBeEmpty);
	}

	[Fact]
	public void Should_Fail_When_NameIsEmpty()
	{
		// Arrange
		UpdateCategoryRequest request = new() { Id = Guid.NewGuid(), Name = "" };

		// Act
		FluentValidation.Results.ValidationResult result = _validator.Validate(request);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.ErrorMessage == UpdateCategoryRequestValidator.NameMustNotBeEmpty);
	}
}
