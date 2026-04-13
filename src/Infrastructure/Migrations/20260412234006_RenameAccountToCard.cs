using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations;

// Stage 1 of RECEIPTS-543: rename the physical-entity table `Accounts` → `Cards`
// and its `AccountCode` column → `CardCode`. The column `Transactions.AccountId`
// and the FK column `YnabAccountMappings.ReceiptsAccountId` are deliberately
// preserved — they refer to the Transaction→Account semantic relationship,
// which is repointed to the new logical Account table in Stage 2.
//
// Uses Rename* operations so existing data is preserved; Drop+Create would
// lose rows. Also rewrites AuditLogs.EntityType = 'Account' → 'Card' so the
// historical audit trail stays consistent with the new entity name.
/// <inheritdoc />
public partial class RenameAccountToCard : Migration
{
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropForeignKey(
			name: "FK_Transactions_Accounts_AccountId",
			table: "Transactions");

		migrationBuilder.DropForeignKey(
			name: "FK_YnabAccountMappings_Accounts_ReceiptsAccountId",
			table: "YnabAccountMappings");

		migrationBuilder.RenameTable(
			name: "Accounts",
			newName: "Cards");

		migrationBuilder.RenameColumn(
			name: "AccountCode",
			table: "Cards",
			newName: "CardCode");

		migrationBuilder.Sql("ALTER TABLE \"Cards\" RENAME CONSTRAINT \"PK_Accounts\" TO \"PK_Cards\";");

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

		migrationBuilder.Sql("UPDATE \"AuditLogs\" SET \"EntityType\" = 'Card' WHERE \"EntityType\" = 'Account';");
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.Sql("UPDATE \"AuditLogs\" SET \"EntityType\" = 'Account' WHERE \"EntityType\" = 'Card';");

		migrationBuilder.DropForeignKey(
			name: "FK_Transactions_Cards_AccountId",
			table: "Transactions");

		migrationBuilder.DropForeignKey(
			name: "FK_YnabAccountMappings_Cards_ReceiptsAccountId",
			table: "YnabAccountMappings");

		migrationBuilder.Sql("ALTER TABLE \"Cards\" RENAME CONSTRAINT \"PK_Cards\" TO \"PK_Accounts\";");

		migrationBuilder.RenameColumn(
			name: "CardCode",
			table: "Cards",
			newName: "AccountCode");

		migrationBuilder.RenameTable(
			name: "Cards",
			newName: "Accounts");

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
	}
}
