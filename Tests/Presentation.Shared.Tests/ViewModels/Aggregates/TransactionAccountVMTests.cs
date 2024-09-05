using SampleData.ViewModels.Aggregates;
using SampleData.ViewModels.Core;
using Shared.ViewModels.Aggregates;
using Shared.ViewModels.Core;

namespace Presentation.Shared.Tests.ViewModels.Aggregates;

public class TransactionAccountVMTests
{
	[Fact]
	public void Constructor_ValidInput_CreatesTransactionAccountVM()
	{
		// Arrange
		AccountVM account = AccountVMGenerator.Generate();
		TransactionVM transaction = TransactionVMGenerator.Generate(accountId: account.Id);

		// Act
		TransactionAccountVM transactionAccountVM = new()
		{
			Transaction = transaction,
			Account = account
		};

		// Assert
		Assert.Equal(transaction, transactionAccountVM.Transaction);
		Assert.Equal(account, transactionAccountVM.Account);
	}

	[Fact]
	public void Equals_SameTransactionAccountVM_ReturnsTrue()
	{
		// Arrange
		AccountVM account = AccountVMGenerator.Generate();
		TransactionVM transaction = TransactionVMGenerator.Generate(accountId: account.Id);

		TransactionAccountVM transactionAccountVM1 = new()
		{
			Transaction = transaction,
			Account = account
		};

		TransactionAccountVM transactionAccountVM2 = new()
		{
			Transaction = transaction,
			Account = account
		};

		// Act & Assert
		Assert.True(transactionAccountVM1 == transactionAccountVM2);
		Assert.False(transactionAccountVM1 != transactionAccountVM2);
		Assert.True(transactionAccountVM1.Equals(transactionAccountVM2));
	}

	[Fact]
	public void Equals_DifferentTransactionAccountVM_ReturnsFalse()
	{
		// Arrange
		TransactionAccountVM transactionAccountVM1 = TransactionAccountVMGenerator.Generate();
		TransactionAccountVM transactionAccountVM2 = TransactionAccountVMGenerator.Generate();

		// Act & Assert
		Assert.False(transactionAccountVM1 == transactionAccountVM2);
		Assert.True(transactionAccountVM1 != transactionAccountVM2);
		Assert.False(transactionAccountVM1.Equals(transactionAccountVM2));
	}

	[Fact]
	public void Equals_NullTransactionAccountVM_ReturnsFalse()
	{
		// Arrange
		TransactionAccountVM transactionAccountVM = TransactionAccountVMGenerator.Generate();

		// Act & Assert
		Assert.False(transactionAccountVM.Equals(null));
	}

	[Fact]
	public void Equals_NullObject_ReturnsFalse()
	{
		// Arrange
		TransactionAccountVM transactionAccountVM = TransactionAccountVMGenerator.Generate();

		// Act & Assert
		Assert.False(transactionAccountVM.Equals((object?)null));
	}

	[Fact]
	public void Equals_DifferentType_ReturnsFalse()
	{
		// Arrange
		TransactionAccountVM transactionAccountVM = TransactionAccountVMGenerator.Generate();

		// Act & Assert
		Assert.False(transactionAccountVM.Equals("not a transaction account VM"));
	}

	[Fact]
	public void GetHashCode_SameTransactionAccountVM_ReturnsSameHashCode()
	{
		// Arrange
		AccountVM account = AccountVMGenerator.Generate();
		TransactionVM transaction = TransactionVMGenerator.Generate(accountId: account.Id);

		TransactionAccountVM transactionAccountVM1 = new()
		{
			Transaction = transaction,
			Account = account
		};

		TransactionAccountVM transactionAccountVM2 = new()
		{
			Transaction = transaction,
			Account = account
		};

		// Act & Assert
		Assert.Equal(transactionAccountVM1.GetHashCode(), transactionAccountVM2.GetHashCode());
	}

	[Fact]
	public void GetHashCode_DifferentTransactionAccountVM_ReturnsDifferentHashCode()
	{
		// Arrange
		TransactionAccountVM transactionAccountVM1 = TransactionAccountVMGenerator.Generate();
		TransactionAccountVM transactionAccountVM2 = TransactionAccountVMGenerator.Generate();

		// Act & Assert
		Assert.NotEqual(transactionAccountVM1.GetHashCode(), transactionAccountVM2.GetHashCode());
	}
}