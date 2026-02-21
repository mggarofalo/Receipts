using Application.Interfaces.Services;
using Infrastructure.Entities;
using Infrastructure.Entities.Core;
using Infrastructure.Interfaces;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
	private const string PostgreSQL = "Npgsql.EntityFrameworkCore.PostgreSQL";
	private const string InMemory = "Microsoft.EntityFrameworkCore.InMemory";
	private const string DatabaseProviderNotSupported = "Database provider {0} not supported";

	private readonly ICurrentUserAccessor? _currentUserAccessor;

	[ActivatorUtilitiesConstructor]
	public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ICurrentUserAccessor currentUserAccessor)
		: base(options)
	{
		_currentUserAccessor = currentUserAccessor;
	}

	public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
		: base(options)
	{
	}

	public virtual DbSet<AccountEntity> Accounts { get; set; } = null!;
	public virtual DbSet<ReceiptEntity> Receipts { get; set; } = null!;
	public virtual DbSet<TransactionEntity> Transactions { get; set; } = null!;
	public virtual DbSet<ReceiptItemEntity> ReceiptItems { get; set; } = null!;
	public virtual DbSet<ApiKeyEntity> ApiKeys { get; set; } = null!;

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);
		PrepareEntityTypesInModelBuilder(modelBuilder, Database.ProviderName);
		CreateEntities(modelBuilder);
		ConfigureSoftDeleteFilters(modelBuilder);
	}

	public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
	{
		HandleSoftDelete();
		return await base.SaveChangesAsync(cancellationToken);
	}

	private void HandleSoftDelete()
	{
		IEnumerable<EntityEntry<ISoftDeletable>> entries = ChangeTracker
			.Entries<ISoftDeletable>()
			.Where(e => e.State == EntityState.Deleted);

		List<ISoftDeletable> cascadeTargets = [];

		foreach (EntityEntry<ISoftDeletable> entry in entries)
		{
			entry.State = EntityState.Modified;
			entry.Entity.DeletedAt = DateTimeOffset.UtcNow;
			entry.Entity.DeletedByUserId = _currentUserAccessor?.UserId;
			entry.Entity.DeletedByApiKeyId = _currentUserAccessor?.ApiKeyId;

			// Cascade soft delete for Receipt children
			if (entry.Entity is ReceiptEntity receipt)
			{
				CollectReceiptChildren(receipt.Id, cascadeTargets);
			}
		}

		foreach (ISoftDeletable target in cascadeTargets)
		{
			if (target.DeletedAt is null)
			{
				target.DeletedAt = DateTimeOffset.UtcNow;
				target.DeletedByUserId = _currentUserAccessor?.UserId;
				target.DeletedByApiKeyId = _currentUserAccessor?.ApiKeyId;
				Entry(target).State = EntityState.Modified;
			}
		}
	}

	private void CollectReceiptChildren(Guid receiptId, List<ISoftDeletable> targets)
	{
		// Find tracked ReceiptItems for this receipt
		IEnumerable<ReceiptItemEntity> trackedItems = ChangeTracker
			.Entries<ReceiptItemEntity>()
			.Where(e => e.Entity.ReceiptId == receiptId && e.State != EntityState.Deleted)
			.Select(e => e.Entity);

		targets.AddRange(trackedItems);

		// Find tracked Transactions for this receipt
		IEnumerable<TransactionEntity> trackedTransactions = ChangeTracker
			.Entries<TransactionEntity>()
			.Where(e => e.Entity.ReceiptId == receiptId && e.State != EntityState.Deleted)
			.Select(e => e.Entity);

		targets.AddRange(trackedTransactions);
	}

	private static void ConfigureSoftDeleteFilters(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<AccountEntity>().HasQueryFilter(e => e.DeletedAt == null);
		modelBuilder.Entity<ReceiptEntity>().HasQueryFilter(e => e.DeletedAt == null);
		modelBuilder.Entity<ReceiptItemEntity>().HasQueryFilter(e => e.DeletedAt == null);
		modelBuilder.Entity<TransactionEntity>().HasQueryFilter(e => e.DeletedAt == null);
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
			{ typeof(DateTimeOffset), GetDateOffsetType(providerName) },
			{ typeof(DateOnly), GetDateOnlyType(providerName) },
			{ typeof(bool), GetBoolType(providerName) },
			{ typeof(string), GetStringType(providerName) },
			{ typeof(Guid), GetGuidType(providerName) },
			{ typeof(int), GetIntType(providerName) },
			{ typeof(long), GetBigIntType(providerName) },
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
			string? columnType = GetColumnType(property, columnTypes);
			if (columnType is not null)
			{
				property.SetColumnType(columnType);
			}
		}
	}

	private static string? GetColumnType(IMutableProperty property, Dictionary<Type, string> columnTypes)
	{
		Type clrType = property.ClrType;

		// Unwrap nullable types (e.g. DateTimeOffset? -> DateTimeOffset)
		Type baseType = Nullable.GetUnderlyingType(clrType) ?? clrType;

		if (columnTypes.TryGetValue(baseType, out string? columnType))
		{
			return columnType;
		}

		if (baseType.IsEnum)
		{
			return SetEnumPropertyColumnType(property, columnTypes[typeof(string)]);
		}

		// Skip unknown types (e.g. byte[]) — let EF/provider handle them
		return null;
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

	private static string GetDateOffsetType(string? providerName)
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

	private static string GetIntType(string? providerName)
	{
		return providerName switch
		{
			PostgreSQL => "integer",
			_ => throw new NotImplementedException(string.Format(DatabaseProviderNotSupported, providerName))
		};
	}

	private static string GetBigIntType(string? providerName)
	{
		return providerName switch
		{
			PostgreSQL => "bigint",
			_ => throw new NotImplementedException(string.Format(DatabaseProviderNotSupported, providerName))
		};
	}

	private static void CreateEntities(ModelBuilder modelBuilder)
	{
		CreateAccountEntity(modelBuilder);
		CreateReceiptEntity(modelBuilder);
		CreateTransactionEntity(modelBuilder);
		CreateReceiptItemEntity(modelBuilder);
		CreateApiKeyEntity(modelBuilder);
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

	private static void CreateApiKeyEntity(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<ApiKeyEntity>(entity =>
		{
			entity.HasKey(e => e.Id);

			entity.Property(e => e.Id)
				.IsRequired()
				.ValueGeneratedNever();

			entity.HasIndex(e => e.KeyHash)
				.IsUnique();

			entity.HasOne(e => e.User)
				.WithMany(u => u.ApiKeys)
				.HasForeignKey(e => e.UserId)
				.OnDelete(DeleteBehavior.Cascade);
		});
	}
}
