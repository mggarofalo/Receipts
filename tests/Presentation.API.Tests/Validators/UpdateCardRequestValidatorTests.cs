using API.Generated.Dtos;
using API.Validators;

namespace Presentation.API.Tests.Validators;

public class UpdateCardRequestValidatorTests
{
	private readonly UpdateCardRequestValidator _validator = new();

	[Fact]
	public void Should_Pass_When_AllFieldsValid()
	{
		// Arrange
		UpdateCardRequest request = new() { Id = Guid.NewGuid(), CardCode = "ABC", Name = "Test", AccountId = Guid.NewGuid() };

		// Act
		FluentValidation.Results.ValidationResult result = _validator.Validate(request);

		// Assert
		Assert.True(result.IsValid);
	}

	[Fact]
	public void Should_Fail_When_IdIsEmpty()
	{
		// Arrange
		UpdateCardRequest request = new() { Id = Guid.Empty, CardCode = "ABC", Name = "Test", AccountId = Guid.NewGuid() };

		// Act
		FluentValidation.Results.ValidationResult result = _validator.Validate(request);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.ErrorMessage == UpdateCardRequestValidator.IdMustNotBeEmpty);
	}

	[Fact]
	public void Should_Fail_When_CardCodeIsEmpty()
	{
		// Arrange
		UpdateCardRequest request = new() { Id = Guid.NewGuid(), CardCode = "", Name = "Test", AccountId = Guid.NewGuid() };

		// Act
		FluentValidation.Results.ValidationResult result = _validator.Validate(request);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.ErrorMessage == UpdateCardRequestValidator.CardCodeMustNotBeEmpty);
	}

	[Fact]
	public void Should_Fail_When_NameIsEmpty()
	{
		// Arrange
		UpdateCardRequest request = new() { Id = Guid.NewGuid(), CardCode = "ABC", Name = "", AccountId = Guid.NewGuid() };

		// Act
		FluentValidation.Results.ValidationResult result = _validator.Validate(request);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.ErrorMessage == UpdateCardRequestValidator.NameMustNotBeEmpty);
	}

	[Fact]
	public void Should_Fail_When_AccountIdIsEmpty()
	{
		// Arrange
		UpdateCardRequest request = new() { Id = Guid.NewGuid(), CardCode = "ABC", Name = "Test", AccountId = Guid.Empty };

		// Act
		FluentValidation.Results.ValidationResult result = _validator.Validate(request);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.ErrorMessage == UpdateCardRequestValidator.AccountIdMustNotBeEmpty);
	}
}
