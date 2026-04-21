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
		// Fail loud if any orphan rows remain — the prior IntroduceAccountAggregate
		// migration seeded AccountId for every existing card, so this should be zero
		// in any migrated environment.
		migrationBuilder.Sql("""
			DO $$
			DECLARE orphan_count INTEGER;
			BEGIN
				SELECT COUNT(*) INTO orphan_count FROM "Cards" WHERE "AccountId" IS NULL;
				IF orphan_count > 0 THEN
					RAISE EXCEPTION 'RequireAccountIdOnCards: % card(s) have NULL AccountId. Backfill before running this migration.', orphan_count;
				END IF;
			END $$;
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
