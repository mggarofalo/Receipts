using API.Generated.Dtos;
using API.Validators;

namespace Presentation.API.Tests.Validators;

public class CreateReceiptRequestValidatorTests
{
	private readonly CreateReceiptRequestValidator _validator = new();

	[Fact]
	public void Should_Pass_When_ValidReceipt()
	{
		// Arrange
		CreateReceiptRequest receipt = new()
		{
			Description = "Valid Description",
			Location = "Valid Location",
			Date = DateOnly.FromDateTime(DateTime.Today)
		};

		// Act
		FluentValidation.Results.ValidationResult result = _validator.Validate(receipt);

		// Assert
		Assert.True(result.IsValid);
	}

	[Fact]
	public void Should_Fail_When_DescriptionExceeds256Characters()
	{
		// Arrange
		CreateReceiptRequest receipt = new()
		{
			Description = new string('a', 257),
			Location = "Valid Location",
			Date = DateOnly.FromDateTime(DateTime.Today)
		};

		// Act
		FluentValidation.Results.ValidationResult result = _validator.Validate(receipt);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.ErrorMessage == CreateReceiptRequestValidator.DescriptionMustNotExceed256Characters);
	}

	[Fact]
	public void Should_Fail_When_LocationExceeds200Characters()
	{
		// Arrange
		CreateReceiptRequest receipt = new()
		{
			Description = "Valid Description",
			Location = new string('a', 201),
			Date = DateOnly.FromDateTime(DateTime.Today)
		};

		// Act
		FluentValidation.Results.ValidationResult result = _validator.Validate(receipt);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.ErrorMessage == CreateReceiptRequestValidator.LocationMustNotExceed200Characters);
	}

	[Fact]
	public void Should_Fail_When_DateIsInTheFuture()
	{
		// Arrange
		CreateReceiptRequest receipt = new()
		{
			Description = "Valid Description",
			Location = "Valid Location",
			Date = DateOnly.FromDateTime(DateTime.Today.AddDays(1))
		};

		// Act
		FluentValidation.Results.ValidationResult result = _validator.Validate(receipt);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.ErrorMessage == CreateReceiptRequestValidator.DateMustBePriorToCurrentDate);
	}
}
