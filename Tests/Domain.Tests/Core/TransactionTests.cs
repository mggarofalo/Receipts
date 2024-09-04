using Domain.Core;
using SampleData.Domain.Core;

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
		Money amount = new(100.50m);
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
		Money amount = new(100.50m);
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
		Money amount = new(100.50m);
		DateOnly date = DateOnly.FromDateTime(DateTime.Today);

		// Act & Assert
		ArgumentException exception = Assert.Throws<ArgumentException>(() => new Transaction(id, Guid.Empty, accountId, amount, date));
		Assert.StartsWith(Transaction.ReceiptIdCannotBeEmpty, exception.Message);
	}

	[Fact]
	public void Constructor_EmptyAccountId_ThrowsArgumentException()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		Guid receiptId = Guid.NewGuid();
		Money amount = new(100.50m);
		DateOnly date = DateOnly.FromDateTime(DateTime.Today);

		// Act & Assert
		ArgumentException exception = Assert.Throws<ArgumentException>(() => new Transaction(id, receiptId, Guid.Empty, amount, date));
		Assert.StartsWith(Transaction.AccountIdCannotBeEmpty, exception.Message);
	}

	[Fact]
	public void Constructor_ZeroAmount_ThrowsArgumentException()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		Guid receiptId = Guid.NewGuid();
		Guid accountId = Guid.NewGuid();
		Money amount = new(0m);
		DateOnly date = DateOnly.FromDateTime(DateTime.Today);

		// Act & Assert
		ArgumentException exception = Assert.Throws<ArgumentException>(() => new Transaction(id, receiptId, accountId, amount, date));
		Assert.StartsWith(Transaction.AmountMustBeNonZero, exception.Message);
	}

	[Fact]
	public void Constructor_FutureDate_ThrowsArgumentException()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		Guid receiptId = Guid.NewGuid();
		Guid accountId = Guid.NewGuid();
		Money amount = new(100.50m);
		DateOnly date = DateOnly.FromDateTime(DateTime.Today.AddDays(1));

		// Act & Assert
		ArgumentException exception = Assert.Throws<ArgumentException>(() => new Transaction(id, receiptId, accountId, amount, date));
		Assert.StartsWith(Transaction.DateCannotBeInTheFuture, exception.Message);
	}

	[Fact]
	public void Equals_SameTransaction_ReturnsTrue()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		Guid receiptId = Guid.NewGuid();
		Guid accountId = Guid.NewGuid();
		Money amount = new(100.50m);
		DateOnly date = DateOnly.FromDateTime(DateTime.Today);

		Transaction transaction1 = new(id, receiptId, accountId, amount, date);
		Transaction transaction2 = new(id, receiptId, accountId, amount, date);

		// Act & Assert
		Assert.True(transaction1 == transaction2);
		Assert.False(transaction1 != transaction2);
		Assert.True(transaction1.Equals(transaction2));
	}

	[Fact]
	public void Equals_DifferentTransaction_ReturnsFalse()
	{
		// Arrange
		Guid id1 = Guid.NewGuid();
		Guid id2 = Guid.NewGuid();
		Guid receiptId = Guid.NewGuid();
		Guid accountId = Guid.NewGuid();
		Money amount = new(100.50m);
		DateOnly date = DateOnly.FromDateTime(DateTime.Today);

		Transaction transaction1 = new(id1, receiptId, accountId, amount, date);
		Transaction transaction2 = new(id2, receiptId, accountId, amount, date);

		// Act & Assert
		Assert.False(transaction1 == transaction2);
		Assert.True(transaction1 != transaction2);
		Assert.False(transaction1.Equals(transaction2));
	}

	[Fact]
	public void Equals_DifferentReceiptId_ReturnsFalse()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		Guid receiptId1 = Guid.NewGuid();
		Guid receiptId2 = Guid.NewGuid();
		Guid accountId = Guid.NewGuid();
		Money amount = new(100.50m);
		DateOnly date = DateOnly.FromDateTime(DateTime.Today);

		Transaction transaction1 = new(id, receiptId1, accountId, amount, date);
		Transaction transaction2 = new(id, receiptId2, accountId, amount, date);

		// Act & Assert
		Assert.False(transaction1 == transaction2);
		Assert.True(transaction1 != transaction2);
		Assert.False(transaction1.Equals(transaction2));
	}

	[Fact]
	public void Equals_DifferentAccountId_ReturnsFalse()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		Guid receiptId = Guid.NewGuid();
		Guid accountId1 = Guid.NewGuid();
		Guid accountId2 = Guid.NewGuid();
		Money amount = new(100.50m);
		DateOnly date = DateOnly.FromDateTime(DateTime.Today);

		Transaction transaction1 = new(id, receiptId, accountId1, amount, date);
		Transaction transaction2 = new(id, receiptId, accountId2, amount, date);

		// Act & Assert
		Assert.False(transaction1 == transaction2);
		Assert.True(transaction1 != transaction2);
		Assert.False(transaction1.Equals(transaction2));
	}

	[Fact]
	public void Equals_DifferentAmount_ReturnsFalse()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		Guid receiptId = Guid.NewGuid();
		Guid accountId = Guid.NewGuid();
		Money amount1 = new(100.50m);
		Money amount2 = new(200.75m);
		DateOnly date = DateOnly.FromDateTime(DateTime.Today);

		Transaction transaction1 = new(id, receiptId, accountId, amount1, date);
		Transaction transaction2 = new(id, receiptId, accountId, amount2, date);

		// Act & Assert
		Assert.False(transaction1 == transaction2);
		Assert.True(transaction1 != transaction2);
		Assert.False(transaction1.Equals(transaction2));
	}

	[Fact]
	public void Equals_DifferentDate_ReturnsFalse()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		Guid receiptId = Guid.NewGuid();
		Guid accountId = Guid.NewGuid();
		Money amount = new(100.50m);
		DateOnly date1 = DateOnly.FromDateTime(DateTime.Today);
		DateOnly date2 = DateOnly.FromDateTime(DateTime.Today.AddDays(-1));

		Transaction transaction1 = new(id, receiptId, accountId, amount, date1);
		Transaction transaction2 = new(id, receiptId, accountId, amount, date2);

		// Act & Assert
		Assert.False(transaction1 == transaction2);
		Assert.True(transaction1 != transaction2);
		Assert.False(transaction1.Equals(transaction2));
	}

	[Fact]
	public void Equals_NullTransaction_ReturnsFalse()
	{
		// Arrange
		Transaction transaction = TransactionGenerator.Generate();

		// Act & Assert
		Assert.False(transaction.Equals(null));
	}

	[Fact]
	public void Equals_NullObject_ReturnsFalse()
	{
		// Arrange
		Transaction transaction = TransactionGenerator.Generate();

		// Act & Assert
		Assert.False(transaction.Equals((object?)null));
	}

	[Fact]
	public void Equals_DifferentType_ReturnsFalse()
	{
		// Arrange
		Transaction transaction = TransactionGenerator.Generate();

		// Act & Assert
		Assert.False(transaction.Equals("not a transaction"));
	}

	[Fact]
	public void GetHashCode_SameTransaction_ReturnsSameHashCode()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		Guid receiptId = Guid.NewGuid();
		Guid accountId = Guid.NewGuid();
		Money amount = new(100.50m);
		DateOnly date = DateOnly.FromDateTime(DateTime.Today);

		Transaction transaction1 = new(id, receiptId, accountId, amount, date);
		Transaction transaction2 = new(id, receiptId, accountId, amount, date);

		// Act & Assert
		Assert.Equal(transaction1.GetHashCode(), transaction2.GetHashCode());
	}

	[Fact]
	public void GetHashCode_DifferentTransaction_ReturnsDifferentHashCode()
	{
		// Arrange
		Guid id1 = Guid.NewGuid();
		Guid id2 = Guid.NewGuid();
		Guid receiptId = Guid.NewGuid();
		Guid accountId = Guid.NewGuid();
		Money amount = new(100.50m);
		DateOnly date = DateOnly.FromDateTime(DateTime.Today);

		Transaction transaction1 = new(id1, receiptId, accountId, amount, date);
		Transaction transaction2 = new(id2, receiptId, accountId, amount, date);

		// Act & Assert
		Assert.NotEqual(transaction1.GetHashCode(), transaction2.GetHashCode());
	}
}