using Application.Models.Reports;
using FluentAssertions;
using Infrastructure.Entities.Core;
using Infrastructure.Services;
using Infrastructure.Tests.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Tests.Services;

public class ReportServiceTests
{
	[Fact]
	public async Task GetOutOfBalanceAsync_ReturnsEmptyWhenAllBalanced()
	{
		// Arrange
		IDbContextFactory<ApplicationDbContext> contextFactory = DbContextHelpers.CreateInMemoryContextFactory();

		Guid receiptId = Guid.NewGuid();
		Guid accountId = Guid.NewGuid();
		DateOnly date = new(2025, 3, 1);

		await using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			context.Receipts.Add(
				new ReceiptEntity { Id = receiptId, Location = "Store A", Date = date, TaxAmount = 1.00m });

			context.ReceiptItems.Add(
				new ReceiptItemEntity { Id = Guid.NewGuid(), ReceiptId = receiptId, Description = "Item 1", Quantity = 1, UnitPrice = 10.00m, TotalAmount = 10.00m, Category = "Food" });

			context.Transactions.Add(
				new TransactionEntity { Id = Guid.NewGuid(), ReceiptId = receiptId, AccountId = accountId, Amount = 11.00m, Date = date });

			await context.SaveChangesAsync();
		}

		ReportService service = new(contextFactory);

		// Act
		OutOfBalanceResult result = await service.GetOutOfBalanceAsync("date", "asc", 1, 50, CancellationToken.None);

		// Assert
		result.Items.Should().BeEmpty();
		result.TotalCount.Should().Be(0);
		result.TotalDiscrepancy.Should().Be(0m);

		contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task GetOutOfBalanceAsync_ReturnsReceiptsWhereExpectedDoesNotMatchTransaction()
	{
		// Arrange
		IDbContextFactory<ApplicationDbContext> contextFactory = DbContextHelpers.CreateInMemoryContextFactory();

		Guid receiptId = Guid.NewGuid();
		Guid accountId = Guid.NewGuid();
		DateOnly date = new(2025, 3, 1);

		await using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			// Receipt: items=10, tax=1, adjustments=0, expected=11, transaction=15
			context.Receipts.Add(
				new ReceiptEntity { Id = receiptId, Location = "Store A", Date = date, TaxAmount = 1.00m });

			context.ReceiptItems.Add(
				new ReceiptItemEntity { Id = Guid.NewGuid(), ReceiptId = receiptId, Description = "Item 1", Quantity = 1, UnitPrice = 10.00m, TotalAmount = 10.00m, Category = "Food" });

			context.Transactions.Add(
				new TransactionEntity { Id = Guid.NewGuid(), ReceiptId = receiptId, AccountId = accountId, Amount = 15.00m, Date = date });

			await context.SaveChangesAsync();
		}

		ReportService service = new(contextFactory);

		// Act
		OutOfBalanceResult result = await service.GetOutOfBalanceAsync("date", "asc", 1, 50, CancellationToken.None);

		// Assert
		result.Items.Should().ContainSingle();
		result.TotalCount.Should().Be(1);

		OutOfBalanceItem item = result.Items[0];
		item.ReceiptId.Should().Be(receiptId);
		item.Location.Should().Be("Store A");
		item.Date.Should().Be(date);
		item.ItemSubtotal.Should().Be(10.00m);
		item.TaxAmount.Should().Be(1.00m);
		item.AdjustmentTotal.Should().Be(0m);
		item.ExpectedTotal.Should().Be(11.00m);
		item.TransactionTotal.Should().Be(15.00m);
		item.Difference.Should().Be(-4.00m);

		contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task GetOutOfBalanceAsync_IncludesAdjustmentsInExpectedTotal()
	{
		// Arrange
		IDbContextFactory<ApplicationDbContext> contextFactory = DbContextHelpers.CreateInMemoryContextFactory();

		Guid receiptId = Guid.NewGuid();
		Guid accountId = Guid.NewGuid();
		DateOnly date = new(2025, 3, 1);

		await using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			// items=10, tax=1, adjustment=2, expected=13, transaction=10 => diff=3
			context.Receipts.Add(
				new ReceiptEntity { Id = receiptId, Location = "Store B", Date = date, TaxAmount = 1.00m });

			context.ReceiptItems.Add(
				new ReceiptItemEntity { Id = Guid.NewGuid(), ReceiptId = receiptId, Description = "Item 1", Quantity = 1, UnitPrice = 10.00m, TotalAmount = 10.00m, Category = "Food" });

			context.Adjustments.Add(
				new AdjustmentEntity { Id = Guid.NewGuid(), ReceiptId = receiptId, Type = Common.AdjustmentType.Discount, Amount = 2.00m });

			context.Transactions.Add(
				new TransactionEntity { Id = Guid.NewGuid(), ReceiptId = receiptId, AccountId = accountId, Amount = 10.00m, Date = date });

			await context.SaveChangesAsync();
		}

		ReportService service = new(contextFactory);

		// Act
		OutOfBalanceResult result = await service.GetOutOfBalanceAsync("date", "asc", 1, 50, CancellationToken.None);

		// Assert
		result.Items.Should().ContainSingle();
		OutOfBalanceItem item = result.Items[0];
		item.AdjustmentTotal.Should().Be(2.00m);
		item.ExpectedTotal.Should().Be(13.00m);
		item.Difference.Should().Be(3.00m);

		contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task GetOutOfBalanceAsync_ExcludesSoftDeletedRecords()
	{
		// Arrange
		IDbContextFactory<ApplicationDbContext> contextFactory = DbContextHelpers.CreateInMemoryContextFactory();

		Guid receiptId = Guid.NewGuid();
		Guid accountId = Guid.NewGuid();
		DateOnly date = new(2025, 3, 1);

		await using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			// Soft-deleted receipt should not appear
			context.Receipts.Add(
				new ReceiptEntity { Id = receiptId, Location = "Deleted Store", Date = date, TaxAmount = 1.00m, DeletedAt = DateTimeOffset.UtcNow });

			context.ReceiptItems.Add(
				new ReceiptItemEntity { Id = Guid.NewGuid(), ReceiptId = receiptId, Description = "Item 1", Quantity = 1, UnitPrice = 10.00m, TotalAmount = 10.00m, Category = "Food" });

			// No transaction — would normally be out of balance
			await context.SaveChangesAsync();
		}

		ReportService service = new(contextFactory);

		// Act
		OutOfBalanceResult result = await service.GetOutOfBalanceAsync("date", "asc", 1, 50, CancellationToken.None);

		// Assert
		result.Items.Should().BeEmpty();

		contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task GetOutOfBalanceAsync_SortsByDateAscByDefault()
	{
		// Arrange
		IDbContextFactory<ApplicationDbContext> contextFactory = DbContextHelpers.CreateInMemoryContextFactory();
		Guid accountId = Guid.NewGuid();

		Guid receiptId1 = Guid.NewGuid();
		Guid receiptId2 = Guid.NewGuid();
		DateOnly day1 = new(2025, 3, 1);
		DateOnly day2 = new(2025, 3, 2);

		await using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			// Insert in reverse order
			context.Receipts.AddRange(
				new ReceiptEntity { Id = receiptId2, Location = "B", Date = day2, TaxAmount = 0m },
				new ReceiptEntity { Id = receiptId1, Location = "A", Date = day1, TaxAmount = 0m });

			context.Transactions.AddRange(
				new TransactionEntity { Id = Guid.NewGuid(), ReceiptId = receiptId1, AccountId = accountId, Amount = 99.00m, Date = day1 },
				new TransactionEntity { Id = Guid.NewGuid(), ReceiptId = receiptId2, AccountId = accountId, Amount = 99.00m, Date = day2 });

			// No items — so expected=0, transaction=99 => diff=-99 for both
			await context.SaveChangesAsync();
		}

