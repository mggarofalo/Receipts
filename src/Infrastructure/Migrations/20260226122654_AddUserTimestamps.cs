using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddUserTimestamps : Migration
{
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.AddColumn<DateTimeOffset>(
			name: "CreatedAt",
			table: "AspNetUsers",
			type: "timestamptz",
			nullable: false,
			defaultValueSql: "CURRENT_TIMESTAMP");

		migrationBuilder.AddColumn<DateTimeOffset>(
			name: "LastLoginAt",
			table: "AspNetUsers",
			type: "timestamptz",
			nullable: true);
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropColumn(
			name: "CreatedAt",
			table: "AspNetUsers");

		migrationBuilder.DropColumn(
			name: "LastLoginAt",
			table: "AspNetUsers");
	}
}
