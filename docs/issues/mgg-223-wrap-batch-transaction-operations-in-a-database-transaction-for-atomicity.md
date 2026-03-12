---
identifier: MGG-223
title: Wrap batch transaction operations in a database transaction for atomicity
id: 7e0ab685-5c80-44f2-acd8-358334d850fb
status: Done
priority:
  value: 2
  name: High
assignee: Michael Garofalo
createdBy: Michael Garofalo
project: Receipts
team: mggarofalo
labels:
  - backend
url: "https://linear.app/mggarofalo/issue/MGG-223/wrap-batch-transaction-operations-in-a-database-transaction-for"
gitBranchName: mggarofalo/mgg-223-wrap-batch-transaction-operations-in-a-database-transaction
createdAt: "2026-03-03T21:21:14.298Z"
updatedAt: "2026-03-04T16:49:04.015Z"
completedAt: "2026-03-04T16:49:03.994Z"
attachments:
  - title: "feat(application): add batch transaction commands for atomic create/update (MGG-223)"
    url: "https://github.com/mggarofalo/Receipts/pull/68"
---

# Wrap batch transaction operations in a database transaction for atomicity

## Summary

`TransactionsController.CreateTransactions` and `UpdateTransactions` group batch requests by `AccountId` and dispatch per-group commands via `Task.WhenAll`. If one group's command fails (validation error, not found, etc.) but others succeed, the successful groups are already persisted, leading to a partial update with no way to roll back.

Wrap the entire batch operation in a database transaction so that either all groups succeed or all are rolled back.

## Acceptance criteria

- [ ] Batch create (`POST /api/receipts/{receiptId}/transactions/batch`) is atomic — partial failure rolls back all groups
- [ ] Batch update (`PUT /api/transactions/batch`) is atomic — partial failure rolls back all groups
- [ ] Add integration or unit tests verifying rollback on partial failure
- [ ] Consider whether the same pattern applies to receipt-items batch operations (they don't group, but future-proofing)

## Implementation options

1. **Controller-level** `IDbContextTransaction`: Inject `DbContext`, begin transaction before `Task.WhenAll`, commit/rollback after
2. **MediatR pipeline behavior**: Add a `TransactionBehavior` pipeline that wraps command handlers in a `TransactionScope`
3. **Move grouping logic into a single command handler**: Instead of dispatching N commands, have a single `CreateTransactionBatchCommand` that handles grouping internally within one unit of work

Option 3 is likely cleanest — it moves the grouping concern out of the controller and into the domain layer where it can be properly unit tested.

## Context

* Identified during PR #65 review
* Current behavior: `Task.WhenAll` fires concurrent commands, each independently validated and persisted
* Risk: partial updates on validation failure leave data in an inconsistent state
