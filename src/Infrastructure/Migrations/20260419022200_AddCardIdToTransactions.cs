using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations;

// RECEIPTS-553: restore the Transaction→Card link that was destroyed when the
// Account aggregate was introduced (migration 20260415114934_IntroduceAccountAggregate)
// and cards were subsequently merged via AccountMergeService.
//
// This migration is additive: CardId is nullable, AccountId stays in place.
// The follow-up will promote CardId to NOT NULL and drop AccountId once
// callers are migrated.
//
// Backfill strategy:
//   1. Prod-specific VALUES block sourced from the 2026-04-18 pre-deploy backup
//      via scripts/extract-card-backfill-mapping.cs. That block only updates
//      rows whose original CardId is recoverable from the backup. Safe to run
//      anywhere — the block is a no-op against unmatched IDs.
//   2. Generic fallback: for any transaction still lacking a CardId, copy
//      AccountId → CardId iff AccountId references an existing Card.Id. This
//      covers dev/test environments where Account.Id == Card.Id (1:1 seed)
//      and rows the VALUES block did not cover.
/// <inheritdoc />
public partial class AddCardIdToTransactions : Migration
{
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.AddColumn<Guid>(
			name: "CardId",
			table: "Transactions",
			type: "uuid",
			nullable: true);

		migrationBuilder.CreateIndex(
			name: "IX_Transactions_CardId",
			table: "Transactions",
			column: "CardId");

		migrationBuilder.AddForeignKey(
			name: "FK_Transactions_Cards_CardId",
			table: "Transactions",
			column: "CardId",
			principalTable: "Cards",
			principalColumn: "Id",
			onDelete: ReferentialAction.SetNull);

		// ──────────────────────────────────────────────────────────────────
		// STEP 1: Prod backfill from the 2026-04-18 pre-deploy backup.
		// Replace the `-- <backup-values>` placeholder with the contents of
		// scripts/data/transaction-card-backfill.generated.sql once that has
		// been produced via `dotnet run scripts/extract-card-backfill-mapping.cs`.
		// The script emits a complete `UPDATE ... FROM (VALUES ...)` block.
		// ──────────────────────────────────────────────────────────────────
		// <backup-values>

		// ──────────────────────────────────────────────────────────────────
		// STEP 2: Generic fallback for any remaining NULL CardIds. In dev
		// and integration environments (and for rows the prod backup did
		// not cover), Transaction.AccountId still matches a Card.Id from
		// the 1:1 seed introduced in 20260415114934_IntroduceAccountAggregate.
		// ──────────────────────────────────────────────────────────────────
		migrationBuilder.Sql("""
            UPDATE "Transactions" t
            SET "CardId" = t."AccountId"
            WHERE t."CardId" IS NULL
              AND EXISTS (SELECT 1 FROM "Cards" c WHERE c."Id" = t."AccountId");
            """);
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropForeignKey(
			name: "FK_Transactions_Cards_CardId",
			table: "Transactions");

		migrationBuilder.DropIndex(
			name: "IX_Transactions_CardId",
			table: "Transactions");

		migrationBuilder.DropColumn(
			name: "CardId",
			table: "Transactions");
	}
}
