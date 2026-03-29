using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddCascadeDeletedByParentId : Migration
{
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.AddColumn<Guid>(
			name: "CascadeDeletedByParentId",
			table: "Receipts",
			type: "uuid",
			nullable: true);

		migrationBuilder.AddColumn<Guid>(
			name: "CascadeDeletedByParentId",
			table: "ReceiptItems",
			type: "uuid",
			nullable: true);

		migrationBuilder.AddColumn<Guid>(
			name: "CascadeDeletedByParentId",
			table: "Transactions",
			type: "uuid",
			nullable: true);

		migrationBuilder.AddColumn<Guid>(
			name: "CascadeDeletedByParentId",
			table: "Adjustments",
			type: "uuid",
			nullable: true);

		migrationBuilder.AddColumn<Guid>(
			name: "CascadeDeletedByParentId",
			table: "ItemTemplates",
			type: "uuid",
			nullable: true);
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropColumn(
			name: "CascadeDeletedByParentId",
			table: "Receipts");

		migrationBuilder.DropColumn(
			name: "CascadeDeletedByParentId",
			table: "ReceiptItems");

		migrationBuilder.DropColumn(
			name: "CascadeDeletedByParentId",
			table: "Transactions");

		migrationBuilder.DropColumn(
			name: "CascadeDeletedByParentId",
			table: "Adjustments");

		migrationBuilder.DropColumn(
			name: "CascadeDeletedByParentId",
			table: "ItemTemplates");
	}
}
