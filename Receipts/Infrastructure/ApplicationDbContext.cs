using Infrastructure.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Infrastructure;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
	public DbSet<AccountEntity> Accounts { get; set; } = null!;
	public DbSet<ReceiptEntity> Receipts { get; set; } = null!;
	public DbSet<TransactionEntity> Transactions { get; set; } = null!;
	public DbSet<ReceiptItemEntity> ReceiptItems { get; set; } = null!;

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		// Global type-to-column mapping based on the database provider
		string decimalType = Database.ProviderName switch
		{
			"Microsoft.EntityFrameworkCore.SqlServer" => "decimal(18,2)", // SQL Server
			"Npgsql" => "decimal(18,2)", // PostgreSQL
			"Pomelo.EntityFrameworkCore.MySql" => "decimal(18,2)", // MySQL
			_ => "decimal(18,2)" // Default case
		};

		string datetimeType = Database.ProviderName switch
		{
			"Microsoft.EntityFrameworkCore.SqlServer" => "datetime2", // SQL Server
			"Npgsql" => "timestamptz", // PostgreSQL
			"Pomelo.EntityFrameworkCore.MySql" => "datetime", // MySQL
			_ => "datetime" // Default case
		};

		string dateOnlyType = Database.ProviderName switch
		{
			"Microsoft.EntityFrameworkCore.SqlServer" => "date", // SQL Server
			"Npgsql" => "date", // PostgreSQL
			"Pomelo.EntityFrameworkCore.MySql" => "date", // MySQL
			_ => "date" // Default case
		};

		string boolType = Database.ProviderName switch
		{
			"Microsoft.EntityFrameworkCore.SqlServer" => "bit", // SQL Server
			"Npgsql" => "boolean", // PostgreSQL
			"Pomelo.EntityFrameworkCore.MySql" => "tinyint(1)", // MySQL
			_ => "bit" // Default case
		};

		string stringType = Database.ProviderName switch
		{
			"Microsoft.EntityFrameworkCore.SqlServer" => "nvarchar(max)", // SQL Server
			"Npgsql" => "text", // PostgreSQL
			"Pomelo.EntityFrameworkCore.MySql" => "varchar(255)", // MySQL
			_ => "nvarchar(max)" // Default case
		};

		foreach (IMutableEntityType entityType in modelBuilder.Model.GetEntityTypes())
		{
			foreach (IMutableProperty property in entityType.GetProperties())
			{
				if (property.ClrType == typeof(decimal))
				{
					property.SetColumnType(decimalType);
				}
				else if (property.ClrType == typeof(DateTime))
				{
					property.SetColumnType(datetimeType);
				}
				else if (property.ClrType == typeof(DateOnly))
				{
					property.SetColumnType(dateOnlyType);
				}
				else if (property.ClrType == typeof(bool))
				{
					property.SetColumnType(boolType);
				}
				else if (property.ClrType == typeof(string))
				{
					property.SetColumnType(stringType);
				}
			}
		}

		modelBuilder.Entity<AccountEntity>(entity =>
		{
			entity.HasKey(e => e.Id);
			entity.Property(e => e.AccountCode).IsRequired();
			entity.Property(e => e.Name).IsRequired();
		});

		modelBuilder.Entity<ReceiptEntity>(entity =>
		{
			entity.HasKey(e => e.Id);
			entity.Property(e => e.Location).IsRequired();
			entity.Property(e => e.TaxAmount).IsRequired();
		});

		modelBuilder.Entity<TransactionEntity>(entity =>
		{
			entity.HasKey(e => e.Id);
			entity.HasOne(e => e.Receipt)
				.WithMany()
				.HasForeignKey(e => e.ReceiptId)
				.OnDelete(DeleteBehavior.Cascade);
			entity.HasOne(e => e.Account)
				.WithMany()
				.HasForeignKey(e => e.AccountId)
				.OnDelete(DeleteBehavior.Cascade);
		});

		modelBuilder.Entity<ReceiptItemEntity>(entity =>
		{
			entity.HasKey(e => e.Id);
			entity.HasOne(e => e.Receipt)
				.WithMany()
				.HasForeignKey(e => e.ReceiptId)
				.OnDelete(DeleteBehavior.Cascade);
		});
	}
}