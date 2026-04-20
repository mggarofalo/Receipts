using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Pgvector;

#nullable disable

namespace Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddNormalizedDescriptions : Migration
{
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.AddColumn<Guid>(
			name: "NormalizedDescriptionId",
			table: "ReceiptItems",
			type: "uuid",
			nullable: true);

		migrationBuilder.CreateTable(
			name: "NormalizedDescriptions",
			columns: table => new
			{
				Id = table.Column<Guid>(type: "uuid", nullable: false),
				CanonicalName = table.Column<string>(type: "text", nullable: false),
				Status = table.Column<string>(type: "text", maxLength: 32, nullable: false),
				Embedding = table.Column<Vector>(type: "vector(1024)", nullable: true),
				EmbeddingModelVersion = table.Column<string>(type: "text", nullable: true),
				CreatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false)
			},
			constraints: table =>
			{
				table.PrimaryKey("PK_NormalizedDescriptions", x => x.Id);
			});

		migrationBuilder.CreateIndex(
			name: "IX_ReceiptItems_NormalizedDescriptionId",
			table: "ReceiptItems",
			column: "NormalizedDescriptionId");

		migrationBuilder.AddForeignKey(
			name: "FK_ReceiptItems_NormalizedDescriptions_NormalizedDescriptionId",
			table: "ReceiptItems",
			column: "NormalizedDescriptionId",
			principalTable: "NormalizedDescriptions",
			principalColumn: "Id",
			onDelete: ReferentialAction.SetNull);

		// Unique functional index on lower("CanonicalName") — EF does not natively model
		// functional indexes. Ensures case-insensitive canonical name uniqueness across the
		// table and supports the service's exact-match short-circuit with an index hit.
		migrationBuilder.Sql(
			"""
			CREATE UNIQUE INDEX "IX_NormalizedDescriptions_CanonicalName_Lower"
			    ON "NormalizedDescriptions" (lower("CanonicalName"));
			""");

		// Partial HNSW index for ANN similarity search on rows that have an embedding.
		// pgvector cosine_ops + a WHERE predicate are not modelable in EF, so raw SQL.
		migrationBuilder.Sql(
			"""
			CREATE INDEX "IX_NormalizedDescriptions_Embedding_hnsw"
			    ON "NormalizedDescriptions" USING hnsw ("Embedding" vector_cosine_ops)
			    WITH (m = 16, ef_construction = 64)
			    WHERE "Embedding" IS NOT NULL;
			""");
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder)
	{
		// Drop raw-SQL indexes first, then let EF drop the foreign key/table/index/column.
		migrationBuilder.Sql("""DROP INDEX IF EXISTS "IX_NormalizedDescriptions_Embedding_hnsw";""");
		migrationBuilder.Sql("""DROP INDEX IF EXISTS "IX_NormalizedDescriptions_CanonicalName_Lower";""");

		migrationBuilder.DropForeignKey(
			name: "FK_ReceiptItems_NormalizedDescriptions_NormalizedDescriptionId",
			table: "ReceiptItems");

		migrationBuilder.DropTable(
			name: "NormalizedDescriptions");

		migrationBuilder.DropIndex(
			name: "IX_ReceiptItems_NormalizedDescriptionId",
			table: "ReceiptItems");

		migrationBuilder.DropColumn(
			name: "NormalizedDescriptionId",
			table: "ReceiptItems");
	}
}
