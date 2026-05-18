using FluentAssertions;
using Infrastructure.Entities;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Tests.Services;

public class SampleDataSeederServiceTests
{
	private const string SampleDataSeedId = "SampleData_v1";

	/// <summary>
	/// Builds a service provider backed by a fresh in-memory database. The returned
	/// <paramref name="dbOptions"/> can be reused to open additional contexts against the
	/// same database for assertions.
	/// </summary>
	private static ServiceProvider BuildServiceProvider(out DbContextOptions<ApplicationDbContext> dbOptions)
	{
		dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
			.UseInMemoryDatabase(Guid.NewGuid().ToString())
			.Options;

		DbContextOptions<ApplicationDbContext> options = dbOptions;
		ServiceCollection services = new();
		services.AddSingleton<ILoggerFactory>(new LoggerFactory());
		services.AddScoped(_ => new ApplicationDbContext(options));
		return services.BuildServiceProvider();
	}

	[Fact]
	public async Task SeedAsync_PopulatesAccountsCardsReceiptsTransactionsAndItems()
	{
		// Arrange
		using ServiceProvider serviceProvider = BuildServiceProvider(out DbContextOptions<ApplicationDbContext> dbOptions);

		// Act
		await SampleDataSeederService.SeedAsync(serviceProvider);

		// Assert
		await using ApplicationDbContext db = new(dbOptions);
		int accounts = await db.Accounts.CountAsync();
		int cards = await db.Cards.CountAsync();
		int receipts = await db.Receipts.CountAsync();
		int items = await db.ReceiptItems.CountAsync();
		int transactions = await db.Transactions.CountAsync();

		accounts.Should().Be(3);
		cards.Should().Be(5);
		// ~4 receipts/week over 3 years — exact count depends on the run date, so assert a range.
		receipts.Should().BeInRange(400, 1000);
		// Every receipt carries one transaction and at least five line items.
		transactions.Should().Be(receipts);
		items.Should().BeGreaterThanOrEqualTo(receipts * 5);
	}

	[Fact]
	public async Task SeedAsync_RecordsSeedHistory()
	{
		// Arrange
		using ServiceProvider serviceProvider = BuildServiceProvider(out DbContextOptions<ApplicationDbContext> dbOptions);

		// Act
		await SampleDataSeederService.SeedAsync(serviceProvider);

		// Assert
		await using ApplicationDbContext db = new(dbOptions);
		bool recorded = await db.SeedHistory.AnyAsync(s => s.SeedId == SampleDataSeedId);
		recorded.Should().BeTrue();
	}

	[Fact]
	public async Task SeedAsync_WhenAlreadySeeded_SkipsSeeding()
	{
		// Arrange — mark the sample-data seed as already applied
		using ServiceProvider serviceProvider = BuildServiceProvider(out DbContextOptions<ApplicationDbContext> dbOptions);
		await using (ApplicationDbContext seedDb = new(dbOptions))
		{
			seedDb.SeedHistory.Add(new SeedHistoryEntry
			{
				SeedId = SampleDataSeedId,
				AppliedAt = DateTimeOffset.UtcNow,
			});
			await seedDb.SaveChangesAsync();
		}

		// Act
		await SampleDataSeederService.SeedAsync(serviceProvider);

		// Assert — nothing was generated
		await using ApplicationDbContext db = new(dbOptions);
		(await db.Receipts.CountAsync()).Should().Be(0);
		(await db.Accounts.CountAsync()).Should().Be(0);
	}

	[Fact]
	public async Task SeedAsync_CalledTwice_IsIdempotent()
	{
		// Arrange
		using ServiceProvider serviceProvider = BuildServiceProvider(out DbContextOptions<ApplicationDbContext> dbOptions);

		// Act — run twice
		await SampleDataSeederService.SeedAsync(serviceProvider);
		int receiptsAfterFirst;
		await using (ApplicationDbContext firstDb = new(dbOptions))
		{
			receiptsAfterFirst = await firstDb.Receipts.CountAsync();
		}

		await SampleDataSeederService.SeedAsync(serviceProvider);

		// Assert — the second call is a no-op
		await using ApplicationDbContext db = new(dbOptions);
		(await db.Receipts.CountAsync()).Should().Be(receiptsAfterFirst);
	}

	[Fact]
	public async Task SeedAsync_IsDeterministic_AcrossSeparateRuns()
	{
		// Arrange — two independent databases seeded on the same day
		using ServiceProvider providerA = BuildServiceProvider(out DbContextOptions<ApplicationDbContext> optionsA);
		using ServiceProvider providerB = BuildServiceProvider(out DbContextOptions<ApplicationDbContext> optionsB);

		// Act
		await SampleDataSeederService.SeedAsync(providerA);
		await SampleDataSeederService.SeedAsync(providerB);

		// Assert — the fixed RNG seed produces an identical dataset
		await using ApplicationDbContext dbA = new(optionsA);
		await using ApplicationDbContext dbB = new(optionsB);

		List<Guid> receiptIdsA = await dbA.Receipts.Select(r => r.Id).OrderBy(id => id).ToListAsync();
		List<Guid> receiptIdsB = await dbB.Receipts.Select(r => r.Id).OrderBy(id => id).ToListAsync();
		receiptIdsB.Should().Equal(receiptIdsA);

		(await dbB.ReceiptItems.CountAsync()).Should().Be(await dbA.ReceiptItems.CountAsync());
		(await dbB.Adjustments.CountAsync()).Should().Be(await dbA.Adjustments.CountAsync());
	}

	[Fact]
	public async Task SeedAsync_DoesNotWriteAuditLogs()
	{
		// Arrange
		using ServiceProvider serviceProvider = BuildServiceProvider(out DbContextOptions<ApplicationDbContext> dbOptions);

		// Act
		await SampleDataSeederService.SeedAsync(serviceProvider);

		// Assert — the bulk seed disables auditing, so no audit rows are produced
		await using ApplicationDbContext db = new(dbOptions);
		(await db.AuditLogs.AnyAsync()).Should().BeFalse();
	}

	[Fact]
	public async Task SeedAsync_GeneratesAdjustmentsAndItemizedReceipts()
	{
		// Arrange
		using ServiceProvider serviceProvider = BuildServiceProvider(out DbContextOptions<ApplicationDbContext> dbOptions);

		// Act
		await SampleDataSeederService.SeedAsync(serviceProvider);

		// Assert
		await using ApplicationDbContext db = new(dbOptions);

		// Restaurant tips and retail discounts produce some adjustments.
		(await db.Adjustments.CountAsync()).Should().BeGreaterThan(0);

		// Every receipt is itemized with at least five line items, each with a positive total.
		List<Guid> receiptIds = await db.Receipts.Select(r => r.Id).ToListAsync();
		Dictionary<Guid, int> itemCountByReceipt = await db.ReceiptItems
			.GroupBy(i => i.ReceiptId)
			.Select(g => new { ReceiptId = g.Key, Count = g.Count() })
			.ToDictionaryAsync(x => x.ReceiptId, x => x.Count);

		receiptIds.Should().OnlyContain(id => itemCountByReceipt.ContainsKey(id) && itemCountByReceipt[id] >= 5);
		(await db.ReceiptItems.AllAsync(i => i.TotalAmount > 0m)).Should().BeTrue();
	}
}
