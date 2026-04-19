using Application.Models.Reports;
using FluentAssertions;
using Infrastructure.Entities.Core;
using Infrastructure.IntegrationTests.Fixtures;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using SampleData.Entities;

namespace Infrastructure.IntegrationTests.Services;

[Collection(PostgresCollection.Name)]
[Trait("Category", "Integration")]
public class ReportServiceItemSimilarityTests(PostgresFixture fixture)
{
	[Fact]
	public async Task GetItemSimilarityAsync_ReturnsClusters_WithoutDisposingBorrowedConnection()
	{
		// Regression test for RECEIPTS-556: before the fix, this threw
		// ObjectDisposedException on the ReceiptItems follow-up query because the
		// raw-SQL block disposed the DbContext's borrowed NpgsqlConnection.

		// Arrange
		await ResetItemTablesAsync();

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

		ReportService service = new(new FixtureDbContextFactory(fixture));

		// Act
		ItemSimilarityResult result = await service.GetItemSimilarityAsync(
			threshold: 0.4,
			sortBy: "occurrences",
			sortDirection: "desc",
			page: 1,
			pageSize: 50,
			CancellationToken.None);

		// Assert — the call completes (no ObjectDisposedException) and returns the cola cluster
		result.Groups.Should().NotBeEmpty();
		ItemSimilarityGroup colaCluster = result.Groups
			.Should().ContainSingle(g => g.Variants.Any(v => v.Contains("COLA", StringComparison.OrdinalIgnoreCase)))
			.Subject;
		colaCluster.Variants.Should().Contain(["COCA COLA", "COCA-COLA", "COCACOLA"]);
		colaCluster.ItemIds.Should().HaveCount(3);
	}

	[Fact]
	public async Task GetItemSimilarityAsync_ReturnsEmpty_WhenNoSimilarDescriptions()
	{
		// Arrange
		await ResetItemTablesAsync();

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

	private async Task ResetItemTablesAsync()
	{
		await using ApplicationDbContext context = fixture.CreateDbContext();
		await context.Database.ExecuteSqlRawAsync(
			"""TRUNCATE "ReceiptItems", "Receipts" RESTART IDENTITY CASCADE;""");
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
