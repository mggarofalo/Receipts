# Domain

Core domain models with zero dependencies on other layers.

## Structure

- **`Core/`** — Entity classes: `Account`, `Receipt`, `ReceiptItem`, `Transaction`, `Category`, `Subcategory`, `ItemTemplate`, `Adjustment`
- **`Aggregates/`** — Composite domain objects: `ReceiptWithItems`, `TransactionAccount`, `Trip`
- **`Money.cs`** — Value object for monetary amounts (`decimal Amount` + `Currency Currency`)
- **`ValidationWarning.cs`** — Soft validation warnings returned alongside successful responses

## Conventions

- **ID convention:** All entities use `Guid` as their primary key type. `Guid.Empty` is the sentinel value for new (unsaved) entities.
- **Immutable construction:** Entities enforce invariants in their constructors. Invalid state cannot be created.
- **No framework dependencies:** Domain has no references to EF Core, ASP.NET, or any infrastructure concern.
- **Value objects:** `Money` wraps `decimal` + `Currency` to prevent currency mixing and provide arithmetic operators.
