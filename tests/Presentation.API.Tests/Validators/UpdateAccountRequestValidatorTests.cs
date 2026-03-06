using API.Generated.Dtos;
using API.Validators;

namespace Presentation.API.Tests.Validators;

public class UpdateAccountRequestValidatorTests
{
	private readonly UpdateAccountRequestValidator _validator = new();

	[Fact]
	public void Should_Pass_When_AllFieldsValid()
	{
		// Arrange
		UpdateAccountRequest request = new() { Id = Guid.NewGuid(), AccountCode = "ABC", Name = "Test" };

		// Act
		FluentValidation.Results.ValidationResult result = _validator.Validate(request);

		// Assert
		Assert.True(result.IsValid);
	}

	[Fact]
	public void Should_Fail_When_IdIsEmpty()
	{
		// Arrange
		UpdateAccountRequest request = new() { Id = Guid.Empty, AccountCode = "ABC", Name = "Test" };

		// Act
		FluentValidation.Results.ValidationResult result = _validator.Validate(request);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.ErrorMessage == UpdateAccountRequestValidator.IdMustNotBeEmpty);
	}

	[Fact]
	public void Should_Fail_When_AccountCodeIsEmpty()
	{
		// Arrange
		UpdateAccountRequest request = new() { Id = Guid.NewGuid(), AccountCode = "", Name = "Test" };

		// Act
		FluentValidation.Results.ValidationResult result = _validator.Validate(request);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.ErrorMessage == UpdateAccountRequestValidator.AccountCodeMustNotBeEmpty);
	}

	[Fact]
	public void Should_Fail_When_NameIsEmpty()
	{
		// Arrange
		UpdateAccountRequest request = new() { Id = Guid.NewGuid(), AccountCode = "ABC", Name = "" };

		// Act
		FluentValidation.Results.ValidationResult result = _validator.Validate(request);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.ErrorMessage == UpdateAccountRequestValidator.NameMustNotBeEmpty);
	}
}
