using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations;

/// <inheritdoc />
public partial class ChangeYnabSyncRecordFkToClientCascade : Migration
{
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropForeignKey(
			name: "FK_YnabSyncRecords_Transactions_LocalTransactionId",
			table: "YnabSyncRecords");

		migrationBuilder.AddForeignKey(
			name: "FK_YnabSyncRecords_Transactions_LocalTransactionId",
			table: "YnabSyncRecords",
			column: "LocalTransactionId",
			principalTable: "Transactions",
			principalColumn: "Id");
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropForeignKey(
			name: "FK_YnabSyncRecords_Transactions_LocalTransactionId",
			table: "YnabSyncRecords");

		migrationBuilder.AddForeignKey(
			name: "FK_YnabSyncRecords_Transactions_LocalTransactionId",
			table: "YnabSyncRecords",
			column: "LocalTransactionId",
			principalTable: "Transactions",
			principalColumn: "Id",
			onDelete: ReferentialAction.Restrict);
	}
}
