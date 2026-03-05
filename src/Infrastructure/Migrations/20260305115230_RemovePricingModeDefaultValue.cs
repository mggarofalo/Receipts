using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations;

/// <inheritdoc />
public partial class RemovePricingModeDefaultValue : Migration
{
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.AlterColumn<string>(
			name: "PricingMode",
			table: "ReceiptItems",
			type: "text",
			maxLength: 8,
			nullable: false,
			oldClrType: typeof(string),
			oldType: "text",
			oldMaxLength: 8,
			oldDefaultValueSql: "'quantity'");
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.AlterColumn<string>(
			name: "PricingMode",
			table: "ReceiptItems",
			type: "text",
			maxLength: 8,
			nullable: false,
			defaultValueSql: "'quantity'",
			oldClrType: typeof(string),
			oldType: "text",
			oldMaxLength: 8);
	}
}
