using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations;

/// <inheritdoc />
public partial class UpgradeEmbeddingModelToBgeLargeV15 : Migration
{
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		// Drop the HNSW index (dimension-specific — cannot be altered in place).
		migrationBuilder.Sql("""DROP INDEX IF EXISTS "IX_ItemEmbeddings_Embedding_hnsw";""");

		// Old 384-dim vectors are incompatible with the new 1024-dim column; clear all rows so
		// EmbeddingGenerationService regenerates them with the new model on its next tick.
		migrationBuilder.Sql("""TRUNCATE TABLE "ItemEmbeddings";""");

		// Resize the column from vector(384) to vector(1024).
		migrationBuilder.Sql("""ALTER TABLE "ItemEmbeddings" ALTER COLUMN "Embedding" TYPE vector(1024);""");

		// Recreate the HNSW index for the new dimension.
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
		migrationBuilder.Sql("""ALTER TABLE "ItemEmbeddings" ALTER COLUMN "Embedding" TYPE vector(384);""");
		migrationBuilder.Sql("""
			CREATE INDEX "IX_ItemEmbeddings_Embedding_hnsw"
			    ON "ItemEmbeddings" USING hnsw ("Embedding" vector_cosine_ops)
			    WITH (m = 16, ef_construction = 64);
			""");
	}
}
