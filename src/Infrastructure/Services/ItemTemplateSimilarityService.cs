using Application.Interfaces.Services;
using Application.Queries.Core.ItemTemplate.GetSimilarItems;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class ItemTemplateSimilarityService(IDbContextFactory<ApplicationDbContext> contextFactory) : IItemTemplateSimilarityService
{
	public async Task<List<SimilarItemResult>> GetSimilarItemsAsync(string searchText, int limit, double threshold, CancellationToken cancellationToken)
	{
		using ApplicationDbContext context = contextFactory.CreateDbContext();

		// Query templates by trigram similarity on Name
		// Query receipt items by trigram similarity on Description (distinct, excluding soft-deleted)
		// Union, deduplicate (prefer template), rank by similarity descending
		string sql = """
			WITH template_matches AS (
			    SELECT
			        "Name" AS name,
			        similarity("Name", {0}) AS similarity,
			        'template' AS source,
			        "DefaultCategory" AS default_category,
			        "DefaultSubcategory" AS default_subcategory,
			        "DefaultUnitPrice" AS default_unit_price,
			        "DefaultPricingMode" AS default_pricing_mode,
			        "DefaultItemCode" AS default_item_code
			    FROM "ItemTemplates"
			    WHERE "DeletedAt" IS NULL
			      AND similarity("Name", {0}) >= {1}
			),
			history_matches AS (
			    SELECT DISTINCT ON ("Description")
			        "Description" AS name,
			        similarity("Description", {0}) AS similarity,
			        'history' AS source,
			        CAST(NULL AS text) AS default_category,
			        CAST(NULL AS text) AS default_subcategory,
			        CAST(NULL AS decimal) AS default_unit_price,
			        CAST(NULL AS text) AS default_pricing_mode,
			        CAST(NULL AS text) AS default_item_code
			    FROM "ReceiptItems"
			    WHERE "DeletedAt" IS NULL
			      AND similarity("Description", {0}) >= {1}
			),
			combined AS (
			    SELECT * FROM template_matches
			    UNION ALL
			    SELECT h.*
			    FROM history_matches h
			    WHERE NOT EXISTS (
			        SELECT 1 FROM template_matches t
			        WHERE LOWER(t.name) = LOWER(h.name)
			    )
			)
			SELECT name, similarity, source, default_category, default_subcategory,
			       default_unit_price, default_pricing_mode, default_item_code
			FROM combined
			ORDER BY similarity DESC
			LIMIT {2}
			""";

		List<SimilarItemRow> rows = await context.Database
			.SqlQueryRaw<SimilarItemRow>(sql, searchText, threshold, limit)
			.ToListAsync(cancellationToken);

		return [.. rows.Select(r => new SimilarItemResult
		{
			Name = r.name,
			Similarity = r.similarity,
			Source = r.source,
			DefaultCategory = r.default_category,
			DefaultSubcategory = r.default_subcategory,
			DefaultUnitPrice = r.default_unit_price,
			DefaultPricingMode = r.default_pricing_mode,
			DefaultItemCode = r.default_item_code,
		})];
	}

	// Internal record for raw SQL result mapping
	private record SimilarItemRow(
		string name,
		double similarity,
		string source,
		string? default_category,
		string? default_subcategory,
		decimal? default_unit_price,
		string? default_pricing_mode,
		string? default_item_code);
}
