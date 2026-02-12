using Common;
using Infrastructure.Entities.Core;
using Infrastructure.Tests.Repositories;
using Microsoft.EntityFrameworkCore;
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
		IDbContextFactory<ApplicationDbContext> contextFactory = DbContextHelpers.CreateInMemoryContextFactory();
		using ApplicationDbContext context = contextFactory.CreateDbContext();
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

		contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task VirtualAccountEntity_IsNavigable()
	{
		// Arrange
		IDbContextFactory<ApplicationDbContext> contextFactory = DbContextHelpers.CreateInMemoryContextFactory();
		using ApplicationDbContext context = contextFactory.CreateDbContext();
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

		contextFactory.ResetDatabase();
	}
}
