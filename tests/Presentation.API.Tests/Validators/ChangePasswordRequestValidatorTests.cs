using API.Generated.Dtos;
using API.Validators;
using FluentValidation.Results;

namespace Presentation.API.Tests.Validators;

public class ChangePasswordRequestValidatorTests
{
	private readonly ChangePasswordRequestValidator _validator = new();

	[Fact]
	public void Should_Pass_When_PasswordsAreDifferentAndValid()
	{
		// Arrange
		ChangePasswordRequest request = new()
		{
			CurrentPassword = "OldPassword1",
			NewPassword = "NewPassword1"
		};

		// Act
		ValidationResult result = _validator.Validate(request);

		// Assert
		Assert.True(result.IsValid);
	}

	[Fact]
	public void Should_Fail_When_NewPasswordMatchesCurrentPassword()
	{
		// Arrange
		ChangePasswordRequest request = new()
		{
			CurrentPassword = "SamePassword1",
			NewPassword = "SamePassword1"
		};

		// Act
		ValidationResult result = _validator.Validate(request);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e =>
			e.PropertyName == "NewPassword" &&
			e.ErrorMessage == "New password must be different from current password.");
	}

	[Fact]
	public void Should_Fail_When_CurrentPasswordIsEmpty()
	{
		// Arrange
		ChangePasswordRequest request = new()
		{
			CurrentPassword = "",
			NewPassword = "ValidPass1"
		};

		// Act
		ValidationResult result = _validator.Validate(request);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.PropertyName == "CurrentPassword");
	}

	[Fact]
	public void Should_Fail_When_NewPasswordIsEmpty()
	{
		// Arrange
		ChangePasswordRequest request = new()
		{
			CurrentPassword = "OldPassword1",
			NewPassword = ""
		};

		// Act
		ValidationResult result = _validator.Validate(request);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.PropertyName == "NewPassword");
	}

	[Fact]
	public void Should_Fail_When_NewPasswordIsTooShort()
	{
		// Arrange
		ChangePasswordRequest request = new()
		{
			CurrentPassword = "OldPassword1",
			NewPassword = "short"
		};

		// Act
		ValidationResult result = _validator.Validate(request);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.PropertyName == "NewPassword");
	}

	[Fact]
	public void Should_Pass_When_NewPasswordIsExactly8Characters()
	{
		// Arrange
		ChangePasswordRequest request = new()
		{
			CurrentPassword = "OldPassword1",
			NewPassword = "12345678"
		};

		// Act
		ValidationResult result = _validator.Validate(request);

		// Assert
		Assert.True(result.IsValid);
	}
}
