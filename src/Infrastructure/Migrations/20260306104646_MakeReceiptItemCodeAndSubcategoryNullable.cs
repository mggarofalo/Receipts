using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations;

/// <inheritdoc />
public partial class MakeReceiptItemCodeAndSubcategoryNullable : Migration
{
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.AlterColumn<string>(
			name: "Subcategory",
			table: "ReceiptItems",
			type: "text",
			nullable: true,
			oldClrType: typeof(string),
			oldType: "text");

		migrationBuilder.AlterColumn<string>(
			name: "ReceiptItemCode",
			table: "ReceiptItems",
			type: "text",
			nullable: true,
			oldClrType: typeof(string),
			oldType: "text");
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.AlterColumn<string>(
			name: "Subcategory",
			table: "ReceiptItems",
			type: "text",
			nullable: false,
			defaultValue: "",
			oldClrType: typeof(string),
			oldType: "text",
			oldNullable: true);

		migrationBuilder.AlterColumn<string>(
			name: "ReceiptItemCode",
			table: "ReceiptItems",
			type: "text",
			nullable: false,
			defaultValue: "",
			oldClrType: typeof(string),
			oldType: "text",
			oldNullable: true);
	}
}
