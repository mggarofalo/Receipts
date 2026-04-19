using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddDistinctDescriptionsAndItemSimilarityEdges : Migration
{
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.CreateTable(
			name: "DistinctDescriptions",
			columns: table => new
			{
				Description = table.Column<string>(type: "text", nullable: false),
				ProcessedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
			},
			constraints: table =>
			{
				table.PrimaryKey("PK_DistinctDescriptions", x => x.Description);
			});

		migrationBuilder.CreateTable(
			name: "ItemSimilarityEdges",
			columns: table => new
			{
				DescA = table.Column<string>(type: "text", nullable: false),
				DescB = table.Column<string>(type: "text", nullable: false),
				Score = table.Column<double>(type: "double precision", nullable: false),
				ComputedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false)
			},
			constraints: table =>
			{
				table.PrimaryKey("PK_ItemSimilarityEdges", x => new { x.DescA, x.DescB });
				table.CheckConstraint("CK_ItemSimilarityEdges_CanonicalOrder", "\"DescA\" < \"DescB\"");
				table.ForeignKey(
					name: "FK_ItemSimilarityEdges_DistinctDescriptions_DescA",
					column: x => x.DescA,
					principalTable: "DistinctDescriptions",
					principalColumn: "Description",
					onDelete: ReferentialAction.Cascade);
				table.ForeignKey(
					name: "FK_ItemSimilarityEdges_DistinctDescriptions_DescB",
					column: x => x.DescB,
					principalTable: "DistinctDescriptions",
					principalColumn: "Description",
					onDelete: ReferentialAction.Cascade);
			});

		migrationBuilder.CreateIndex(
			name: "IX_ItemSimilarityEdges_DescB",
			table: "ItemSimilarityEdges",
			column: "DescB");

		migrationBuilder.CreateIndex(
			name: "IX_ItemSimilarityEdges_Score",
			table: "ItemSimilarityEdges",
			column: "Score");

		// GIN trigram index on DistinctDescriptions.Description so ItemSimilarityEdgeRefresher's
		// `%` operator can use the index to prune candidate pairs. EF does not model GIN + custom
		// operator classes, so this is raw SQL. pg_trgm was installed by
		// 20260309124500_AddPgTrgmExtensionAndTrigramIndexes.
		migrationBuilder.Sql(
			"""
                CREATE INDEX "IX_DistinctDescriptions_Description_trgm"
                    ON "DistinctDescriptions" USING gin ("Description" gin_trgm_ops);
                """);
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.Sql("""DROP INDEX IF EXISTS "IX_DistinctDescriptions_Description_trgm";""");

		migrationBuilder.DropTable(
			name: "ItemSimilarityEdges");

		migrationBuilder.DropTable(
			name: "DistinctDescriptions");
	}
}
