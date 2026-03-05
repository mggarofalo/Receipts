using System.Text.Json;
using Application.Interfaces.Services;
using Common;
using Infrastructure.Entities;
using Infrastructure.Entities.Audit;
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

	public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ICurrentUserAccessor currentUserAccessor)
		: base(options)
	{
		_currentUserAccessor = currentUserAccessor;
	}

	[ActivatorUtilitiesConstructor]
	public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
		: base(options)
	{
	}

	public virtual DbSet<AccountEntity> Accounts { get; set; } = null!;
	public virtual DbSet<CategoryEntity> Categories { get; set; } = null!;
	public virtual DbSet<SubcategoryEntity> Subcategories { get; set; } = null!;
	public virtual DbSet<ReceiptEntity> Receipts { get; set; } = null!;
	public virtual DbSet<TransactionEntity> Transactions { get; set; } = null!;
	public virtual DbSet<ReceiptItemEntity> ReceiptItems { get; set; } = null!;
	public virtual DbSet<AdjustmentEntity> Adjustments { get; set; } = null!;
	public virtual DbSet<ApiKeyEntity> ApiKeys { get; set; } = null!;
	public virtual DbSet<ItemTemplateEntity> ItemTemplates { get; set; } = null!;
	public virtual DbSet<AuditLogEntity> AuditLogs { get; set; } = null!;
	public virtual DbSet<AuthAuditLogEntity> AuthAuditLogs { get; set; } = null!;

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

		List<AuditEntry> auditEntries = CollectAuditEntries();

		int result = await base.SaveChangesAsync(cancellationToken);

		if (auditEntries.Count > 0)
		{
			foreach (AuditEntry entry in auditEntries)
			{
				// For Created entities, fill in the generated ID after save
				if (entry.AuditLog.Action == AuditAction.Create && entry.TrackedEntry is not null)
				{
					object? idValue = entry.TrackedEntry.Property("Id").CurrentValue;
					if (idValue is not null)
					{
						entry.AuditLog.EntityId = idValue.ToString()!;
					}
				}
			}

			AuditLogs.AddRange(auditEntries.Select(e => e.AuditLog));
			await base.SaveChangesAsync(cancellationToken);
		}

		return result;
	}

	private sealed class AuditEntry(AuditLogEntity auditLog, EntityEntry? trackedEntry = null)
	{
		public AuditLogEntity AuditLog { get; } = auditLog;
		public EntityEntry? TrackedEntry { get; } = trackedEntry;
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

			// Cascade soft delete for Category children
			if (entry.Entity is CategoryEntity category)
			{
				CollectCategoryChildren(category.Id, cascadeTargets);
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

		// Find tracked Adjustments for this receipt
		IEnumerable<AdjustmentEntity> trackedAdjustments = ChangeTracker
			.Entries<AdjustmentEntity>()
			.Where(e => e.Entity.ReceiptId == receiptId && e.State != EntityState.Deleted)
			.Select(e => e.Entity);

		targets.AddRange(trackedAdjustments);
	}

	private void CollectCategoryChildren(Guid categoryId, List<ISoftDeletable> targets)
	{
		IEnumerable<SubcategoryEntity> trackedSubcategories = ChangeTracker
			.Entries<SubcategoryEntity>()
			.Where(e => e.Entity.CategoryId == categoryId && e.State != EntityState.Deleted)
			.Select(e => e.Entity);

		targets.AddRange(trackedSubcategories);
	}

	private List<AuditEntry> CollectAuditEntries()
	{
		HashSet<Type> excludedTypes = [typeof(AuditLogEntity), typeof(AuthAuditLogEntity)];
		List<AuditEntry> auditEntries = [];
		DateTimeOffset now = DateTimeOffset.UtcNow;

		foreach (EntityEntry entry in ChangeTracker.Entries())
		{
			Type entryType = entry.Entity.GetType();

			if (excludedTypes.Contains(entryType))
			{
				continue;
			}

			// Skip ASP.NET Identity internal entities (IdentityRole, IdentityUserRole, etc.)
			// — they use composite keys and are not part of our domain audit trail.
			if (entryType.Namespace?.StartsWith("Microsoft.AspNetCore.Identity", StringComparison.Ordinal) == true)
			{
				continue;
			}

			if (entry.State is not (EntityState.Added or EntityState.Modified or EntityState.Deleted))
			{
				continue;
			}

			string entityType = entry.Entity.GetType().Name.Replace("Entity", "");
			AuditAction action = GetAuditAction(entry);
			List<FieldChange> changes = GetFieldChanges(entry, action);

			if (action == AuditAction.Update && changes.Count == 0)
			{
				continue;
			}

			object? entityId = entry.Property("Id").CurrentValue;
			AuditLogEntity auditLog = new()
			{
				Id = Guid.NewGuid(),
				EntityType = entityType,
				EntityId = action == AuditAction.Create ? "" : entityId?.ToString() ?? "",
				Action = action,
				ChangedByUserId = _currentUserAccessor?.UserId,
				ChangedByApiKeyId = _currentUserAccessor?.ApiKeyId,
				ChangedAt = now,
				IpAddress = _currentUserAccessor?.IpAddress,
			};
			auditLog.SetChanges(changes);

			auditEntries.Add(new AuditEntry(
				auditLog,
				action == AuditAction.Create ? entry : null));
		}

		return auditEntries;
	}

	private static AuditAction GetAuditAction(EntityEntry entry)
	{
		if (entry.State == EntityState.Added)
		{
			return AuditAction.Create;
		}

		if (entry.State == EntityState.Deleted)
		{
			return AuditAction.Delete;
		}

		// Modified — check for soft delete / restore
		if (entry.Entity is ISoftDeletable)
		{
			PropertyEntry deletedAtProp = entry.Property(nameof(ISoftDeletable.DeletedAt));
			object? originalValue = deletedAtProp.OriginalValue;
			object? currentValue = deletedAtProp.CurrentValue;

			if (originalValue is null && currentValue is not null)
			{
				return AuditAction.Delete;
			}

			if (originalValue is not null && currentValue is null)
			{
				return AuditAction.Restore;
			}
		}

		return AuditAction.Update;
	}

	private static List<FieldChange> GetFieldChanges(EntityEntry entry, AuditAction action)
	{
		List<FieldChange> changes = [];

		foreach (PropertyEntry property in entry.Properties)
		{
			string propertyName = property.Metadata.Name;

			if (action == AuditAction.Create)
			{
				changes.Add(new FieldChange
				{
					FieldName = propertyName,
					OldValue = null,
					NewValue = SerializeValue(property.CurrentValue),
				});
			}
			else if (entry.State == EntityState.Modified && property.IsModified)
			{
				string? oldValue = SerializeValue(property.OriginalValue);
				string? newValue = SerializeValue(property.CurrentValue);

				if (oldValue != newValue)
				{
					changes.Add(new FieldChange
					{
						FieldName = propertyName,
						OldValue = oldValue,
						NewValue = newValue,
					});
				}
			}
		}

		return changes;
	}

	private static string? SerializeValue(object? value)
	{
		if (value is null)
		{
			return null;
		}

		return value switch
		{
			string s => s,
			DateTime dt => dt.ToString("O"),
			DateTimeOffset dto => dto.ToString("O"),
			DateOnly d => d.ToString("O"),
			Guid g => g.ToString(),
			bool b => b.ToString(),
			_ => JsonSerializer.Serialize(value),
		};
	}

	private static void ConfigureSoftDeleteFilters(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<AccountEntity>().HasQueryFilter(e => e.DeletedAt == null);
		modelBuilder.Entity<CategoryEntity>().HasQueryFilter(e => e.DeletedAt == null);
		modelBuilder.Entity<SubcategoryEntity>().HasQueryFilter(e => e.DeletedAt == null);
		modelBuilder.Entity<ReceiptEntity>().HasQueryFilter(e => e.DeletedAt == null);
		modelBuilder.Entity<ReceiptItemEntity>().HasQueryFilter(e => e.DeletedAt == null);
		modelBuilder.Entity<TransactionEntity>().HasQueryFilter(e => e.DeletedAt == null);
		modelBuilder.Entity<AdjustmentEntity>().HasQueryFilter(e => e.DeletedAt == null);
		modelBuilder.Entity<ItemTemplateEntity>().HasQueryFilter(e => e.DeletedAt == null);
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
		Type enumType = Nullable.GetUnderlyingType(property.ClrType) ?? property.ClrType;
		Type converterType = typeof(EnumToStringConverter<>).MakeGenericType(enumType);
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
		CreateCategoryEntity(modelBuilder);
		CreateSubcategoryEntity(modelBuilder);
		CreateReceiptEntity(modelBuilder);
		CreateTransactionEntity(modelBuilder);
		CreateReceiptItemEntity(modelBuilder);
		CreateAdjustmentEntity(modelBuilder);
		CreateItemTemplateEntity(modelBuilder);
		CreateApiKeyEntity(modelBuilder);
		CreateAuditLogEntity(modelBuilder);
		CreateAuthAuditLogEntity(modelBuilder);
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

	private static void CreateCategoryEntity(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<CategoryEntity>(entity =>
		{
			entity.HasKey(e => e.Id);

			entity.Property(e => e.Id)
				.IsRequired()
				.ValueGeneratedOnAdd();

			entity.HasIndex(e => e.Name)
				.IsUnique()
				.HasFilter("\"DeletedAt\" IS NULL");

			entity.HasData(
				new CategoryEntity { Id = new Guid("83a7f4ea-f771-40b3-850f-35b90a3bd05e"), Name = "Groceries", Description = "Food and household supplies" },
				new CategoryEntity { Id = new Guid("e37ce004-56ea-4a33-8983-55a9552d05be"), Name = "Dining", Description = "Restaurants, takeout, and delivery" },
				new CategoryEntity { Id = new Guid("3a131ca1-3300-4cde-b7ee-24704934feea"), Name = "Transportation", Description = "Gas, transit, parking, and rideshare" },
				new CategoryEntity { Id = new Guid("92eae007-7d82-492c-9370-ff64873cc63a"), Name = "Shopping", Description = "Clothing, electronics, and general retail" },
				new CategoryEntity { Id = new Guid("705da6c5-6fb6-4b3c-aef1-f42e5136a499"), Name = "Utilities", Description = "Electric, water, internet, and phone" }
			);
		});
	}

	private static void CreateSubcategoryEntity(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<SubcategoryEntity>(entity =>
		{
			entity.HasKey(e => e.Id);

			entity.Property(e => e.Id)
				.IsRequired()
				.ValueGeneratedOnAdd();

			entity.HasIndex(e => new { e.CategoryId, e.Name })
				.IsUnique()
				.HasFilter("\"DeletedAt\" IS NULL");

			entity.Navigation(e => e.Category)
				.AutoInclude();

			Guid groceries = new("83a7f4ea-f771-40b3-850f-35b90a3bd05e");
			Guid dining = new("e37ce004-56ea-4a33-8983-55a9552d05be");
			Guid transportation = new("3a131ca1-3300-4cde-b7ee-24704934feea");
			Guid shopping = new("92eae007-7d82-492c-9370-ff64873cc63a");
			Guid utilities = new("705da6c5-6fb6-4b3c-aef1-f42e5136a499");

			entity.HasData(
				new SubcategoryEntity { Id = new Guid("bdc5740a-352e-44a9-bd1d-a85f5b0cb833"), CategoryId = groceries, Name = "Produce", Description = "Fruits and vegetables" },
				new SubcategoryEntity { Id = new Guid("d8052b39-045a-4c1f-b0a5-3b94920fe010"), CategoryId = groceries, Name = "Dairy", Description = "Milk, cheese, yogurt" },
				new SubcategoryEntity { Id = new Guid("7bb01875-3807-44a2-b8fb-3459a514d81f"), CategoryId = groceries, Name = "Meat & Seafood" },
				new SubcategoryEntity { Id = new Guid("2e5bef54-3b06-4d8d-8f53-7f94e8d88e99"), CategoryId = groceries, Name = "Bakery" },
				new SubcategoryEntity { Id = new Guid("2ba877ec-9581-4927-aaea-729a778fb8ae"), CategoryId = dining, Name = "Fast Food" },
				new SubcategoryEntity { Id = new Guid("af940cc4-9838-46ac-8c30-3573d876ae47"), CategoryId = dining, Name = "Sit-Down Restaurant" },
				new SubcategoryEntity { Id = new Guid("9c90a29d-546c-4ab8-a5f7-4168b675cda8"), CategoryId = dining, Name = "Coffee Shop" },
				new SubcategoryEntity { Id = new Guid("8fd8809c-7081-4997-925c-49c0a244a4e4"), CategoryId = transportation, Name = "Gas" },
				new SubcategoryEntity { Id = new Guid("079d5267-8091-460b-86d4-7b2565b8bb25"), CategoryId = transportation, Name = "Parking" },
				new SubcategoryEntity { Id = new Guid("ad045a79-d5a1-404e-b7a8-c475680681f1"), CategoryId = shopping, Name = "Electronics" },
				new SubcategoryEntity { Id = new Guid("d00f80ca-5e7a-44f8-bf97-b44fccb430b1"), CategoryId = shopping, Name = "Clothing" },
				new SubcategoryEntity { Id = new Guid("f7c4d461-ed9f-421c-94d3-32e9a55d6bb0"), CategoryId = utilities, Name = "Electric" },
				new SubcategoryEntity { Id = new Guid("c9205933-8cc6-4dfc-b08f-46ba221036fa"), CategoryId = utilities, Name = "Internet" }
			);
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

			entity.Property(e => e.PricingMode)
				.HasConversion(
					v => v.ToString().ToLowerInvariant(),
					v => Enum.Parse<PricingMode>(v, ignoreCase: true))
				.HasMaxLength(8);

			entity.Navigation(e => e.Receipt)
				.AutoInclude();
		});
	}

	private static void CreateAdjustmentEntity(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<AdjustmentEntity>(entity =>
		{
			entity.HasKey(e => e.Id);

			entity.Property(e => e.Id)
				.IsRequired()
				.ValueGeneratedOnAdd();

			entity.Navigation(e => e.Receipt)
				.AutoInclude();
		});
	}

	private static void CreateItemTemplateEntity(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<ItemTemplateEntity>(entity =>
		{
			entity.ToTable("ItemTemplates", t => t.HasCheckConstraint(
				"CK_ItemTemplates_Money_Consistency",
				"((\"DefaultUnitPrice\" IS NULL AND \"DefaultUnitPriceCurrency\" IS NULL) OR (\"DefaultUnitPrice\" IS NOT NULL AND \"DefaultUnitPriceCurrency\" IS NOT NULL))"));

			entity.HasKey(e => e.Id);

			entity.Property(e => e.Id)
				.IsRequired()
				.ValueGeneratedOnAdd();

			entity.HasIndex(e => e.Name)
				.IsUnique()
				.HasFilter("\"DeletedAt\" IS NULL");

			entity.HasData(
				new ItemTemplateEntity { Id = new Guid("cb05ed31-92a0-4c3d-bdbe-b9bd05183f38"), Name = "Gallon of Milk", DefaultCategory = "Groceries", DefaultSubcategory = "Dairy", DefaultUnitPrice = 4.99m, DefaultUnitPriceCurrency = Currency.USD, DefaultPricingMode = "quantity", DefaultItemCode = "MILK-GAL" },
				new ItemTemplateEntity { Id = new Guid("33255f68-44df-4813-ad55-92260303c0ce"), Name = "Loaf of Bread", DefaultCategory = "Groceries", DefaultSubcategory = "Bakery", DefaultUnitPrice = 3.49m, DefaultUnitPriceCurrency = Currency.USD, DefaultPricingMode = "quantity", DefaultItemCode = "BREAD" },
				new ItemTemplateEntity { Id = new Guid("3d11bbb7-ee69-4701-a45c-58bfc6458158"), Name = "Coffee (Medium)", DefaultCategory = "Dining", DefaultSubcategory = "Coffee Shop", DefaultUnitPrice = 4.50m, DefaultUnitPriceCurrency = Currency.USD, DefaultPricingMode = "flat", DefaultItemCode = "COFFEE-M" },
				new ItemTemplateEntity { Id = new Guid("a2de7840-ef72-42d5-b90f-9c25eb63f502"), Name = "Regular Unleaded Gas", DefaultCategory = "Transportation", DefaultSubcategory = "Gas", DefaultPricingMode = "quantity", DefaultItemCode = "GAS-REG" }
			);
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

	private static void CreateAuditLogEntity(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<AuditLogEntity>(entity =>
		{
			entity.HasKey(e => e.Id);

			entity.Property(e => e.Id)
				.IsRequired()
				.ValueGeneratedOnAdd();

			entity.HasIndex(e => new { e.EntityType, e.EntityId });
			entity.HasIndex(e => e.ChangedAt);
			entity.HasIndex(e => e.ChangedByUserId);
			entity.HasIndex(e => e.ChangedByApiKeyId);
		});
	}

	private static void CreateAuthAuditLogEntity(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<AuthAuditLogEntity>(entity =>
		{
			entity.HasKey(e => e.Id);

			entity.Property(e => e.Id)
				.IsRequired()
				.ValueGeneratedOnAdd();

			entity.HasIndex(e => new { e.UserId, e.Timestamp });
			entity.HasIndex(e => new { e.EventType, e.Timestamp });
			entity.HasIndex(e => e.Timestamp);
			entity.HasIndex(e => e.IpAddress);
		});
	}
}
