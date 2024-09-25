using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MoneyUpdates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AmountCurrency",
                table: "Transactions",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TaxAmountCurrency",
                table: "Receipts",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TotalAmountCurrency",
                table: "ReceiptItems",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UnitPriceCurrency",
                table: "ReceiptItems",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AmountCurrency",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "TaxAmountCurrency",
                table: "Receipts");

            migrationBuilder.DropColumn(
                name: "TotalAmountCurrency",
                table: "ReceiptItems");

            migrationBuilder.DropColumn(
                name: "UnitPriceCurrency",
                table: "ReceiptItems");
        }
    }
}
