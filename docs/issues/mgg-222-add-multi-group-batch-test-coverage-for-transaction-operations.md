---
identifier: MGG-222
title: Add multi-group batch test coverage for transaction operations
id: 46aa8f77-9e70-43a8-8930-340c7fbbffc7
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
url: "https://linear.app/mggarofalo/issue/MGG-222/add-multi-group-batch-test-coverage-for-transaction-operations"
gitBranchName: mggarofalo/mgg-222-add-multi-group-batch-test-coverage-for-transaction
createdAt: "2026-03-03T21:21:04.508Z"
updatedAt: "2026-03-03T21:48:40.426Z"
completedAt: "2026-03-03T21:48:40.393Z"
attachments:
  - title: Add multi-group batch test coverage for transaction operations
    url: "https://github.com/mggarofalo/Receipts/pull/66"
---

# Add multi-group batch test coverage for transaction operations

## Summary

Add test coverage for the multi-group code path in `TransactionsController.CreateTransactions` and `UpdateTransactions`. Currently all batch tests use transactions with the same `AccountId`, so the `GroupBy(m => m.AccountId)` + `Task.WhenAll` path with multiple groups is never exercised.

## Acceptance criteria

- [ ] Add tests for `CreateTransactions` with transactions spanning 2+ distinct `AccountId` values
- [ ] Add tests for `UpdateTransactions` with transactions spanning 2+ distinct `AccountId` values
- [ ] Verify that results from all groups are correctly aggregated in the response
- [ ] Test the partial-failure scenario: one group succeeds, another fails — verify expected behavior

## Context

* `TransactionsController` groups batch requests by `AccountId` and dispatches per-group via `Task.WhenAll`
* Current tests only exercise the single-group path
* This was identified during PR #65 review
