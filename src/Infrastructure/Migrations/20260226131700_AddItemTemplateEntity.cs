using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddItemTemplateEntity : Migration
{
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.CreateTable(
			name: "ItemTemplates",
			columns: table => new
			{
				Id = table.Column<Guid>(type: "uuid", nullable: false),
				Name = table.Column<string>(type: "text", nullable: false),
				DefaultCategory = table.Column<string>(type: "text", nullable: true),
				DefaultSubcategory = table.Column<string>(type: "text", nullable: true),
				DefaultUnitPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
				DefaultUnitPriceCurrency = table.Column<string>(type: "text", nullable: true),
				DefaultPricingMode = table.Column<string>(type: "text", nullable: true),
				DefaultItemCode = table.Column<string>(type: "text", nullable: true),
				Description = table.Column<string>(type: "text", nullable: true),
				DeletedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
				DeletedByUserId = table.Column<string>(type: "text", nullable: true),
				DeletedByApiKeyId = table.Column<Guid>(type: "uuid", nullable: true)
			},
			constraints: table =>
			{
				table.PrimaryKey("PK_ItemTemplates", x => x.Id);
			});

		migrationBuilder.CreateIndex(
			name: "IX_ItemTemplates_Name",
			table: "ItemTemplates",
			column: "Name",
			unique: true,
			filter: "\"DeletedAt\" IS NULL");
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropTable(
			name: "ItemTemplates");
	}
}
