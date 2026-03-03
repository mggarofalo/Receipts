using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Infrastructure.Tests.Services;

public class DevelopmentDataSeederTests
{
	private static ApplicationDbContext CreateInMemoryContext()
	{
		DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
			.UseInMemoryDatabase($"SeederTest_{Guid.NewGuid()}")
			.Options;
		return new ApplicationDbContext(options);
	}

	private static Mock<IHostEnvironment> CreateDevelopmentEnvironment()
	{
		Mock<IHostEnvironment> mock = new();
		mock.Setup(e => e.EnvironmentName).Returns(Environments.Development);
		return mock;
	}

	private static Mock<IHostEnvironment> CreateProductionEnvironment()
	{
		Mock<IHostEnvironment> mock = new();
		mock.Setup(e => e.EnvironmentName).Returns(Environments.Production);
		return mock;
	}

	[Fact]
	public async Task SeedAsync_InDevelopment_SeedsAllEntityTypes()
	{
		// Arrange
		using ApplicationDbContext dbContext = CreateInMemoryContext();
		DevelopmentDataSeeder seeder = new(
			dbContext,
			CreateDevelopmentEnvironment().Object,
			NullLogger<DevelopmentDataSeeder>.Instance);

		// Act
		await seeder.SeedAsync();

		// Assert
		Assert.True(await dbContext.Accounts.AnyAsync());
		Assert.True(await dbContext.Categories.AnyAsync());
		Assert.True(await dbContext.Subcategories.AnyAsync());
		Assert.True(await dbContext.Receipts.AnyAsync());
		Assert.True(await dbContext.ReceiptItems.AnyAsync());
		Assert.True(await dbContext.Transactions.AnyAsync());
		Assert.True(await dbContext.Adjustments.AnyAsync());
		Assert.True(await dbContext.ItemTemplates.AnyAsync());
	}

	[Fact]
	public async Task SeedAsync_InDevelopment_SeedsExpectedCounts()
	{
		// Arrange
		using ApplicationDbContext dbContext = CreateInMemoryContext();
		DevelopmentDataSeeder seeder = new(
			dbContext,
			CreateDevelopmentEnvironment().Object,
			NullLogger<DevelopmentDataSeeder>.Instance);

		// Act
		await seeder.SeedAsync();

		// Assert
		int expectedAccounts = 5;
		int expectedCategories = 5;
		int expectedSubcategories = 13;
		int expectedReceipts = 6;
		int expectedTemplates = 4;

		Assert.Equal(expectedAccounts, await dbContext.Accounts.CountAsync());
		Assert.Equal(expectedCategories, await dbContext.Categories.CountAsync());
		Assert.Equal(expectedSubcategories, await dbContext.Subcategories.CountAsync());
		Assert.Equal(expectedReceipts, await dbContext.Receipts.CountAsync());
		Assert.Equal(expectedTemplates, await dbContext.ItemTemplates.CountAsync());
	}

	[Fact]
	public async Task SeedAsync_IsIdempotent_DoesNotDuplicateData()
	{
		// Arrange
		using ApplicationDbContext dbContext = CreateInMemoryContext();
		DevelopmentDataSeeder seeder = new(
			dbContext,
			CreateDevelopmentEnvironment().Object,
			NullLogger<DevelopmentDataSeeder>.Instance);

		// Act
		await seeder.SeedAsync();
		int accountsAfterFirstSeed = await dbContext.Accounts.CountAsync();

		await seeder.SeedAsync();
		int accountsAfterSecondSeed = await dbContext.Accounts.CountAsync();

		// Assert
		Assert.Equal(accountsAfterFirstSeed, accountsAfterSecondSeed);
	}

	[Fact]
	public async Task SeedAsync_InProduction_DoesNotSeedData()
	{
		// Arrange
		using ApplicationDbContext dbContext = CreateInMemoryContext();
		DevelopmentDataSeeder seeder = new(
			dbContext,
			CreateProductionEnvironment().Object,
			NullLogger<DevelopmentDataSeeder>.Instance);

		// Act
		await seeder.SeedAsync();

		// Assert
		Assert.False(await dbContext.Accounts.AnyAsync());
		Assert.False(await dbContext.Receipts.AnyAsync());
	}

	[Fact]
	public async Task SeedAsync_TransactionsReferenceValidReceiptsAndAccounts()
	{
		// Arrange
		using ApplicationDbContext dbContext = CreateInMemoryContext();
		DevelopmentDataSeeder seeder = new(
			dbContext,
			CreateDevelopmentEnvironment().Object,
			NullLogger<DevelopmentDataSeeder>.Instance);

		// Act
		await seeder.SeedAsync();

		// Assert
		List<Guid> receiptIds = await dbContext.Receipts.Select(r => r.Id).ToListAsync();
		List<Guid> accountIds = await dbContext.Accounts.Select(a => a.Id).ToListAsync();

		var transactions = await dbContext.Transactions.ToListAsync();
		foreach (var transaction in transactions)
		{
			Assert.Contains(transaction.ReceiptId, receiptIds);
			Assert.Contains(transaction.AccountId, accountIds);
		}
	}
}
