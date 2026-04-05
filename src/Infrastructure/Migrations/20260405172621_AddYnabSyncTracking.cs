using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddYnabSyncTracking : Migration
{
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropIndex(
			name: "IX_Subcategories_CategoryId_Name",
			table: "Subcategories");

		migrationBuilder.DropIndex(
			name: "IX_Categories_Name",
			table: "Categories");

		migrationBuilder.AddColumn<Guid>(
			name: "CascadeDeletedByParentId",
			table: "Transactions",
			type: "uuid",
			nullable: true);

		migrationBuilder.AddColumn<Guid>(
			name: "CascadeDeletedByParentId",
			table: "Subcategories",
			type: "uuid",
			nullable: true);

		migrationBuilder.AddColumn<DateTimeOffset>(
			name: "DeletedAt",
			table: "Subcategories",
			type: "timestamptz",
			nullable: true);

		migrationBuilder.AddColumn<Guid>(
			name: "DeletedByApiKeyId",
			table: "Subcategories",
			type: "uuid",
			nullable: true);

		migrationBuilder.AddColumn<string>(
			name: "DeletedByUserId",
			table: "Subcategories",
			type: "text",
			nullable: true);

		migrationBuilder.AddColumn<bool>(
			name: "IsActive",
			table: "Subcategories",
			type: "boolean",
			nullable: false,
			defaultValue: false);

		migrationBuilder.AlterColumn<string>(
			name: "ProcessedImagePath",
			table: "Receipts",
			type: "text",
			maxLength: 1024,
			nullable: true,
			oldClrType: typeof(string),
			oldType: "character varying(1024)",
			oldMaxLength: 1024,
			oldNullable: true);

		migrationBuilder.AlterColumn<string>(
			name: "OriginalImagePath",
			table: "Receipts",
			type: "text",
			maxLength: 1024,
			nullable: true,
			oldClrType: typeof(string),
			oldType: "character varying(1024)",
			oldMaxLength: 1024,
			oldNullable: true);

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
			table: "ItemTemplates",
			type: "uuid",
			nullable: true);

		migrationBuilder.AddColumn<Guid>(
			name: "CascadeDeletedByParentId",
			table: "Categories",
			type: "uuid",
			nullable: true);

		migrationBuilder.AddColumn<DateTimeOffset>(
			name: "DeletedAt",
			table: "Categories",
			type: "timestamptz",
			nullable: true);

		migrationBuilder.AddColumn<Guid>(
			name: "DeletedByApiKeyId",
			table: "Categories",
			type: "uuid",
			nullable: true);

		migrationBuilder.AddColumn<string>(
			name: "DeletedByUserId",
			table: "Categories",
			type: "text",
			nullable: true);

		migrationBuilder.AddColumn<bool>(
			name: "IsActive",
			table: "Categories",
			type: "boolean",
			nullable: false,
			defaultValue: false);

		migrationBuilder.AddColumn<Guid>(
			name: "CascadeDeletedByParentId",
			table: "Adjustments",
			type: "uuid",
			nullable: true);

		migrationBuilder.CreateTable(
			name: "YnabSelectedBudgets",
			columns: table => new
			{
				Id = table.Column<Guid>(type: "uuid", nullable: false),
				BudgetId = table.Column<string>(type: "text", maxLength: 36, nullable: false),
				UpdatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false)
			},
			constraints: table =>
			{
				table.PrimaryKey("PK_YnabSelectedBudgets", x => x.Id);
			});

		migrationBuilder.CreateTable(
			name: "YnabSyncRecords",
			columns: table => new
			{
				Id = table.Column<Guid>(type: "uuid", nullable: false),
				LocalTransactionId = table.Column<Guid>(type: "uuid", nullable: false),
				YnabTransactionId = table.Column<string>(type: "text", nullable: true),
				YnabBudgetId = table.Column<string>(type: "text", nullable: false),
				YnabAccountId = table.Column<string>(type: "text", nullable: true),
				SyncType = table.Column<string>(type: "text", nullable: false),
				SyncStatus = table.Column<string>(type: "text", nullable: false),
				SyncedAtUtc = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
				LastError = table.Column<string>(type: "text", maxLength: 2000, nullable: true),
				CreatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
				UpdatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false)
			},
			constraints: table =>
			{
				table.PrimaryKey("PK_YnabSyncRecords", x => x.Id);
				table.ForeignKey(
					name: "FK_YnabSyncRecords_Transactions_LocalTransactionId",
					column: x => x.LocalTransactionId,
					principalTable: "Transactions",
					principalColumn: "Id",
					onDelete: ReferentialAction.Restrict);
			});

		migrationBuilder.UpdateData(
			table: "Categories",
			keyColumn: "Id",
			keyValue: new Guid("3a131ca1-3300-4cde-b7ee-24704934feea"),
			columns: ["CascadeDeletedByParentId", "DeletedAt", "DeletedByApiKeyId", "DeletedByUserId", "IsActive"],
			values: [null, null, null, null, true]);

		migrationBuilder.UpdateData(
			table: "Categories",
			keyColumn: "Id",
			keyValue: new Guid("705da6c5-6fb6-4b3c-aef1-f42e5136a499"),
			columns: ["CascadeDeletedByParentId", "DeletedAt", "DeletedByApiKeyId", "DeletedByUserId", "IsActive"],
			values: [null, null, null, null, true]);

		migrationBuilder.UpdateData(
			table: "Categories",
			keyColumn: "Id",
			keyValue: new Guid("83a7f4ea-f771-40b3-850f-35b90a3bd05e"),
			columns: ["CascadeDeletedByParentId", "DeletedAt", "DeletedByApiKeyId", "DeletedByUserId", "IsActive"],
			values: [null, null, null, null, true]);

		migrationBuilder.UpdateData(
			table: "Categories",
			keyColumn: "Id",
			keyValue: new Guid("92eae007-7d82-492c-9370-ff64873cc63a"),
			columns: ["CascadeDeletedByParentId", "DeletedAt", "DeletedByApiKeyId", "DeletedByUserId", "IsActive"],
			values: [null, null, null, null, true]);

		migrationBuilder.UpdateData(
			table: "Categories",
			keyColumn: "Id",
			keyValue: new Guid("e37ce004-56ea-4a33-8983-55a9552d05be"),
			columns: ["CascadeDeletedByParentId", "DeletedAt", "DeletedByApiKeyId", "DeletedByUserId", "IsActive"],
			values: [null, null, null, null, true]);

		migrationBuilder.InsertData(
			table: "Categories",
			columns: ["Id", "CascadeDeletedByParentId", "DeletedAt", "DeletedByApiKeyId", "DeletedByUserId", "Description", "IsActive", "Name"],
			values: [new Guid("f0e7a123-9b56-4d3a-8c1e-2a5b7d9f4e6c"), null, null, null, null, "Default category for items without a valid category", true, "Uncategorized"]);

		migrationBuilder.UpdateData(
			table: "ItemTemplates",
			keyColumn: "Id",
			keyValue: new Guid("33255f68-44df-4813-ad55-92260303c0ce"),
			column: "CascadeDeletedByParentId",
			value: null);

		migrationBuilder.UpdateData(
			table: "ItemTemplates",
			keyColumn: "Id",
			keyValue: new Guid("3d11bbb7-ee69-4701-a45c-58bfc6458158"),
			column: "CascadeDeletedByParentId",
			value: null);

		migrationBuilder.UpdateData(
			table: "ItemTemplates",
			keyColumn: "Id",
			keyValue: new Guid("a2de7840-ef72-42d5-b90f-9c25eb63f502"),
			column: "CascadeDeletedByParentId",
			value: null);

		migrationBuilder.UpdateData(
			table: "ItemTemplates",
			keyColumn: "Id",
			keyValue: new Guid("cb05ed31-92a0-4c3d-bdbe-b9bd05183f38"),
			column: "CascadeDeletedByParentId",
			value: null);

		migrationBuilder.UpdateData(
			table: "Subcategories",
			keyColumn: "Id",
			keyValue: new Guid("079d5267-8091-460b-86d4-7b2565b8bb25"),
			columns: ["CascadeDeletedByParentId", "DeletedAt", "DeletedByApiKeyId", "DeletedByUserId", "IsActive"],
			values: [null, null, null, null, true]);

		migrationBuilder.UpdateData(
			table: "Subcategories",
			keyColumn: "Id",
			keyValue: new Guid("2ba877ec-9581-4927-aaea-729a778fb8ae"),
			columns: ["CascadeDeletedByParentId", "DeletedAt", "DeletedByApiKeyId", "DeletedByUserId", "IsActive"],
			values: [null, null, null, null, true]);

		migrationBuilder.UpdateData(
			table: "Subcategories",
			keyColumn: "Id",
			keyValue: new Guid("2e5bef54-3b06-4d8d-8f53-7f94e8d88e99"),
			columns: ["CascadeDeletedByParentId", "DeletedAt", "DeletedByApiKeyId", "DeletedByUserId", "IsActive"],
			values: [null, null, null, null, true]);

		migrationBuilder.UpdateData(
			table: "Subcategories",
			keyColumn: "Id",
			keyValue: new Guid("7bb01875-3807-44a2-b8fb-3459a514d81f"),
			columns: ["CascadeDeletedByParentId", "DeletedAt", "DeletedByApiKeyId", "DeletedByUserId", "IsActive"],
			values: [null, null, null, null, true]);

		migrationBuilder.UpdateData(
			table: "Subcategories",
			keyColumn: "Id",
			keyValue: new Guid("8fd8809c-7081-4997-925c-49c0a244a4e4"),
			columns: ["CascadeDeletedByParentId", "DeletedAt", "DeletedByApiKeyId", "DeletedByUserId", "IsActive"],
			values: [null, null, null, null, true]);

		migrationBuilder.UpdateData(
			table: "Subcategories",
			keyColumn: "Id",
			keyValue: new Guid("9c90a29d-546c-4ab8-a5f7-4168b675cda8"),
			columns: ["CascadeDeletedByParentId", "DeletedAt", "DeletedByApiKeyId", "DeletedByUserId", "IsActive"],
			values: [null, null, null, null, true]);

		migrationBuilder.UpdateData(
			table: "Subcategories",
			keyColumn: "Id",
			keyValue: new Guid("ad045a79-d5a1-404e-b7a8-c475680681f1"),
			columns: ["CascadeDeletedByParentId", "DeletedAt", "DeletedByApiKeyId", "DeletedByUserId", "IsActive"],
			values: [null, null, null, null, true]);

		migrationBuilder.UpdateData(
			table: "Subcategories",
			keyColumn: "Id",
			keyValue: new Guid("af940cc4-9838-46ac-8c30-3573d876ae47"),
			columns: ["CascadeDeletedByParentId", "DeletedAt", "DeletedByApiKeyId", "DeletedByUserId", "IsActive"],
			values: [null, null, null, null, true]);

		migrationBuilder.UpdateData(
			table: "Subcategories",
			keyColumn: "Id",
			keyValue: new Guid("bdc5740a-352e-44a9-bd1d-a85f5b0cb833"),
			columns: ["CascadeDeletedByParentId", "DeletedAt", "DeletedByApiKeyId", "DeletedByUserId", "IsActive"],
			values: [null, null, null, null, true]);

		migrationBuilder.UpdateData(
			table: "Subcategories",
			keyColumn: "Id",
			keyValue: new Guid("c9205933-8cc6-4dfc-b08f-46ba221036fa"),
			columns: ["CascadeDeletedByParentId", "DeletedAt", "DeletedByApiKeyId", "DeletedByUserId", "IsActive"],
			values: [null, null, null, null, true]);

		migrationBuilder.UpdateData(
			table: "Subcategories",
			keyColumn: "Id",
			keyValue: new Guid("d00f80ca-5e7a-44f8-bf97-b44fccb430b1"),
			columns: ["CascadeDeletedByParentId", "DeletedAt", "DeletedByApiKeyId", "DeletedByUserId", "IsActive"],
			values: [null, null, null, null, true]);

		migrationBuilder.UpdateData(
			table: "Subcategories",
			keyColumn: "Id",
			keyValue: new Guid("d8052b39-045a-4c1f-b0a5-3b94920fe010"),
			columns: ["CascadeDeletedByParentId", "DeletedAt", "DeletedByApiKeyId", "DeletedByUserId", "IsActive"],
			values: [null, null, null, null, true]);

		migrationBuilder.UpdateData(
			table: "Subcategories",
			keyColumn: "Id",
			keyValue: new Guid("f7c4d461-ed9f-421c-94d3-32e9a55d6bb0"),
			columns: ["CascadeDeletedByParentId", "DeletedAt", "DeletedByApiKeyId", "DeletedByUserId", "IsActive"],
			values: [null, null, null, null, true]);

		migrationBuilder.CreateIndex(
			name: "IX_Subcategories_CategoryId_Name",
			table: "Subcategories",
			columns: ["CategoryId", "Name"],
			unique: true,
			filter: "\"DeletedAt\" IS NULL");

		migrationBuilder.CreateIndex(
			name: "IX_Categories_Name",
			table: "Categories",
			column: "Name",
			unique: true,
			filter: "\"DeletedAt\" IS NULL");

		migrationBuilder.CreateIndex(
			name: "IX_YnabSyncRecords_LocalTransactionId_SyncType",
			table: "YnabSyncRecords",
			columns: ["LocalTransactionId", "SyncType"],
			unique: true);
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropTable(
			name: "YnabSelectedBudgets");

		migrationBuilder.DropTable(
			name: "YnabSyncRecords");

		migrationBuilder.DropIndex(
			name: "IX_Subcategories_CategoryId_Name",
			table: "Subcategories");

		migrationBuilder.DropIndex(
			name: "IX_Categories_Name",
			table: "Categories");

		migrationBuilder.DeleteData(
			table: "Categories",
			keyColumn: "Id",
			keyValue: new Guid("f0e7a123-9b56-4d3a-8c1e-2a5b7d9f4e6c"));

		migrationBuilder.DropColumn(
			name: "CascadeDeletedByParentId",
			table: "Transactions");

		migrationBuilder.DropColumn(
			name: "CascadeDeletedByParentId",
			table: "Subcategories");

		migrationBuilder.DropColumn(
			name: "DeletedAt",
			table: "Subcategories");

		migrationBuilder.DropColumn(
			name: "DeletedByApiKeyId",
			table: "Subcategories");

		migrationBuilder.DropColumn(
			name: "DeletedByUserId",
			table: "Subcategories");

		migrationBuilder.DropColumn(
			name: "IsActive",
			table: "Subcategories");

		migrationBuilder.DropColumn(
			name: "CascadeDeletedByParentId",
			table: "Receipts");

		migrationBuilder.DropColumn(
			name: "CascadeDeletedByParentId",
			table: "ReceiptItems");

		migrationBuilder.DropColumn(
			name: "CascadeDeletedByParentId",
			table: "ItemTemplates");

		migrationBuilder.DropColumn(
			name: "CascadeDeletedByParentId",
			table: "Categories");

		migrationBuilder.DropColumn(
			name: "DeletedAt",
			table: "Categories");

		migrationBuilder.DropColumn(
			name: "DeletedByApiKeyId",
			table: "Categories");

		migrationBuilder.DropColumn(
			name: "DeletedByUserId",
			table: "Categories");

		migrationBuilder.DropColumn(
			name: "IsActive",
			table: "Categories");

		migrationBuilder.DropColumn(
			name: "CascadeDeletedByParentId",
			table: "Adjustments");

		migrationBuilder.AlterColumn<string>(
			name: "ProcessedImagePath",
			table: "Receipts",
			type: "character varying(1024)",
			maxLength: 1024,
			nullable: true,
			oldClrType: typeof(string),
			oldType: "text",
			oldMaxLength: 1024,
			oldNullable: true);

		migrationBuilder.AlterColumn<string>(
			name: "OriginalImagePath",
			table: "Receipts",
			type: "character varying(1024)",
			maxLength: 1024,
			nullable: true,
			oldClrType: typeof(string),
			oldType: "text",
			oldMaxLength: 1024,
			oldNullable: true);

		migrationBuilder.CreateIndex(
			name: "IX_Subcategories_CategoryId_Name",
			table: "Subcategories",
			columns: ["CategoryId", "Name"],
			unique: true);

		migrationBuilder.CreateIndex(
			name: "IX_Categories_Name",
			table: "Categories",
			column: "Name",
			unique: true);
	}
}
