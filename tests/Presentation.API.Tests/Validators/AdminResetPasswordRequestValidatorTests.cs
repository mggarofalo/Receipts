using API.Generated.Dtos;
using API.Validators;
using FluentValidation.Results;

namespace Presentation.API.Tests.Validators;

public class AdminResetPasswordRequestValidatorTests
{
	private readonly AdminResetPasswordRequestValidator _validator = new();

	[Fact]
	public void Should_Pass_When_PasswordIsValid()
	{
		// Arrange
		AdminResetPasswordRequest request = new() { NewPassword = "ValidPass1" };

		// Act
		ValidationResult result = _validator.Validate(request);

		// Assert
		Assert.True(result.IsValid);
	}

	[Fact]
	public void Should_Fail_When_PasswordIsEmpty()
	{
		// Arrange
		AdminResetPasswordRequest request = new() { NewPassword = "" };

		// Act
		ValidationResult result = _validator.Validate(request);

		// Assert
		Assert.False(result.IsValid);
	}

	[Fact]
	public void Should_Fail_When_PasswordIsTooShort()
	{
		// Arrange
		AdminResetPasswordRequest request = new() { NewPassword = "short" };

		// Act
		ValidationResult result = _validator.Validate(request);

		// Assert
		Assert.False(result.IsValid);
	}

	[Fact]
	public void Should_Pass_When_PasswordIsExactly8Characters()
	{
		// Arrange
		AdminResetPasswordRequest request = new() { NewPassword = "12345678" };

		// Act
		ValidationResult result = _validator.Validate(request);

		// Assert
		Assert.True(result.IsValid);
	}
}
