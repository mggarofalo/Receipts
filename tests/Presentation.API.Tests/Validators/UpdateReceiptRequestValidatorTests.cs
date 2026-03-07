using API.Generated.Dtos;
using API.Validators;

namespace Presentation.API.Tests.Validators;

public class UpdateReceiptRequestValidatorTests
{
	private readonly UpdateReceiptRequestValidator _validator = new();

	[Fact]
	public void Should_Pass_When_AllFieldsValid()
	{
		// Arrange
		UpdateReceiptRequest request = new()
		{
			Id = Guid.NewGuid(),
			Location = "Store",
			Date = DateOnly.FromDateTime(DateTime.Today)
		};

		// Act
		FluentValidation.Results.ValidationResult result = _validator.Validate(request);

		// Assert
		Assert.True(result.IsValid);
	}

	[Fact]
	public void Should_Fail_When_IdIsEmpty()
	{
		// Arrange
		UpdateReceiptRequest request = new()
		{
			Id = Guid.Empty,
			Location = "Store",
			Date = DateOnly.FromDateTime(DateTime.Today)
		};

		// Act
		FluentValidation.Results.ValidationResult result = _validator.Validate(request);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.ErrorMessage == UpdateReceiptRequestValidator.IdMustNotBeEmpty);
	}

	[Fact]
	public void Should_Fail_When_LocationIsEmpty()
	{
		// Arrange
		UpdateReceiptRequest request = new()
		{
			Id = Guid.NewGuid(),
			Location = "",
			Date = DateOnly.FromDateTime(DateTime.Today)
		};

		// Act
		FluentValidation.Results.ValidationResult result = _validator.Validate(request);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.ErrorMessage == UpdateReceiptRequestValidator.LocationMustNotBeEmpty);
	}

	[Fact]
	public void Should_Fail_When_LocationExceeds200Characters()
	{
		// Arrange
		UpdateReceiptRequest request = new()
		{
			Id = Guid.NewGuid(),
			Location = new string('a', 201),
			Date = DateOnly.FromDateTime(DateTime.Today)
		};

		// Act
		FluentValidation.Results.ValidationResult result = _validator.Validate(request);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.ErrorMessage == UpdateReceiptRequestValidator.LocationMustNotExceed200Characters);
	}

	[Fact]
	public void Should_Fail_When_DateIsInTheFuture()
	{
		// Arrange
		UpdateReceiptRequest request = new()
		{
			Id = Guid.NewGuid(),
			Location = "Store",
			Date = DateOnly.FromDateTime(DateTime.Today.AddDays(1))
		};

		// Act
		FluentValidation.Results.ValidationResult result = _validator.Validate(request);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.ErrorMessage == UpdateReceiptRequestValidator.DateMustBePriorToCurrentDate);
	}
}
