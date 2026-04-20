using Application.Interfaces.Services;
using Application.Models.Reports;
using FluentAssertions;
using Infrastructure.Entities.Core;
using Infrastructure.IntegrationTests.Fixtures;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using SampleData.Entities;

namespace Infrastructure.IntegrationTests.Services;

[Collection(PostgresCollection.Name)]
[Trait("Category", "Integration")]
public class ReportServiceItemSimilarityTests(PostgresFixture fixture)
{
	[Fact]
	public async Task GetItemSimilarityAsync_ReturnsClusters_AfterBackgroundRefresh()
	{
		// Arrange
		await ResetItemAndEdgeTablesAsync();

		ReceiptEntity receipt = ReceiptEntityGenerator.Generate();
		List<ReceiptItemEntity> items =
		[
			WithDescription(receipt.Id, "COCA COLA"),
			WithDescription(receipt.Id, "COCA-COLA"),
			WithDescription(receipt.Id, "COCACOLA"),
			WithDescription(receipt.Id, "MILK GALLON"),
		];

		await using (ApplicationDbContext setup = fixture.CreateDbContext())
		{
			setup.Receipts.Add(receipt);
			setup.ReceiptItems.AddRange(items);
			await setup.SaveChangesAsync();
		}

		// DistinctDescriptions are populated by the DbContext reconciliation hook. However, the
		// test fixture's DbContext factory uses the parameterless ctor, so the reconciliation
		// doesn't run automatically here — seed it explicitly.
		await SeedDistinctDescriptionsFromReceiptItemsAsync();

		// Run a refresh cycle so the edges table is populated for the assertions.
		await RunRefreshAsync();

		ReportService service = new(new FixtureDbContextFactory(fixture));

		// Act
		ItemSimilarityResult result = await service.GetItemSimilarityAsync(
			threshold: 0.4,
			sortBy: "occurrences",
			sortDirection: "desc",
			page: 1,
			pageSize: 50,
			CancellationToken.None);

		// Assert
		result.Groups.Should().NotBeEmpty();
		ItemSimilarityGroup colaCluster = result.Groups
			.Should().ContainSingle(g => g.Variants.Any(v => v.Contains("COLA", StringComparison.OrdinalIgnoreCase)))
			.Subject;
		colaCluster.Variants.Should().Contain(["COCA COLA", "COCA-COLA", "COCACOLA"]);
		colaCluster.ItemIds.Should().HaveCount(3);
		result.ComputedAt.Should().NotBeNull("the refresher populated ComputedAt on each edge");
	}

	[Fact]
	public async Task GetItemSimilarityAsync_ReturnsEmpty_WhenNoSimilarDescriptions()
	{
		// Arrange
		await ResetItemAndEdgeTablesAsync();

		ReceiptEntity receipt = ReceiptEntityGenerator.Generate();
		List<ReceiptItemEntity> items =
		[
			WithDescription(receipt.Id, "BREAD"),
			WithDescription(receipt.Id, "GASOLINE"),
			WithDescription(receipt.Id, "NOTEBOOK"),
		];

		await using (ApplicationDbContext setup = fixture.CreateDbContext())
		{
			setup.Receipts.Add(receipt);
			setup.ReceiptItems.AddRange(items);
			await setup.SaveChangesAsync();
		}

		await SeedDistinctDescriptionsFromReceiptItemsAsync();
		await RunRefreshAsync();

		ReportService service = new(new FixtureDbContextFactory(fixture));

		// Act
		ItemSimilarityResult result = await service.GetItemSimilarityAsync(
			threshold: 0.4,
			sortBy: "occurrences",
			sortDirection: "desc",
			page: 1,
			pageSize: 50,
			CancellationToken.None);

		// Assert
		result.Groups.Should().BeEmpty();
		result.TotalCount.Should().Be(0);
	}

	[Fact]
	public async Task ItemSimilarityEdgeRefresher_RepeatsProduceSameEdgeSet()
	{
		// Arrange
		await ResetItemAndEdgeTablesAsync();

		ReceiptEntity receipt = ReceiptEntityGenerator.Generate();
		await using (ApplicationDbContext setup = fixture.CreateDbContext())
		{
			setup.Receipts.Add(receipt);
			setup.ReceiptItems.AddRange(
				WithDescription(receipt.Id, "BANANA"),
				WithDescription(receipt.Id, "BANANAS"),
				WithDescription(receipt.Id, "APPLE"));
			await setup.SaveChangesAsync();
		}

		await SeedDistinctDescriptionsFromReceiptItemsAsync();

		// Act — run twice in a row
		await RunRefreshAsync();
		int firstCount = await EdgeCountAsync();
		await RunRefreshAsync();
		int secondCount = await EdgeCountAsync();

		// Assert — idempotent: re-running doesn't duplicate or lose edges
		secondCount.Should().Be(firstCount);
		firstCount.Should().BeGreaterThan(0, "BANANA and BANANAS share trigrams above 0.3");
	}

