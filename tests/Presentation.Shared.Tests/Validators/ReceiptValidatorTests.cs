using Shared.Validators;
using Shared.ViewModels.Core;

namespace Presentation.Shared.Tests.Validators;

public class ReceiptValidatorTests
{
	private readonly ReceiptValidator _validator = new();

	[Fact]
	public void Should_Pass_When_ValidReceipt()
	{
		// Arrange
		ReceiptVM receipt = new()
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
		ReceiptVM receipt = new()
		{
			Description = new string('a', 257),
			Location = "Valid Location",
			Date = DateOnly.FromDateTime(DateTime.Today)
		};

		// Act
		FluentValidation.Results.ValidationResult result = _validator.Validate(receipt);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.ErrorMessage == ReceiptValidator.DescriptionMustNotExceed256Characters);
	}

	[Fact]
	public void Should_Fail_When_LocationIsEmpty()
	{
		// Arrange
		ReceiptVM receipt = new()
		{
			Description = "Valid Description",
			Location = string.Empty,
			Date = DateOnly.FromDateTime(DateTime.Today)
		};

		// Act
		FluentValidation.Results.ValidationResult result = _validator.Validate(receipt);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.ErrorMessage == ReceiptValidator.LocationIsRequired);
	}

	[Fact]
	public void Should_Fail_When_LocationExceeds200Characters()
	{
		// Arrange
		ReceiptVM receipt = new()
		{
			Description = "Valid Description",
			Location = new string('a', 201),
			Date = DateOnly.FromDateTime(DateTime.Today)
		};

		// Act
		FluentValidation.Results.ValidationResult result = _validator.Validate(receipt);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.ErrorMessage == ReceiptValidator.LocationMustNotExceed200Characters);
	}

	[Fact]
	public void Should_Fail_When_DateIsInTheFuture()
	{
		// Arrange
		ReceiptVM receipt = new()
		{
			Description = "Valid Description",
			Location = "Valid Location",
			Date = DateOnly.FromDateTime(DateTime.Today.AddDays(1))
		};

		// Act
		FluentValidation.Results.ValidationResult result = _validator.Validate(receipt);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.ErrorMessage == ReceiptValidator.DateMustBePriorToCurrentDate);
	}
}
