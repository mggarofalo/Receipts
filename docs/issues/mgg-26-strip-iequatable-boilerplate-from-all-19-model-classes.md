---
identifier: MGG-26
title: Strip IEquatable boilerplate from all 19 model classes
id: 45806148-c1e6-4e23-96d7-225b82c426a4
status: Done
priority:
  value: 2
  name: High
createdBy: Michael Garofalo
project: Receipts
team: mggarofalo
url: "https://linear.app/mggarofalo/issue/MGG-26/strip-iequatable-boilerplate-from-all-19-model-classes"
gitBranchName: mggarofalo/mgg-26-strip-iequatable-boilerplate-from-all-19-model-classes
createdAt: "2026-02-11T04:59:26.618Z"
updatedAt: "2026-02-11T10:39:06.245Z"
completedAt: "2026-02-11T10:39:06.230Z"
---

# Strip IEquatable boilerplate from all 19 model classes

## Core change — must be done atomically

Remove `IEquatable<T>`, `Equals(T?)`, `Equals(object?)`, `GetHashCode()`, `operator==`, `operator!=` from all model classes. Keep properties, constructors, and validation.

**Domain Core (4 files):**

* `src/Domain/Core/Account.cs`
* `src/Domain/Core/Receipt.cs`
* `src/Domain/Core/ReceiptItem.cs`
* `src/Domain/Core/Transaction.cs`

**Domain Aggregates (3 files):**

* `src/Domain/Aggregates/ReceiptWithItems.cs`
* `src/Domain/Aggregates/TransactionAccount.cs`
* `src/Domain/Aggregates/Trip.cs`

**Infrastructure Entities (5 files):**

* `src/Infrastructure/Entities/Core/AccountEntity.cs`
* `src/Infrastructure/Entities/Core/ReceiptEntity.cs`
* `src/Infrastructure/Entities/Core/ReceiptItemEntity.cs`
* `src/Infrastructure/Entities/Core/TransactionEntity.cs`
* `src/Infrastructure/Entities/ApiKeyEntity.cs`

**Presentation ViewModels (7 files):**

* `src/Presentation/Shared/ViewModels/Core/AccountVM.cs`
* `src/Presentation/Shared/ViewModels/Core/ReceiptVM.cs`
* `src/Presentation/Shared/ViewModels/Core/ReceiptItemVM.cs`
* `src/Presentation/Shared/ViewModels/Core/TransactionVM.cs`
* `src/Presentation/Shared/ViewModels/Aggregates/ReceiptWithItemsVM.cs`
* `src/Presentation/Shared/ViewModels/Aggregates/TransactionAccountVM.cs`
* `src/Presentation/Shared/ViewModels/Aggregates/TripVM.cs`

**Note:** Must be done in one pass — aggregates use `==` on child types within their own `Equals` methods, so piecemeal removal would break compilation.

**Keep unchanged:** `Money` record type (built-in value equality is appropriate).

Commit: `refactor: remove IEquatable boilerplate from all model classes`
