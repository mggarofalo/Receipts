using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddYnabCategoryMapping : Migration
{
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.CreateTable(
			name: "YnabCategoryMappings",
			columns: table => new
			{
				Id = table.Column<Guid>(type: "uuid", nullable: false),
				ReceiptsCategory = table.Column<string>(type: "text", maxLength: 200, nullable: false),
				YnabCategoryId = table.Column<string>(type: "text", maxLength: 100, nullable: false),
				YnabCategoryName = table.Column<string>(type: "text", maxLength: 200, nullable: false),
				YnabCategoryGroupName = table.Column<string>(type: "text", maxLength: 200, nullable: false),
				YnabBudgetId = table.Column<string>(type: "text", maxLength: 100, nullable: false),
				CreatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
				UpdatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false)
			},
			constraints: table =>
			{
				table.PrimaryKey("PK_YnabCategoryMappings", x => x.Id);
			});

		migrationBuilder.CreateIndex(
			name: "IX_YnabCategoryMappings_ReceiptsCategory",
			table: "YnabCategoryMappings",
			column: "ReceiptsCategory",
			unique: true);
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropTable(
			name: "YnabCategoryMappings");
	}
}
