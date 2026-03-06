using API.Generated.Dtos;
using API.Validators;

namespace Presentation.API.Tests.Validators;

public class CreateCategoryRequestValidatorTests
{
	private readonly CreateCategoryRequestValidator _validator = new();

	[Fact]
	public void Should_Pass_When_NameIsValid()
	{
		// Arrange
		CreateCategoryRequest request = new() { Name = "Food" };

		// Act
		FluentValidation.Results.ValidationResult result = _validator.Validate(request);

		// Assert
		Assert.True(result.IsValid);
	}

	[Fact]
	public void Should_Fail_When_NameIsEmpty()
	{
		// Arrange
		CreateCategoryRequest request = new() { Name = "" };

		// Act
		FluentValidation.Results.ValidationResult result = _validator.Validate(request);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.ErrorMessage == CreateCategoryRequestValidator.NameMustNotBeEmpty);
	}
}
