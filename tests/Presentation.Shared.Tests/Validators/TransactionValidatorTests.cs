using Shared.Validators;
using Shared.ViewModels.Core;

namespace Presentation.Shared.Tests.Validators;

public class TransactionValidatorTests
{
	private readonly TransactionValidator _validator = new();

	[Fact]
	public void Should_Pass_When_ValidTransaction()
	{
		// Arrange
		TransactionVM transaction = new()
		{
			Amount = 100,
			Date = DateOnly.FromDateTime(DateTime.Today),
		};

		// Act
		FluentValidation.Results.ValidationResult result = _validator.Validate(transaction);

		// Assert
		Assert.True(result.IsValid);
	}

	[Fact]
	public void Should_Fail_When_AmountIsZero()
	{
		// Arrange
		TransactionVM transaction = new()
		{
			Amount = 0,
			Date = DateOnly.FromDateTime(DateTime.Today),
		};

		// Act
		FluentValidation.Results.ValidationResult result = _validator.Validate(transaction);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.ErrorMessage == TransactionValidator.AmountMustBeNonZero);
	}

	[Fact]
	public void Should_Pass_When_DateIsInThePast()
	{
		// Arrange
		DateOnly pastDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-1));
		TransactionVM transaction = new()
		{
			Amount = 100,
			Date = pastDate
		};

		// Act
		FluentValidation.Results.ValidationResult result = _validator.Validate(transaction);

		// Assert
		Assert.True(result.IsValid);
	}

	[Fact]
	public void Should_Pass_When_DateIsToday()
	{
		// Arrange
		DateOnly today = DateOnly.FromDateTime(DateTime.Today);
		TransactionVM transaction = new()
		{
			Amount = 100,
			Date = today
		};

		// Act
		FluentValidation.Results.ValidationResult result = _validator.Validate(transaction);

		// Assert
		Assert.True(result.IsValid);
	}

	[Fact]
	public void Should_Fail_When_DateIsInTheFuture()
	{
		// Arrange
		TransactionVM transaction = new()
		{
			Amount = 100,
			Date = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
		};

		// Act
		FluentValidation.Results.ValidationResult result = _validator.Validate(transaction);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.ErrorMessage == TransactionValidator.DateMustBePriorToCurrentDate);
	}

	[Fact]
	public void Should_Fail_When_DateIsNull()
	{
		// Arrange
		TransactionVM transaction = new()
		{
			Amount = 100,
			Date = null
		};

		// Act
		FluentValidation.Results.ValidationResult result = _validator.Validate(transaction);

		// Assert
		Assert.False(result.IsValid);
		Assert.Contains(result.Errors, e => e.ErrorMessage == TransactionValidator.DateMustBePriorToCurrentDate);
	}
}
