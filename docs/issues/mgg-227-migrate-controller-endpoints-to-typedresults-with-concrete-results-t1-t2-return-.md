---
identifier: MGG-227
title: "Migrate controller endpoints to TypedResults with concrete Results<T1, T2, ...> return types"
id: c4a2189d-780c-4576-b767-b2f21641b86c
status: Done
priority:
  value: 3
  name: Medium
assignee: Michael Garofalo
createdBy: Michael Garofalo
project: Receipts
team: mggarofalo
labels:
  - backend
  - Improvement
url: "https://linear.app/mggarofalo/issue/MGG-227/migrate-controller-endpoints-to-typedresults-with-concrete-resultst1"
gitBranchName: mggarofalo/mgg-227-migrate-controller-endpoints-to-typedresults-with-concrete
createdAt: "2026-03-04T18:49:16.640Z"
updatedAt: "2026-03-04T20:39:38.606Z"
completedAt: "2026-03-04T20:39:38.587Z"
attachments:
  - title: "refactor(api): migrate endpoints to TypedResults (MGG-227)"
    url: "https://github.com/mggarofalo/Receipts/pull/77"
---

# Migrate controller endpoints to TypedResults with concrete Results<T1, T2, ...> return types

## Context

All 18 controllers currently use `ActionResult<T>` and `IActionResult` return types. These rely on runtime type checking and don't give the OpenAPI generator or compiler full knowledge of possible response types.

## Goal

Migrate all endpoint methods to use `TypedResults` with concrete `Results<T1, T2, ...>` union return types. For example:

```csharp
// Before
public async Task<ActionResult<AccountResponse>> GetAccountById([FromRoute] Guid id)
{
    // ...
    return NotFound();
    // or
    return Ok(model);
}

// After
public async Task<Results<Ok<AccountResponse>, NotFound>> GetAccountById([FromRoute] Guid id)
{
    // ...
    return TypedResults.NotFound();
    // or
    return TypedResults.Ok(model);
}
```

## Benefits

* Compile-time enforcement of declared response types — can't accidentally return an undeclared status code
* Better OpenAPI spec generation (response types derived from code, not attributes)
* Removes need for `[ProducesResponseType]` attributes — the return type IS the contract
* Aligns with modern [ASP.NET](<http://ASP.NET>) Core best practices

## Scope

All controllers in `src/Presentation/API/Controllers/`:

* Core: Accounts, Adjustments, Categories, ItemTemplates, ReceiptItems, Receipts, Subcategories, Transactions, Trash
* Aggregates: ReceiptWithItems, TransactionAccount, Trip
* Auth: Auth, ApiKey, Users, UserRoles
* Support: Health, Audit, AuthAudit

\~72 `ActionResult<T>` endpoints + several `IActionResult` endpoints.

## Acceptance Criteria

* All endpoints use `Results<T1, T2, ...>` return types with `TypedResults.*` helpers
* `[ProducesResponseType]` attributes removed (redundant with typed returns)
* All existing tests pass
* OpenAPI spec drift check passes
