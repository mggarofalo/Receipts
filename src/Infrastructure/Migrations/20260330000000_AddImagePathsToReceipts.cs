using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddImagePathsToReceipts : Migration
{
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.AddColumn<string>(
			name: "OriginalImagePath",
			table: "Receipts",
			type: "character varying(1024)",
			maxLength: 1024,
			nullable: true);

		migrationBuilder.AddColumn<string>(
			name: "ProcessedImagePath",
			table: "Receipts",
			type: "character varying(1024)",
			maxLength: 1024,
			nullable: true);
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropColumn(
			name: "OriginalImagePath",
			table: "Receipts");

		migrationBuilder.DropColumn(
			name: "ProcessedImagePath",
			table: "Receipts");
	}
}
