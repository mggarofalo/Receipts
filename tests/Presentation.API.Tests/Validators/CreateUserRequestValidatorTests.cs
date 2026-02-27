using API.Generated.Dtos;
using API.Validators;

namespace Presentation.API.Tests.Validators;

public class CreateUserRequestValidatorTests
{
	private readonly CreateUserRequestValidator _validator = new();

	[Fact]
	public void Should_Pass_When_AllFieldsAreValid()
	{
		// Arrange
		CreateUserRequest request = new()
		{
			Email = "user@example.com",
			Password = "ValidPass1",
			Role = "User"
		};

		// Act
		FluentValidation.Results.ValidationResult result = _validator.Validate(request);

		// Assert
		Assert.True(result.IsValid);
	}

	[Fact]
	public void Should_Fail_When_EmailIsEmpty()
	{
		// Arrange
		CreateUserRequest request = new()
		{
			Email = "",
			Password = "ValidPass1",
			Role = "User"
		};

		// Act
		FluentValidation.Results.ValidationResult result = _validator.Validate(request);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.PropertyName == "Email");
	}

	[Fact]
	public void Should_Fail_When_EmailIsInvalid()
	{
		// Arrange
		CreateUserRequest request = new()
		{
			Email = "not-an-email",
			Password = "ValidPass1",
			Role = "User"
		};

		// Act
		FluentValidation.Results.ValidationResult result = _validator.Validate(request);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.PropertyName == "Email");
	}

	[Fact]
	public void Should_Fail_When_PasswordIsEmpty()
	{
		// Arrange
		CreateUserRequest request = new()
		{
			Email = "user@example.com",
			Password = "",
			Role = "User"
		};

		// Act
		FluentValidation.Results.ValidationResult result = _validator.Validate(request);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.PropertyName == "Password");
	}

	[Fact]
	public void Should_Fail_When_PasswordIsTooShort()
	{
		// Arrange
		CreateUserRequest request = new()
		{
			Email = "user@example.com",
			Password = "short",
			Role = "User"
		};

		// Act
		FluentValidation.Results.ValidationResult result = _validator.Validate(request);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.PropertyName == "Password");
	}

	[Fact]
	public void Should_Fail_When_RoleIsEmpty()
	{
		// Arrange
		CreateUserRequest request = new()
		{
			Email = "user@example.com",
			Password = "ValidPass1",
			Role = ""
		};

		// Act
		FluentValidation.Results.ValidationResult result = _validator.Validate(request);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.PropertyName == "Role");
	}

	[Fact]
	public void Should_Fail_When_RoleIsInvalid()
	{
		// Arrange
		CreateUserRequest request = new()
		{
			Email = "user@example.com",
			Password = "ValidPass1",
			Role = "SuperAdmin"
		};

		// Act
		FluentValidation.Results.ValidationResult result = _validator.Validate(request);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.PropertyName == "Role");
	}

	[Theory]
	[InlineData("Admin")]
	[InlineData("User")]
	public void Should_Pass_When_RoleIsValid(string role)
	{
		// Arrange
		CreateUserRequest request = new()
		{
			Email = "user@example.com",
			Password = "ValidPass1",
			Role = role
		};

		// Act
		FluentValidation.Results.ValidationResult result = _validator.Validate(request);

		// Assert
		Assert.True(result.IsValid);
	}
}