	[Fact]
	public async Task ItemSimilarityEdgeRefresher_CascadesEdgeDeletion_WhenDescriptionRemoved()
	{
		// Arrange
		await ResetItemAndEdgeTablesAsync();

		ReceiptEntity receipt = ReceiptEntityGenerator.Generate();
		await using (ApplicationDbContext setup = fixture.CreateDbContext())
		{
			setup.Receipts.Add(receipt);
			setup.ReceiptItems.AddRange(
				WithDescription(receipt.Id, "ORANGE"),
				WithDescription(receipt.Id, "ORANGES"));
			await setup.SaveChangesAsync();
		}

		await SeedDistinctDescriptionsFromReceiptItemsAsync();
		await RunRefreshAsync();
		(await EdgeCountAsync()).Should().BeGreaterThan(0);

		// Remove one description entirely — should cascade-delete edges referencing it.
		// Raw SQL (not EF): DistinctDescriptions has no Id column, so EF change tracking
		// on delete trips code paths that assume every tracked entity has an Id property.
		await using (ApplicationDbContext remove = fixture.CreateDbContext())
		{
			int rowsDeleted = await remove.Database.ExecuteSqlInterpolatedAsync(
				$"""DELETE FROM "DistinctDescriptions" WHERE "Description" = {"ORANGE"};""");
			rowsDeleted.Should().Be(1, "the ORANGE description must exist before the cascade test");
		}

		// Assert — edges involving ORANGE are gone (FK ON DELETE CASCADE)
		(await EdgeCountAsync()).Should().Be(0);
	}

	private async Task ResetItemAndEdgeTablesAsync()
	{
		await using ApplicationDbContext context = fixture.CreateDbContext();
		// DistinctDescriptions cascades to ItemSimilarityEdges via FK; TRUNCATE CASCADE covers both.
		await context.Database.ExecuteSqlRawAsync(
			"""TRUNCATE "ReceiptItems", "Receipts", "DistinctDescriptions" RESTART IDENTITY CASCADE;""");
	}

	private async Task SeedDistinctDescriptionsFromReceiptItemsAsync()
	{
		await using ApplicationDbContext context = fixture.CreateDbContext();
		await context.Database.ExecuteSqlRawAsync(
			"""
			INSERT INTO "DistinctDescriptions" ("Description", "ProcessedAt")
			SELECT DISTINCT "Description", NULL::timestamptz
			FROM "ReceiptItems"
			WHERE "DeletedAt" IS NULL
			ON CONFLICT DO NOTHING;
			""");
	}

	private async Task RunRefreshAsync()
	{
		// Build a minimal DI scope exposing the fixture's context factory, then call
		// ItemSimilarityEdgeRefresher.RefreshAsync directly (bypasses the ExecuteAsync loop).
		ServiceCollection services = new();
		services.AddSingleton<IDbContextFactory<ApplicationDbContext>>(new FixtureDbContextFactory(fixture));
		services.AddSingleton<IDescriptionChangeSignal, DescriptionChangeSignal>();
		ServiceProvider provider = services.BuildServiceProvider();

		ItemSimilarityEdgeRefresher refresher = new(
			provider.GetRequiredService<IServiceScopeFactory>(),
			provider.GetRequiredService<IDescriptionChangeSignal>(),
			NullLogger<ItemSimilarityEdgeRefresher>.Instance);

		await refresher.RefreshAsync(CancellationToken.None);
	}

	private async Task<int> EdgeCountAsync()
	{
		await using ApplicationDbContext context = fixture.CreateDbContext();
		return await context.ItemSimilarityEdges.AsNoTracking().CountAsync();
	}

	private static ReceiptItemEntity WithDescription(Guid receiptId, string description)
	{
		ReceiptItemEntity item = ReceiptItemEntityGenerator.Generate(receiptId);
		item.Description = description;
		return item;
	}

	private sealed class FixtureDbContextFactory(PostgresFixture fixture) : IDbContextFactory<ApplicationDbContext>
	{
		public ApplicationDbContext CreateDbContext() => fixture.CreateDbContext();
	}
}
