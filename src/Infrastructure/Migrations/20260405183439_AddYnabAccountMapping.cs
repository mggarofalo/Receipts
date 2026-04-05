using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddYnabAccountMapping : Migration
{
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.CreateTable(
			name: "YnabAccountMappings",
			columns: table => new
			{
				Id = table.Column<Guid>(type: "uuid", nullable: false),
				ReceiptsAccountId = table.Column<Guid>(type: "uuid", nullable: false),
				YnabAccountId = table.Column<string>(type: "text", maxLength: 256, nullable: false),
				YnabAccountName = table.Column<string>(type: "text", maxLength: 500, nullable: false),
				YnabBudgetId = table.Column<string>(type: "text", maxLength: 256, nullable: false),
				CreatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
				UpdatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false)
			},
			constraints: table =>
			{
				table.PrimaryKey("PK_YnabAccountMappings", x => x.Id);
				table.ForeignKey(
					name: "FK_YnabAccountMappings_Accounts_ReceiptsAccountId",
					column: x => x.ReceiptsAccountId,
					principalTable: "Accounts",
					principalColumn: "Id",
					onDelete: ReferentialAction.Cascade);
			});

		migrationBuilder.CreateIndex(
			name: "IX_YnabAccountMappings_ReceiptsAccountId",
			table: "YnabAccountMappings",
			column: "ReceiptsAccountId",
			unique: true);
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropTable(
			name: "YnabAccountMappings");
	}
}
