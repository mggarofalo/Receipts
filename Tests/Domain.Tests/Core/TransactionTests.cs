using Domain.Core;

namespace Domain.Tests.Core;

public class TransactionTests
{
	[Fact]
	public void Constructor_ValidInput_CreatesTransaction()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		Guid receiptId = Guid.NewGuid();
		Guid accountId = Guid.NewGuid();
		Money amount = new(100.50m, "USD");
		DateOnly date = DateOnly.FromDateTime(DateTime.Today);

		// Act
		Transaction transaction = new(id, receiptId, accountId, amount, date);

		// Assert
		Assert.Equal(id, transaction.Id);
		Assert.Equal(receiptId, transaction.ReceiptId);
		Assert.Equal(accountId, transaction.AccountId);
		Assert.Equal(amount, transaction.Amount);
		Assert.Equal(date, transaction.Date);
	}

	[Fact]
	public void Constructor_NullId_CreatesTransactionWithNullId()
	{
		// Arrange
		Guid receiptId = Guid.NewGuid();
		Guid accountId = Guid.NewGuid();
		Money amount = new(100.50m, "USD");
		DateOnly date = DateOnly.FromDateTime(DateTime.Today);

		// Act
		Transaction transaction = new(null, receiptId, accountId, amount, date);

		// Assert
		Assert.Null(transaction.Id);
	}

	[Fact]
	public void Constructor_EmptyReceiptId_ThrowsArgumentException()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		Guid accountId = Guid.NewGuid();
		Money amount = new(100.50m, "USD");
		DateOnly date = DateOnly.FromDateTime(DateTime.Today);

		// Act & Assert
		ArgumentException exception = Assert.Throws<ArgumentException>(() => new Transaction(id, Guid.Empty, accountId, amount, date));
		Assert.Equal("Receipt ID cannot be empty (Parameter 'receiptId')", exception.Message);
	}

	[Fact]
	public void Constructor_EmptyAccountId_ThrowsArgumentException()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		Guid receiptId = Guid.NewGuid();
		Money amount = new(100.50m, "USD");
		DateOnly date = DateOnly.FromDateTime(DateTime.Today);

		// Act & Assert
		ArgumentException exception = Assert.Throws<ArgumentException>(() => new Transaction(id, receiptId, Guid.Empty, amount, date));
		Assert.Equal("Account ID cannot be empty (Parameter 'accountId')", exception.Message);
	}

	[Fact]
	public void Constructor_ZeroAmount_ThrowsArgumentException()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		Guid receiptId = Guid.NewGuid();
		Guid accountId = Guid.NewGuid();
		Money amount = new(0m, "USD");
		DateOnly date = DateOnly.FromDateTime(DateTime.Today);

		// Act & Assert
		ArgumentException exception = Assert.Throws<ArgumentException>(() => new Transaction(id, receiptId, accountId, amount, date));
		Assert.Equal("Amount must be non-zero (Parameter 'amount')", exception.Message);
	}

	[Fact]
	public void Constructor_FutureDate_ThrowsArgumentException()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		Guid receiptId = Guid.NewGuid();
		Guid accountId = Guid.NewGuid();
		Money amount = new(100.50m, "USD");
		DateOnly date = DateOnly.FromDateTime(DateTime.Today.AddDays(1));

		// Act & Assert
		ArgumentException exception = Assert.Throws<ArgumentException>(() => new Transaction(id, receiptId, accountId, amount, date));
		Assert.Equal("Date cannot be in the future (Parameter 'date')", exception.Message);
	}
}