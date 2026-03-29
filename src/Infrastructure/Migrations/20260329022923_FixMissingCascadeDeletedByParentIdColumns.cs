using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations;

/// <inheritdoc />
public partial class FixMissingCascadeDeletedByParentIdColumns : Migration
{
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		// The original AddCascadeDeletedByParentId migration was generated empty
		// due to a model snapshot mismatch. Add columns idempotently.
		migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Receipts' AND column_name = 'CascadeDeletedByParentId') THEN
                        ALTER TABLE "Receipts" ADD COLUMN "CascadeDeletedByParentId" uuid;
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'ReceiptItems' AND column_name = 'CascadeDeletedByParentId') THEN
                        ALTER TABLE "ReceiptItems" ADD COLUMN "CascadeDeletedByParentId" uuid;
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Transactions' AND column_name = 'CascadeDeletedByParentId') THEN
                        ALTER TABLE "Transactions" ADD COLUMN "CascadeDeletedByParentId" uuid;
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Adjustments' AND column_name = 'CascadeDeletedByParentId') THEN
                        ALTER TABLE "Adjustments" ADD COLUMN "CascadeDeletedByParentId" uuid;
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'ItemTemplates' AND column_name = 'CascadeDeletedByParentId') THEN
                        ALTER TABLE "ItemTemplates" ADD COLUMN "CascadeDeletedByParentId" uuid;
                    END IF;
                END $$;
                """);
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder)
	{

	}
}
