using API.Generated.Dtos;
using API.Validators;

namespace Presentation.API.Tests.Validators;

public class UpdateUserRequestValidatorTests
{
	private readonly UpdateUserRequestValidator _validator = new();

	[Fact]
	public void Should_Pass_When_AllFieldsAreValid()
	{
		// Arrange
		UpdateUserRequest request = new()
		{
			Email = "user@example.com",
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
		UpdateUserRequest request = new()
		{
			Email = "",
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
		UpdateUserRequest request = new()
		{
			Email = "not-an-email",
			Role = "User"
		};

		// Act
		FluentValidation.Results.ValidationResult result = _validator.Validate(request);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.PropertyName == "Email");
	}

	[Fact]
	public void Should_Fail_When_RoleIsEmpty()
	{
		// Arrange
		UpdateUserRequest request = new()
		{
			Email = "user@example.com",
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
		UpdateUserRequest request = new()
		{
			Email = "user@example.com",
			Role = "InvalidRole"
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
		UpdateUserRequest request = new()
		{
			Email = "user@example.com",
			Role = role
		};

		// Act
		FluentValidation.Results.ValidationResult result = _validator.Validate(request);

		// Assert
		Assert.True(result.IsValid);
	}
}
