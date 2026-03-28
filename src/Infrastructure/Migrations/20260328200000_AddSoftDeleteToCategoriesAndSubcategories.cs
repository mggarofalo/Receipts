using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddSoftDeleteToCategoriesAndSubcategories : Migration
{
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		// Add soft-delete columns to Categories
		migrationBuilder.AddColumn<DateTimeOffset>(
			name: "DeletedAt",
			table: "Categories",
			type: "timestamptz",
			nullable: true);

		migrationBuilder.AddColumn<string>(
			name: "DeletedByUserId",
			table: "Categories",
			type: "text",
			nullable: true);

		migrationBuilder.AddColumn<Guid>(
			name: "DeletedByApiKeyId",
			table: "Categories",
			type: "uuid",
			nullable: true);

		migrationBuilder.AddColumn<Guid>(
			name: "CascadeDeletedByParentId",
			table: "Categories",
			type: "uuid",
			nullable: true);

		// Add soft-delete columns to Subcategories
		migrationBuilder.AddColumn<DateTimeOffset>(
			name: "DeletedAt",
			table: "Subcategories",
			type: "timestamptz",
			nullable: true);

		migrationBuilder.AddColumn<string>(
			name: "DeletedByUserId",
			table: "Subcategories",
			type: "text",
			nullable: true);

		migrationBuilder.AddColumn<Guid>(
			name: "DeletedByApiKeyId",
			table: "Subcategories",
			type: "uuid",
			nullable: true);

		migrationBuilder.AddColumn<Guid>(
			name: "CascadeDeletedByParentId",
			table: "Subcategories",
			type: "uuid",
			nullable: true);

		// Update unique indexes to filter out soft-deleted rows
		migrationBuilder.DropIndex(
			name: "IX_Categories_Name",
			table: "Categories");

		migrationBuilder.CreateIndex(
			name: "IX_Categories_Name",
			table: "Categories",
			column: "Name",
			unique: true,
			filter: "\"DeletedAt\" IS NULL");

		migrationBuilder.DropIndex(
			name: "IX_Subcategories_CategoryId_Name",
			table: "Subcategories");

		migrationBuilder.CreateIndex(
			name: "IX_Subcategories_CategoryId_Name",
			table: "Subcategories",
			columns: new[] { "CategoryId", "Name" },
			unique: true,
			filter: "\"DeletedAt\" IS NULL");
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder)
	{
		// Remove indexes
		migrationBuilder.DropIndex(
			name: "IX_Subcategories_CategoryId_Name",
			table: "Subcategories");

		migrationBuilder.DropIndex(
			name: "IX_Categories_Name",
			table: "Categories");

		// Remove soft-delete columns from Subcategories
		migrationBuilder.DropColumn(name: "CascadeDeletedByParentId", table: "Subcategories");
		migrationBuilder.DropColumn(name: "DeletedByApiKeyId", table: "Subcategories");
		migrationBuilder.DropColumn(name: "DeletedByUserId", table: "Subcategories");
		migrationBuilder.DropColumn(name: "DeletedAt", table: "Subcategories");

		// Remove soft-delete columns from Categories
		migrationBuilder.DropColumn(name: "CascadeDeletedByParentId", table: "Categories");
		migrationBuilder.DropColumn(name: "DeletedByApiKeyId", table: "Categories");
		migrationBuilder.DropColumn(name: "DeletedByUserId", table: "Categories");
		migrationBuilder.DropColumn(name: "DeletedAt", table: "Categories");

		// Restore original indexes
		migrationBuilder.CreateIndex(
			name: "IX_Categories_Name",
			table: "Categories",
			column: "Name",
			unique: true);

		migrationBuilder.CreateIndex(
			name: "IX_Subcategories_CategoryId_Name",
			table: "Subcategories",
			columns: new[] { "CategoryId", "Name" },
			unique: true);
	}
}
