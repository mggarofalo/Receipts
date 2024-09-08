using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ForeignKeyUpdates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReceiptItems_Receipts_ReceiptEntityId",
                table: "ReceiptItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Receipts_ReceiptEntityId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_ReceiptEntityId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_ReceiptItems_ReceiptEntityId",
                table: "ReceiptItems");

            migrationBuilder.DropColumn(
                name: "ReceiptEntityId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "ReceiptEntityId",
                table: "ReceiptItems");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ReceiptEntityId",
                table: "Transactions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReceiptEntityId",
                table: "ReceiptItems",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_ReceiptEntityId",
                table: "Transactions",
                column: "ReceiptEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptItems_ReceiptEntityId",
                table: "ReceiptItems",
                column: "ReceiptEntityId");

            migrationBuilder.AddForeignKey(
                name: "FK_ReceiptItems_Receipts_ReceiptEntityId",
                table: "ReceiptItems",
                column: "ReceiptEntityId",
                principalTable: "Receipts",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Receipts_ReceiptEntityId",
                table: "Transactions",
                column: "ReceiptEntityId",
                principalTable: "Receipts",
                principalColumn: "Id");
        }
    }
}
