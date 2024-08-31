using Infrastructure.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Infrastructure;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
	private const string MSSQL = "Microsoft.EntityFrameworkCore.SqlServer";
	private const string PostgreSQL = "Npgsql.EntityFrameworkCore.PostgreSQL";
	private const string MySQL = "Pomelo.EntityFrameworkCore.MySql";

	public DbSet<AccountEntity> Accounts { get; set; } = null!;
	public DbSet<ReceiptEntity> Receipts { get; set; } = null!;
	public DbSet<TransactionEntity> Transactions { get; set; } = null!;
	public DbSet<ReceiptItemEntity> ReceiptItems { get; set; } = null!;

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		string moneyType = Database.ProviderName switch
		{
			MSSQL => "decimal(18,2)",
			PostgreSQL => "decimal(18,2)",
			MySQL => "decimal(18,2)",
			_ => throw new NotImplementedException($"Database provider {Database.ProviderName} not supported")
		};

		string datetimeType = Database.ProviderName switch
		{
			MSSQL => "datetime2",
			PostgreSQL => "timestamptz",
			MySQL => "datetime",
			_ => throw new NotImplementedException($"Database provider {Database.ProviderName} not supported")
		};

		string dateOnlyType = Database.ProviderName switch
		{
			MSSQL => "date",
			PostgreSQL => "date",
			MySQL => "date",
			_ => throw new NotImplementedException($"Database provider {Database.ProviderName} not supported")
		};

		string boolType = Database.ProviderName switch
		{
			MSSQL => "bit",
			PostgreSQL => "boolean",
			MySQL => "tinyint(1)",
			_ => throw new NotImplementedException($"Database provider {Database.ProviderName} not supported")
		};

		string stringType = Database.ProviderName switch
		{
			MSSQL => "nvarchar(max)",
			PostgreSQL => "text",
			MySQL => "varchar(255)",
			_ => throw new NotImplementedException($"Database provider {Database.ProviderName} not supported")
		};

		foreach (IMutableEntityType entityType in modelBuilder.Model.GetEntityTypes())
		{
			foreach (IMutableProperty property in entityType.GetProperties())
			{
				if (property.ClrType == typeof(decimal))
				{
					property.SetColumnType(moneyType);
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
				else if (property.ClrType.IsEnum)
				{
					property.SetColumnType(stringType);

					var converterType = typeof(EnumToStringConverter<>).MakeGenericType(property.ClrType);
					var converter = (ValueConverter)Activator.CreateInstance(converterType)!;
					property.SetValueConverter(converter);
				}
			}
		}

		CreateAccountEntity(modelBuilder);
		CreateReceiptEntity(modelBuilder);
		CreateTransactionEntity(modelBuilder);
		CreateReceiptItemEntity(modelBuilder);
	}

	private static void CreateAccountEntity(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<AccountEntity>(entity =>
		{
			entity.HasKey(e => e.Id);
			entity.Property(e => e.Id).ValueGeneratedOnAdd();
			entity.Property(e => e.AccountCode).IsRequired();
			entity.Property(e => e.Name).IsRequired();
		});
	}

	private static void CreateReceiptEntity(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<ReceiptEntity>(entity =>
		{
			entity.HasKey(e => e.Id);
			entity.Property(e => e.Id).ValueGeneratedOnAdd();
			entity.Property(e => e.Location).IsRequired();
			entity.Property(e => e.TaxAmount).IsRequired();
		});
	}

	private static void CreateTransactionEntity(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<TransactionEntity>(entity =>
		{
			entity.HasKey(e => e.Id);
			entity.Property(e => e.Id).ValueGeneratedOnAdd();
			entity.HasOne(e => e.Receipt)
				.WithMany()
				.HasForeignKey(e => e.ReceiptId)
				.OnDelete(DeleteBehavior.Cascade);
			entity.HasOne(e => e.Account)
				.WithMany()
				.HasForeignKey(e => e.AccountId)
				.OnDelete(DeleteBehavior.Cascade);
		});
	}

	private static void CreateReceiptItemEntity(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<ReceiptItemEntity>(entity =>
		{
			entity.HasKey(e => e.Id);
			entity.Property(e => e.Id).ValueGeneratedOnAdd();
			entity.HasOne(e => e.Receipt)
				.WithMany()
				.HasForeignKey(e => e.ReceiptId)
				.OnDelete(DeleteBehavior.Cascade);
		});
	}
}