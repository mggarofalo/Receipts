using Common;
using Infrastructure.Entities.Core;
using SampleData.Entities;

namespace Infrastructure.Tests.Entities.Core;

public class TransactionEntityTests
{
	[Fact]
	public void Constructor_ValidInput_CreatesTransactionEntity()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		Guid receiptId = Guid.NewGuid();
		Guid accountId = Guid.NewGuid();
		decimal amount = 100.50m;
		Currency currency = Currency.USD;
		DateOnly date = DateOnly.FromDateTime(DateTime.Today);

		// Act
		TransactionEntity transaction = new()
		{
			Id = id,
			ReceiptId = receiptId,
			AccountId = accountId,
			Amount = amount,
			AmountCurrency = currency,
			Date = date
		};

		// Assert
		Assert.Equal(id, transaction.Id);
		Assert.Equal(receiptId, transaction.ReceiptId);
		Assert.Equal(accountId, transaction.AccountId);
		Assert.Equal(amount, transaction.Amount);
		Assert.Equal(currency, transaction.AmountCurrency);
		Assert.Equal(date, transaction.Date);
	}

	[Fact]
	public async Task VirtualReceiptEntity_IsNavigable()
	{
		// Arrange
		ApplicationDbContext context = DbContextHelpers.CreateInMemoryContext();
		AccountEntity account = AccountEntityGenerator.Generate();
		ReceiptEntity receipt = ReceiptEntityGenerator.Generate();
		TransactionEntity transaction = TransactionEntityGenerator.Generate(receipt.Id, account.Id);

		await context.Accounts.AddAsync(account);
		await context.Receipts.AddAsync(receipt);
		await context.Transactions.AddAsync(transaction);
		await context.SaveChangesAsync(CancellationToken.None);

		ReceiptEntity? loadedReceipt = await context.Receipts.FindAsync(receipt.Id);
		TransactionEntity? loadedTransaction = await context.Transactions.FindAsync(transaction.Id);

		// Act & Assert
		Assert.NotNull(loadedReceipt);
		Assert.NotNull(loadedTransaction);
		Assert.NotNull(loadedTransaction.Receipt);
		Assert.Equal(loadedReceipt, loadedTransaction.Receipt);
	}

	[Fact]
	public async Task VirtualAccountEntity_IsNavigable()
	{
		// Arrange
		ApplicationDbContext context = DbContextHelpers.CreateInMemoryContext();
		AccountEntity account = AccountEntityGenerator.Generate();
		ReceiptEntity receipt = ReceiptEntityGenerator.Generate();
		TransactionEntity transaction = TransactionEntityGenerator.Generate(receipt.Id, account.Id);

		await context.Accounts.AddAsync(account);
		await context.Receipts.AddAsync(receipt);
		await context.Transactions.AddAsync(transaction);
		await context.SaveChangesAsync(CancellationToken.None);

		AccountEntity? loadedAccount = await context.Accounts.FindAsync(account.Id);
		TransactionEntity? loadedTransaction = await context.Transactions.FindAsync(transaction.Id);

		// Act & Assert
		Assert.NotNull(loadedAccount);
		Assert.NotNull(loadedTransaction);
		Assert.NotNull(loadedTransaction.Account);
		Assert.Equal(loadedAccount, loadedTransaction.Account);
	}

	[Fact]
	public void Equals_SameTransactionEntity_ReturnsTrue()
	{
		// Arrange
		TransactionEntity transaction1 = TransactionEntityGenerator.Generate();
		TransactionEntity transaction2 = new()
		{
			Id = transaction1.Id,
			ReceiptId = transaction1.ReceiptId,
			AccountId = transaction1.AccountId,
			Amount = transaction1.Amount,
			AmountCurrency = transaction1.AmountCurrency,
			Date = transaction1.Date
		};

		// Act & Assert
		Assert.Equal(transaction1, transaction2);
	}

	[Fact]
	public void Equals_DifferentTransactionEntity_ReturnsFalse()
	{
		// Arrange
		TransactionEntity transaction1 = TransactionEntityGenerator.Generate();
		TransactionEntity transaction2 = TransactionEntityGenerator.Generate();

		// Act & Assert
		Assert.NotEqual(transaction1, transaction2);
	}

	[Fact]
	public void Equals_NullTransactionEntity_ReturnsFalse()
	{
		// Arrange
		TransactionEntity transaction = TransactionEntityGenerator.Generate();

		// Act & Assert
		Assert.False(transaction.Equals(null));
	}

	[Fact]
	public void Equals_DifferentType_ReturnsFalse()
	{
		// Arrange
		TransactionEntity transaction = TransactionEntityGenerator.Generate();

		// Act & Assert
		Assert.False(transaction.Equals("not a transaction entity"));
	}

	[Fact]
	public void Equals_NullObject_ReturnsFalse()
	{
		// Arrange
		TransactionEntity transaction = TransactionEntityGenerator.Generate();

		// Act & Assert
		Assert.False(transaction.Equals((object?)null));
	}

	[Fact]
	public void GetHashCode_SameTransactionEntity_ReturnsSameHashCode()
	{
		// Arrange
		TransactionEntity transaction1 = TransactionEntityGenerator.Generate();
		TransactionEntity transaction2 = new()
		{
			Id = transaction1.Id,
			ReceiptId = transaction1.ReceiptId,
			AccountId = transaction1.AccountId,
			Amount = transaction1.Amount,
			AmountCurrency = transaction1.AmountCurrency,
			Date = transaction1.Date
		};

		// Act & Assert
		Assert.Equal(transaction1.GetHashCode(), transaction2.GetHashCode());
	}

	[Fact]
	public void GetHashCode_DifferentTransactionEntity_ReturnsDifferentHashCode()
	{
		// Arrange
		TransactionEntity transaction1 = TransactionEntityGenerator.Generate();
		TransactionEntity transaction2 = TransactionEntityGenerator.Generate();

		// Act & Assert
		Assert.NotEqual(transaction1.GetHashCode(), transaction2.GetHashCode());
	}

	[Fact]
	public void OperatorEqual_SameTransactionEntity_ReturnsTrue()
	{
		// Arrange
		TransactionEntity transaction1 = TransactionEntityGenerator.Generate();
		TransactionEntity transaction2 = transaction1;

		// Act
		bool result = transaction1 == transaction2;

		// Assert
		Assert.True(result);
	}

	[Fact]
	public void OperatorEqual_DifferentTransactionEntity_ReturnsFalse()
	{
		// Arrange
		TransactionEntity transaction1 = TransactionEntityGenerator.Generate();
		TransactionEntity transaction2 = TransactionEntityGenerator.Generate();

		// Act
		bool result = transaction1 == transaction2;

		// Assert
		Assert.False(result);
	}

	[Fact]
	public void OperatorNotEqual_SameTransactionEntity_ReturnsFalse()
	{
		// Arrange
		TransactionEntity transaction1 = TransactionEntityGenerator.Generate();
		TransactionEntity transaction2 = transaction1;

		// Act
		bool result = transaction1 != transaction2;

		// Assert
		Assert.False(result);
	}

	[Fact]
	public void OperatorNotEqual_DifferentTransactionEntity_ReturnsTrue()
	{
		// Arrange
		TransactionEntity transaction1 = TransactionEntityGenerator.Generate();
		TransactionEntity transaction2 = TransactionEntityGenerator.Generate();

		// Act
		bool result = transaction1 != transaction2;

		// Assert
		Assert.True(result);
	}
}