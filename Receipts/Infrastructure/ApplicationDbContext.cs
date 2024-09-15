using Infrastructure.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Infrastructure;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
	private const string PostgreSQL = "Npgsql.EntityFrameworkCore.PostgreSQL";
	private const string InMemory = "Microsoft.EntityFrameworkCore.InMemory";
	private const string DatabaseProviderNotSupported = "Database provider {0} not supported";

	public virtual DbSet<AccountEntity> Accounts { get; set; } = null!;
	public virtual DbSet<ReceiptEntity> Receipts { get; set; } = null!;
	public virtual DbSet<TransactionEntity> Transactions { get; set; } = null!;
	public virtual DbSet<ReceiptItemEntity> ReceiptItems { get; set; } = null!;

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);
		PrepareEntityTypesInModelBuilder(modelBuilder, Database.ProviderName);
		CreateEntities(modelBuilder);
	}

	private static void PrepareEntityTypesInModelBuilder(ModelBuilder modelBuilder, string? providerName)
	{
		if (providerName == InMemory)
		{
			return;
		}

		Dictionary<Type, string> columnTypes = new()
		{
			{ typeof(decimal), GetMoneyType(providerName) },
			{ typeof(DateTime), GetDateTimeType(providerName) },
			{ typeof(DateOnly), GetDateOnlyType(providerName) },
			{ typeof(bool), GetBoolType(providerName) },
			{ typeof(string), GetStringType(providerName) },
			{ typeof(Guid), GetGuidType(providerName) }
		};

		foreach (IMutableEntityType entityType in modelBuilder.Model.GetEntityTypes())
		{
			LoopPropertiesAndSetColumnTypes(columnTypes, entityType);
		}
	}

	private static void LoopPropertiesAndSetColumnTypes(Dictionary<Type, string> columnTypes, IMutableEntityType entityType)
	{
		foreach (IMutableProperty property in entityType.GetProperties())
		{
			string columnType = GetColumnType(property, columnTypes);
			property.SetColumnType(columnType);
		}
	}

	private static string GetColumnType(IMutableProperty property, Dictionary<Type, string> columnTypes)
	{
		Type clrType = property.ClrType;

		if (columnTypes.TryGetValue(clrType, out string? columnType))
		{
			return columnType;
		}

		if (clrType.IsEnum)
		{
			return SetEnumPropertyColumnType(property, columnTypes[typeof(string)]);
		}

		return columnTypes[typeof(string)];
	}

	private static string SetEnumPropertyColumnType(IMutableProperty property, string stringType)
	{
		property.SetColumnType(stringType);
		Type converterType = typeof(EnumToStringConverter<>).MakeGenericType(property.ClrType);
		ValueConverter converter = (ValueConverter)Activator.CreateInstance(converterType)!;
		property.SetValueConverter(converter);
		return stringType;
	}

	private static string GetMoneyType(string? providerName)
	{
		return providerName switch
		{
			PostgreSQL => "decimal(18,2)",
			_ => throw new NotImplementedException(string.Format(DatabaseProviderNotSupported, providerName))
		};
	}

	private static string GetDateTimeType(string? providerName)
	{
		return providerName switch
		{
			PostgreSQL => "timestamptz",
			_ => throw new NotImplementedException(string.Format(DatabaseProviderNotSupported, providerName))
		};
	}

	private static string GetDateOnlyType(string? providerName)
	{
		return providerName switch
		{
			PostgreSQL => "date",
			_ => throw new NotImplementedException(string.Format(DatabaseProviderNotSupported, providerName))
		};
	}

	private static string GetBoolType(string? providerName)
	{
		return providerName switch
		{
			PostgreSQL => "boolean",
			_ => throw new NotImplementedException(string.Format(DatabaseProviderNotSupported, providerName))
		};
	}

	private static string GetStringType(string? providerName)
	{
		return providerName switch
		{
			PostgreSQL => "text",
			_ => throw new NotImplementedException(string.Format(DatabaseProviderNotSupported, providerName))
		};
	}

	private static string GetGuidType(string? providerName)
	{
		return providerName switch
		{
			PostgreSQL => "uuid",
			_ => throw new NotImplementedException(string.Format(DatabaseProviderNotSupported, providerName))
		};
	}

	private static void CreateEntities(ModelBuilder modelBuilder)
	{
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

			entity.Property(e => e.Id)
				.IsRequired()
				.ValueGeneratedOnAdd();
		});
	}

	private static void CreateReceiptEntity(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<ReceiptEntity>(entity =>
		{
			entity.HasKey(e => e.Id);

			entity.Property(e => e.Id)
				.IsRequired()
				.ValueGeneratedOnAdd();
		});
	}

	private static void CreateTransactionEntity(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<TransactionEntity>(entity =>
		{
			entity.HasKey(e => e.Id);

			entity.Property(e => e.Id)
				.IsRequired()
				.ValueGeneratedOnAdd();

			entity.Navigation(e => e.Receipt)
				.AutoInclude();

			entity.Navigation(e => e.Account)
				.AutoInclude();
		});
	}

	private static void CreateReceiptItemEntity(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<ReceiptItemEntity>(entity =>
		{
			entity.HasKey(e => e.Id);

			entity.Property(e => e.Id)
				.IsRequired()
				.ValueGeneratedOnAdd();

			entity.Navigation(e => e.Receipt)
				.AutoInclude();
		});
	}
}