		ReportService service = new(contextFactory);

		// Act
		OutOfBalanceResult result = await service.GetOutOfBalanceAsync("date", "asc", 1, 50, CancellationToken.None);

		// Assert
		result.Items.Should().HaveCount(2);
		result.Items[0].Date.Should().Be(day1);
		result.Items[1].Date.Should().Be(day2);

		contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task GetOutOfBalanceAsync_SortsByDifferenceDesc()
	{
		// Arrange
		IDbContextFactory<ApplicationDbContext> contextFactory = DbContextHelpers.CreateInMemoryContextFactory();
		Guid accountId = Guid.NewGuid();

		Guid receiptId1 = Guid.NewGuid();
		Guid receiptId2 = Guid.NewGuid();
		DateOnly date = new(2025, 3, 1);

		await using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			context.Receipts.AddRange(
				new ReceiptEntity { Id = receiptId1, Location = "A", Date = date, TaxAmount = 0m },
				new ReceiptEntity { Id = receiptId2, Location = "B", Date = date, TaxAmount = 0m });

			// Receipt 1: items=10, transaction=5 => diff=5
			context.ReceiptItems.Add(
				new ReceiptItemEntity { Id = Guid.NewGuid(), ReceiptId = receiptId1, Description = "Item", Quantity = 1, UnitPrice = 10.00m, TotalAmount = 10.00m, Category = "Food" });
			context.Transactions.Add(
				new TransactionEntity { Id = Guid.NewGuid(), ReceiptId = receiptId1, AccountId = accountId, Amount = 5.00m, Date = date });

			// Receipt 2: items=20, transaction=5 => diff=15
			context.ReceiptItems.Add(
				new ReceiptItemEntity { Id = Guid.NewGuid(), ReceiptId = receiptId2, Description = "Item", Quantity = 1, UnitPrice = 20.00m, TotalAmount = 20.00m, Category = "Food" });
			context.Transactions.Add(
				new TransactionEntity { Id = Guid.NewGuid(), ReceiptId = receiptId2, AccountId = accountId, Amount = 5.00m, Date = date });

			await context.SaveChangesAsync();
		}

		ReportService service = new(contextFactory);

		// Act
		OutOfBalanceResult result = await service.GetOutOfBalanceAsync("difference", "desc", 1, 50, CancellationToken.None);

		// Assert
		result.Items.Should().HaveCount(2);
		result.Items[0].Difference.Should().Be(15.00m);
		result.Items[1].Difference.Should().Be(5.00m);

		contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task GetOutOfBalanceAsync_PaginatesCorrectly()
	{
		// Arrange
		IDbContextFactory<ApplicationDbContext> contextFactory = DbContextHelpers.CreateInMemoryContextFactory();
		Guid accountId = Guid.NewGuid();

		await using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			// Create 3 out-of-balance receipts
			for (int i = 0; i < 3; i++)
			{
				Guid receiptId = Guid.NewGuid();
				DateOnly date = new(2025, 3, i + 1);

				context.Receipts.Add(
					new ReceiptEntity { Id = receiptId, Location = $"Store {i}", Date = date, TaxAmount = 0m });

				context.Transactions.Add(
					new TransactionEntity { Id = Guid.NewGuid(), ReceiptId = receiptId, AccountId = accountId, Amount = 99.00m, Date = date });
			}

			await context.SaveChangesAsync();
		}

		ReportService service = new(contextFactory);

		// Act - page 1, size 2
		OutOfBalanceResult page1 = await service.GetOutOfBalanceAsync("date", "asc", 1, 2, CancellationToken.None);

		// Assert
		page1.Items.Should().HaveCount(2);
		page1.TotalCount.Should().Be(3);

