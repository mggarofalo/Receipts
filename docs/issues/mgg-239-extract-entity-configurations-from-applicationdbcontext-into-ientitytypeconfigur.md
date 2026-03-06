---
identifier: MGG-239
title: Extract entity configurations from ApplicationDbContext into IEntityTypeConfiguration classes
id: b16e6cae-c3fc-4c7d-af14-aaa23ce18ddd
status: Done
priority:
  value: 4
  name: Low
assignee: Michael Garofalo
createdBy: Michael Garofalo
project: Receipts
team: mggarofalo
labels:
  - cleanup
  - backend
url: "https://linear.app/mggarofalo/issue/MGG-239/extract-entity-configurations-from-applicationdbcontext-into"
gitBranchName: mggarofalo/mgg-239-extract-entity-configurations-from-applicationdbcontext-into
createdAt: "2026-03-05T11:52:17.240Z"
updatedAt: "2026-03-05T14:39:59.937Z"
completedAt: "2026-03-05T14:39:59.920Z"
attachments:
  - title: "refactor(infrastructure): extract entity configurations from ApplicationDbContext (MGG-239)"
    url: "https://github.com/mggarofalo/Receipts/pull/87"
---

# Extract entity configurations from ApplicationDbContext into IEntityTypeConfiguration classes

## Problem

All entity configurations (key, property, navigation, conversion, etc.) are currently defined inline in `ApplicationDbContext.cs` via private static methods like `CreateReceiptItemEntity()`, `CreateReceiptEntity()`, etc. This makes the DbContext large and harder to maintain.

## Proposed Change

Extract each entity's configuration into its own `IEntityTypeConfiguration<T>` class under `src/Infrastructure/Configurations/`:

* `ReceiptEntityConfiguration`
* `ReceiptItemEntityConfiguration`
* `AdjustmentEntityConfiguration`
* `AccountEntityConfiguration`
* `ItemTemplateEntityConfiguration`
* (any others currently in ApplicationDbContext)

Then replace the private static methods in `ApplicationDbContext.OnModelCreating` with:

```csharp
modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
```

## Acceptance Criteria

- [ ] Each entity has its own `IEntityTypeConfiguration<T>` class
- [ ] All configurations live in `src/Infrastructure/Configurations/` (or `EntityConfigurations/`)
- [ ] `ApplicationDbContext.OnModelCreating` uses `ApplyConfigurationsFromAssembly`
- [ ] All private static `Create*Entity` methods are removed from `ApplicationDbContext`
- [ ] No behavioral changes — migrations snapshot should be identical before and after
- [ ] All existing tests pass
