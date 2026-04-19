using System.Reflection;
using FluentAssertions;
using Infrastructure.Entities;
using Infrastructure.Entities.Audit;
using Infrastructure.Entities.Core;
using Infrastructure.Tests.Helpers;
using Infrastructure.Tests.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SampleData.Entities;

namespace Infrastructure.Tests;

/// <summary>
/// Tests targeting uncovered branches in ApplicationDbContext:
/// - Soft-delete cascade logic (null accessor, entity not in OwnedChildrenMap, FK mismatch)
/// - Audit trail collection (Identity entity exclusion, update-with-no-changes, EntityId handling)
/// - Provider-specific type helpers (NotImplementedException for unsupported providers)
/// </summary>
public class ApplicationDbContextBranchCoverageTests
{
	#region Soft-Delete: Null Accessor

	[Fact]
	public async Task SoftDelete_WithNullAccessor_SetsDeletedByFieldsToNull()
	{
		// Arrange — use the single-arg constructor (no ICurrentUserAccessor)
		IDbContextFactory<ApplicationDbContext> contextFactory = DbContextHelpers.CreateInMemoryContextFactory();
		ItemTemplateEntity entity = ItemTemplateEntityGenerator.Generate();

		using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			await context.ItemTemplates.AddAsync(entity);
			await context.SaveChangesAsync();
		}

