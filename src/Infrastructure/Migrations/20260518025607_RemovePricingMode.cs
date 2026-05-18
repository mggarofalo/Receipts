using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations;

/// <inheritdoc />
public partial class RemovePricingMode : Migration
{
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		// Data-loss guard. PricingMode is being dropped because 'flat' and 'quantity'
		// are numerically equivalent: total = quantity x unitPrice in both modes, and a
		// 'flat' row is constrained to quantity = 1. The one case that is NOT safely
		// reconstructable is a 'flat' row whose quantity is not 1 — there the flat total
		// was set independently of quantity, so collapsing to quantity-mode would silently
		// corrupt the line. Abort the migration if any such row exists rather than lose data.
		migrationBuilder.Sql("""
                DO $$
                DECLARE
                    bad_count integer;
                BEGIN
                    SELECT COUNT(*) INTO bad_count
                    FROM "ReceiptItems"
                    WHERE "PricingMode" IS DISTINCT FROM 'quantity'
                      AND "Quantity" <> 1;

                    IF bad_count > 0 THEN
                        RAISE EXCEPTION 'RemovePricingMode aborted: % ReceiptItems row(s) have a non-quantity PricingMode with Quantity <> 1. Dropping PricingMode would lose flat-pricing semantics for these rows. Reconcile them before applying this migration.', bad_count;
                    END IF;
                END $$;
                """);

		migrationBuilder.DropColumn(
			name: "PricingMode",
			table: "ReceiptItems");

		migrationBuilder.DropColumn(
			name: "DefaultPricingMode",
			table: "ItemTemplates");
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.AddColumn<string>(
			name: "PricingMode",
			table: "ReceiptItems",
			type: "text",
			maxLength: 8,
			nullable: false,
			defaultValue: "quantity");

		migrationBuilder.AddColumn<string>(
			name: "DefaultPricingMode",
			table: "ItemTemplates",
			type: "text",
			nullable: true);

		migrationBuilder.UpdateData(
			table: "ItemTemplates",
			keyColumn: "Id",
			keyValue: new Guid("33255f68-44df-4813-ad55-92260303c0ce"),
			column: "DefaultPricingMode",
			value: "quantity");

		migrationBuilder.UpdateData(
			table: "ItemTemplates",
			keyColumn: "Id",
			keyValue: new Guid("3d11bbb7-ee69-4701-a45c-58bfc6458158"),
			column: "DefaultPricingMode",
			value: "flat");

		migrationBuilder.UpdateData(
			table: "ItemTemplates",
			keyColumn: "Id",
			keyValue: new Guid("a2de7840-ef72-42d5-b90f-9c25eb63f502"),
			column: "DefaultPricingMode",
			value: "quantity");

		migrationBuilder.UpdateData(
			table: "ItemTemplates",
			keyColumn: "Id",
			keyValue: new Guid("cb05ed31-92a0-4c3d-bdbe-b9bd05183f38"),
			column: "DefaultPricingMode",
			value: "quantity");
	}
}
