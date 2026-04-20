using Application.Models.Reports;
using Domain.NormalizedDescriptions;
using FluentAssertions;
using Infrastructure.Entities.Core;
using Infrastructure.IntegrationTests.Fixtures;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using SampleData.Entities;

namespace Infrastructure.IntegrationTests.Services;

[Collection(PostgresCollection.Name)]
[Trait("Category", "Integration")]
public class ReportServiceSpendingByNormalizedDescriptionTests(PostgresFixture fixture)
{
	[Fact]
	public async Task GetSpendingByNormalizedDescriptionAsync_AggregatesByCanonicalName_AndBucketsNullFk()
	{
		// Arrange
		await ResetTablesAsync();

		Guid normalizedId = Guid.NewGuid();
		ReceiptEntity receipt = ReceiptEntityGenerator.Generate();

		await using (ApplicationDbContext setup = fixture.CreateDbContext())
		{
			setup.NormalizedDescriptions.Add(new NormalizedDescriptionEntity
			{
				Id = normalizedId,
				CanonicalName = "Organic Milk",
				Status = NormalizedDescriptionStatus.Active,
				CreatedAt = DateTimeOffset.UtcNow,
			});

			setup.Receipts.Add(receipt);

			ReceiptItemEntity linked1 = ReceiptItemEntityGenerator.Generate(receipt.Id);
			linked1.Description = "organic milk";
			linked1.TotalAmount = 4.00m;
			linked1.NormalizedDescriptionId = normalizedId;

			ReceiptItemEntity linked2 = ReceiptItemEntityGenerator.Generate(receipt.Id);
			linked2.Description = "ORGANIC MILK";
			linked2.TotalAmount = 5.50m;
			linked2.NormalizedDescriptionId = normalizedId;

			ReceiptItemEntity unlinked = ReceiptItemEntityGenerator.Generate(receipt.Id);
			unlinked.Description = "mystery item";
			unlinked.TotalAmount = 2.00m;
			unlinked.NormalizedDescriptionId = null;

			setup.ReceiptItems.AddRange(linked1, linked2, unlinked);
			await setup.SaveChangesAsync();
		}

		ReportService service = new(new FixtureDbContextFactory(fixture));

		// Act
		SpendingByNormalizedDescriptionResult result = await service
			.GetSpendingByNormalizedDescriptionAsync(from: null, to: null, CancellationToken.None);

		// Assert
		result.Items.Should().HaveCount(2);

		SpendingByNormalizedDescriptionItem milk = result.Items.Single(i => i.CanonicalName == "Organic Milk");
		milk.TotalAmount.Should().Be(9.50m);
		milk.ItemCount.Should().Be(2);
		milk.Currency.Should().Be("USD");
		milk.FirstSeen.Should().NotBeNull();
		milk.LastSeen.Should().NotBeNull();

		SpendingByNormalizedDescriptionItem notNormalized = result.Items.Single(i => i.CanonicalName == "(Not Normalized)");
		notNormalized.TotalAmount.Should().Be(2.00m);
		notNormalized.ItemCount.Should().Be(1);
	}

