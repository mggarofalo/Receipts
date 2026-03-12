---
identifier: MGG-228
title: Enforce narrow EF Core projections — minimum required fields in all queries
id: 40829fcf-c161-4d76-9a1e-107f65ead7f9
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
url: "https://linear.app/mggarofalo/issue/MGG-228/enforce-narrow-ef-core-projections-minimum-required-fields-in-all"
gitBranchName: mggarofalo/mgg-228-enforce-narrow-ef-core-projections-minimum-required-fields
createdAt: "2026-03-04T18:49:28.305Z"
updatedAt: "2026-03-04T20:06:16.978Z"
completedAt: "2026-03-04T20:06:16.963Z"
attachments:
  - title: "refactor(infrastructure): enforce narrow EF Core projections (MGG-228)"
    url: "https://github.com/mggarofalo/Receipts/pull/76"
---

# Enforce narrow EF Core projections — minimum required fields in all queries

## Context

EF Core queries in the repository layer mostly load full entities (`ToListAsync()` without `.Select()`), while audit/service queries already use good projection patterns. AutoInclude on several navigations (Subcategory→Category, Transaction→Receipt/Account, ReceiptItem→Receipt, Adjustment→Receipt) compounds the problem by loading related entities on every query.

### Current Good Patterns (keep)

* `AuditService` / `AuthAuditService`: `.Select(a => ToDto(a))`
* `ApiKeyService.GetApiKeysForUserAsync`: `.Select(k => new ApiKeyInfo(...))`
* `UserService.FindUserIdByRefreshTokenAsync`: `.Select(u => u.Id)`
* `AccountRepository.GetByTransactionIdAsync`: `.Select(t => t.Account)`

### Current Bad Patterns (fix)

* `ReceiptRepository.GetAllAsync`: loads full `ReceiptEntity` with all columns
* `TransactionRepository.GetByReceiptIdAsync`: loads full entities + AutoInclude navigations
* `ReceiptItemRepository.GetByReceiptIdAsync`: full entities + AutoInclude
* `AdjustmentRepository.GetByReceiptIdAsync`: full entities + AutoInclude
* All `DeleteAsync` methods: load full entities when only IDs needed for soft-delete
* `GetTransactionAccountsByReceiptIdQueryHandler`: N+1 queries in a loop

## Goal

1. Add `.Select()` projections to repository queries that currently load full entities
2. Use `.IgnoreAutoIncludes()` where navigation properties aren't needed
3. Fix the N+1 query in `GetTransactionAccountsByReceiptIdQueryHandler` with a single joined query
4. For delete operations, consider `ExecuteUpdateAsync` for soft-delete without materializing entities

## Acceptance Criteria

* All repository read queries use `.Select()` to project only required fields
* `.IgnoreAutoIncludes()` used where navigations aren't needed
* N+1 query patterns eliminated
* All tests pass
* No behavioral changes to API responses
