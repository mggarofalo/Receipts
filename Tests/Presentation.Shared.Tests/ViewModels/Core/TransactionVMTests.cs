using SampleData.ViewModels.Core;
using Shared.ViewModels.Core;

namespace Presentation.Shared.Tests.ViewModels.Core;

public class TransactionVMTests
{
	[Fact]
	public void Constructor_ValidInput_CreatesTransactionVM()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		Guid receiptId = Guid.NewGuid();
		Guid accountId = Guid.NewGuid();
		decimal amount = 100.0m;
		DateOnly date = DateOnly.FromDateTime(DateTime.Now);

		// Act
		TransactionVM transactionVM = new()
		{
			Id = id,
			ReceiptId = receiptId,
			AccountId = accountId,
			Amount = amount,
			Date = date
		};

		// Assert
		Assert.Equal(id, transactionVM.Id);
		Assert.Equal(receiptId, transactionVM.ReceiptId);
		Assert.Equal(accountId, transactionVM.AccountId);
		Assert.Equal(amount, transactionVM.Amount);
		Assert.Equal(date, transactionVM.Date);
	}

	[Fact]
	public void Constructor_NullId_CreatesTransactionVMWithNullId()
	{
		// Arrange
		Guid receiptId = Guid.NewGuid();
		Guid accountId = Guid.NewGuid();
		decimal amount = 100.0m;
		DateOnly date = DateOnly.FromDateTime(DateTime.Now);

		// Act
		TransactionVM transactionVM = new()
		{
			ReceiptId = receiptId,
			AccountId = accountId,
			Amount = amount,
			Date = date
		};

		// Assert
		Assert.Null(transactionVM.Id);
	}

	[Fact]
	public void Equals_SameTransactionVM_ReturnsTrue()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		Guid receiptId = Guid.NewGuid();
		Guid accountId = Guid.NewGuid();
		decimal amount = 100.0m;
		DateOnly date = DateOnly.FromDateTime(DateTime.Now);

		TransactionVM transactionVM1 = new()
		{
			Id = id,
			ReceiptId = receiptId,
			AccountId = accountId,
			Amount = amount,
			Date = date
		};
		TransactionVM transactionVM2 = new()
		{
			Id = id,
			ReceiptId = receiptId,
			AccountId = accountId,
			Amount = amount,
			Date = date
		};

		// Act & Assert
		Assert.True(transactionVM1 == transactionVM2);
		Assert.False(transactionVM1 != transactionVM2);
		Assert.True(transactionVM1.Equals(transactionVM2));
	}

	[Fact]
	public void Equals_DifferentTransactionVM_ReturnsFalse()
	{
		// Arrange
		TransactionVM transactionVM1 = TransactionVMGenerator.Generate();
		TransactionVM transactionVM2 = TransactionVMGenerator.Generate();

		// Act & Assert
		Assert.False(transactionVM1 == transactionVM2);
		Assert.True(transactionVM1 != transactionVM2);
		Assert.False(transactionVM1.Equals(transactionVM2));
	}

	[Fact]
	public void Equals_NullTransactionVM_ReturnsFalse()
	{
		// Arrange
		TransactionVM transactionVM = TransactionVMGenerator.Generate();

		// Act & Assert
		Assert.False(transactionVM.Equals(null));
	}

	[Fact]
	public void Equals_NullObject_ReturnsFalse()
	{
		// Arrange
		TransactionVM transactionVM = TransactionVMGenerator.Generate();

		// Act & Assert
		Assert.False(transactionVM.Equals((object?)null));
	}

	[Fact]
	public void Equals_DifferentType_ReturnsFalse()
	{
		// Arrange
		TransactionVM transactionVM = TransactionVMGenerator.Generate();

		// Act & Assert
		Assert.False(transactionVM.Equals("not a transactionVM"));
	}

	[Fact]
	public void GetHashCode_SameTransactionVM_ReturnsSameHashCode()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		Guid receiptId = Guid.NewGuid();
		Guid accountId = Guid.NewGuid();
		decimal amount = 100.0m;
		DateOnly date = DateOnly.FromDateTime(DateTime.Now);

		TransactionVM transactionVM1 = new()
		{
			Id = id,
			ReceiptId = receiptId,
			AccountId = accountId,
			Amount = amount,
			Date = date
		};
		TransactionVM transactionVM2 = new()
		{
			Id = id,
			ReceiptId = receiptId,
			AccountId = accountId,
			Amount = amount,
			Date = date
		};

		// Act & Assert
		Assert.Equal(transactionVM1.GetHashCode(), transactionVM2.GetHashCode());
	}

	[Fact]
	public void GetHashCode_NullId_ReturnsSameHashCode()
	{
		// Arrange
		Guid receiptId = Guid.NewGuid();
		Guid accountId = Guid.NewGuid();
		TransactionVM transactionVM1 = new()
		{
			ReceiptId = receiptId,
			AccountId = accountId,
			Amount = 100.0m,
			Date = DateOnly.FromDateTime(DateTime.Now)
		};
		TransactionVM transactionVM2 = new()
		{
			ReceiptId = receiptId,
			AccountId = accountId,
			Amount = 100.0m,
			Date = DateOnly.FromDateTime(DateTime.Now)
		};

		// Act & Assert
		Assert.Equal(transactionVM1.GetHashCode(), transactionVM2.GetHashCode());
	}

	[Fact]
	public void GetHashCode_DifferentTransactionVM_ReturnsDifferentHashCode()
	{
		// Arrange
		Guid id1 = Guid.NewGuid();
		Guid id2 = Guid.NewGuid();
		Guid receiptId = Guid.NewGuid();
		Guid accountId = Guid.NewGuid();
		decimal amount = 100.0m;
		DateOnly date = DateOnly.FromDateTime(DateTime.Now);

		TransactionVM transactionVM1 = new()
		{
			Id = id1,
			ReceiptId = receiptId,
			AccountId = accountId,
			Amount = amount,
			Date = date
		};
		TransactionVM transactionVM2 = new()
		{
			Id = id2,
			ReceiptId = receiptId,
			AccountId = accountId,
			Amount = amount,
			Date = date
		};

		// Act & Assert
		Assert.NotEqual(transactionVM1.GetHashCode(), transactionVM2.GetHashCode());
	}
}
