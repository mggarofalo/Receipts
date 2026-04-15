using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations;

// Stage 2 of RECEIPTS-543: introduce a new logical Account aggregate above
// Card. Each existing Card gets a 1:1 Account with the SAME primary key,
// so Transactions.AccountId and YnabAccountMappings.ReceiptsAccountId
// column values do not change — only the FK constraint target moves from
// Cards to Accounts.
/// <inheritdoc />
public partial class IntroduceAccountAggregate : Migration
{
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		// 1. Create the Accounts table
		migrationBuilder.CreateTable(
			name: "Accounts",
			columns: table => new
			{
				Id = table.Column<Guid>(type: "uuid", nullable: false),
				Name = table.Column<string>(type: "text", nullable: false),
				IsActive = table.Column<bool>(type: "boolean", nullable: false)
			},
			constraints: table =>
			{
				table.PrimaryKey("PK_Accounts", x => x.Id);
			});

		// 2. Seed one Account per Card, same Id — so FK values don't change
		migrationBuilder.Sql("""
            INSERT INTO "Accounts" ("Id", "Name", "IsActive")
            SELECT "Id", "Name", "IsActive" FROM "Cards"
            """);

		// 3. Drop old FKs that point at Cards
		migrationBuilder.DropForeignKey(
			name: "FK_Transactions_Cards_AccountId",
			table: "Transactions");

		migrationBuilder.DropForeignKey(
			name: "FK_YnabAccountMappings_Cards_ReceiptsAccountId",
			table: "YnabAccountMappings");

		// 4. Re-add FKs pointing at Accounts (same column values)
		migrationBuilder.AddForeignKey(
			name: "FK_Transactions_Accounts_AccountId",
			table: "Transactions",
			column: "AccountId",
			principalTable: "Accounts",
			principalColumn: "Id",
			onDelete: ReferentialAction.Cascade);

		migrationBuilder.AddForeignKey(
			name: "FK_YnabAccountMappings_Accounts_ReceiptsAccountId",
			table: "YnabAccountMappings",
			column: "ReceiptsAccountId",
			principalTable: "Accounts",
			principalColumn: "Id",
			onDelete: ReferentialAction.Cascade);

		// 5. Add Cards.AccountId nullable FK, default to 1:1 relationship
		migrationBuilder.AddColumn<Guid>(
			name: "AccountId",
			table: "Cards",
			type: "uuid",
			nullable: true);

		migrationBuilder.Sql("""UPDATE "Cards" SET "AccountId" = "Id" """);

		migrationBuilder.CreateIndex(
			name: "IX_Cards_AccountId",
			table: "Cards",
			column: "AccountId");

		migrationBuilder.AddForeignKey(
			name: "FK_Cards_Accounts_AccountId",
			table: "Cards",
			column: "AccountId",
			principalTable: "Accounts",
			principalColumn: "Id",
			onDelete: ReferentialAction.SetNull);

	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropForeignKey(
			name: "FK_Cards_Accounts_AccountId",
			table: "Cards");

		migrationBuilder.DropForeignKey(
			name: "FK_Transactions_Accounts_AccountId",
			table: "Transactions");

		migrationBuilder.DropForeignKey(
			name: "FK_YnabAccountMappings_Accounts_ReceiptsAccountId",
			table: "YnabAccountMappings");

		migrationBuilder.DropTable(
			name: "Accounts");

		migrationBuilder.DropIndex(
			name: "IX_Cards_AccountId",
			table: "Cards");

		migrationBuilder.DropColumn(
			name: "AccountId",
			table: "Cards");

		migrationBuilder.AddForeignKey(
			name: "FK_Transactions_Cards_AccountId",
			table: "Transactions",
			column: "AccountId",
			principalTable: "Cards",
			principalColumn: "Id",
			onDelete: ReferentialAction.Cascade);

		migrationBuilder.AddForeignKey(
			name: "FK_YnabAccountMappings_Cards_ReceiptsAccountId",
			table: "YnabAccountMappings",
			column: "ReceiptsAccountId",
			principalTable: "Cards",
			principalColumn: "Id",
			onDelete: ReferentialAction.Cascade);
	}
}
