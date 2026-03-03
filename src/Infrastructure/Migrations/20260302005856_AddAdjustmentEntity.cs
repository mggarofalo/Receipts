using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddAdjustmentEntity : Migration
{
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.CreateTable(
			name: "Adjustments",
			columns: table => new
			{
				Id = table.Column<Guid>(type: "uuid", nullable: false),
				ReceiptId = table.Column<Guid>(type: "uuid", nullable: false),
				Type = table.Column<string>(type: "text", nullable: false),
				Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
				AmountCurrency = table.Column<string>(type: "text", nullable: false),
				Description = table.Column<string>(type: "text", nullable: true),
				DeletedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
				DeletedByUserId = table.Column<string>(type: "text", nullable: true),
				DeletedByApiKeyId = table.Column<Guid>(type: "uuid", nullable: true)
			},
			constraints: table =>
			{
				table.PrimaryKey("PK_Adjustments", x => x.Id);
				table.ForeignKey(
					name: "FK_Adjustments_Receipts_ReceiptId",
					column: x => x.ReceiptId,
					principalTable: "Receipts",
					principalColumn: "Id",
					onDelete: ReferentialAction.Cascade);
			});

		migrationBuilder.CreateIndex(
			name: "IX_Adjustments_ReceiptId",
			table: "Adjustments",
			column: "ReceiptId");
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropTable(
			name: "Adjustments");
	}
}
