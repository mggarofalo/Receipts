using FluentAssertions;
using Infrastructure.Entities.Core;
using Infrastructure.Extensions;
using Infrastructure.IntegrationTests.Fixtures;
using Microsoft.EntityFrameworkCore;
using SampleData.Entities;

namespace Infrastructure.IntegrationTests;

[Collection(PostgresCollection.Name)]
[Trait("Category", "Integration")]
public class SoftDeleteTests(PostgresFixture fixture)
{
	[Fact]
	public async Task SoftDelete_Receipt_SetsDeletedAt()
	{
		// Arrange
		await using ApplicationDbContext context = fixture.CreateDbContext();
		ReceiptEntity receipt = ReceiptEntityGenerator.Generate();
		context.Receipts.Add(receipt);
		await context.SaveChangesAsync();

		// Act — remove triggers soft delete via HandleSoftDelete()
		context.Receipts.Remove(receipt);
		await context.SaveChangesAsync();

		// Assert — query filter hides soft-deleted entities
		await using ApplicationDbContext readContext = fixture.CreateDbContext();
		ReceiptEntity? hidden = await readContext.Receipts.FirstOrDefaultAsync(r => r.Id == receipt.Id);
		hidden.Should().BeNull("query filter should exclude soft-deleted receipts");

		// Assert — IgnoreQueryFilters reveals the soft-deleted entity
		ReceiptEntity? deleted = await readContext.Receipts
			.IgnoreQueryFilters()
			.FirstOrDefaultAsync(r => r.Id == receipt.Id);
		deleted.Should().NotBeNull();
		deleted!.DeletedAt.Should().NotBeNull();
	}

	[Fact]
	public async Task SoftDelete_Receipt_CascadesToTransactions()
	{
		// Arrange — receipt with a child transaction
		await using ApplicationDbContext context = fixture.CreateDbContext();
		ReceiptEntity receipt = ReceiptEntityGenerator.Generate();
		AccountEntity account = AccountEntityGenerator.Generate();
		context.Receipts.Add(receipt);
		context.Accounts.Add(account);
		await context.SaveChangesAsync();

		TransactionEntity transaction = TransactionEntityGenerator.Generate(receipt.Id, account.Id);
		context.Transactions.Add(transaction);
		await context.SaveChangesAsync();

		// Act — soft-delete the receipt
		context.Receipts.Remove(receipt);
		await context.SaveChangesAsync();

		// Assert — transaction should also be soft-deleted (cascade)
		await using ApplicationDbContext readContext = fixture.CreateDbContext();

		TransactionEntity? hiddenTx = await readContext.Transactions
			.FirstOrDefaultAsync(t => t.Id == transaction.Id);
		hiddenTx.Should().BeNull("query filter should exclude cascade-soft-deleted transactions");

		TransactionEntity? deletedTx = await readContext.Transactions
			.IgnoreQueryFilters()
			.FirstOrDefaultAsync(t => t.Id == transaction.Id);
		deletedTx.Should().NotBeNull();
		deletedTx!.DeletedAt.Should().NotBeNull();
	}

	[Fact]
	public async Task SoftDelete_Receipt_CascadesToReceiptItems()
	{
		// Arrange — receipt with a child receipt item
		await using ApplicationDbContext context = fixture.CreateDbContext();
		ReceiptEntity receipt = ReceiptEntityGenerator.Generate();
		context.Receipts.Add(receipt);
		await context.SaveChangesAsync();

		ReceiptItemEntity item = ReceiptItemEntityGenerator.Generate(receipt.Id);
		context.ReceiptItems.Add(item);
		await context.SaveChangesAsync();

		// Act — soft-delete the receipt
		context.Receipts.Remove(receipt);
		await context.SaveChangesAsync();

		// Assert — receipt item should be cascade soft-deleted
		await using ApplicationDbContext readContext = fixture.CreateDbContext();

		ReceiptItemEntity? hiddenItem = await readContext.ReceiptItems
			.FirstOrDefaultAsync(i => i.Id == item.Id);
		hiddenItem.Should().BeNull("query filter should exclude cascade-soft-deleted receipt items");

		ReceiptItemEntity? deletedItem = await readContext.ReceiptItems
			.IgnoreQueryFilters()
			.FirstOrDefaultAsync(i => i.Id == item.Id);
		deletedItem.Should().NotBeNull();
		deletedItem!.DeletedAt.Should().NotBeNull();
	}

