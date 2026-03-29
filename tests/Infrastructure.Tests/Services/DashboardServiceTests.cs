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
	public async Task GetSpendingOverTimeAsync_MonthlyGranularity_ReturnsCorrectBuckets()
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
			"monthly",
			CancellationToken.None);

		// Assert
		result.Buckets.Should().ContainSingle();
		result.Buckets[0].Period.Should().Be("2025-03");
		result.Buckets[0].Amount.Should().Be(175.00m);

		contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task GetSpendingOverTimeAsync_MonthlyGranularity_EmptyRange_ReturnsEmptyBuckets()
	{
		// Arrange
		IDbContextFactory<ApplicationDbContext> contextFactory = DbContextHelpers.CreateInMemoryContextFactory();

		DashboardService service = new(contextFactory);

		// Act
		SpendingOverTimeResult result = await service.GetSpendingOverTimeAsync(
			new DateOnly(2025, 1, 1),
			new DateOnly(2025, 1, 31),
			"monthly",
			CancellationToken.None);

		// Assert
		result.Buckets.Should().BeEmpty();

		contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task GetSpendingOverTimeAsync_MonthlyGranularity_MultipleTransactionsSameMonth_AggregatesCorrectly()
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
			"monthly",
			CancellationToken.None);

		// Assert
		result.Buckets.Should().ContainSingle();
		result.Buckets[0].Period.Should().Be("2025-06");
		result.Buckets[0].Amount.Should().Be(60.00m);

		contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task GetSpendingOverTimeAsync_QuarterlyGranularity_ReturnsCorrectBuckets()
	{
		// Arrange
		IDbContextFactory<ApplicationDbContext> contextFactory = DbContextHelpers.CreateInMemoryContextFactory();

		Guid receiptId1 = Guid.NewGuid();
		Guid receiptId2 = Guid.NewGuid();
		Guid receiptId3 = Guid.NewGuid();
		Guid accountId = Guid.NewGuid();

		await using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			context.Receipts.AddRange(
				new ReceiptEntity { Id = receiptId1, Location = "Store A", Date = new DateOnly(2025, 1, 15), TaxAmount = 0 },
				new ReceiptEntity { Id = receiptId2, Location = "Store B", Date = new DateOnly(2025, 4, 10), TaxAmount = 0 },
				new ReceiptEntity { Id = receiptId3, Location = "Store C", Date = new DateOnly(2025, 7, 20), TaxAmount = 0 });

			context.Transactions.AddRange(
				new TransactionEntity { Id = Guid.NewGuid(), ReceiptId = receiptId1, AccountId = accountId, Amount = 100.00m, Date = new DateOnly(2025, 1, 15) },
				new TransactionEntity { Id = Guid.NewGuid(), ReceiptId = receiptId2, AccountId = accountId, Amount = 200.00m, Date = new DateOnly(2025, 4, 10) },
				new TransactionEntity { Id = Guid.NewGuid(), ReceiptId = receiptId3, AccountId = accountId, Amount = 300.00m, Date = new DateOnly(2025, 7, 20) });

			await context.SaveChangesAsync();
		}

		DashboardService service = new(contextFactory);

		// Act
		SpendingOverTimeResult result = await service.GetSpendingOverTimeAsync(
			new DateOnly(2025, 1, 1),
			new DateOnly(2025, 12, 31),
			"quarterly",
			CancellationToken.None);

		// Assert
		result.Buckets.Should().HaveCount(3);
		result.Buckets[0].Period.Should().Be("2025 Q1");
		result.Buckets[0].Amount.Should().Be(100.00m);
		result.Buckets[1].Period.Should().Be("2025 Q2");
		result.Buckets[1].Amount.Should().Be(200.00m);
		result.Buckets[2].Period.Should().Be("2025 Q3");
		result.Buckets[2].Amount.Should().Be(300.00m);

		contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task GetSpendingOverTimeAsync_YtdGranularity_ReturnsMonthlyBuckets()
	{
		// Arrange
		IDbContextFactory<ApplicationDbContext> contextFactory = DbContextHelpers.CreateInMemoryContextFactory();

		Guid receiptId1 = Guid.NewGuid();
		Guid receiptId2 = Guid.NewGuid();
		Guid accountId = Guid.NewGuid();

		await using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			context.Receipts.AddRange(
				new ReceiptEntity { Id = receiptId1, Location = "Store A", Date = new DateOnly(2025, 1, 10), TaxAmount = 0 },
				new ReceiptEntity { Id = receiptId2, Location = "Store B", Date = new DateOnly(2025, 2, 15), TaxAmount = 0 });

			context.Transactions.AddRange(
				new TransactionEntity { Id = Guid.NewGuid(), ReceiptId = receiptId1, AccountId = accountId, Amount = 50.00m, Date = new DateOnly(2025, 1, 10) },
				new TransactionEntity { Id = Guid.NewGuid(), ReceiptId = receiptId2, AccountId = accountId, Amount = 75.00m, Date = new DateOnly(2025, 2, 15) });

			await context.SaveChangesAsync();
		}

		DashboardService service = new(contextFactory);

		// Act
		SpendingOverTimeResult result = await service.GetSpendingOverTimeAsync(
			new DateOnly(2025, 1, 1),
			new DateOnly(2025, 3, 31),
			"ytd",
			CancellationToken.None);

		// Assert
		result.Buckets.Should().HaveCount(2);
		result.Buckets[0].Period.Should().Be("2025-01");
		result.Buckets[0].Amount.Should().Be(50.00m);
		result.Buckets[1].Period.Should().Be("2025-02");
		result.Buckets[1].Amount.Should().Be(75.00m);

		contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task GetSpendingOverTimeAsync_YearlyGranularity_ReturnsCorrectBuckets()
	{
		// Arrange
		IDbContextFactory<ApplicationDbContext> contextFactory = DbContextHelpers.CreateInMemoryContextFactory();

		Guid receiptId1 = Guid.NewGuid();
		Guid receiptId2 = Guid.NewGuid();
		Guid accountId = Guid.NewGuid();

		await using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			context.Receipts.AddRange(
				new ReceiptEntity { Id = receiptId1, Location = "Store A", Date = new DateOnly(2024, 6, 15), TaxAmount = 0 },
				new ReceiptEntity { Id = receiptId2, Location = "Store B", Date = new DateOnly(2025, 3, 10), TaxAmount = 0 });

			context.Transactions.AddRange(
				new TransactionEntity { Id = Guid.NewGuid(), ReceiptId = receiptId1, AccountId = accountId, Amount = 100.00m, Date = new DateOnly(2024, 6, 15) },
				new TransactionEntity { Id = Guid.NewGuid(), ReceiptId = receiptId2, AccountId = accountId, Amount = 200.00m, Date = new DateOnly(2025, 3, 10) });

			await context.SaveChangesAsync();
		}

		DashboardService service = new(contextFactory);

		// Act
		SpendingOverTimeResult result = await service.GetSpendingOverTimeAsync(
			new DateOnly(2024, 1, 1),
			new DateOnly(2025, 12, 31),
			"yearly",
			CancellationToken.None);

		// Assert
		result.Buckets.Should().HaveCount(2);
		result.Buckets[0].Period.Should().Be("2024");
		result.Buckets[0].Amount.Should().Be(100.00m);
		result.Buckets[1].Period.Should().Be("2025");
		result.Buckets[1].Amount.Should().Be(200.00m);

		contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task GetEarliestReceiptYearAsync_ReturnsEarliestYear()
	{
		// Arrange
		IDbContextFactory<ApplicationDbContext> contextFactory = DbContextHelpers.CreateInMemoryContextFactory();

		Guid receiptId1 = Guid.NewGuid();
		Guid receiptId2 = Guid.NewGuid();

		await using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			context.Receipts.AddRange(
				new ReceiptEntity { Id = receiptId1, Location = "Store A", Date = new DateOnly(2022, 3, 1), TaxAmount = 0 },
				new ReceiptEntity { Id = receiptId2, Location = "Store B", Date = new DateOnly(2025, 1, 15), TaxAmount = 0 });

			await context.SaveChangesAsync();
		}

		DashboardService service = new(contextFactory);

		// Act
		int result = await service.GetEarliestReceiptYearAsync(CancellationToken.None);

		// Assert
		result.Should().Be(2022);

		contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task GetEarliestReceiptYearAsync_ReturnsCurrentYear_WhenNoReceipts()
	{
		// Arrange
		IDbContextFactory<ApplicationDbContext> contextFactory = DbContextHelpers.CreateInMemoryContextFactory();

		DashboardService service = new(contextFactory);

		// Act
		int result = await service.GetEarliestReceiptYearAsync(CancellationToken.None);

		// Assert
		result.Should().Be(DateTime.Today.Year);

		contextFactory.ResetDatabase();
	}
}
