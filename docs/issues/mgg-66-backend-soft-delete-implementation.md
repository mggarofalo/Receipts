---
identifier: MGG-66
title: "Backend: Soft Delete Implementation"
id: 8cc43515-f26d-4f9d-b380-cfaac6af632d
status: Done
priority:
  value: 2
  name: High
createdBy: Michael Garofalo
project: Receipts
team: mggarofalo
labels:
  - backend
  - Feature
milestone: "Phase 4 [Backend]"
url: "https://linear.app/mggarofalo/issue/MGG-66/backend-soft-delete-implementation"
gitBranchName: mggarofalo/mgg-66-backend-soft-delete-implementation
createdAt: "2026-02-11T05:29:25.839Z"
updatedAt: "2026-02-21T13:01:17.154Z"
completedAt: "2026-02-21T13:01:17.136Z"
---

# Backend: Soft Delete Implementation

## Objective

Implement soft delete functionality across all core entities to prevent accidental data loss.

## Tasks

- [ ] Add soft delete fields to core entities (Receipt, ReceiptItem, Account, Transaction, Trip):
  - `DeletedAt` (DateTime?, nullable - null means not deleted)
  - `DeletedByUserId` (Guid?, nullable - FK to User)
  - `DeletedByApiKeyId` (Guid?, nullable - FK to ApiKey)
- [ ] Create `ISoftDeletable` interface for consistency
- [ ] Add navigation properties:
  - `DeletedByUser` (User, navigation property)
  - `DeletedByApiKey` (ApiKey, navigation property)
- [ ] Configure global query filter in DbContext:
  - `.HasQueryFilter(e => e.DeletedAt == null)` on all soft-deletable entities
  - Automatically exclude deleted records from all queries
- [ ] Configure foreign key relationships with cascading behavior
- [ ] Update all delete operations to set DeletedAt and appropriate foreign key
- [ ] Create extension methods:
  - `IncludeDeleted()` - Include deleted records in query
  - `OnlyDeleted()` - Show only deleted records (where DeletedAt != null)
- [ ] Update delete commands/handlers to perform soft delete
- [ ] Add restore functionality:
  - POST /api/{entity}/{id}/restore endpoint for each entity
  - Sets DeletedAt = null, clears DeletedByUserId/DeletedByApiKeyId
- [ ] Create database migration for new fields
- [ ] Add indexes on DeletedAt for query performance
- [ ] Document soft delete behavior

## Example Implementation

```csharp
public interface ISoftDeletable {
    DateTime? DeletedAt { get; set; }
    Guid? DeletedByUserId { get; set; }
    User DeletedByUser { get; set; }
    Guid? DeletedByApiKeyId { get; set; }
    ApiKey DeletedByApiKey { get; set; }
}

// DbContext configuration
modelBuilder.Entity<Receipt>()
    .HasQueryFilter(e => e.DeletedAt == null);

modelBuilder.Entity<Receipt>()
    .HasOne(r => r.DeletedByUser)
    .WithMany()
    .HasForeignKey(r => r.DeletedByUserId)
    .OnDelete(DeleteBehavior.Restrict);

modelBuilder.Entity<Receipt>()
    .HasOne(r => r.DeletedByApiKey)
    .WithMany()
    .HasForeignKey(r => r.DeletedByApiKeyId)
    .OnDelete(DeleteBehavior.Restrict);

// Delete command
receipt.DeletedAt = DateTime.UtcNow;
if (currentUser.IsApiKey) {
    receipt.DeletedByApiKeyId = currentUser.ApiKeyId;
} else {
    receipt.DeletedByUserId = currentUser.UserId;
}

// Restore command
receipt.DeletedAt = null;
receipt.DeletedByUserId = null;
receipt.DeletedByApiKeyId = null;
```

## Query Examples

```csharp
// Normal query - excludes deleted
var receipts = await _db.Receipts.ToListAsync();

// Include deleted items with who deleted them
var allReceipts = await _db.Receipts
    .IgnoreQueryFilters()
    .Include(r => r.DeletedByUser)
    .Include(r => r.DeletedByApiKey)
    .ToListAsync();

// Only deleted items
var deleted = await _db.Receipts
    .IgnoreQueryFilters()
    .Where(r => r.DeletedAt != null)
    .Include(r => r.DeletedByUser)
    .Include(r => r.DeletedByApiKey)
    .ToListAsync();

// Display who deleted it
var deletedBy = receipt.DeletedByUser?.UserName 
             ?? receipt.DeletedByApiKey?.Name 
             ?? "Unknown";
```

## Acceptance Criteria

* All core entities support soft delete
* Deleted records automatically hidden from queries (DeletedAt == null filter)
* Foreign keys enforce referential integrity
* Can explicitly query deleted records when needed
* Can eager load User/ApiKey details for deleted items
* Restore functionality works correctly (sets DeletedAt = null)
* Migrations applied successfully
* Performance impact minimal (indexes added)
* Navigation properties work correctly
