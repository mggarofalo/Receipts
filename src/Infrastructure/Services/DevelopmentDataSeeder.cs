using Common;
using Infrastructure.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class DevelopmentDataSeeder(
	ApplicationDbContext dbContext,
	IHostEnvironment environment,
	ILogger<DevelopmentDataSeeder> logger)
{
	public async Task SeedAsync()
	{
		if (!environment.IsDevelopment())
		{
			logger.LogWarning("DevelopmentDataSeeder should only run in Development. Skipping.");
			return;
		}

		if (await dbContext.Accounts.AnyAsync())
		{
			logger.LogInformation("Database already contains data. Skipping seed.");
			return;
		}

		logger.LogInformation("Seeding development sample data...");

		List<AccountEntity> accounts = CreateAccounts();
		List<CategoryEntity> categories = CreateCategories();
		dbContext.Accounts.AddRange(accounts);
		dbContext.Categories.AddRange(categories);
		await dbContext.SaveChangesAsync();

		List<SubcategoryEntity> subcategories = CreateSubcategories(categories);
		dbContext.Subcategories.AddRange(subcategories);
		await dbContext.SaveChangesAsync();

		List<ItemTemplateEntity> templates = CreateItemTemplates();
		dbContext.ItemTemplates.AddRange(templates);

		List<ReceiptEntity> receipts = CreateReceipts();
		dbContext.Receipts.AddRange(receipts);
		await dbContext.SaveChangesAsync();

		List<ReceiptItemEntity> receiptItems = CreateReceiptItems(receipts, categories, subcategories);
		dbContext.ReceiptItems.AddRange(receiptItems);

		List<AdjustmentEntity> adjustments = CreateAdjustments(receipts);
		dbContext.Adjustments.AddRange(adjustments);

		List<TransactionEntity> transactions = CreateTransactions(receipts, accounts);
		dbContext.Transactions.AddRange(transactions);
		await dbContext.SaveChangesAsync();

		logger.LogInformation(
			"Seeded {Accounts} accounts, {Categories} categories, {Subcategories} subcategories, " +
			"{Templates} item templates, {Receipts} receipts, {Items} receipt items, " +
			"{Adjustments} adjustments, {Transactions} transactions.",
			accounts.Count, categories.Count, subcategories.Count,
			templates.Count, receipts.Count, receiptItems.Count,
			adjustments.Count, transactions.Count);
	}

	private static List<AccountEntity> CreateAccounts()
	{
		return
		[
			new() { Id = Guid.NewGuid(), AccountCode = "CHECKING", Name = "Primary Checking", IsActive = true },
			new() { Id = Guid.NewGuid(), AccountCode = "SAVINGS", Name = "Savings Account", IsActive = true },
			new() { Id = Guid.NewGuid(), AccountCode = "CREDIT-VISA", Name = "Visa Credit Card", IsActive = true },
			new() { Id = Guid.NewGuid(), AccountCode = "CREDIT-MC", Name = "Mastercard Credit Card", IsActive = true },
			new() { Id = Guid.NewGuid(), AccountCode = "CASH", Name = "Cash", IsActive = true },
		];
	}

	private static List<CategoryEntity> CreateCategories()
	{
		return
		[
			new() { Id = Guid.NewGuid(), Name = "Groceries", Description = "Food and household supplies" },
			new() { Id = Guid.NewGuid(), Name = "Dining", Description = "Restaurants, takeout, and delivery" },
			new() { Id = Guid.NewGuid(), Name = "Transportation", Description = "Gas, transit, parking, and rideshare" },
			new() { Id = Guid.NewGuid(), Name = "Shopping", Description = "Clothing, electronics, and general retail" },
			new() { Id = Guid.NewGuid(), Name = "Utilities", Description = "Electric, water, internet, and phone" },
		];
	}

	private static List<SubcategoryEntity> CreateSubcategories(List<CategoryEntity> categories)
	{
		CategoryEntity groceries = categories[0];
		CategoryEntity dining = categories[1];
		CategoryEntity transportation = categories[2];
		CategoryEntity shopping = categories[3];
		CategoryEntity utilities = categories[4];

		return
		[
			new() { Id = Guid.NewGuid(), CategoryId = groceries.Id, Name = "Produce", Description = "Fruits and vegetables" },
			new() { Id = Guid.NewGuid(), CategoryId = groceries.Id, Name = "Dairy", Description = "Milk, cheese, yogurt" },
			new() { Id = Guid.NewGuid(), CategoryId = groceries.Id, Name = "Meat & Seafood" },
			new() { Id = Guid.NewGuid(), CategoryId = groceries.Id, Name = "Bakery" },
			new() { Id = Guid.NewGuid(), CategoryId = dining.Id, Name = "Fast Food" },
			new() { Id = Guid.NewGuid(), CategoryId = dining.Id, Name = "Sit-Down Restaurant" },
			new() { Id = Guid.NewGuid(), CategoryId = dining.Id, Name = "Coffee Shop" },
			new() { Id = Guid.NewGuid(), CategoryId = transportation.Id, Name = "Gas" },
			new() { Id = Guid.NewGuid(), CategoryId = transportation.Id, Name = "Parking" },
			new() { Id = Guid.NewGuid(), CategoryId = shopping.Id, Name = "Electronics" },
			new() { Id = Guid.NewGuid(), CategoryId = shopping.Id, Name = "Clothing" },
			new() { Id = Guid.NewGuid(), CategoryId = utilities.Id, Name = "Electric" },
			new() { Id = Guid.NewGuid(), CategoryId = utilities.Id, Name = "Internet" },
		];
	}

	private static List<ItemTemplateEntity> CreateItemTemplates()
	{
		return
		[
			new() { Id = Guid.NewGuid(), Name = "Gallon of Milk", DefaultCategory = "Groceries", DefaultSubcategory = "Dairy", DefaultUnitPrice = 4.99m, DefaultUnitPriceCurrency = Currency.USD, DefaultPricingMode = "quantity", DefaultItemCode = "MILK-GAL" },
			new() { Id = Guid.NewGuid(), Name = "Loaf of Bread", DefaultCategory = "Groceries", DefaultSubcategory = "Bakery", DefaultUnitPrice = 3.49m, DefaultUnitPriceCurrency = Currency.USD, DefaultPricingMode = "quantity", DefaultItemCode = "BREAD" },
			new() { Id = Guid.NewGuid(), Name = "Coffee (Medium)", DefaultCategory = "Dining", DefaultSubcategory = "Coffee Shop", DefaultUnitPrice = 4.50m, DefaultUnitPriceCurrency = Currency.USD, DefaultPricingMode = "flat", DefaultItemCode = "COFFEE-M" },
			new() { Id = Guid.NewGuid(), Name = "Regular Unleaded Gas", DefaultCategory = "Transportation", DefaultSubcategory = "Gas", DefaultPricingMode = "quantity", DefaultItemCode = "GAS-REG" },
		];
	}

	private static List<ReceiptEntity> CreateReceipts()
	{
		return
		[
			new() { Id = Guid.NewGuid(), Description = "Weekly grocery run", Location = "Whole Foods Market", Date = new DateOnly(2026, 2, 15), TaxAmount = 3.42m, TaxAmountCurrency = Currency.USD },
			new() { Id = Guid.NewGuid(), Description = "Lunch with team", Location = "Chipotle", Date = new DateOnly(2026, 2, 18), TaxAmount = 1.87m, TaxAmountCurrency = Currency.USD },
			new() { Id = Guid.NewGuid(), Description = "Gas fill-up", Location = "Shell Station", Date = new DateOnly(2026, 2, 20), TaxAmount = 0.00m, TaxAmountCurrency = Currency.USD },
			new() { Id = Guid.NewGuid(), Description = "New headphones", Location = "Best Buy", Date = new DateOnly(2026, 2, 22), TaxAmount = 7.80m, TaxAmountCurrency = Currency.USD },
			new() { Id = Guid.NewGuid(), Description = "Morning coffee", Location = "Starbucks", Date = new DateOnly(2026, 2, 25), TaxAmount = 0.52m, TaxAmountCurrency = Currency.USD },
			new() { Id = Guid.NewGuid(), Description = "Internet bill - March", Location = "Comcast", Date = new DateOnly(2026, 3, 1), TaxAmount = 5.99m, TaxAmountCurrency = Currency.USD },
		];
	}

	private static List<ReceiptItemEntity> CreateReceiptItems(
		List<ReceiptEntity> receipts,
		List<CategoryEntity> categories,
		List<SubcategoryEntity> subcategories)
	{
		ReceiptEntity groceryReceipt = receipts[0];
		ReceiptEntity lunchReceipt = receipts[1];
		ReceiptEntity gasReceipt = receipts[2];
		ReceiptEntity electronicsReceipt = receipts[3];
		ReceiptEntity coffeeReceipt = receipts[4];
		ReceiptEntity internetReceipt = receipts[5];

		return
		[
			// Grocery items
			new() { Id = Guid.NewGuid(), ReceiptId = groceryReceipt.Id, ReceiptItemCode = "MILK-GAL", Description = "Organic Whole Milk", Quantity = 2, UnitPrice = 5.99m, UnitPriceCurrency = Currency.USD, TotalAmount = 11.98m, TotalAmountCurrency = Currency.USD, Category = categories[0].Name, Subcategory = subcategories[1].Name, PricingMode = PricingMode.Quantity },
			new() { Id = Guid.NewGuid(), ReceiptId = groceryReceipt.Id, ReceiptItemCode = "BREAD", Description = "Sourdough Bread", Quantity = 1, UnitPrice = 4.49m, UnitPriceCurrency = Currency.USD, TotalAmount = 4.49m, TotalAmountCurrency = Currency.USD, Category = categories[0].Name, Subcategory = subcategories[3].Name, PricingMode = PricingMode.Quantity },
			new() { Id = Guid.NewGuid(), ReceiptId = groceryReceipt.Id, ReceiptItemCode = "CHICKEN", Description = "Chicken Breast (2 lb)", Quantity = 1, UnitPrice = 9.99m, UnitPriceCurrency = Currency.USD, TotalAmount = 9.99m, TotalAmountCurrency = Currency.USD, Category = categories[0].Name, Subcategory = subcategories[2].Name, PricingMode = PricingMode.Flat },
			new() { Id = Guid.NewGuid(), ReceiptId = groceryReceipt.Id, ReceiptItemCode = "APPLES", Description = "Honeycrisp Apples", Quantity = 4, UnitPrice = 1.50m, UnitPriceCurrency = Currency.USD, TotalAmount = 6.00m, TotalAmountCurrency = Currency.USD, Category = categories[0].Name, Subcategory = subcategories[0].Name, PricingMode = PricingMode.Quantity },

			// Lunch items
			new() { Id = Guid.NewGuid(), ReceiptId = lunchReceipt.Id, ReceiptItemCode = "BURRITO", Description = "Chicken Burrito Bowl", Quantity = 1, UnitPrice = 11.25m, UnitPriceCurrency = Currency.USD, TotalAmount = 11.25m, TotalAmountCurrency = Currency.USD, Category = categories[1].Name, Subcategory = subcategories[4].Name, PricingMode = PricingMode.Flat },
			new() { Id = Guid.NewGuid(), ReceiptId = lunchReceipt.Id, ReceiptItemCode = "CHIPS", Description = "Chips & Guac", Quantity = 1, UnitPrice = 4.25m, UnitPriceCurrency = Currency.USD, TotalAmount = 4.25m, TotalAmountCurrency = Currency.USD, Category = categories[1].Name, Subcategory = subcategories[4].Name, PricingMode = PricingMode.Flat },
			new() { Id = Guid.NewGuid(), ReceiptId = lunchReceipt.Id, ReceiptItemCode = "DRINK", Description = "Fountain Drink", Quantity = 1, UnitPrice = 2.50m, UnitPriceCurrency = Currency.USD, TotalAmount = 2.50m, TotalAmountCurrency = Currency.USD, Category = categories[1].Name, Subcategory = subcategories[4].Name, PricingMode = PricingMode.Flat },

			// Gas
			new() { Id = Guid.NewGuid(), ReceiptId = gasReceipt.Id, ReceiptItemCode = "GAS-REG", Description = "Regular Unleaded (12.5 gal)", Quantity = 12.5m, UnitPrice = 3.29m, UnitPriceCurrency = Currency.USD, TotalAmount = 41.13m, TotalAmountCurrency = Currency.USD, Category = categories[2].Name, Subcategory = subcategories[7].Name, PricingMode = PricingMode.Quantity },

			// Electronics
			new() { Id = Guid.NewGuid(), ReceiptId = electronicsReceipt.Id, ReceiptItemCode = "HDPHN", Description = "Sony WH-1000XM5 Headphones", Quantity = 1, UnitPrice = 299.99m, UnitPriceCurrency = Currency.USD, TotalAmount = 299.99m, TotalAmountCurrency = Currency.USD, Category = categories[3].Name, Subcategory = subcategories[9].Name, PricingMode = PricingMode.Flat },

			// Coffee
			new() { Id = Guid.NewGuid(), ReceiptId = coffeeReceipt.Id, ReceiptItemCode = "COFFEE-M", Description = "Grande Latte", Quantity = 1, UnitPrice = 5.75m, UnitPriceCurrency = Currency.USD, TotalAmount = 5.75m, TotalAmountCurrency = Currency.USD, Category = categories[1].Name, Subcategory = subcategories[6].Name, PricingMode = PricingMode.Flat },

			// Internet bill
			new() { Id = Guid.NewGuid(), ReceiptId = internetReceipt.Id, ReceiptItemCode = "INET-MO", Description = "Internet Service - Monthly", Quantity = 1, UnitPrice = 79.99m, UnitPriceCurrency = Currency.USD, TotalAmount = 79.99m, TotalAmountCurrency = Currency.USD, Category = categories[4].Name, Subcategory = subcategories[12].Name, PricingMode = PricingMode.Flat },
		];
	}

	private static List<AdjustmentEntity> CreateAdjustments(List<ReceiptEntity> receipts)
	{
		return
		[
			// Tip at Chipotle
			new() { Id = Guid.NewGuid(), ReceiptId = receipts[1].Id, Type = AdjustmentType.Tip, Amount = 3.00m, AmountCurrency = Currency.USD, Description = "Tip" },
			// Loyalty discount at Whole Foods
			new() { Id = Guid.NewGuid(), ReceiptId = receipts[0].Id, Type = AdjustmentType.LoyaltyRedemption, Amount = -2.00m, AmountCurrency = Currency.USD, Description = "Prime member discount" },
			// Rounding at Shell
			new() { Id = Guid.NewGuid(), ReceiptId = receipts[2].Id, Type = AdjustmentType.Rounding, Amount = -0.01m, AmountCurrency = Currency.USD },
		];
	}

	private static List<TransactionEntity> CreateTransactions(
		List<ReceiptEntity> receipts,
		List<AccountEntity> accounts)
	{
		AccountEntity checking = accounts[0];
		AccountEntity visa = accounts[2];
		AccountEntity cash = accounts[4];

		// Grocery subtotal: 32.46, adj: -2.00, tax: 3.42 => total: 33.88
		// Lunch subtotal: 18.00, adj: 3.00, tax: 1.87 => total: 22.87
		// Gas subtotal: 41.13, adj: -0.01, tax: 0.00 => total: 41.12
		// Electronics subtotal: 299.99, tax: 7.80 => total: 307.79
		// Coffee subtotal: 5.75, tax: 0.52 => total: 6.27
		// Internet subtotal: 79.99, tax: 5.99 => total: 85.98

		return
		[
			new() { Id = Guid.NewGuid(), ReceiptId = receipts[0].Id, AccountId = visa.Id, Amount = 33.88m, AmountCurrency = Currency.USD, Date = receipts[0].Date },
			new() { Id = Guid.NewGuid(), ReceiptId = receipts[1].Id, AccountId = visa.Id, Amount = 22.87m, AmountCurrency = Currency.USD, Date = receipts[1].Date },
			new() { Id = Guid.NewGuid(), ReceiptId = receipts[2].Id, AccountId = checking.Id, Amount = 41.12m, AmountCurrency = Currency.USD, Date = receipts[2].Date },
			new() { Id = Guid.NewGuid(), ReceiptId = receipts[3].Id, AccountId = visa.Id, Amount = 307.79m, AmountCurrency = Currency.USD, Date = receipts[3].Date },
			new() { Id = Guid.NewGuid(), ReceiptId = receipts[4].Id, AccountId = cash.Id, Amount = 6.27m, AmountCurrency = Currency.USD, Date = receipts[4].Date },
			new() { Id = Guid.NewGuid(), ReceiptId = receipts[5].Id, AccountId = checking.Id, Amount = 85.98m, AmountCurrency = Currency.USD, Date = receipts[5].Date },
		];
	}
}