		// Act - page 2, size 2
		OutOfBalanceResult page2 = await service.GetOutOfBalanceAsync("date", "asc", 2, 2, CancellationToken.None);

		// Assert
		page2.Items.Should().ContainSingle();
		page2.TotalCount.Should().Be(3);

		contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task GetOutOfBalanceAsync_CalculatesTotalDiscrepancyAsAbsoluteSum()
	{
		// Arrange
		IDbContextFactory<ApplicationDbContext> contextFactory = DbContextHelpers.CreateInMemoryContextFactory();
		Guid accountId = Guid.NewGuid();
		DateOnly date = new(2025, 3, 1);

		Guid receiptId1 = Guid.NewGuid();
		Guid receiptId2 = Guid.NewGuid();

		await using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			context.Receipts.AddRange(
				new ReceiptEntity { Id = receiptId1, Location = "A", Date = date, TaxAmount = 0m },
				new ReceiptEntity { Id = receiptId2, Location = "B", Date = date, TaxAmount = 0m });

			// Receipt 1: items=10, transaction=5 => diff=5 (positive)
			context.ReceiptItems.Add(
				new ReceiptItemEntity { Id = Guid.NewGuid(), ReceiptId = receiptId1, Description = "Item", Quantity = 1, UnitPrice = 10.00m, TotalAmount = 10.00m, Category = "Food" });
			context.Transactions.Add(
				new TransactionEntity { Id = Guid.NewGuid(), ReceiptId = receiptId1, AccountId = accountId, Amount = 5.00m, Date = date });

			// Receipt 2: items=5, transaction=10 => diff=-5 (negative)
			context.ReceiptItems.Add(
				new ReceiptItemEntity { Id = Guid.NewGuid(), ReceiptId = receiptId2, Description = "Item", Quantity = 1, UnitPrice = 5.00m, TotalAmount = 5.00m, Category = "Food" });
			context.Transactions.Add(
				new TransactionEntity { Id = Guid.NewGuid(), ReceiptId = receiptId2, AccountId = accountId, Amount = 10.00m, Date = date });

			await context.SaveChangesAsync();
		}

		ReportService service = new(contextFactory);

		// Act
		OutOfBalanceResult result = await service.GetOutOfBalanceAsync("date", "asc", 1, 50, CancellationToken.None);

		// Assert
		result.TotalDiscrepancy.Should().Be(10.00m); // |5| + |-5| = 10

		contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task GetOutOfBalanceAsync_ExcludesSoftDeletedItemsFromCalculation()
	{
		// Arrange
		IDbContextFactory<ApplicationDbContext> contextFactory = DbContextHelpers.CreateInMemoryContextFactory();

		Guid receiptId = Guid.NewGuid();
		Guid accountId = Guid.NewGuid();
		DateOnly date = new(2025, 3, 1);

		await using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			// Receipt with items=10 (active) + 5 (deleted), tax=1, transaction=11
			// Expected with active only: 10+1=11 => balanced
			context.Receipts.Add(
				new ReceiptEntity { Id = receiptId, Location = "Store", Date = date, TaxAmount = 1.00m });

			context.ReceiptItems.AddRange(
				new ReceiptItemEntity { Id = Guid.NewGuid(), ReceiptId = receiptId, Description = "Active", Quantity = 1, UnitPrice = 10.00m, TotalAmount = 10.00m, Category = "Food" },
				new ReceiptItemEntity { Id = Guid.NewGuid(), ReceiptId = receiptId, Description = "Deleted", Quantity = 1, UnitPrice = 5.00m, TotalAmount = 5.00m, Category = "Food", DeletedAt = DateTimeOffset.UtcNow });

			context.Transactions.Add(
				new TransactionEntity { Id = Guid.NewGuid(), ReceiptId = receiptId, AccountId = accountId, Amount = 11.00m, Date = date });

			await context.SaveChangesAsync();
		}

