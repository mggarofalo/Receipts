using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations;

/// <inheritdoc />
public partial class DropProcessedImagePathFromReceipts : Migration
{
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropColumn(
			name: "ProcessedImagePath",
			table: "Receipts");
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.AddColumn<string>(
			name: "ProcessedImagePath",
			table: "Receipts",
			type: "character varying(1024)",
			maxLength: 1024,
			nullable: true);
	}
}
