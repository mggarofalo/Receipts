using Domain.Core;

namespace Domain.Tests.Core;

public class TransactionTests
{
	[Fact]
	public void Constructor_ValidInput_CreatesTransaction()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		Money amount = new(100.50m);
		DateOnly date = DateOnly.FromDateTime(DateTime.Today);

		// Act
		Transaction transaction = new(id, amount, date);

		// Assert
		Assert.Equal(id, transaction.Id);
		Assert.Equal(amount, transaction.Amount);
		Assert.Equal(date, transaction.Date);
	}

	[Fact]
	public void Constructor_EmptyId_CreatesTransactionWithEmptyId()
	{
		// Arrange
		Money amount = new(100.50m);
		DateOnly date = DateOnly.FromDateTime(DateTime.Today);

		// Act
		Transaction transaction = new(Guid.Empty, amount, date);

		// Assert
		Assert.Equal(Guid.Empty, transaction.Id);
	}

	[Fact]
	public void Constructor_ZeroAmount_ThrowsArgumentException()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		Money amount = new(0m);
		DateOnly date = DateOnly.FromDateTime(DateTime.Today);

		// Act & Assert
		ArgumentException exception = Assert.Throws<ArgumentException>(() => new Transaction(id, amount, date));
		Assert.StartsWith(Transaction.AmountMustBeNonZero, exception.Message);
	}

	[Fact]
	public void Constructor_FutureDate_ThrowsArgumentException()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		Money amount = new(100.50m);
		DateOnly date = DateOnly.FromDateTime(DateTime.Today.AddDays(1));

		// Act & Assert
		ArgumentException exception = Assert.Throws<ArgumentException>(() => new Transaction(id, amount, date));
		Assert.StartsWith(Transaction.DateCannotBeInTheFuture, exception.Message);
	}
}
