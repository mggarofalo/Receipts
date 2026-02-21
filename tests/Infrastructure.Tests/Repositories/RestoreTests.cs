using FluentAssertions;
using Infrastructure.Entities.Core;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using SampleData.Entities;

namespace Infrastructure.Tests.Repositories;

public class RestoreTests
{
	[Fact]
	public async Task RestoreAsync_SoftDeletedEntity_ClearsDeletedAt()
	{
		// Arrange
		IDbContextFactory<ApplicationDbContext> contextFactory = DbContextHelpers.CreateInMemoryContextFactory();
		AccountEntity entity = AccountEntityGenerator.Generate();

		using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			await context.Accounts.AddAsync(entity);
			await context.SaveChangesAsync();
		}

		// Soft delete the entity
		using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			AccountEntity account = await context.Accounts.FirstAsync();
			context.Accounts.Remove(account);
			await context.SaveChangesAsync();
		}

		AccountRepository repository = new(contextFactory);

		// Act
		bool result = await repository.RestoreAsync(entity.Id, CancellationToken.None);

		// Assert
		Assert.True(result);

		using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			AccountEntity? restored = await context.Accounts.IgnoreQueryFilters().FirstOrDefaultAsync(a => a.Id == entity.Id);
			restored.Should().NotBeNull();
			restored!.DeletedAt.Should().BeNull();
		}

		contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task RestoreAsync_SoftDeletedEntity_BecomesVisibleInNormalQueries()
	{
		// Arrange
		IDbContextFactory<ApplicationDbContext> contextFactory = DbContextHelpers.CreateInMemoryContextFactory();
		AccountEntity entity = AccountEntityGenerator.Generate();

		using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			await context.Accounts.AddAsync(entity);
			await context.SaveChangesAsync();
		}

		// Soft delete the entity
		using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			AccountEntity account = await context.Accounts.FirstAsync();
			context.Accounts.Remove(account);
			await context.SaveChangesAsync();
		}

		AccountRepository repository = new(contextFactory);

		// Act
		await repository.RestoreAsync(entity.Id, CancellationToken.None);

		// Assert
		using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			List<AccountEntity> visibleAccounts = await context.Accounts.ToListAsync();
			visibleAccounts.Should().HaveCount(1);
			visibleAccounts[0].Id.Should().Be(entity.Id);
		}

		contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task RestoreAsync_NonExistentEntity_ReturnsFalse()
	{
		// Arrange
		IDbContextFactory<ApplicationDbContext> contextFactory = DbContextHelpers.CreateInMemoryContextFactory();
		AccountRepository repository = new(contextFactory);

		// Act
		bool result = await repository.RestoreAsync(Guid.NewGuid(), CancellationToken.None);

		// Assert
		Assert.False(result);

		contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task RestoreAsync_NotDeletedEntity_ReturnsFalse()
	{
		// Arrange
		IDbContextFactory<ApplicationDbContext> contextFactory = DbContextHelpers.CreateInMemoryContextFactory();
		AccountEntity entity = AccountEntityGenerator.Generate();

		using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			await context.Accounts.AddAsync(entity);
			await context.SaveChangesAsync();
		}

		AccountRepository repository = new(contextFactory);

		// Act - entity exists but is not deleted
		bool result = await repository.RestoreAsync(entity.Id, CancellationToken.None);

		// Assert
		Assert.False(result);

		contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task RestoreAsync_Receipt_CascadeRestoresReceiptItems()
	{
		// Arrange
		IDbContextFactory<ApplicationDbContext> contextFactory = DbContextHelpers.CreateInMemoryContextFactory();
		ReceiptEntity receipt = ReceiptEntityGenerator.Generate();

		using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			await context.Receipts.AddAsync(receipt);
			ReceiptItemEntity item1 = ReceiptItemEntityGenerator.Generate(receipt.Id);
			ReceiptItemEntity item2 = ReceiptItemEntityGenerator.Generate(receipt.Id);
			await context.ReceiptItems.AddRangeAsync(item1, item2);
			await context.SaveChangesAsync();
		}

		// Soft delete receipt and its items
		using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			ReceiptEntity receiptToDelete = await context.Receipts.FirstAsync();
			await context.ReceiptItems.Where(i => i.ReceiptId == receiptToDelete.Id).LoadAsync();
			context.Receipts.Remove(receiptToDelete);
			await context.SaveChangesAsync();
		}

		ReceiptRepository repository = new(contextFactory);

		// Act
		bool result = await repository.RestoreAsync(receipt.Id, CancellationToken.None);

		// Assert
		Assert.True(result);

		using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			List<ReceiptItemEntity> items = await context.ReceiptItems.ToListAsync();
			items.Should().HaveCount(2);
			items.Should().AllSatisfy(i => i.DeletedAt.Should().BeNull());
		}

		contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task RestoreAsync_Receipt_CascadeRestoresTransactions()
	{
		// Arrange
		IDbContextFactory<ApplicationDbContext> contextFactory = DbContextHelpers.CreateInMemoryContextFactory();
		AccountEntity account = AccountEntityGenerator.Generate();
		ReceiptEntity receipt = ReceiptEntityGenerator.Generate();

		using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			await context.Accounts.AddAsync(account);
			await context.Receipts.AddAsync(receipt);
			TransactionEntity transaction1 = TransactionEntityGenerator.Generate(receiptId: receipt.Id, accountId: account.Id);
			TransactionEntity transaction2 = TransactionEntityGenerator.Generate(receiptId: receipt.Id, accountId: account.Id);
			await context.Transactions.AddRangeAsync(transaction1, transaction2);
			await context.SaveChangesAsync();
		}

		// Soft delete receipt and its transactions
		using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			ReceiptEntity receiptToDelete = await context.Receipts.FirstAsync();
			await context.Transactions.Where(t => t.ReceiptId == receiptToDelete.Id).LoadAsync();
			context.Receipts.Remove(receiptToDelete);
			await context.SaveChangesAsync();
		}

		ReceiptRepository repository = new(contextFactory);

		// Act
		bool result = await repository.RestoreAsync(receipt.Id, CancellationToken.None);

		// Assert
		Assert.True(result);

		using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			List<TransactionEntity> transactions = await context.Transactions.IgnoreQueryFilters().Where(t => t.ReceiptId == receipt.Id).ToListAsync();
			transactions.Should().HaveCount(2);
			transactions.Should().AllSatisfy(t => t.DeletedAt.Should().BeNull());
		}

		contextFactory.ResetDatabase();
	}
}
