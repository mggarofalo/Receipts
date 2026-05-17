using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations;

// RECEIPTS-575: enforce the Card→Account invariant at the DB layer.
// Cards must have a parent Account — AccountId becomes NOT NULL and the FK
// on-delete changes from SET NULL (which silently orphaned cards) to RESTRICT.
/// <inheritdoc />
public partial class RequireAccountIdOnCards : Migration
{
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		// Self-heal any orphan Cards (AccountId IS NULL). Mirrors the 1:1 backfill
		// in IntroduceAccountAggregate: create a matching Account with the same Id,
		// then point the Card at it. Covers Cards that were inserted between the
		// two migrations (e.g., via backup restore or data sync paths that bypassed
		// the application-layer validators). The ON CONFLICT guards the edge case
		// where an Account row with the target Id already exists.
		migrationBuilder.Sql("""
			INSERT INTO "Accounts" ("Id", "Name", "IsActive")
			SELECT "Id", "Name", "IsActive" FROM "Cards" WHERE "AccountId" IS NULL
			ON CONFLICT ("Id") DO NOTHING;

			UPDATE "Cards" SET "AccountId" = "Id" WHERE "AccountId" IS NULL;
			""");

		migrationBuilder.DropForeignKey(
			name: "FK_Cards_Accounts_AccountId",
			table: "Cards");

		migrationBuilder.AlterColumn<Guid>(
			name: "AccountId",
			table: "Cards",
			type: "uuid",
			nullable: false,
			oldClrType: typeof(Guid),
			oldType: "uuid",
			oldNullable: true);

		migrationBuilder.AddForeignKey(
			name: "FK_Cards_Accounts_AccountId",
			table: "Cards",
			column: "AccountId",
			principalTable: "Accounts",
			principalColumn: "Id",
			onDelete: ReferentialAction.Restrict);
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropForeignKey(
			name: "FK_Cards_Accounts_AccountId",
			table: "Cards");

		migrationBuilder.AlterColumn<Guid>(
			name: "AccountId",
			table: "Cards",
			type: "uuid",
			nullable: true,
			oldClrType: typeof(Guid),
			oldType: "uuid");

		migrationBuilder.AddForeignKey(
			name: "FK_Cards_Accounts_AccountId",
			table: "Cards",
			column: "AccountId",
			principalTable: "Accounts",
			principalColumn: "Id",
			onDelete: ReferentialAction.SetNull);
	}
}
