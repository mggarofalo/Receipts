using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations;

// RECEIPTS-574: promote Transaction.CardId from nullable to NOT NULL.
//
// The additive phase (RECEIPTS-553) added CardId as nullable and backfilled it from
// the 2026-04-18 pre-deploy backup. This migration finalizes that work.
//
// Guard: the first statement is a fail-loud pre-check — if any Transactions.CardId
// IS NULL rows remain at the time of application, the migration aborts before any
// schema change, preserving the FK and column in their current (nullable) state.
// This prevents a silent default-to-Guid.Empty that would break FK integrity.
//
// FK on-delete changes from SetNull (no longer valid on a NOT NULL column) to
// Restrict — hard-deleting a Card must be a deliberate action that forces the
// caller to handle transactions first. Soft-delete remains the normal flow.
//
// Non-goal: dropping Transaction.AccountId. That is a separate later phase.
/// <inheritdoc />
public partial class PromoteTransactionCardIdNotNull : Migration
{
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		// Pre-check guard. Must run before any schema mutation so that failure leaves
		// the database in the original nullable state.
		migrationBuilder.Sql("""
			DO $$
			DECLARE
				null_count bigint;
			BEGIN
				SELECT COUNT(*) INTO null_count FROM "Transactions" WHERE "CardId" IS NULL;
				IF null_count > 0 THEN
					RAISE EXCEPTION 'RECEIPTS-574: cannot promote Transactions.CardId to NOT NULL — % rows still have NULL CardId. Backfill or soft-delete them before applying.', null_count;
				END IF;
			END
			$$;
			""");

		migrationBuilder.DropForeignKey(
			name: "FK_Transactions_Cards_CardId",
			table: "Transactions");

		migrationBuilder.AlterColumn<Guid>(
			name: "CardId",
			table: "Transactions",
			type: "uuid",
			nullable: false,
			oldClrType: typeof(Guid),
			oldType: "uuid",
			oldNullable: true);

		migrationBuilder.AddForeignKey(
			name: "FK_Transactions_Cards_CardId",
			table: "Transactions",
			column: "CardId",
			principalTable: "Cards",
			principalColumn: "Id",
			onDelete: ReferentialAction.Restrict);
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropForeignKey(
			name: "FK_Transactions_Cards_CardId",
			table: "Transactions");

		migrationBuilder.AlterColumn<Guid>(
			name: "CardId",
			table: "Transactions",
			type: "uuid",
			nullable: true,
			oldClrType: typeof(Guid),
			oldType: "uuid");

		migrationBuilder.AddForeignKey(
			name: "FK_Transactions_Cards_CardId",
			table: "Transactions",
			column: "CardId",
			principalTable: "Cards",
			principalColumn: "Id",
			onDelete: ReferentialAction.SetNull);
	}
}
