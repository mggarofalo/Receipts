using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddPgTrgmExtensionAndTrigramIndexes : Migration
{
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS pg_trgm;");

		migrationBuilder.Sql(
			"""
			CREATE INDEX "IX_ItemTemplates_Name_trgm"
			    ON "ItemTemplates" USING gin ("Name" gin_trgm_ops);
			""");

		migrationBuilder.Sql(
			"""
			CREATE INDEX "IX_ReceiptItems_Description_trgm"
			    ON "ReceiptItems" USING gin ("Description" gin_trgm_ops);
			""");
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.Sql("""DROP INDEX IF EXISTS "IX_ReceiptItems_Description_trgm";""");
		migrationBuilder.Sql("""DROP INDEX IF EXISTS "IX_ItemTemplates_Name_trgm";""");
		migrationBuilder.Sql("DROP EXTENSION IF EXISTS pg_trgm;");
	}
}
