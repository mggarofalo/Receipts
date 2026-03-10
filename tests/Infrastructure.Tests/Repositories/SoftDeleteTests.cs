using FluentAssertions;
using Infrastructure.Entities.Core;
using Infrastructure.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using SampleData.Entities;

namespace Infrastructure.Tests.Repositories;

public class SoftDeleteTests
{
	[Fact]
	public async Task SoftDelete_ItemTemplate_SetsDeletedAtOnDelete()
	{
		// Arrange
		IDbContextFactory<ApplicationDbContext> contextFactory = DbContextHelpers.CreateInMemoryContextFactory();
		ItemTemplateEntity entity = ItemTemplateEntityGenerator.Generate();

		using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			await context.ItemTemplates.AddAsync(entity);
			await context.SaveChangesAsync();
		}

		// Act
		using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			ItemTemplateEntity template = await context.ItemTemplates.FirstAsync();
			context.ItemTemplates.Remove(template);
			await context.SaveChangesAsync();
		}

		// Assert
		using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			List<ItemTemplateEntity> allTemplates = await context.ItemTemplates.IgnoreQueryFilters().ToListAsync();
			allTemplates.Should().HaveCount(1);
			allTemplates[0].DeletedAt.Should().NotBeNull();
		}

		contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task SoftDelete_ItemTemplate_ExcludedFromNormalQueries()
	{
		// Arrange
		IDbContextFactory<ApplicationDbContext> contextFactory = DbContextHelpers.CreateInMemoryContextFactory();
		ItemTemplateEntity entity = ItemTemplateEntityGenerator.Generate();

		using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			await context.ItemTemplates.AddAsync(entity);
			await context.SaveChangesAsync();
		}

		// Act
		using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			ItemTemplateEntity template = await context.ItemTemplates.FirstAsync();
			context.ItemTemplates.Remove(template);
			await context.SaveChangesAsync();
		}

		// Assert
		using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			List<ItemTemplateEntity> visibleTemplates = await context.ItemTemplates.ToListAsync();
			visibleTemplates.Should().BeEmpty();
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
	public async Task SoftDelete_SetsDeletedByUserId()
	{
		// Arrange
		(IDbContextFactory<ApplicationDbContext> contextFactory, MockCurrentUserAccessor accessor) = DbContextWithUserHelpers.CreateInMemoryContextFactoryWithUser();
		accessor.UserId = "test-user-id";

		using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			ItemTemplateEntity entity = ItemTemplateEntityGenerator.Generate();
			await context.ItemTemplates.AddAsync(entity);
			await context.SaveChangesAsync();
		}

		// Act
		using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			ItemTemplateEntity template = await context.ItemTemplates.FirstAsync();
			context.ItemTemplates.Remove(template);
			await context.SaveChangesAsync();
		}

		// Assert
		using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			List<ItemTemplateEntity> allTemplates = await context.ItemTemplates.IgnoreQueryFilters().ToListAsync();
			allTemplates.Should().HaveCount(1);
			allTemplates[0].DeletedByUserId.Should().Be("test-user-id");
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
			ItemTemplateEntity entity = ItemTemplateEntityGenerator.Generate();
			await context.ItemTemplates.AddAsync(entity);
			await context.SaveChangesAsync();
		}

		// Act
		using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			ItemTemplateEntity template = await context.ItemTemplates.FirstAsync();
			context.ItemTemplates.Remove(template);
			await context.SaveChangesAsync();
		}

		// Assert
		using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			List<ItemTemplateEntity> allTemplates = await context.ItemTemplates.IgnoreQueryFilters().ToListAsync();
			allTemplates.Should().HaveCount(1);
			allTemplates[0].DeletedByApiKeyId.Should().Be(apiKeyId);
		}

		contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task OnlyDeleted_ReturnsOnlySoftDeletedEntities()
	{
		// Arrange
		IDbContextFactory<ApplicationDbContext> contextFactory = DbContextHelpers.CreateInMemoryContextFactory();
		List<ItemTemplateEntity> entities = ItemTemplateEntityGenerator.GenerateList(3);

		using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			await context.ItemTemplates.AddRangeAsync(entities);
			await context.SaveChangesAsync();
		}

		// Delete only the first entity
		using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			ItemTemplateEntity templateToDelete = await context.ItemTemplates.FirstAsync(t => t.Id == entities[0].Id);
			context.ItemTemplates.Remove(templateToDelete);
			await context.SaveChangesAsync();
		}

		// Act
		using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			List<ItemTemplateEntity> onlyDeleted = await context.ItemTemplates
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
		List<ItemTemplateEntity> entities = ItemTemplateEntityGenerator.GenerateList(3);

		using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			await context.ItemTemplates.AddRangeAsync(entities);
			await context.SaveChangesAsync();
		}

		// Delete one entity
		using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			ItemTemplateEntity templateToDelete = await context.ItemTemplates.FirstAsync(t => t.Id == entities[0].Id);
			context.ItemTemplates.Remove(templateToDelete);
			await context.SaveChangesAsync();
		}

		// Act
		using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			List<ItemTemplateEntity> allIncludingDeleted = await context.ItemTemplates
				.IgnoreQueryFilters()
				.ToListAsync();

			// Assert
			allIncludingDeleted.Should().HaveCount(3);
		}

		contextFactory.ResetDatabase();
	}
}
