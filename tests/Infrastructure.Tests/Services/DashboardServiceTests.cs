using Application.Models.Dashboard;
using FluentAssertions;
using Infrastructure.Entities.Core;
using Infrastructure.Services;
using Infrastructure.Tests.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Tests.Services;

public class DashboardServiceTests
{
	[Fact]
	public async Task GetSpendingOverTimeAsync_DailyGranularity_ReturnsCorrectBuckets()
	{
		// Arrange
		IDbContextFactory<ApplicationDbContext> contextFactory = DbContextHelpers.CreateInMemoryContextFactory();

		Guid receiptId1 = Guid.NewGuid();
		Guid receiptId2 = Guid.NewGuid();
		Guid accountId = Guid.NewGuid();

		DateOnly day1 = new(2025, 3, 1);
		DateOnly day2 = new(2025, 3, 2);

		await using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			context.Receipts.AddRange(
				new ReceiptEntity { Id = receiptId1, Location = "Store A", Date = day1, TaxAmount = 0 },
				new ReceiptEntity { Id = receiptId2, Location = "Store B", Date = day2, TaxAmount = 0 });

			context.Transactions.AddRange(
				new TransactionEntity { Id = Guid.NewGuid(), ReceiptId = receiptId1, AccountId = accountId, Amount = 50.00m, Date = day1 },
				new TransactionEntity { Id = Guid.NewGuid(), ReceiptId = receiptId1, AccountId = accountId, Amount = 25.00m, Date = day1 },
				new TransactionEntity { Id = Guid.NewGuid(), ReceiptId = receiptId2, AccountId = accountId, Amount = 100.00m, Date = day2 });

			await context.SaveChangesAsync();
		}

		DashboardService service = new(contextFactory);

		// Act
		SpendingOverTimeResult result = await service.GetSpendingOverTimeAsync(
			new DateOnly(2025, 3, 1),
			new DateOnly(2025, 3, 31),
			"daily",
			CancellationToken.None);

		// Assert
		result.Buckets.Should().HaveCount(2);
		result.Buckets[0].Period.Should().Be("2025-03-01");
		result.Buckets[0].Amount.Should().Be(75.00m);
		result.Buckets[1].Period.Should().Be("2025-03-02");
		result.Buckets[1].Amount.Should().Be(100.00m);

		contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task GetSpendingOverTimeAsync_DailyGranularity_EmptyRange_ReturnsEmptyBuckets()
	{
		// Arrange
		IDbContextFactory<ApplicationDbContext> contextFactory = DbContextHelpers.CreateInMemoryContextFactory();

		DashboardService service = new(contextFactory);

		// Act
		SpendingOverTimeResult result = await service.GetSpendingOverTimeAsync(
			new DateOnly(2025, 1, 1),
			new DateOnly(2025, 1, 31),
			"daily",
			CancellationToken.None);

		// Assert
		result.Buckets.Should().BeEmpty();

		contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task GetSpendingOverTimeAsync_DailyGranularity_MultipleTransactionsSameDay_AggregatesCorrectly()
	{
		// Arrange
		IDbContextFactory<ApplicationDbContext> contextFactory = DbContextHelpers.CreateInMemoryContextFactory();

		Guid receiptId = Guid.NewGuid();
		Guid accountId = Guid.NewGuid();
		DateOnly date = new(2025, 6, 15);

		await using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			context.Receipts.Add(
				new ReceiptEntity { Id = receiptId, Location = "Store", Date = date, TaxAmount = 0 });

			context.Transactions.AddRange(
				new TransactionEntity { Id = Guid.NewGuid(), ReceiptId = receiptId, AccountId = accountId, Amount = 10.00m, Date = date },
				new TransactionEntity { Id = Guid.NewGuid(), ReceiptId = receiptId, AccountId = accountId, Amount = 20.00m, Date = date },
				new TransactionEntity { Id = Guid.NewGuid(), ReceiptId = receiptId, AccountId = accountId, Amount = 30.00m, Date = date });

			await context.SaveChangesAsync();
		}

		DashboardService service = new(contextFactory);

		// Act
		SpendingOverTimeResult result = await service.GetSpendingOverTimeAsync(
			new DateOnly(2025, 6, 1),
			new DateOnly(2025, 6, 30),
			"daily",
			CancellationToken.None);

		// Assert
		result.Buckets.Should().ContainSingle();
		result.Buckets[0].Period.Should().Be("2025-06-15");
		result.Buckets[0].Amount.Should().Be(60.00m);

		contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task GetSpendingOverTimeAsync_DailyGranularity_ResultsAreOrderedByDate()
	{
		// Arrange
		IDbContextFactory<ApplicationDbContext> contextFactory = DbContextHelpers.CreateInMemoryContextFactory();

		Guid receiptId1 = Guid.NewGuid();
		Guid receiptId2 = Guid.NewGuid();
		Guid receiptId3 = Guid.NewGuid();
		Guid accountId = Guid.NewGuid();

		// Insert in non-chronological order to verify ordering
		DateOnly day3 = new(2025, 4, 3);
		DateOnly day1 = new(2025, 4, 1);
		DateOnly day2 = new(2025, 4, 2);

		await using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			context.Receipts.AddRange(
				new ReceiptEntity { Id = receiptId1, Location = "Store C", Date = day3, TaxAmount = 0 },
				new ReceiptEntity { Id = receiptId2, Location = "Store A", Date = day1, TaxAmount = 0 },
				new ReceiptEntity { Id = receiptId3, Location = "Store B", Date = day2, TaxAmount = 0 });

			context.Transactions.AddRange(
				new TransactionEntity { Id = Guid.NewGuid(), ReceiptId = receiptId1, AccountId = accountId, Amount = 30.00m, Date = day3 },
				new TransactionEntity { Id = Guid.NewGuid(), ReceiptId = receiptId2, AccountId = accountId, Amount = 10.00m, Date = day1 },
				new TransactionEntity { Id = Guid.NewGuid(), ReceiptId = receiptId3, AccountId = accountId, Amount = 20.00m, Date = day2 });

			await context.SaveChangesAsync();
		}

		DashboardService service = new(contextFactory);

		// Act
		SpendingOverTimeResult result = await service.GetSpendingOverTimeAsync(
			new DateOnly(2025, 4, 1),
			new DateOnly(2025, 4, 30),
			"daily",
			CancellationToken.None);

		// Assert
		result.Buckets.Should().HaveCount(3);
		result.Buckets.Select(b => b.Period).Should().BeInAscendingOrder();
		result.Buckets[0].Period.Should().Be("2025-04-01");
		result.Buckets[1].Period.Should().Be("2025-04-02");
		result.Buckets[2].Period.Should().Be("2025-04-03");

		contextFactory.ResetDatabase();
	}
}
