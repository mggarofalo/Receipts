using FluentAssertions;
using Infrastructure.Entities.Core;
using Infrastructure.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using SampleData.Entities;

namespace Infrastructure.Tests.Repositories;

public class SoftDeleteTests
{
	[Fact]
	public async Task SoftDelete_Account_SetsDeletedAtOnDelete()
	{
		// Arrange
		IDbContextFactory<ApplicationDbContext> contextFactory = DbContextHelpers.CreateInMemoryContextFactory();
		AccountEntity entity = AccountEntityGenerator.Generate();

		using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			await context.Accounts.AddAsync(entity);
			await context.SaveChangesAsync();
		}

		// Act
		using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			AccountEntity account = await context.Accounts.FirstAsync();
			context.Accounts.Remove(account);
			await context.SaveChangesAsync();
		}

		// Assert
		using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			List<AccountEntity> allAccounts = await context.Accounts.IgnoreQueryFilters().ToListAsync();
			allAccounts.Should().HaveCount(1);
			allAccounts[0].DeletedAt.Should().NotBeNull();
		}

		contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task SoftDelete_Account_ExcludedFromNormalQueries()
	{
		// Arrange
		IDbContextFactory<ApplicationDbContext> contextFactory = DbContextHelpers.CreateInMemoryContextFactory();
		AccountEntity entity = AccountEntityGenerator.Generate();

		using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			await context.Accounts.AddAsync(entity);
			await context.SaveChangesAsync();
		}

		// Act
		using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			AccountEntity account = await context.Accounts.FirstAsync();
			context.Accounts.Remove(account);
			await context.SaveChangesAsync();
		}

		// Assert
		using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			List<AccountEntity> visibleAccounts = await context.Accounts.ToListAsync();
			visibleAccounts.Should().BeEmpty();
		}

		contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task SoftDelete_Receipt_CascadesToReceiptItems()
	{
		// Arrange
		IDbContextFactory<ApplicationDbContext> contextFactory = DbContextHelpers.CreateInMemoryContextFactory();

		using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			ReceiptEntity receipt = ReceiptEntityGenerator.Generate();
			await context.Receipts.AddAsync(receipt);
			ReceiptItemEntity item1 = ReceiptItemEntityGenerator.Generate(receipt.Id);
			ReceiptItemEntity item2 = ReceiptItemEntityGenerator.Generate(receipt.Id);
			await context.ReceiptItems.AddRangeAsync(item1, item2);
			await context.SaveChangesAsync();
		}

		// Act - delete the receipt (need to load items into context for cascade)
		using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			ReceiptEntity receipt = await context.Receipts.FirstAsync();
			// Load related items into tracker
			await context.ReceiptItems.Where(i => i.ReceiptId == receipt.Id).LoadAsync();
			context.Receipts.Remove(receipt);
			await context.SaveChangesAsync();
		}

		// Assert
		using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			List<ReceiptItemEntity> allItems = await context.ReceiptItems.IgnoreQueryFilters().ToListAsync();
			allItems.Should().HaveCount(2);
			allItems.Should().AllSatisfy(i => i.DeletedAt.Should().NotBeNull());
		}

		contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task SoftDelete_Receipt_CascadesToTransactions()
	{
		// Arrange
		IDbContextFactory<ApplicationDbContext> contextFactory = DbContextHelpers.CreateInMemoryContextFactory();
		AccountEntity account = AccountEntityGenerator.Generate();

		using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			await context.Accounts.AddAsync(account);
			ReceiptEntity receipt = ReceiptEntityGenerator.Generate();
			await context.Receipts.AddAsync(receipt);
			TransactionEntity transaction1 = TransactionEntityGenerator.Generate(receiptId: receipt.Id, accountId: account.Id);
			TransactionEntity transaction2 = TransactionEntityGenerator.Generate(receiptId: receipt.Id, accountId: account.Id);
			await context.Transactions.AddRangeAsync(transaction1, transaction2);
			await context.SaveChangesAsync();
		}

		// Act - delete the receipt (need to load transactions into context for cascade)
		using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			ReceiptEntity receipt = await context.Receipts.FirstAsync();
			// Load related transactions into tracker
			await context.Transactions.Where(t => t.ReceiptId == receipt.Id).LoadAsync();
			context.Receipts.Remove(receipt);
			await context.SaveChangesAsync();
		}

		// Assert
		using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			List<TransactionEntity> allTransactions = await context.Transactions.IgnoreQueryFilters().ToListAsync();
			allTransactions.Should().HaveCount(2);
			allTransactions.Should().AllSatisfy(t => t.DeletedAt.Should().NotBeNull());
		}

		contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task SoftDelete_Account_DoesNotCascade()
	{
		// Arrange
		IDbContextFactory<ApplicationDbContext> contextFactory = DbContextHelpers.CreateInMemoryContextFactory();
		AccountEntity account = AccountEntityGenerator.Generate();
		Guid receiptId = Guid.NewGuid();

		using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			await context.Accounts.AddAsync(account);
			ReceiptEntity receipt = ReceiptEntityGenerator.Generate();
			receipt.Id = receiptId;
			await context.Receipts.AddAsync(receipt);
			TransactionEntity transaction = TransactionEntityGenerator.Generate(receiptId: receiptId, accountId: account.Id);
			await context.Transactions.AddAsync(transaction);
			await context.SaveChangesAsync();
		}

		// Act
		using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			AccountEntity accountToDelete = await context.Accounts.FirstAsync();
			context.Accounts.Remove(accountToDelete);
			await context.SaveChangesAsync();
		}

		// Assert - receipt and transactions should not be soft-deleted
		using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			List<ReceiptEntity> receipts = await context.Receipts.IgnoreQueryFilters().ToListAsync();
			receipts.Should().HaveCount(1);
			receipts[0].DeletedAt.Should().BeNull();
		}

		contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task SoftDelete_SetsDeletedByUserId()
	{
		// Arrange
		(IDbContextFactory<ApplicationDbContext> contextFactory, MockCurrentUserAccessor accessor) = DbContextWithUserHelpers.CreateInMemoryContextFactoryWithUser();
		accessor.UserId = "test-user-id";

		using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			AccountEntity entity = AccountEntityGenerator.Generate();
			await context.Accounts.AddAsync(entity);
			await context.SaveChangesAsync();
		}

		// Act
		using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			AccountEntity account = await context.Accounts.FirstAsync();
			context.Accounts.Remove(account);
			await context.SaveChangesAsync();
		}

		// Assert
		using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			List<AccountEntity> allAccounts = await context.Accounts.IgnoreQueryFilters().ToListAsync();
			allAccounts.Should().HaveCount(1);
			allAccounts[0].DeletedByUserId.Should().Be("test-user-id");
		}

		contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task SoftDelete_SetsDeletedByApiKeyId()
	{
		// Arrange
		(IDbContextFactory<ApplicationDbContext> contextFactory, MockCurrentUserAccessor accessor) = DbContextWithUserHelpers.CreateInMemoryContextFactoryWithUser();
		Guid apiKeyId = Guid.NewGuid();
		accessor.ApiKeyId = apiKeyId;

		using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			AccountEntity entity = AccountEntityGenerator.Generate();
			await context.Accounts.AddAsync(entity);
			await context.SaveChangesAsync();
		}

		// Act
		using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			AccountEntity account = await context.Accounts.FirstAsync();
			context.Accounts.Remove(account);
			await context.SaveChangesAsync();
		}

		// Assert
		using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			List<AccountEntity> allAccounts = await context.Accounts.IgnoreQueryFilters().ToListAsync();
			allAccounts.Should().HaveCount(1);
			allAccounts[0].DeletedByApiKeyId.Should().Be(apiKeyId);
		}

		contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task OnlyDeleted_ReturnsOnlySoftDeletedEntities()
	{
		// Arrange
		IDbContextFactory<ApplicationDbContext> contextFactory = DbContextHelpers.CreateInMemoryContextFactory();
		List<AccountEntity> entities = AccountEntityGenerator.GenerateList(3);

		using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			await context.Accounts.AddRangeAsync(entities);
			await context.SaveChangesAsync();
		}

		// Delete only the first entity
		using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			AccountEntity accountToDelete = await context.Accounts.FirstAsync(a => a.Id == entities[0].Id);
			context.Accounts.Remove(accountToDelete);
			await context.SaveChangesAsync();
		}

		// Act
		using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			List<AccountEntity> onlyDeleted = await context.Accounts
				.IgnoreQueryFilters()
				.Where(e => e.DeletedAt != null)
				.ToListAsync();

			// Assert
			onlyDeleted.Should().HaveCount(1);
			onlyDeleted[0].Id.Should().Be(entities[0].Id);
		}

		contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task IncludeDeleted_ReturnsBothActiveAndDeleted()
	{
		// Arrange
		IDbContextFactory<ApplicationDbContext> contextFactory = DbContextHelpers.CreateInMemoryContextFactory();
		List<AccountEntity> entities = AccountEntityGenerator.GenerateList(3);

		using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			await context.Accounts.AddRangeAsync(entities);
			await context.SaveChangesAsync();
		}

		// Delete one entity
		using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			AccountEntity accountToDelete = await context.Accounts.FirstAsync(a => a.Id == entities[0].Id);
			context.Accounts.Remove(accountToDelete);
			await context.SaveChangesAsync();
		}

		// Act
		using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			List<AccountEntity> allIncludingDeleted = await context.Accounts
				.IgnoreQueryFilters()
				.ToListAsync();

			// Assert
			allIncludingDeleted.Should().HaveCount(3);
		}

		contextFactory.ResetDatabase();
	}
}
