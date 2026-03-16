using Common;
using FluentAssertions;
using Infrastructure.Entities;
using Infrastructure.Entities.Core;
using Infrastructure.IntegrationTests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Pgvector;
using SampleData.Entities;

namespace Infrastructure.IntegrationTests;

[Collection(PostgresCollection.Name)]
[Trait("Category", "Integration")]
public class ColumnTypeMappingTests(PostgresFixture fixture)
{
	[Fact]
	public async Task AccountEntity_RoundTrips_AllColumnTypes()
	{
		// Arrange — uuid, text, boolean
		await using ApplicationDbContext context = fixture.CreateDbContext();
		AccountEntity account = AccountEntityGenerator.Generate();

		// Act
		context.Accounts.Add(account);
		await context.SaveChangesAsync();

		// Assert
		await using ApplicationDbContext readContext = fixture.CreateDbContext();
		AccountEntity? loaded = await readContext.Accounts.FirstOrDefaultAsync(a => a.Id == account.Id);

		loaded.Should().NotBeNull();
		loaded!.Id.Should().Be(account.Id);
		loaded.AccountCode.Should().Be(account.AccountCode);
		loaded.Name.Should().Be(account.Name);
		loaded.IsActive.Should().Be(account.IsActive);
	}

	[Fact]
	public async Task ReceiptEntity_RoundTrips_DecimalAndDateOnly()
	{
		// Arrange — decimal(18,2), date, uuid, text, enum-to-text
		await using ApplicationDbContext context = fixture.CreateDbContext();
		ReceiptEntity receipt = ReceiptEntityGenerator.Generate();

		// Act
		context.Receipts.Add(receipt);
		await context.SaveChangesAsync();

		// Assert
		await using ApplicationDbContext readContext = fixture.CreateDbContext();
		ReceiptEntity? loaded = await readContext.Receipts.FirstOrDefaultAsync(r => r.Id == receipt.Id);

		loaded.Should().NotBeNull();
		loaded!.Location.Should().Be(receipt.Location);
		loaded.Date.Should().Be(receipt.Date);
		loaded.TaxAmount.Should().Be(receipt.TaxAmount);
		loaded.TaxAmountCurrency.Should().Be(Currency.USD);
	}

	[Fact]
	public async Task TransactionEntity_RoundTrips_WithForeignKeys()
	{
		// Arrange — FK to Receipt and Account
		await using ApplicationDbContext context = fixture.CreateDbContext();
		ReceiptEntity receipt = ReceiptEntityGenerator.Generate();
		AccountEntity account = AccountEntityGenerator.Generate();
		context.Receipts.Add(receipt);
		context.Accounts.Add(account);
		await context.SaveChangesAsync();

		TransactionEntity transaction = TransactionEntityGenerator.Generate(receipt.Id, account.Id);

		// Act
		context.Transactions.Add(transaction);
		await context.SaveChangesAsync();

		// Assert
		await using ApplicationDbContext readContext = fixture.CreateDbContext();
		TransactionEntity? loaded = await readContext.Transactions.FirstOrDefaultAsync(t => t.Id == transaction.Id);

		loaded.Should().NotBeNull();
		loaded!.ReceiptId.Should().Be(receipt.Id);
		loaded.AccountId.Should().Be(account.Id);
		loaded.Amount.Should().Be(transaction.Amount);
		loaded.AmountCurrency.Should().Be(Currency.USD);
		loaded.Date.Should().Be(transaction.Date);
	}

	[Fact]
	public async Task ReceiptItemEntity_RoundTrips_WithEnumToStringConversion()
	{
		// Arrange — PricingMode enum stored as text
		await using ApplicationDbContext context = fixture.CreateDbContext();
		ReceiptEntity receipt = ReceiptEntityGenerator.Generate();
		context.Receipts.Add(receipt);
		await context.SaveChangesAsync();

		ReceiptItemEntity item = ReceiptItemEntityGenerator.Generate(receipt.Id, PricingMode.Flat);

		// Act
		context.ReceiptItems.Add(item);
		await context.SaveChangesAsync();

		// Assert
		await using ApplicationDbContext readContext = fixture.CreateDbContext();
		ReceiptItemEntity? loaded = await readContext.ReceiptItems.FirstOrDefaultAsync(i => i.Id == item.Id);

		loaded.Should().NotBeNull();
		loaded!.PricingMode.Should().Be(PricingMode.Flat);
		loaded.Quantity.Should().Be(item.Quantity);
		loaded.UnitPrice.Should().Be(item.UnitPrice);
		loaded.TotalAmount.Should().Be(item.TotalAmount);
		loaded.Category.Should().Be(item.Category);
	}

