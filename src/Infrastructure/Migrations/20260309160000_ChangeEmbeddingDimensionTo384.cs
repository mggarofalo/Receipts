using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations;

/// <inheritdoc />
public partial class ChangeEmbeddingDimensionTo384 : Migration
{
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		// Drop the HNSW index (dimension-specific)
		migrationBuilder.Sql("""DROP INDEX IF EXISTS "IX_ItemEmbeddings_Embedding_hnsw";""");

		// Old 1536-dim vectors are incompatible; clear all rows so they regenerate
		migrationBuilder.Sql("""TRUNCATE TABLE "ItemEmbeddings";""");

		// Resize the column from vector(1536) to vector(384)
		migrationBuilder.Sql("""ALTER TABLE "ItemEmbeddings" ALTER COLUMN "Embedding" TYPE vector(384);""");

		// Recreate the HNSW index for the new dimension
		migrationBuilder.Sql("""
			CREATE INDEX "IX_ItemEmbeddings_Embedding_hnsw"
			    ON "ItemEmbeddings" USING hnsw ("Embedding" vector_cosine_ops)
			    WITH (m = 16, ef_construction = 64);
			""");
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.Sql("""DROP INDEX IF EXISTS "IX_ItemEmbeddings_Embedding_hnsw";""");
		migrationBuilder.Sql("""TRUNCATE TABLE "ItemEmbeddings";""");
		migrationBuilder.Sql("""ALTER TABLE "ItemEmbeddings" ALTER COLUMN "Embedding" TYPE vector(1536);""");
		migrationBuilder.Sql("""
			CREATE INDEX "IX_ItemEmbeddings_Embedding_hnsw"
			    ON "ItemEmbeddings" USING hnsw ("Embedding" vector_cosine_ops)
			    WITH (m = 16, ef_construction = 64);
			""");
	}
}