	[Fact]
	public async Task SoftDelete_Receipt_CascadesToAdjustments()
	{
		// Arrange — receipt with a child adjustment
		await using ApplicationDbContext context = fixture.CreateDbContext();
		ReceiptEntity receipt = ReceiptEntityGenerator.Generate();
		context.Receipts.Add(receipt);
		await context.SaveChangesAsync();

		AdjustmentEntity adjustment = AdjustmentEntityGenerator.Generate();
		adjustment.ReceiptId = receipt.Id;
		context.Adjustments.Add(adjustment);
		await context.SaveChangesAsync();

		// Act — soft-delete the receipt
		context.Receipts.Remove(receipt);
		await context.SaveChangesAsync();

		// Assert — adjustment should be cascade soft-deleted
		await using ApplicationDbContext readContext = fixture.CreateDbContext();

		AdjustmentEntity? hiddenAdj = await readContext.Adjustments
			.FirstOrDefaultAsync(a => a.Id == adjustment.Id);
		hiddenAdj.Should().BeNull("query filter should exclude cascade-soft-deleted adjustments");

		AdjustmentEntity? deletedAdj = await readContext.Adjustments
			.IgnoreQueryFilters()
			.FirstOrDefaultAsync(a => a.Id == adjustment.Id);
		deletedAdj.Should().NotBeNull();
		deletedAdj!.DeletedAt.Should().NotBeNull();
	}

	[Fact]
	public async Task QueryFilter_ExcludesSoftDeletedEntities_InLinqQueries()
	{
		// Arrange — create two receipts, soft-delete one
		await using ApplicationDbContext context = fixture.CreateDbContext();
		ReceiptEntity activeReceipt = ReceiptEntityGenerator.Generate();
		ReceiptEntity deletedReceipt = ReceiptEntityGenerator.Generate();

		context.Receipts.AddRange(activeReceipt, deletedReceipt);
		await context.SaveChangesAsync();

		context.Receipts.Remove(deletedReceipt);
		await context.SaveChangesAsync();

		// Act — query with filter
		await using ApplicationDbContext readContext = fixture.CreateDbContext();
		List<ReceiptEntity> filtered = await readContext.Receipts
			.Where(r => r.Id == activeReceipt.Id || r.Id == deletedReceipt.Id)
			.ToListAsync();

		// Assert — only the active receipt should appear
		filtered.Should().ContainSingle()
			.Which.Id.Should().Be(activeReceipt.Id);
	}

	[Fact]
	public async Task OnlyDeleted_ReturnsOnlySoftDeletedEntities()
	{
		// Arrange — create a receipt and soft-delete it
		await using ApplicationDbContext context = fixture.CreateDbContext();
		ReceiptEntity receipt = ReceiptEntityGenerator.Generate();
		context.Receipts.Add(receipt);
		await context.SaveChangesAsync();

		context.Receipts.Remove(receipt);
		await context.SaveChangesAsync();

		// Act
		await using ApplicationDbContext readContext = fixture.CreateDbContext();
		ReceiptEntity? onlyDeleted = await readContext.Receipts
			.OnlyDeleted()
			.FirstOrDefaultAsync(r => r.Id == receipt.Id);

		// Assert
		onlyDeleted.Should().NotBeNull();
		onlyDeleted!.DeletedAt.Should().NotBeNull();
	}
}
