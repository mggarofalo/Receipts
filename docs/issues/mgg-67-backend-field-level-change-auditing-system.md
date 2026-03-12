---
identifier: MGG-67
title: "Backend: Field-Level Change Auditing System"
id: 3251bc22-9971-489e-8944-5067edeeb0e4
status: Done
priority:
  value: 2
  name: High
createdBy: Michael Garofalo
project: Receipts
team: mggarofalo
labels:
  - security
  - backend
milestone: "Phase 4 [Backend]"
url: "https://linear.app/mggarofalo/issue/MGG-67/backend-field-level-change-auditing-system"
gitBranchName: mggarofalo/mgg-67-backend-field-level-change-auditing-system
createdAt: "2026-02-11T05:29:37.707Z"
updatedAt: "2026-02-21T14:10:37.318Z"
completedAt: "2026-02-21T14:10:37.276Z"
---

# Backend: Field-Level Change Auditing System

## Objective

Implement comprehensive field-level audit logging that tracks what changed, when, and by whom on all core entities, using foreign keys for referential integrity.

## Tasks

- [ ] Create `AuditLog` entity:
  * `Id` (Guid)
  * `EntityType` (string, e.g., "Receipt")
  * `EntityId` (string, the entity's ID)
  * `Action` (enum: Create, Update, Delete, Restore)
  * `Changes` (JSON, array of field changes)
  * `ChangedByUserId` (Guid?, nullable - FK to User)
  * `ChangedByApiKeyId` (Guid?, nullable - FK to ApiKey)
  * `ChangedAt` (DateTime)
  * `IpAddress` (string, optional)
- [ ] Add navigation properties to AuditLog:
  * `ChangedByUser` (User)
  * `ChangedByApiKey` (ApiKey)
- [ ] Create `FieldChange` model for JSON storage:
  * `FieldName` (string)
  * `OldValue` (string/JSON)
  * `NewValue` (string/JSON)
- [ ] Configure foreign key relationships in DbContext
- [ ] Override `SaveChangesAsync` in DbContext:
  * Detect all entity changes using ChangeTracker
  * For each modified entity, capture field-level changes
  * Create AuditLog entries with proper foreign keys
  * Save audit logs in same transaction
- [ ] Implement audit log for all core entities:
  * Receipt, ReceiptItem, Account, Transaction, Trip
- [ ] Add audit logging for soft deletes and restores
- [ ] Create `IAuditable` interface to mark entities for auditing
- [ ] Exclude sensitive fields if needed (e.g., password hashes)
- [ ] Add indexes:
  * `(EntityType, EntityId)` for entity history queries
  * `ChangedAt` for time-based queries
  * `ChangedByUserId` for user activity queries
  * `ChangedByApiKeyId` for API key activity queries
- [ ] Create query methods:
  * `GetEntityHistory(entityType, entityId)` - All changes to specific entity
  * `GetUserActivity(userId)` - All changes by specific user
  * `GetApiKeyActivity(apiKeyId)` - All changes by specific API key
  * `GetRecentChanges(limit)` - Recent audit log entries
- [ ] Add API endpoints:
  * GET /api/audit/entity/{type}/{id} - Entity history
  * GET /api/audit/recent - Recent changes
  * GET /api/audit/user/{userId} - User activity
  * GET /api/audit/apikey/{apiKeyId} - API key activity

## Example AuditLog Entry

```json
{
  "id": "abc-123",
  "entityType": "Receipt",
  "entityId": "550e8400...",
  "action": "Update",
  "changes": [
    {
      "fieldName": "Amount",
      "oldValue": "50.00",
      "newValue": "75.00"
    },
    {
      "fieldName": "Merchant",
      "oldValue": "Costco",
      "newValue": "Costco Wholesale"
    }
  ],
  "changedByUserId": "abc-123",
  "changedByApiKeyId": null,
  "changedAt": "2024-01-16T10:30:00Z",
  "ipAddress": "192.168.1.100"
}
```

## Query Examples

```csharp
// Get entity history with who made changes
var history = await _db.AuditLogs
    .Where(a => a.EntityType == "Receipt" && a.EntityId == receiptId)
    .Include(a => a.ChangedByUser)
    .Include(a => a.ChangedByApiKey)
    .OrderByDescending(a => a.ChangedAt)
    .ToListAsync();

// Display who made the change
foreach (var log in history) {
    var changedBy = log.ChangedByUser?.UserName 
                 ?? log.ChangedByApiKey?.Name 
                 ?? "System";
    Console.WriteLine($"{changedBy} changed {log.Changes.Count} fields");
}

// Get all changes by a specific user
var userChanges = await _db.AuditLogs
    .Where(a => a.ChangedByUserId == userId)
    .Include(a => a.ChangedByUser)
    .ToListAsync();

// Get all changes by a specific API key
var apiKeyChanges = await _db.AuditLogs
    .Where(a => a.ChangedByApiKeyId == apiKeyId)
    .Include(a => a.ChangedByApiKey)
    .ToListAsync();
```

## DbContext Configuration

```csharp
modelBuilder.Entity<AuditLog>()
    .HasOne(a => a.ChangedByUser)
    .WithMany()
    .HasForeignKey(a => a.ChangedByUserId)
    .OnDelete(DeleteBehavior.Restrict);

modelBuilder.Entity<AuditLog>()
    .HasOne(a => a.ChangedByApiKey)
    .WithMany()
    .HasForeignKey(a => a.ChangedByApiKeyId)
    .OnDelete(DeleteBehavior.Restrict);
```

## Acceptance Criteria

* All creates, updates, deletes logged automatically
* Field-level changes captured with before/after values
* Foreign keys enforce referential integrity (User/ApiKey must exist)
* Audit logs stored in same transaction (atomic)
* No retention policy (keep forever)
* API endpoints return audit history with user/API key details
* Queries performant with proper indexes
* Soft deletes and restores audited
* Navigation properties allow eager loading of User/ApiKey details
* Can filter by user or API key efficiently