		ReportService service = new(contextFactory);

		// Act
		OutOfBalanceResult result = await service.GetOutOfBalanceAsync("date", "asc", 1, 50, CancellationToken.None);

		// Assert - should be balanced since deleted items are excluded
		result.Items.Should().BeEmpty();

		contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task GetOutOfBalanceAsync_HandlesReceiptWithNoItems()
	{
		// Arrange
		IDbContextFactory<ApplicationDbContext> contextFactory = DbContextHelpers.CreateInMemoryContextFactory();

		Guid receiptId = Guid.NewGuid();
		Guid accountId = Guid.NewGuid();
		DateOnly date = new(2025, 3, 1);

		await using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			// Receipt with no items, tax=0, transaction=5 => expected=0, diff=-5
			context.Receipts.Add(
				new ReceiptEntity { Id = receiptId, Location = "Empty Store", Date = date, TaxAmount = 0m });

			context.Transactions.Add(
				new TransactionEntity { Id = Guid.NewGuid(), ReceiptId = receiptId, AccountId = accountId, Amount = 5.00m, Date = date });

			await context.SaveChangesAsync();
		}

		ReportService service = new(contextFactory);

		// Act
		OutOfBalanceResult result = await service.GetOutOfBalanceAsync("date", "asc", 1, 50, CancellationToken.None);

		// Assert
		result.Items.Should().ContainSingle();
		result.Items[0].ItemSubtotal.Should().Be(0m);
		result.Items[0].TransactionTotal.Should().Be(5.00m);
		result.Items[0].Difference.Should().Be(-5.00m);

		contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task GetOutOfBalanceAsync_HandlesReceiptWithNoTransactions()
	{
		// Arrange
		IDbContextFactory<ApplicationDbContext> contextFactory = DbContextHelpers.CreateInMemoryContextFactory();

		Guid receiptId = Guid.NewGuid();
		DateOnly date = new(2025, 3, 1);

		await using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			// Receipt with items=10, tax=1, no transaction => expected=11, transaction=0, diff=11
			context.Receipts.Add(
				new ReceiptEntity { Id = receiptId, Location = "No Payment", Date = date, TaxAmount = 1.00m });

			context.ReceiptItems.Add(
				new ReceiptItemEntity { Id = Guid.NewGuid(), ReceiptId = receiptId, Description = "Item 1", Quantity = 1, UnitPrice = 10.00m, TotalAmount = 10.00m, Category = "Food" });

			await context.SaveChangesAsync();
		}

		ReportService service = new(contextFactory);

		// Act
		OutOfBalanceResult result = await service.GetOutOfBalanceAsync("date", "asc", 1, 50, CancellationToken.None);

		// Assert
		result.Items.Should().ContainSingle();
		result.Items[0].TransactionTotal.Should().Be(0m);
		result.Items[0].ExpectedTotal.Should().Be(11.00m);
		result.Items[0].Difference.Should().Be(11.00m);

		contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task GetItemDescriptionsAsync_ReturnsMatchingItems()
	{
		// Arrange
		IDbContextFactory<ApplicationDbContext> contextFactory = DbContextHelpers.CreateInMemoryContextFactory();

		Guid receiptId = Guid.NewGuid();

		await using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			context.Receipts.Add(
				new ReceiptEntity { Id = receiptId, Location = "Store", Date = new DateOnly(2025, 3, 1), TaxAmount = 0m });

			context.ReceiptItems.AddRange(
				new ReceiptItemEntity { Id = Guid.NewGuid(), ReceiptId = receiptId, Description = "Milk", Quantity = 1, UnitPrice = 3.00m, TotalAmount = 3.00m, Category = "Dairy" },
				new ReceiptItemEntity { Id = Guid.NewGuid(), ReceiptId = receiptId, Description = "Milk", Quantity = 1, UnitPrice = 3.50m, TotalAmount = 3.50m, Category = "Dairy" },
				new ReceiptItemEntity { Id = Guid.NewGuid(), ReceiptId = receiptId, Description = "Bread", Quantity = 1, UnitPrice = 2.00m, TotalAmount = 2.00m, Category = "Bakery" });

			await context.SaveChangesAsync();
		}