		// Act
		using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			ItemTemplateEntity template = await context.ItemTemplates.FirstAsync();
			context.ItemTemplates.Remove(template);
			await context.SaveChangesAsync();
		}

		// Assert
		using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			List<ItemTemplateEntity> allTemplates = await context.ItemTemplates.IgnoreQueryFilters().ToListAsync();
			allTemplates.Should().HaveCount(1);
			allTemplates[0].DeletedAt.Should().NotBeNull();
			allTemplates[0].DeletedByUserId.Should().BeNull();
			allTemplates[0].DeletedByApiKeyId.Should().BeNull();
		}

		contextFactory.ResetDatabase();
	}

	#endregion

	#region Soft-Delete: Entity Not in OwnedChildrenMap

	[Fact]
	public async Task SoftDelete_EntityNotInOwnedChildrenMap_DoesNotCascade()
	{
		// Arrange — ItemTemplateEntity implements ISoftDeletable but is NOT a parent in OwnedChildrenMapProvider.Map
		// This covers the TryGetValue false branch
		IDbContextFactory<ApplicationDbContext> contextFactory = DbContextHelpers.CreateInMemoryContextFactory();
		ItemTemplateEntity template = ItemTemplateEntityGenerator.Generate();

		using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			await context.ItemTemplates.AddAsync(template);
			await context.SaveChangesAsync();
		}

		// Act
		using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			ItemTemplateEntity loaded = await context.ItemTemplates.FirstAsync();
			context.ItemTemplates.Remove(loaded);
			await context.SaveChangesAsync();
		}

		// Assert — entity is soft-deleted, no cascade needed
		using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			List<ItemTemplateEntity> all = await context.ItemTemplates.IgnoreQueryFilters().ToListAsync();
			all.Should().HaveCount(1);
			all[0].DeletedAt.Should().NotBeNull();
		}

		contextFactory.ResetDatabase();
	}

	#endregion

	#region Soft-Delete: Cascade with FK Mismatch

	[Fact]
	public async Task SoftDelete_Receipt_DoesNotCascadeToChildrenWithDifferentFk()
	{
		// Arrange — create two receipts, each with a child ReceiptItem
		// Delete only receipt1; receipt2's child should NOT be cascade-deleted
		// This covers the fkValue == parentId false branch in CollectOwnedChildren
		IDbContextFactory<ApplicationDbContext> contextFactory = DbContextHelpers.CreateInMemoryContextFactory();

		ReceiptEntity receipt1 = ReceiptEntityGenerator.Generate();
		ReceiptEntity receipt2 = ReceiptEntityGenerator.Generate();
		ReceiptItemEntity item1 = ReceiptItemEntityGenerator.Generate(receipt1.Id);
		ReceiptItemEntity item2 = ReceiptItemEntityGenerator.Generate(receipt2.Id);

		using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			await context.Receipts.AddRangeAsync(receipt1, receipt2);
			await context.ReceiptItems.AddRangeAsync(item1, item2);
			await context.SaveChangesAsync();
		}

		// Act — delete receipt1, but load both items into the tracker
		using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			ReceiptEntity r1 = await context.Receipts.FirstAsync(r => r.Id == receipt1.Id);
			// Load ALL items into tracker so CollectOwnedChildren iterates over both
			await context.ReceiptItems.LoadAsync();
			context.Receipts.Remove(r1);
			await context.SaveChangesAsync();
		}

		// Assert — item1 should be soft-deleted, item2 should NOT
		using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			List<ReceiptItemEntity> allItems = await context.ReceiptItems.IgnoreQueryFilters().ToListAsync();
			allItems.Should().HaveCount(2);

			ReceiptItemEntity deletedItem = allItems.First(i => i.Id == item1.Id);
			deletedItem.DeletedAt.Should().NotBeNull();

			ReceiptItemEntity survivingItem = allItems.First(i => i.Id == item2.Id);
			survivingItem.DeletedAt.Should().BeNull();
		}

		contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task SoftDelete_Receipt_SkipsChildrenAlreadyMarkedDeleted()
	{
		// Arrange — delete both a receipt and its child item in the same save operation.
		// HandleSoftDelete processes ISoftDeletable entries sequentially. When the receipt
		// is processed first, CollectOwnedChildren iterates all tracked entries. The child
		// item is still in EntityState.Deleted at that point (not yet processed by the
		// foreach), so the entry.State == EntityState.Deleted continue branch (L147) fires.
		(IDbContextFactory<ApplicationDbContext> contextFactory, MockCurrentUserAccessor _) = DbContextWithUserHelpers.CreateInMemoryContextFactoryWithUser();

		ReceiptEntity receipt = ReceiptEntityGenerator.Generate();
		ReceiptItemEntity item1 = ReceiptItemEntityGenerator.Generate(receipt.Id);
		ReceiptItemEntity item2 = ReceiptItemEntityGenerator.Generate(receipt.Id);

		using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			await context.Receipts.AddAsync(receipt);
			await context.ReceiptItems.AddRangeAsync(item1, item2);
			await context.SaveChangesAsync();
		}

		// Act — delete both the receipt and item1 in the same save operation
		using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			ReceiptEntity r = await context.Receipts.FirstAsync();
			// Load all items into tracker
			await context.ReceiptItems.Where(i => i.ReceiptId == receipt.Id).LoadAsync();
			ReceiptItemEntity trackedItem1 = await context.ReceiptItems.FirstAsync(i => i.Id == item1.Id);

			// Remove both — item1 will be Deleted when CollectOwnedChildren runs for receipt
			context.ReceiptItems.Remove(trackedItem1);
			context.Receipts.Remove(r);
			await context.SaveChangesAsync();
		}

		// Assert — both items should be soft-deleted (item1 via direct Remove, item2 via cascade)
		using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			List<ReceiptItemEntity> allItems = await context.ReceiptItems.IgnoreQueryFilters().ToListAsync();
			allItems.Should().HaveCount(2);
			allItems.Should().AllSatisfy(i => i.DeletedAt.Should().NotBeNull());
		}

		contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task SoftDelete_Receipt_SkipsChildOfWrongType()
	{
		// Arrange — create receipt with a child of a different child type than what CollectOwnedChildren is iterating
		// All three owned child types (ReceiptItem, Transaction, Adjustment) are for Receipt,
		// so we exercise the entry.Entity.GetType() != child.ChildType continue branch
		// by ensuring there are entities of non-child types (e.g., Card) in the tracker
		IDbContextFactory<ApplicationDbContext> contextFactory = DbContextHelpers.CreateInMemoryContextFactory();

		ReceiptEntity receipt = ReceiptEntityGenerator.Generate();
		CardEntity account = CardEntityGenerator.Generate();
		ReceiptItemEntity item = ReceiptItemEntityGenerator.Generate(receipt.Id);

		using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			await context.Receipts.AddAsync(receipt);
			await context.Cards.AddAsync(account);
			await context.ReceiptItems.AddAsync(item);
			await context.SaveChangesAsync();
		}

		// Act — delete receipt with Card also tracked (Card is not a child type)
		using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			ReceiptEntity r = await context.Receipts.FirstAsync();
			await context.Cards.LoadAsync();
			await context.ReceiptItems.Where(i => i.ReceiptId == receipt.Id).LoadAsync();
			context.Receipts.Remove(r);
			await context.SaveChangesAsync();
		}

		// Assert — receipt item soft-deleted, account unaffected
		using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			List<ReceiptItemEntity> items = await context.ReceiptItems.IgnoreQueryFilters().ToListAsync();
			items.Should().HaveCount(1);
			items[0].DeletedAt.Should().NotBeNull();

			List<CardEntity> accounts = await context.Cards.ToListAsync();
			accounts.Should().HaveCount(1);
		}

		contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task SoftDelete_CascadeTarget_AlreadySoftDeleted_SkipsCascade()
	{
		// Arrange — create a receipt with a child item that is already soft-deleted (DeletedAt != null)
		// This covers the target.DeletedAt is null check in the cascade loop
		(IDbContextFactory<ApplicationDbContext> contextFactory, MockCurrentUserAccessor _) = DbContextWithUserHelpers.CreateInMemoryContextFactoryWithUser();

		ReceiptEntity receipt = ReceiptEntityGenerator.Generate();
		ReceiptItemEntity item = ReceiptItemEntityGenerator.Generate(receipt.Id);

		using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			await context.Receipts.AddAsync(receipt);
			await context.ReceiptItems.AddAsync(item);
			await context.SaveChangesAsync();
		}

		// Soft-delete the item first
		using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			ReceiptItemEntity loaded = await context.ReceiptItems.FirstAsync();
			context.ReceiptItems.Remove(loaded);
			await context.SaveChangesAsync();
		}

		// Act — now delete the receipt; the item is already soft-deleted so cascade should skip it
		using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			ReceiptEntity r = await context.Receipts.FirstAsync();
			// Load the soft-deleted item via IgnoreQueryFilters so it's tracked
			await context.ReceiptItems.IgnoreQueryFilters()
				.Where(i => i.ReceiptId == receipt.Id)
				.LoadAsync();
			context.Receipts.Remove(r);
			await context.SaveChangesAsync();
		}

		// Assert — both soft-deleted
		using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			List<ReceiptItemEntity> allItems = await context.ReceiptItems.IgnoreQueryFilters().ToListAsync();
			allItems.Should().HaveCount(1);
			allItems[0].DeletedAt.Should().NotBeNull();
		}

		contextFactory.ResetDatabase();
	}

	#endregion

	#region Audit: Identity Entity Exclusion

	[Fact]
	public async Task Audit_IdentityEntity_ExcludedFromAuditTrail()
	{
		// Arrange — add an Identity IdentityRole entity and verify no audit log is created for it
		// This covers the namespace check at L178
		(IDbContextFactory<ApplicationDbContext> contextFactory, MockCurrentUserAccessor _) = DbContextWithUserHelpers.CreateInMemoryContextFactoryWithUser();

		IdentityRole role = new()
		{
			Id = Guid.NewGuid().ToString(),
			Name = "TestRole",
			NormalizedName = "TESTROLE"
		};

		// Act
		await using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			context.Set<IdentityRole>().Add(role);
			await context.SaveChangesAsync();
		}

		// Assert — no audit log should exist for IdentityRole
		await using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			List<AuditLogEntity> auditLogs = await context.AuditLogs.ToListAsync();
			auditLogs.Should().BeEmpty();
		}

		contextFactory.ResetDatabase();
	}

	#endregion

	#region Audit: Update With No Changes

	[Fact]
	public async Task Audit_UpdateWithNoChanges_SkipsAuditEntry()
	{
		// Arrange — modify an entity's state to Modified without changing values
		// This covers the changes.Count == 0 skip at L192
		(IDbContextFactory<ApplicationDbContext> contextFactory, MockCurrentUserAccessor _) = DbContextWithUserHelpers.CreateInMemoryContextFactoryWithUser();
		CardEntity entity = CardEntityGenerator.Generate();

		await using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			await context.Cards.AddAsync(entity);
			await context.SaveChangesAsync();
		}

		// Act — mark as modified without changing anything
		await using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			CardEntity account = await context.Cards.FirstAsync();
			context.Entry(account).State = EntityState.Modified;
			await context.SaveChangesAsync();
		}

		// Assert — only the Create audit log from initial save, no Update audit
		await using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			List<AuditLogEntity> auditLogs = await context.AuditLogs.ToListAsync();
			auditLogs.Should().ContainSingle(a => a.Action == AuditAction.Create);
			auditLogs.Should().NotContain(a => a.Action == AuditAction.Update);
		}

		contextFactory.ResetDatabase();
	}

	#endregion

	#region Audit: Delete Action EntityId

	[Fact]
	public async Task Audit_DeleteAction_CapturesEntityId()
	{
		// This covers the non-Create EntityId branch at L202: entityId?.ToString() ?? ""
		(IDbContextFactory<ApplicationDbContext> contextFactory, MockCurrentUserAccessor _) = DbContextWithUserHelpers.CreateInMemoryContextFactoryWithUser();
		ItemTemplateEntity entity = ItemTemplateEntityGenerator.Generate();

		await using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			await context.ItemTemplates.AddAsync(entity);
			await context.SaveChangesAsync();
		}

		// Act — soft-delete
		await using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			ItemTemplateEntity template = await context.ItemTemplates.FirstAsync();
			context.ItemTemplates.Remove(template);
			await context.SaveChangesAsync();
		}

		// Assert — the Delete audit log should have the entity's ID
		await using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			List<AuditLogEntity> auditLogs = await context.AuditLogs
				.Where(a => a.Action == AuditAction.Delete)
				.ToListAsync();
			auditLogs.Should().HaveCount(1);
			auditLogs[0].EntityId.Should().Be(entity.Id.ToString());
		}

		contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task Audit_UpdateAction_CapturesEntityId()
	{
		// Covers the non-Create EntityId branch at L202 for Update action
		(IDbContextFactory<ApplicationDbContext> contextFactory, MockCurrentUserAccessor _) = DbContextWithUserHelpers.CreateInMemoryContextFactoryWithUser();
		CardEntity entity = CardEntityGenerator.Generate();

		await using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			await context.Cards.AddAsync(entity);
			await context.SaveChangesAsync();
		}

		// Act — update
		await using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			CardEntity account = await context.Cards.FirstAsync();
			account.Name = "Updated Name";
			await context.SaveChangesAsync();
		}

		// Assert — the Update audit log should have the entity's ID
		await using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			AuditLogEntity? updateLog = await context.AuditLogs
				.FirstOrDefaultAsync(a => a.Action == AuditAction.Update);
			updateLog.Should().NotBeNull();
			updateLog!.EntityId.Should().Be(entity.Id.ToString());
		}

		contextFactory.ResetDatabase();
	}

	#endregion

	#region Audit: Unchanged Entity State (not Added/Modified/Deleted) is skipped

	[Fact]
	public async Task Audit_UnchangedEntity_NoAuditLog()
	{
		// Arrange — load entity, don't change anything, save
		// This covers the entry.State is not (Added or Modified or Deleted) continue at L183
		(IDbContextFactory<ApplicationDbContext> contextFactory, MockCurrentUserAccessor _) = DbContextWithUserHelpers.CreateInMemoryContextFactoryWithUser();
		CardEntity entity = CardEntityGenerator.Generate();

		await using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			await context.Cards.AddAsync(entity);
			await context.SaveChangesAsync();
		}

		// Act — just load and save without changes
		await using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			await context.Cards.FirstAsync();
			await context.SaveChangesAsync();
		}

		// Assert — only the original Create audit
		await using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			List<AuditLogEntity> auditLogs = await context.AuditLogs.ToListAsync();
			auditLogs.Should().ContainSingle();
			auditLogs[0].Action.Should().Be(AuditAction.Create);
		}

		contextFactory.ResetDatabase();
	}

	#endregion

	#region Audit: SeedHistoryEntry exclusion

	[Fact]
	public async Task Audit_SeedHistoryEntry_NotAudited()
	{
		// Arrange — SeedHistoryEntry is in the excludedTypes set
		(IDbContextFactory<ApplicationDbContext> contextFactory, MockCurrentUserAccessor _) = DbContextWithUserHelpers.CreateInMemoryContextFactoryWithUser();
		SeedHistoryEntry entry = new()
		{
			SeedId = "TestSeed_" + Guid.NewGuid().ToString("N"),
			AppliedAt = DateTimeOffset.UtcNow
		};

		// Act
		await using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			context.SeedHistory.Add(entry);
			await context.SaveChangesAsync();
		}

		// Assert — no audit log for SeedHistoryEntry
		await using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			List<AuditLogEntity> auditLogs = await context.AuditLogs.ToListAsync();
			auditLogs.Should().BeEmpty();
		}

		contextFactory.ResetDatabase();
	}

	#endregion

	#region Audit: AuthAuditLogEntity exclusion

	[Fact]
	public async Task Audit_AuthAuditLogEntity_NotAudited()
	{
		// Arrange — AuthAuditLogEntity is in the excludedTypes set
		(IDbContextFactory<ApplicationDbContext> contextFactory, MockCurrentUserAccessor _) = DbContextWithUserHelpers.CreateInMemoryContextFactoryWithUser();
		AuthAuditLogEntity authLog = AuthAuditLogEntityGenerator.Generate();

		// Act
		await using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			context.AuthAuditLogs.Add(authLog);
			await context.SaveChangesAsync();
		}

		// Assert — no audit log for AuthAuditLogEntity
		await using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			List<AuditLogEntity> auditLogs = await context.AuditLogs.ToListAsync();
			auditLogs.Should().BeEmpty();
		}

		contextFactory.ResetDatabase();
	}

	#endregion

	#region Provider-Specific Type Helpers

	[Theory]
	[InlineData("GetMoneyType")]
	[InlineData("GetDateTimeType")]
	[InlineData("GetDateOffsetType")]
	[InlineData("GetDateOnlyType")]
	[InlineData("GetBoolType")]
	[InlineData("GetStringType")]
	[InlineData("GetGuidType")]
	[InlineData("GetIntType")]
	[InlineData("GetBigIntType")]
	public void ProviderTypeHelper_UnsupportedProvider_ThrowsNotImplementedException(string methodName)
	{
		// Arrange
		MethodInfo? method = typeof(ApplicationDbContext)
			.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Static);
		method.Should().NotBeNull($"method {methodName} should exist");

		// Act
		Action act = () => method!.Invoke(null, ["SomeUnsupportedProvider"]);

		// Assert
		act.Should().Throw<TargetInvocationException>()
			.WithInnerException<NotImplementedException>()
			.WithMessage("*SomeUnsupportedProvider*");
	}

	[Theory]
	[InlineData("GetMoneyType", "decimal(18,2)")]
	[InlineData("GetDateTimeType", "timestamptz")]
	[InlineData("GetDateOffsetType", "timestamptz")]
	[InlineData("GetDateOnlyType", "date")]
	[InlineData("GetBoolType", "boolean")]
	[InlineData("GetStringType", "text")]
	[InlineData("GetGuidType", "uuid")]
	[InlineData("GetIntType", "integer")]
	[InlineData("GetBigIntType", "bigint")]
	public void ProviderTypeHelper_PostgreSQL_ReturnsCorrectType(string methodName, string expectedType)
	{
		// Arrange
		MethodInfo? method = typeof(ApplicationDbContext)
			.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Static);
		method.Should().NotBeNull($"method {methodName} should exist");

		// Act
		string? result = (string?)method!.Invoke(null, ["Npgsql.EntityFrameworkCore.PostgreSQL"]);

		// Assert
		result.Should().Be(expectedType);
	}

	[Theory]
	[InlineData("GetMoneyType")]
	[InlineData("GetDateTimeType")]
	[InlineData("GetDateOffsetType")]
	[InlineData("GetDateOnlyType")]
	[InlineData("GetBoolType")]
	[InlineData("GetStringType")]
	[InlineData("GetGuidType")]
	[InlineData("GetIntType")]
	[InlineData("GetBigIntType")]
	public void ProviderTypeHelper_NullProvider_ThrowsNotImplementedException(string methodName)
	{
		// Arrange
		MethodInfo? method = typeof(ApplicationDbContext)
			.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Static);
		method.Should().NotBeNull($"method {methodName} should exist");

		// Act
		Action act = () => method!.Invoke(null, [null]);

		// Assert
		act.Should().Throw<TargetInvocationException>()
			.WithInnerException<NotImplementedException>();
	}

	#endregion

	#region Audit: SerializeValue branches

	[Fact]
	public async Task Audit_CreateEntity_SerializesDifferentPropertyTypes()
	{
		// Covers various SerializeValue branches (DateTime, DateTimeOffset, DateOnly, Guid, bool, string, etc.)
		(IDbContextFactory<ApplicationDbContext> contextFactory, MockCurrentUserAccessor _) = DbContextWithUserHelpers.CreateInMemoryContextFactoryWithUser();
		ReceiptEntity receipt = ReceiptEntityGenerator.Generate();

		// Act
		await using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			await context.Receipts.AddAsync(receipt);
			await context.SaveChangesAsync();
		}

		// Assert — the audit log should serialize all field types
		await using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			List<AuditLogEntity> auditLogs = await context.AuditLogs
				.Where(a => a.Action == AuditAction.Create)
				.ToListAsync();
			auditLogs.Should().HaveCount(1);

			List<FieldChange> changes = auditLogs[0].GetChanges();
			changes.Should().NotBeEmpty();

			// Verify Date (DateOnly) serialization
			FieldChange? dateChange = changes.FirstOrDefault(c => c.FieldName == "Date");
			dateChange.Should().NotBeNull();
			dateChange!.NewValue.Should().NotBeNullOrEmpty();

			// Verify Id (Guid) serialization
			FieldChange? idChange = changes.FirstOrDefault(c => c.FieldName == "Id");
			idChange.Should().NotBeNull();
			idChange!.NewValue.Should().NotBeNullOrEmpty();
		}

		contextFactory.ResetDatabase();
	}

	#endregion

	#region SaveChangesAsync: Create entity with tracked entry post-save ID fill

	[Fact]
	public async Task SaveChangesAsync_CreateEntity_FillsGeneratedIdAfterSave()
	{
		// This covers the SaveChangesAsync loop for AuditAction.Create where TrackedEntry is not null
		// and idValue is not null (L83-88)
		(IDbContextFactory<ApplicationDbContext> contextFactory, MockCurrentUserAccessor _) = DbContextWithUserHelpers.CreateInMemoryContextFactoryWithUser();
		CardEntity entity = new()
		{
			Id = Guid.Empty, // Will be generated
			CardCode = "NEW",
			Name = "New Account",
			IsActive = true
		};

		// Act
		await using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			await context.Cards.AddAsync(entity);
			await context.SaveChangesAsync();
		}

		// Assert — audit log should have the generated ID
		await using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			List<AuditLogEntity> auditLogs = await context.AuditLogs.ToListAsync();
			auditLogs.Should().HaveCount(1);
			auditLogs[0].EntityId.Should().NotBeNullOrEmpty();
			// The entity should have gotten an ID even if it was Guid.Empty
			auditLogs[0].EntityId.Should().Be(entity.Id.ToString());
		}

		contextFactory.ResetDatabase();
	}

	#endregion

	#region SaveChangesAsync: No audit entries skips second save

	[Fact]
	public async Task SaveChangesAsync_NoAuditableChanges_SkipsSecondSave()
	{
		// When only excluded entity types are changed, auditEntries.Count == 0
		// and the second SaveChangesAsync call is skipped (L78)
		(IDbContextFactory<ApplicationDbContext> contextFactory, MockCurrentUserAccessor _) = DbContextWithUserHelpers.CreateInMemoryContextFactoryWithUser();
		AuditLogEntity auditLog = AuditLogEntityGenerator.Generate();

		// Act — only add an AuditLogEntity (excluded from audit)
		await using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			await context.AuditLogs.AddAsync(auditLog);
			await context.SaveChangesAsync();
		}

		// Assert — only the manually added audit log exists (no self-audit)
		await using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			List<AuditLogEntity> auditLogs = await context.AuditLogs.ToListAsync();
			auditLogs.Should().HaveCount(1);
			auditLogs[0].Id.Should().Be(auditLog.Id);
		}

		contextFactory.ResetDatabase();
	}

	#endregion

	#region Audit: GetFieldChanges for Modified entity with property.IsModified false

	[Fact]
	public async Task Audit_ModifiedEntity_SkipsUnmodifiedProperties()
	{
		// Covers the property.IsModified check in GetFieldChanges — properties that are
		// not modified should not appear in the changes list
		(IDbContextFactory<ApplicationDbContext> contextFactory, MockCurrentUserAccessor _) = DbContextWithUserHelpers.CreateInMemoryContextFactoryWithUser();
		CardEntity entity = CardEntityGenerator.Generate();

		await using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			await context.Cards.AddAsync(entity);
			await context.SaveChangesAsync();
		}

		// Act — only change Name, not CardCode or IsActive
		await using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			CardEntity account = await context.Cards.FirstAsync();
			account.Name = "Only Name Changed";
			await context.SaveChangesAsync();
		}

		// Assert — only Name should be in the changes
		await using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			AuditLogEntity? updateLog = await context.AuditLogs
				.FirstOrDefaultAsync(a => a.Action == AuditAction.Update);
			updateLog.Should().NotBeNull();

			List<FieldChange> changes = updateLog!.GetChanges();
			changes.Should().ContainSingle(c => c.FieldName == "Name");
			changes.Should().NotContain(c => c.FieldName == "CardCode");
			changes.Should().NotContain(c => c.FieldName == "IsActive");
		}

		contextFactory.ResetDatabase();
	}

	#endregion

	#region Audit: GetFieldChanges for Modified entity where oldValue == newValue

	[Fact]
	public async Task Audit_ModifiedEntity_SkipsFieldsWhereOldEqualsNew()
	{
		// Covers the oldValue != newValue check in GetFieldChanges (L274)
		// Force a property into IsModified = true while keeping the same value,
		// so the audit system sees it as modified but the serialized old/new values are equal.
		(IDbContextFactory<ApplicationDbContext> contextFactory, MockCurrentUserAccessor _) = DbContextWithUserHelpers.CreateInMemoryContextFactoryWithUser();
		CardEntity entity = CardEntityGenerator.Generate();

		await using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			await context.Cards.AddAsync(entity);
			await context.SaveChangesAsync();
		}

		// Act — force IsModified on Name without changing its value
		await using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			CardEntity account = await context.Cards.FirstAsync();
			// Force the property to be marked as modified even though the value is unchanged
			context.Entry(account).Property(nameof(CardEntity.Name)).IsModified = true;
			await context.SaveChangesAsync();
		}

		// Assert — no Update audit should be created because oldValue == newValue for all fields
		await using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			List<AuditLogEntity> updateLogs = await context.AuditLogs
				.Where(a => a.Action == AuditAction.Update)
				.ToListAsync();

			// The changes.Count == 0 check at L192 should skip creating an audit entry
			updateLogs.Should().BeEmpty("no audit entry should be created when all modified fields have identical old and new values");
		}

		contextFactory.ResetDatabase();
	}

	#endregion

	#region OwnedChildrenMapProvider: YnabSyncRecord wired correctly

	[Fact]
	public void OwnedChildrenMapProvider_YnabSyncRecordEntity_IsChildOfTransactionEntity()
	{
		// Verify that OwnedChildrenMapProvider correctly discovers YnabSyncRecordEntity
		// as a child of TransactionEntity via IOwnedBy<TransactionEntity> with custom FK
		OwnedChildrenMapProvider.Map.Should().ContainKey(typeof(TransactionEntity));

		OwnedChildrenMapProvider.ParentEntry parentEntry = OwnedChildrenMapProvider.Map[typeof(TransactionEntity)];
		parentEntry.Children.Should().Contain(c =>
			c.ChildType == typeof(YnabSyncRecordEntity) &&
			c.FkPropertyName == "LocalTransactionId");
	}

	[Fact]
	public void OwnedChildrenMapProvider_AllSoftDeletableOwnedRelationships_AreDiscovered()
	{
		// Verify all expected parent-child relationships are wired
		// ReceiptEntity -> TransactionEntity, ReceiptItemEntity, AdjustmentEntity
		OwnedChildrenMapProvider.Map.Should().ContainKey(typeof(ReceiptEntity));
		OwnedChildrenMapProvider.ParentEntry receiptEntry = OwnedChildrenMapProvider.Map[typeof(ReceiptEntity)];
		receiptEntry.Children.Select(c => c.ChildType).Should().Contain(typeof(TransactionEntity));
		receiptEntry.Children.Select(c => c.ChildType).Should().Contain(typeof(ReceiptItemEntity));
		receiptEntry.Children.Select(c => c.ChildType).Should().Contain(typeof(AdjustmentEntity));

		// TransactionEntity -> YnabSyncRecordEntity
		OwnedChildrenMapProvider.Map.Should().ContainKey(typeof(TransactionEntity));
		OwnedChildrenMapProvider.ParentEntry transactionEntry = OwnedChildrenMapProvider.Map[typeof(TransactionEntity)];
		transactionEntry.Children.Select(c => c.ChildType).Should().Contain(typeof(YnabSyncRecordEntity));

		// CategoryEntity -> SubcategoryEntity
		OwnedChildrenMapProvider.Map.Should().ContainKey(typeof(CategoryEntity));
		OwnedChildrenMapProvider.ParentEntry categoryEntry = OwnedChildrenMapProvider.Map[typeof(CategoryEntity)];
		categoryEntry.Children.Select(c => c.ChildType).Should().Contain(typeof(SubcategoryEntity));
	}

	#endregion

	#region Soft-Delete: Cascade to Adjustments

	[Fact]
	public async Task SoftDelete_Receipt_CascadesToAdjustments()
	{
		// Covers cascade with AdjustmentEntity (another IOwnedBy<ReceiptEntity> type)
		IDbContextFactory<ApplicationDbContext> contextFactory = DbContextHelpers.CreateInMemoryContextFactory();

		ReceiptEntity receipt = ReceiptEntityGenerator.Generate();
		AdjustmentEntity adjustment = new()
		{
			Id = Guid.NewGuid(),
			ReceiptId = receipt.Id,
			Type = Common.AdjustmentType.Tip,
			Amount = 5.00m,
			AmountCurrency = Common.Currency.USD
		};

		using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			await context.Receipts.AddAsync(receipt);
			await context.Adjustments.AddAsync(adjustment);
			await context.SaveChangesAsync();
		}

		// Act
		using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			ReceiptEntity r = await context.Receipts.FirstAsync();
			await context.Adjustments.Where(a => a.ReceiptId == receipt.Id).LoadAsync();
			context.Receipts.Remove(r);
			await context.SaveChangesAsync();
		}

		// Assert
		using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			List<AdjustmentEntity> allAdjustments = await context.Adjustments.IgnoreQueryFilters().ToListAsync();
			allAdjustments.Should().HaveCount(1);
			allAdjustments[0].DeletedAt.Should().NotBeNull();
		}

		contextFactory.ResetDatabase();
	}

	#endregion
}
