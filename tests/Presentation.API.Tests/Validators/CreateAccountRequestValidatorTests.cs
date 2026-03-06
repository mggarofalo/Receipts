using API.Generated.Dtos;
using API.Validators;

namespace Presentation.API.Tests.Validators;

public class CreateAccountRequestValidatorTests
{
	private readonly CreateAccountRequestValidator _validator = new();

	[Fact]
	public void Should_Pass_When_AllFieldsValid()
	{
		// Arrange
		CreateAccountRequest request = new() { AccountCode = "ABC", Name = "Test" };

		// Act
		FluentValidation.Results.ValidationResult result = _validator.Validate(request);

		// Assert
		Assert.True(result.IsValid);
	}

	[Fact]
	public void Should_Fail_When_AccountCodeIsEmpty()
	{
		// Arrange
		CreateAccountRequest request = new() { AccountCode = "", Name = "Test" };

		// Act
		FluentValidation.Results.ValidationResult result = _validator.Validate(request);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.ErrorMessage == CreateAccountRequestValidator.AccountCodeMustNotBeEmpty);
	}

	[Fact]
	public void Should_Fail_When_NameIsEmpty()
	{
		// Arrange
		CreateAccountRequest request = new() { AccountCode = "ABC", Name = "" };

		// Act
		FluentValidation.Results.ValidationResult result = _validator.Validate(request);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.ErrorMessage == CreateAccountRequestValidator.NameMustNotBeEmpty);
	}
}
