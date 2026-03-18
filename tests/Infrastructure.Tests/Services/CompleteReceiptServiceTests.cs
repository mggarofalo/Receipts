using Domain.Core;
using FluentAssertions;
using Infrastructure.Mapping;
using Infrastructure.Services;
using Infrastructure.Tests.Helpers;
using Infrastructure.Tests.Repositories;
using Microsoft.EntityFrameworkCore;

using SampleData.Domain.Core;

namespace Infrastructure.Tests.Services;

public class CompleteReceiptServiceTests
{
	[Fact]
	public async Task CreateCompleteReceiptAsync_PersistsAllEntitiesAtomically()
	{
		// Arrange
		(IDbContextFactory<ApplicationDbContext> contextFactory, MockCurrentUserAccessor _) =
			DbContextWithUserHelpers.CreateInMemoryContextFactoryWithUser();

		CompleteReceiptService service = new(
			contextFactory,
			new ReceiptMapper(),
			new TransactionMapper(),
			new ReceiptItemMapper());

		Receipt receipt = ReceiptGenerator.Generate();
		receipt.Id = Guid.Empty; // simulate new

		List<Transaction> transactions = TransactionGenerator.GenerateList(2);
		foreach (Transaction t in transactions)
		{
			t.Id = Guid.Empty;
			t.AccountId = Guid.NewGuid();
		}

		List<ReceiptItem> items = ReceiptItemGenerator.GenerateList(2);
		foreach (ReceiptItem i in items)
		{
			i.Id = Guid.Empty;
		}

		// Act
		Application.Commands.Receipt.CreateComplete.CreateCompleteReceiptResult result =
			await service.CreateCompleteReceiptAsync(receipt, transactions, items, CancellationToken.None);

		// Assert
		result.Receipt.Id.Should().NotBe(Guid.Empty);
		result.Transactions.Should().HaveCount(2);
		result.Items.Should().HaveCount(2);

		// Verify all transactions have the receipt's ID
		foreach (Transaction t in result.Transactions)
		{
			t.ReceiptId.Should().Be(result.Receipt.Id);
		}

		// Verify all items have the receipt's ID
		foreach (ReceiptItem i in result.Items)
		{
			i.ReceiptId.Should().Be(result.Receipt.Id);
		}

		// Verify persistence
		await using ApplicationDbContext context = contextFactory.CreateDbContext();
		(await context.Receipts.CountAsync()).Should().Be(1);
		(await context.Transactions.CountAsync()).Should().Be(2);
		(await context.ReceiptItems.CountAsync()).Should().Be(2);

		contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task CreateCompleteReceiptAsync_WithEmptyLists_PersistsOnlyReceipt()
	{
		// Arrange
		(IDbContextFactory<ApplicationDbContext> contextFactory, MockCurrentUserAccessor _) =
			DbContextWithUserHelpers.CreateInMemoryContextFactoryWithUser();

		CompleteReceiptService service = new(
			contextFactory,
			new ReceiptMapper(),
			new TransactionMapper(),
			new ReceiptItemMapper());

		Receipt receipt = ReceiptGenerator.Generate();
		receipt.Id = Guid.Empty;

		// Act
		Application.Commands.Receipt.CreateComplete.CreateCompleteReceiptResult result =
			await service.CreateCompleteReceiptAsync(receipt, [], [], CancellationToken.None);

		// Assert
		result.Receipt.Id.Should().NotBe(Guid.Empty);
		result.Transactions.Should().BeEmpty();
		result.Items.Should().BeEmpty();

		await using ApplicationDbContext context = contextFactory.CreateDbContext();
		(await context.Receipts.CountAsync()).Should().Be(1);
		(await context.Transactions.CountAsync()).Should().Be(0);
		(await context.ReceiptItems.CountAsync()).Should().Be(0);

		contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task CreateCompleteReceiptAsync_PreGeneratesGuidForNewReceipt()
	{
		// Arrange
		(IDbContextFactory<ApplicationDbContext> contextFactory, MockCurrentUserAccessor _) =
			DbContextWithUserHelpers.CreateInMemoryContextFactoryWithUser();

		CompleteReceiptService service = new(
			contextFactory,
			new ReceiptMapper(),
			new TransactionMapper(),
			new ReceiptItemMapper());

		Receipt receipt = ReceiptGenerator.Generate();
		receipt.Id = Guid.Empty;

		// Act
		Application.Commands.Receipt.CreateComplete.CreateCompleteReceiptResult result =
			await service.CreateCompleteReceiptAsync(receipt, [], [], CancellationToken.None);

		// Assert — the receipt should have been assigned a non-empty GUID
		result.Receipt.Id.Should().NotBe(Guid.Empty);

		contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task CreateCompleteReceiptAsync_PropagatesAccountIdToTransactions()
	{
		// Arrange
		(IDbContextFactory<ApplicationDbContext> contextFactory, MockCurrentUserAccessor _) =
			DbContextWithUserHelpers.CreateInMemoryContextFactoryWithUser();

		CompleteReceiptService service = new(
			contextFactory,
			new ReceiptMapper(),
			new TransactionMapper(),
			new ReceiptItemMapper());

		Receipt receipt = ReceiptGenerator.Generate();
		receipt.Id = Guid.Empty;

		Guid accountId = Guid.NewGuid();
		Transaction txn = TransactionGenerator.Generate();
		txn.Id = Guid.Empty;
		txn.AccountId = accountId;

		// Act
		Application.Commands.Receipt.CreateComplete.CreateCompleteReceiptResult result =
			await service.CreateCompleteReceiptAsync(receipt, [txn], [], CancellationToken.None);

		// Assert
		result.Transactions.Should().HaveCount(1);
		result.Transactions[0].AccountId.Should().Be(accountId);

		contextFactory.ResetDatabase();
	}
}
