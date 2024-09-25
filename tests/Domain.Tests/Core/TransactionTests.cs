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
	public void Constructor_NullId_CreatesTransactionWithNullId()
	{
		// Arrange
		Money amount = new(100.50m);
		DateOnly date = DateOnly.FromDateTime(DateTime.Today);

		// Act
		Transaction transaction = new(null, amount, date);

		// Assert
		Assert.Null(transaction.Id);
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

	[Fact]
	public void Equals_SameTransaction_ReturnsTrue()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		Money amount = new(100.50m);
		DateOnly date = DateOnly.FromDateTime(DateTime.Today);

		Transaction transaction1 = new(id, amount, date);
		Transaction transaction2 = new(id, amount, date);

		// Act & Assert
		Assert.Equal(transaction1, transaction2);
	}

	[Fact]
	public void Equals_DifferentTransaction_ReturnsFalse()
	{
		// Arrange
		Guid id1 = Guid.NewGuid();
		Guid id2 = Guid.NewGuid();
		Money amount = new(100.50m);
		DateOnly date = DateOnly.FromDateTime(DateTime.Today);

		Transaction transaction1 = new(id1, amount, date);
		Transaction transaction2 = new(id2, amount, date);

		// Act & Assert
		Assert.NotEqual(transaction1, transaction2);
	}

	[Fact]
	public void Equals_DifferentAmount_ReturnsFalse()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		Money amount1 = new(100.50m);
		Money amount2 = new(200.75m);
		DateOnly date = DateOnly.FromDateTime(DateTime.Today);

		Transaction transaction1 = new(id, amount1, date);
		Transaction transaction2 = new(id, amount2, date);

		// Act & Assert
		Assert.NotEqual(transaction1, transaction2);
	}

	[Fact]
	public void Equals_DifferentDate_ReturnsFalse()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		Money amount = new(100.50m);
		DateOnly date1 = DateOnly.FromDateTime(DateTime.Today);
		DateOnly date2 = DateOnly.FromDateTime(DateTime.Today.AddDays(-1));

		Transaction transaction1 = new(id, amount, date1);
		Transaction transaction2 = new(id, amount, date2);

		// Act & Assert
		Assert.NotEqual(transaction1, transaction2);
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
		Money amount = new(100.50m);
		DateOnly date = DateOnly.FromDateTime(DateTime.Today);

		Transaction transaction1 = new(id, amount, date);
		Transaction transaction2 = new(id, amount, date);

		// Act & Assert
		Assert.Equal(transaction1.GetHashCode(), transaction2.GetHashCode());
	}

	[Fact]
	public void GetHashCode_DifferentTransaction_ReturnsDifferentHashCode()
	{
		// Arrange
		Guid id1 = Guid.NewGuid();
		Guid id2 = Guid.NewGuid();
		Money amount = new(100.50m);
		DateOnly date = DateOnly.FromDateTime(DateTime.Today);

		Transaction transaction1 = new(id1, amount, date);
		Transaction transaction2 = new(id2, amount, date);

		// Act & Assert
		Assert.NotEqual(transaction1.GetHashCode(), transaction2.GetHashCode());
	}

	[Fact]
	public void OperatorEqual_SameTransaction_ReturnsTrue()
	{
		// Arrange
		Transaction transaction1 = TransactionGenerator.Generate();
		Transaction transaction2 = transaction1;

		// Act
		bool result = transaction1 == transaction2;

		// Assert
		Assert.True(result);
	}

	[Fact]
	public void OperatorEqual_DifferentTransaction_ReturnsFalse()
	{
		// Arrange
		Transaction transaction1 = TransactionGenerator.Generate();
		Transaction transaction2 = TransactionGenerator.Generate();

		// Act
		bool result = transaction1 == transaction2;

		// Assert
		Assert.False(result);
	}

	[Fact]
	public void OperatorNotEqual_SameTransaction_ReturnsFalse()
	{
		// Arrange
		Transaction transaction1 = TransactionGenerator.Generate();
		Transaction transaction2 = transaction1;

		// Act
		bool result = transaction1 != transaction2;

		// Assert
		Assert.False(result);
	}

	[Fact]
	public void OperatorNotEqual_DifferentTransaction_ReturnsTrue()
	{
		// Arrange
		Transaction transaction1 = TransactionGenerator.Generate();
		Transaction transaction2 = TransactionGenerator.Generate();

		// Act
		bool result = transaction1 != transaction2;

		// Assert
		Assert.True(result);
	}
}