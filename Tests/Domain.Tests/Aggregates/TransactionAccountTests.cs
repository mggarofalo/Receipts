using Domain.Aggregates;
using Domain.Core;
using SampleData.Domain.Core;
using SampleData.Domain.Aggregates;

namespace Domain.Tests.Aggregates;

public class TransactionAccountTests
{
	[Fact]
	public void TransactionAccount_ShouldHaveRequiredProperties()
	{
		// Arrange
		Transaction transaction = TransactionGenerator.Generate();
		Account account = AccountGenerator.Generate();

		// Act
		TransactionAccount transactionAccount = new()
		{
			Transaction = transaction,
			Account = account
		};

		// Assert
		Assert.NotNull(transactionAccount.Transaction);
		Assert.Equal(transaction, transactionAccount.Transaction);
		Assert.NotNull(transactionAccount.Account);
		Assert.Equal(account, transactionAccount.Account);
	}

	[Fact]
	public void Equals_SameTransactionAccount_ReturnsTrue()
	{
		// Arrange
		Transaction transaction = TransactionGenerator.Generate();
		Account account = AccountGenerator.Generate();

		TransactionAccount transactionAccount1 = new()
		{
			Transaction = transaction,
			Account = account
		};

		TransactionAccount transactionAccount2 = new()
		{
			Transaction = transaction,
			Account = account
		};

		// Act & Assert
		Assert.Equal(transactionAccount1, transactionAccount2);
	}

	[Fact]
	public void Equals_DifferentTransactionAccount_ReturnsFalse()
	{
		// Arrange
		TransactionAccount transactionAccount1 = TransactionAccountGenerator.Generate();
		TransactionAccount transactionAccount2 = TransactionAccountGenerator.Generate();

		// Act & Assert
		Assert.NotEqual(transactionAccount1, transactionAccount2);
	}

	[Fact]
	public void Equals_NullTransactionAccount_ReturnsFalse()
	{
		// Arrange
		TransactionAccount transactionAccount = TransactionAccountGenerator.Generate();

		// Act & Assert
		Assert.False(transactionAccount.Equals(null));
	}

	[Fact]
	public void Equals_NullObject_ReturnsFalse()
	{
		// Arrange
		TransactionAccount transactionAccount = TransactionAccountGenerator.Generate();

		// Act & Assert
		Assert.False(transactionAccount.Equals((object?)null));
	}

	[Fact]
	public void Equals_DifferentType_ReturnsFalse()
	{
		// Arrange
		TransactionAccount transactionAccount = TransactionAccountGenerator.Generate();

		// Act & Assert
		Assert.False(transactionAccount.Equals("not a transaction account"));
	}

	[Fact]
	public void GetHashCode_SameTransactionAccount_ReturnsSameHashCode()
	{
		// Arrange
		Transaction transaction = TransactionGenerator.Generate();
		Account account = AccountGenerator.Generate();

		TransactionAccount transactionAccount1 = new()
		{
			Transaction = transaction,
			Account = account
		};

		TransactionAccount transactionAccount2 = new()
		{
			Transaction = transaction,
			Account = account
		};

		// Act & Assert
		Assert.Equal(transactionAccount1.GetHashCode(), transactionAccount2.GetHashCode());
	}

	[Fact]
	public void GetHashCode_DifferentTransactionAccount_ReturnsDifferentHashCode()
	{
		// Arrange
		TransactionAccount transactionAccount1 = TransactionAccountGenerator.Generate();
		TransactionAccount transactionAccount2 = TransactionAccountGenerator.Generate();

		// Act & Assert
		Assert.NotEqual(transactionAccount1.GetHashCode(), transactionAccount2.GetHashCode());
	}

	[Fact]
	public void OperatorEqual_SameTransactionAccount_ReturnsTrue()
	{
		// Arrange
		TransactionAccount transactionAccount1 = TransactionAccountGenerator.Generate();
		TransactionAccount transactionAccount2 = transactionAccount1;

		// Act
		bool result = transactionAccount1 == transactionAccount2;

		// Assert
		Assert.True(result);
	}

	[Fact]
	public void OperatorEqual_DifferentTransactionAccount_ReturnsFalse()
	{
		// Arrange
		TransactionAccount transactionAccount1 = TransactionAccountGenerator.Generate();
		TransactionAccount transactionAccount2 = TransactionAccountGenerator.Generate();

		// Act
		bool result = transactionAccount1 == transactionAccount2;

		// Assert
		Assert.False(result);
	}

	[Fact]
	public void OperatorNotEqual_SameTransactionAccount_ReturnsFalse()
	{
		// Arrange
		TransactionAccount transactionAccount1 = TransactionAccountGenerator.Generate();
		TransactionAccount transactionAccount2 = transactionAccount1;

		// Act
		bool result = transactionAccount1 != transactionAccount2;

		// Assert
		Assert.False(result);
	}

	[Fact]
	public void OperatorNotEqual_DifferentTransactionAccount_ReturnsTrue()
	{
		// Arrange
		TransactionAccount transactionAccount1 = TransactionAccountGenerator.Generate();
		TransactionAccount transactionAccount2 = TransactionAccountGenerator.Generate();

		// Act
		bool result = transactionAccount1 != transactionAccount2;

		// Assert
		Assert.True(result);
	}
}