		ReportService service = new(contextFactory);

		// Act
		ItemDescriptionResult result = await service.GetItemDescriptionsAsync("milk", false, 10, CancellationToken.None);

		// Assert
		result.Items.Should().ContainSingle();
		result.Items[0].Description.Should().Be("Milk");
		result.Items[0].Category.Should().Be("Dairy");
		result.Items[0].Occurrences.Should().Be(2);

		contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task GetItemDescriptionsAsync_CategoryOnlyMode_ReturnsCategories()
	{
		// Arrange
		IDbContextFactory<ApplicationDbContext> contextFactory = DbContextHelpers.CreateInMemoryContextFactory();

		Guid receiptId = Guid.NewGuid();

		await using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			context.Receipts.Add(
				new ReceiptEntity { Id = receiptId, Location = "Store", Date = new DateOnly(2025, 3, 1), TaxAmount = 0m });

			context.ReceiptItems.AddRange(
				new ReceiptItemEntity { Id = Guid.NewGuid(), ReceiptId = receiptId, Description = "Milk", Quantity = 1, UnitPrice = 3.00m, TotalAmount = 3.00m, Category = "Dairy" },
				new ReceiptItemEntity { Id = Guid.NewGuid(), ReceiptId = receiptId, Description = "Cheese", Quantity = 1, UnitPrice = 5.00m, TotalAmount = 5.00m, Category = "Dairy" },
				new ReceiptItemEntity { Id = Guid.NewGuid(), ReceiptId = receiptId, Description = "Bread", Quantity = 1, UnitPrice = 2.00m, TotalAmount = 2.00m, Category = "Bakery" });

			await context.SaveChangesAsync();
		}

		ReportService service = new(contextFactory);

		// Act
		ItemDescriptionResult result = await service.GetItemDescriptionsAsync("dairy", true, 10, CancellationToken.None);

		// Assert
		result.Items.Should().ContainSingle();
		result.Items[0].Description.Should().Be("Dairy");
		result.Items[0].Category.Should().Be("Dairy");
		result.Items[0].Occurrences.Should().Be(2);

		contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task GetItemDescriptionsAsync_ExcludesSoftDeletedItems()
	{
		// Arrange
		IDbContextFactory<ApplicationDbContext> contextFactory = DbContextHelpers.CreateInMemoryContextFactory();

		Guid receiptId = Guid.NewGuid();

		await using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			context.Receipts.Add(
				new ReceiptEntity { Id = receiptId, Location = "Store", Date = new DateOnly(2025, 3, 1), TaxAmount = 0m });

			context.ReceiptItems.AddRange(
				new ReceiptItemEntity { Id = Guid.NewGuid(), ReceiptId = receiptId, Description = "Milk", Quantity = 1, UnitPrice = 3.00m, TotalAmount = 3.00m, Category = "Dairy" },
				new ReceiptItemEntity { Id = Guid.NewGuid(), ReceiptId = receiptId, Description = "Milk", Quantity = 1, UnitPrice = 3.00m, TotalAmount = 3.00m, Category = "Dairy", DeletedAt = DateTimeOffset.UtcNow });

			await context.SaveChangesAsync();
		}

		ReportService service = new(contextFactory);

		// Act
		ItemDescriptionResult result = await service.GetItemDescriptionsAsync("milk", false, 10, CancellationToken.None);

		// Assert
		result.Items.Should().ContainSingle();
		result.Items[0].Occurrences.Should().Be(1);

		contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task GetItemDescriptionsAsync_RespectsLimit()
	{
		// Arrange
		IDbContextFactory<ApplicationDbContext> contextFactory = DbContextHelpers.CreateInMemoryContextFactory();

		Guid receiptId = Guid.NewGuid();

		await using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			context.Receipts.Add(
				new ReceiptEntity { Id = receiptId, Location = "Store", Date = new DateOnly(2025, 3, 1), TaxAmount = 0m });

			context.ReceiptItems.AddRange(
				new ReceiptItemEntity { Id = Guid.NewGuid(), ReceiptId = receiptId, Description = "Item A", Quantity = 1, UnitPrice = 1.00m, TotalAmount = 1.00m, Category = "Cat1" },
				new ReceiptItemEntity { Id = Guid.NewGuid(), ReceiptId = receiptId, Description = "Item B", Quantity = 1, UnitPrice = 2.00m, TotalAmount = 2.00m, Category = "Cat2" },
				new ReceiptItemEntity { Id = Guid.NewGuid(), ReceiptId = receiptId, Description = "Item C", Quantity = 1, UnitPrice = 3.00m, TotalAmount = 3.00m, Category = "Cat3" });

			await context.SaveChangesAsync();
		}

		ReportService service = new(contextFactory);

		// Act
		ItemDescriptionResult result = await service.GetItemDescriptionsAsync("item", false, 2, CancellationToken.None);

		// Assert
		result.Items.Should().HaveCount(2);

		contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task GetItemDescriptionsAsync_NoMatch_ReturnsEmpty()
	{
		// Arrange
		IDbContextFactory<ApplicationDbContext> contextFactory = DbContextHelpers.CreateInMemoryContextFactory();

		Guid receiptId = Guid.NewGuid();

		await using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			context.Receipts.Add(
				new ReceiptEntity { Id = receiptId, Location = "Store", Date = new DateOnly(2025, 3, 1), TaxAmount = 0m });

			context.ReceiptItems.Add(
				new ReceiptItemEntity { Id = Guid.NewGuid(), ReceiptId = receiptId, Description = "Milk", Quantity = 1, UnitPrice = 3.00m, TotalAmount = 3.00m, Category = "Dairy" });

			await context.SaveChangesAsync();
		}

		ReportService service = new(contextFactory);

		// Act
		ItemDescriptionResult result = await service.GetItemDescriptionsAsync("xyz", false, 10, CancellationToken.None);

		// Assert
		result.Items.Should().BeEmpty();

		contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task GetItemDescriptionsAsync_GroupsByDescriptionAndCategory()
	{
		// Arrange
		IDbContextFactory<ApplicationDbContext> contextFactory = DbContextHelpers.CreateInMemoryContextFactory();

		Guid receiptId = Guid.NewGuid();

		await using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			context.Receipts.Add(
				new ReceiptEntity { Id = receiptId, Location = "Store", Date = new DateOnly(2025, 3, 1), TaxAmount = 0m });

			// Same description, different categories => separate groups
			context.ReceiptItems.AddRange(
				new ReceiptItemEntity { Id = Guid.NewGuid(), ReceiptId = receiptId, Description = "Milk", Quantity = 1, UnitPrice = 3.00m, TotalAmount = 3.00m, Category = "Dairy" },
				new ReceiptItemEntity { Id = Guid.NewGuid(), ReceiptId = receiptId, Description = "Milk", Quantity = 1, UnitPrice = 4.00m, TotalAmount = 4.00m, Category = "Beverages" });

			await context.SaveChangesAsync();
		}

		ReportService service = new(contextFactory);

		// Act
		ItemDescriptionResult result = await service.GetItemDescriptionsAsync("milk", false, 10, CancellationToken.None);

		// Assert
		result.Items.Should().HaveCount(2);
		result.Items.Should().Contain(x => x.Category == "Dairy");
		result.Items.Should().Contain(x => x.Category == "Beverages");

		contextFactory.ResetDatabase();
	}
}
