using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddPgvectorAndItemEmbeddings : Migration
{
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS vector;");

		migrationBuilder.CreateTable(
			name: "ItemEmbeddings",
			columns: table => new
			{
				Id = table.Column<Guid>(type: "uuid", nullable: false),
				EntityType = table.Column<string>(type: "text", nullable: false),
				EntityId = table.Column<Guid>(type: "uuid", nullable: false),
				EntityText = table.Column<string>(type: "text", nullable: false),
				Embedding = table.Column<Pgvector.Vector>(type: "vector(1536)", nullable: false),
				ModelVersion = table.Column<string>(type: "text", nullable: false),
				CreatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
			},
			constraints: table =>
			{
				table.PrimaryKey("PK_ItemEmbeddings", x => x.Id);
			});

		migrationBuilder.CreateIndex(
			name: "IX_ItemEmbeddings_EntityType_EntityId",
			table: "ItemEmbeddings",
			columns: ["EntityType", "EntityId"],
			unique: true);

		migrationBuilder.Sql("""
			CREATE INDEX "IX_ItemEmbeddings_Embedding_hnsw"
			    ON "ItemEmbeddings" USING hnsw ("Embedding" vector_cosine_ops)
			    WITH (m = 16, ef_construction = 64);
			""");
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropTable(name: "ItemEmbeddings");
		migrationBuilder.Sql("DROP EXTENSION IF EXISTS vector;");
	}
}
