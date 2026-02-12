using Shared.Validators;
using Shared.ViewModels.Core;

namespace Presentation.Shared.Tests.Validators;

public class AccountValidatorTests
{
	private readonly AccountValidator _validator = new();

	[Fact]
	public void Should_Pass_When_ValidAccount()
	{
		// Arrange
		AccountVM account = new()
		{
			AccountCode = "ACC123",
			Name = "Test Account",
			IsActive = true
		};

		// Act
		FluentValidation.Results.ValidationResult result = _validator.Validate(account);

		// Assert
		Assert.True(result.IsValid);
	}

	[Fact]
	public void Should_Fail_When_AccountCodeIsEmpty()
	{
		// Arrange
		AccountVM account = new()
		{
			AccountCode = string.Empty,
			Name = "Test Account",
			IsActive = true
		};

		// Act
		FluentValidation.Results.ValidationResult result = _validator.Validate(account);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.ErrorMessage == AccountValidator.AccountCodeIsRequired);
	}

	[Fact]
	public void Should_Fail_When_NameIsEmpty()
	{
		// Arrange
		AccountVM account = new()
		{
			AccountCode = "ACC123",
			Name = string.Empty,
			IsActive = true
		};

		// Act
		FluentValidation.Results.ValidationResult result = _validator.Validate(account);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.ErrorMessage == AccountValidator.NameIsRequired);
	}
}