	[Fact]
	public async Task AdjustmentEntity_RoundTrips_WithEnumToStringConversion()
	{
		// Arrange — AdjustmentType enum stored as text
		await using ApplicationDbContext context = fixture.CreateDbContext();
		ReceiptEntity receipt = ReceiptEntityGenerator.Generate();
		context.Receipts.Add(receipt);
		await context.SaveChangesAsync();

		AdjustmentEntity adjustment = new()
		{
			Id = Guid.NewGuid(),
			ReceiptId = receipt.Id,
			Type = AdjustmentType.Coupon,
			Amount = 3.50m,
			AmountCurrency = Currency.USD,
			Description = "Test coupon",
		};

		// Act
		context.Adjustments.Add(adjustment);
		await context.SaveChangesAsync();

		// Assert
		await using ApplicationDbContext readContext = fixture.CreateDbContext();
		AdjustmentEntity? loaded = await readContext.Adjustments.FirstOrDefaultAsync(a => a.Id == adjustment.Id);

		loaded.Should().NotBeNull();
		loaded!.Type.Should().Be(AdjustmentType.Coupon);
		loaded.Amount.Should().Be(3.50m);
		loaded.Description.Should().Be("Test coupon");
	}

	[Fact]
	public async Task CategoryAndSubcategory_RoundTrip_WithForeignKey()
	{
		// Arrange
		await using ApplicationDbContext context = fixture.CreateDbContext();
		CategoryEntity category = CategoryEntityGenerator.Generate();
		context.Categories.Add(category);
		await context.SaveChangesAsync();

		SubcategoryEntity subcategory = new()
		{
			Id = Guid.NewGuid(),
			Name = "Test Sub",
			CategoryId = category.Id,
			Description = "Sub description",
		};
		context.Subcategories.Add(subcategory);
		await context.SaveChangesAsync();

		// Assert
		await using ApplicationDbContext readContext = fixture.CreateDbContext();
		SubcategoryEntity? loaded = await readContext.Subcategories
			.Include(s => s.Category)
			.FirstOrDefaultAsync(s => s.Id == subcategory.Id);

		loaded.Should().NotBeNull();
		loaded!.CategoryId.Should().Be(category.Id);
		loaded.Category.Should().NotBeNull();
		loaded.Category!.Name.Should().Be(category.Name);
	}

	[Fact]
	public async Task ItemTemplateEntity_RoundTrips_AllFields()
	{
		// Arrange
		await using ApplicationDbContext context = fixture.CreateDbContext();
		ItemTemplateEntity template = new()
		{
			Id = Guid.NewGuid(),
			Name = $"Template_{Guid.NewGuid():N}",
			DefaultCategory = "Groceries",
			DefaultSubcategory = "Produce",
			DefaultUnitPrice = 2.99m,
			DefaultUnitPriceCurrency = Currency.USD,
			DefaultPricingMode = "quantity",
			DefaultItemCode = "PROD001",
			Description = "Fresh produce template",
		};

		// Act
		context.ItemTemplates.Add(template);
		await context.SaveChangesAsync();

		// Assert
		await using ApplicationDbContext readContext = fixture.CreateDbContext();
		ItemTemplateEntity? loaded = await readContext.ItemTemplates.FirstOrDefaultAsync(t => t.Id == template.Id);

		loaded.Should().NotBeNull();
		loaded!.Name.Should().Be(template.Name);
		loaded.DefaultCategory.Should().Be("Groceries");
		loaded.DefaultUnitPrice.Should().Be(2.99m);
		loaded.DefaultPricingMode.Should().Be("quantity");
	}

	[Fact]
	public async Task ItemEmbeddingEntity_RoundTrips_VectorColumn()
	{
		// Arrange — pgvector column type
		await using ApplicationDbContext context = fixture.CreateDbContext();
		float[] values = new float[384];
		for (int i = 0; i < values.Length; i++)
		{
			values[i] = i * 0.001f;
		}

		ItemEmbeddingEntity embedding = new()
		{
			Id = Guid.NewGuid(),
			EntityType = "ItemTemplate",
			EntityId = Guid.NewGuid(),
			EntityText = "Test embedding text",
			Embedding = new Vector(values),
			ModelVersion = "all-MiniLM-L6-v2",
			CreatedAt = DateTimeOffset.UtcNow,
		};

		// Act
		context.ItemEmbeddings.Add(embedding);
		await context.SaveChangesAsync();

		// Assert
		await using ApplicationDbContext readContext = fixture.CreateDbContext();
		ItemEmbeddingEntity? loaded = await readContext.ItemEmbeddings
			.FirstOrDefaultAsync(e => e.Id == embedding.Id);

		loaded.Should().NotBeNull();
		loaded!.EntityType.Should().Be("ItemTemplate");
		loaded.Embedding.ToArray().Should().HaveCount(384);
		loaded.Embedding.ToArray()[0].Should().BeApproximately(0f, 0.001f);
		loaded.ModelVersion.Should().Be("all-MiniLM-L6-v2");
	}
}
