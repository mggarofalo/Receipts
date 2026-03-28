using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddIsActiveToCategoriesAndSubcategories : Migration
{
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.AddColumn<bool>(
			name: "IsActive",
			table: "Subcategories",
			type: "boolean",
			nullable: false,
			defaultValue: true);

		migrationBuilder.AddColumn<bool>(
			name: "IsActive",
			table: "Categories",
			type: "boolean",
			nullable: false,
			defaultValue: true);

		migrationBuilder.UpdateData(
			table: "Categories",
			keyColumn: "Id",
			keyValue: new Guid("3a131ca1-3300-4cde-b7ee-24704934feea"),
			column: "IsActive",
			value: true);

		migrationBuilder.UpdateData(
			table: "Categories",
			keyColumn: "Id",
			keyValue: new Guid("705da6c5-6fb6-4b3c-aef1-f42e5136a499"),
			column: "IsActive",
			value: true);

		migrationBuilder.UpdateData(
			table: "Categories",
			keyColumn: "Id",
			keyValue: new Guid("83a7f4ea-f771-40b3-850f-35b90a3bd05e"),
			column: "IsActive",
			value: true);

		migrationBuilder.UpdateData(
			table: "Categories",
			keyColumn: "Id",
			keyValue: new Guid("92eae007-7d82-492c-9370-ff64873cc63a"),
			column: "IsActive",
			value: true);

		migrationBuilder.UpdateData(
			table: "Categories",
			keyColumn: "Id",
			keyValue: new Guid("e37ce004-56ea-4a33-8983-55a9552d05be"),
			column: "IsActive",
			value: true);

		migrationBuilder.UpdateData(
			table: "Categories",
			keyColumn: "Id",
			keyValue: new Guid("f0e7a123-9b56-4d3a-8c1e-2a5b7d9f4e6c"),
			column: "IsActive",
			value: true);

		migrationBuilder.UpdateData(
			table: "Subcategories",
			keyColumn: "Id",
			keyValue: new Guid("079d5267-8091-460b-86d4-7b2565b8bb25"),
			column: "IsActive",
			value: true);

		migrationBuilder.UpdateData(
			table: "Subcategories",
			keyColumn: "Id",
			keyValue: new Guid("2ba877ec-9581-4927-aaea-729a778fb8ae"),
			column: "IsActive",
			value: true);

		migrationBuilder.UpdateData(
			table: "Subcategories",
			keyColumn: "Id",
			keyValue: new Guid("2e5bef54-3b06-4d8d-8f53-7f94e8d88e99"),
			column: "IsActive",
			value: true);

		migrationBuilder.UpdateData(
			table: "Subcategories",
			keyColumn: "Id",
			keyValue: new Guid("7bb01875-3807-44a2-b8fb-3459a514d81f"),
			column: "IsActive",
			value: true);

		migrationBuilder.UpdateData(
			table: "Subcategories",
			keyColumn: "Id",
			keyValue: new Guid("8fd8809c-7081-4997-925c-49c0a244a4e4"),
			column: "IsActive",
			value: true);

		migrationBuilder.UpdateData(
			table: "Subcategories",
			keyColumn: "Id",
			keyValue: new Guid("9c90a29d-546c-4ab8-a5f7-4168b675cda8"),
			column: "IsActive",
			value: true);

		migrationBuilder.UpdateData(
			table: "Subcategories",
			keyColumn: "Id",
			keyValue: new Guid("ad045a79-d5a1-404e-b7a8-c475680681f1"),
			column: "IsActive",
			value: true);

		migrationBuilder.UpdateData(
			table: "Subcategories",
			keyColumn: "Id",
			keyValue: new Guid("af940cc4-9838-46ac-8c30-3573d876ae47"),
			column: "IsActive",
			value: true);

		migrationBuilder.UpdateData(
			table: "Subcategories",
			keyColumn: "Id",
			keyValue: new Guid("bdc5740a-352e-44a9-bd1d-a85f5b0cb833"),
			column: "IsActive",
			value: true);

		migrationBuilder.UpdateData(
			table: "Subcategories",
			keyColumn: "Id",
			keyValue: new Guid("c9205933-8cc6-4dfc-b08f-46ba221036fa"),
			column: "IsActive",
			value: true);

		migrationBuilder.UpdateData(
			table: "Subcategories",
			keyColumn: "Id",
			keyValue: new Guid("d00f80ca-5e7a-44f8-bf97-b44fccb430b1"),
			column: "IsActive",
			value: true);

		migrationBuilder.UpdateData(
			table: "Subcategories",
			keyColumn: "Id",
			keyValue: new Guid("d8052b39-045a-4c1f-b0a5-3b94920fe010"),
			column: "IsActive",
			value: true);

		migrationBuilder.UpdateData(
			table: "Subcategories",
			keyColumn: "Id",
			keyValue: new Guid("f7c4d461-ed9f-421c-94d3-32e9a55d6bb0"),
			column: "IsActive",
			value: true);
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropColumn(
			name: "IsActive",
			table: "Subcategories");

		migrationBuilder.DropColumn(
			name: "IsActive",
			table: "Categories");
	}
}