	[Fact]
	public async Task GetSpendingByNormalizedDescriptionAsync_FiltersByDateRange()
	{
		// Arrange
		await ResetTablesAsync();

		Guid normalizedId = Guid.NewGuid();

		ReceiptEntity receiptInRange = ReceiptEntityGenerator.Generate();
		receiptInRange.Date = new DateOnly(2025, 6, 15);

		ReceiptEntity receiptOutOfRange = ReceiptEntityGenerator.Generate();
		receiptOutOfRange.Date = new DateOnly(2024, 1, 1);

		await using (ApplicationDbContext setup = fixture.CreateDbContext())
		{
			setup.NormalizedDescriptions.Add(new NormalizedDescriptionEntity
			{
				Id = normalizedId,
				CanonicalName = "Bananas",
				Status = NormalizedDescriptionStatus.Active,
				CreatedAt = DateTimeOffset.UtcNow,
			});
			setup.Receipts.AddRange(receiptInRange, receiptOutOfRange);

			ReceiptItemEntity inRange = ReceiptItemEntityGenerator.Generate(receiptInRange.Id);
			inRange.Description = "bananas";
			inRange.TotalAmount = 1.50m;
			inRange.NormalizedDescriptionId = normalizedId;

			ReceiptItemEntity outOfRange = ReceiptItemEntityGenerator.Generate(receiptOutOfRange.Id);
			outOfRange.Description = "bananas";
			outOfRange.TotalAmount = 99.99m;
			outOfRange.NormalizedDescriptionId = normalizedId;

			setup.ReceiptItems.AddRange(inRange, outOfRange);
			await setup.SaveChangesAsync();
		}

		ReportService service = new(new FixtureDbContextFactory(fixture));

		DateTimeOffset from = new(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
		DateTimeOffset to = new(2025, 12, 31, 0, 0, 0, TimeSpan.Zero);

		// Act
		SpendingByNormalizedDescriptionResult result = await service
			.GetSpendingByNormalizedDescriptionAsync(from, to, CancellationToken.None);

		// Assert — only the receipt within the range contributed
		result.Items.Should().ContainSingle();
		result.Items[0].CanonicalName.Should().Be("Bananas");
		result.Items[0].TotalAmount.Should().Be(1.50m);
		result.Items[0].ItemCount.Should().Be(1);
		result.FromDate.Should().Be(from);
		result.ToDate.Should().Be(to);
	}

	[Fact]
	public async Task GetSpendingByNormalizedDescriptionAsync_IgnoresSoftDeletedItemsAndReceipts()
	{
		// Arrange
		await ResetTablesAsync();

		Guid normalizedId = Guid.NewGuid();
		ReceiptEntity live = ReceiptEntityGenerator.Generate();
		ReceiptEntity deleted = ReceiptEntityGenerator.Generate();
		deleted.DeletedAt = DateTimeOffset.UtcNow;

		await using (ApplicationDbContext setup = fixture.CreateDbContext())
		{
			setup.NormalizedDescriptions.Add(new NormalizedDescriptionEntity
			{
				Id = normalizedId,
				CanonicalName = "Eggs",
				Status = NormalizedDescriptionStatus.Active,
				CreatedAt = DateTimeOffset.UtcNow,
			});

			setup.Receipts.AddRange(live, deleted);

			ReceiptItemEntity liveItem = ReceiptItemEntityGenerator.Generate(live.Id);
			liveItem.Description = "eggs";
			liveItem.TotalAmount = 3.00m;
			liveItem.NormalizedDescriptionId = normalizedId;

			ReceiptItemEntity softDeletedItem = ReceiptItemEntityGenerator.Generate(live.Id);
			softDeletedItem.Description = "eggs";
			softDeletedItem.TotalAmount = 100.00m;
			softDeletedItem.NormalizedDescriptionId = normalizedId;
			softDeletedItem.DeletedAt = DateTimeOffset.UtcNow;

			ReceiptItemEntity itemOnDeletedReceipt = ReceiptItemEntityGenerator.Generate(deleted.Id);
			itemOnDeletedReceipt.Description = "eggs";
			itemOnDeletedReceipt.TotalAmount = 50.00m;
			itemOnDeletedReceipt.NormalizedDescriptionId = normalizedId;

			setup.ReceiptItems.AddRange(liveItem, softDeletedItem, itemOnDeletedReceipt);
			await setup.SaveChangesAsync();
		}

		ReportService service = new(new FixtureDbContextFactory(fixture));

		// Act
		SpendingByNormalizedDescriptionResult result = await service
			.GetSpendingByNormalizedDescriptionAsync(null, null, CancellationToken.None);

		// Assert — only the live item on the live receipt counted
		result.Items.Should().ContainSingle();
		result.Items[0].CanonicalName.Should().Be("Eggs");
		result.Items[0].TotalAmount.Should().Be(3.00m);
		result.Items[0].ItemCount.Should().Be(1);
	}

	private async Task ResetTablesAsync()
	{
		await using ApplicationDbContext context = fixture.CreateDbContext();
		await context.Database.ExecuteSqlRawAsync(
			"""TRUNCATE "ReceiptItems", "Receipts", "NormalizedDescriptions", "DistinctDescriptions" RESTART IDENTITY CASCADE;""");
	}

	private sealed class FixtureDbContextFactory(PostgresFixture fixture) : IDbContextFactory<ApplicationDbContext>
	{
		public ApplicationDbContext CreateDbContext() => fixture.CreateDbContext();
	}
}
