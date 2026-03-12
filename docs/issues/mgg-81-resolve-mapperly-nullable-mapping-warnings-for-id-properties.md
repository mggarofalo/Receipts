---
identifier: MGG-81
title: Resolve Mapperly nullable mapping warnings for ID properties
id: af4d60c6-ce8f-4fe6-b224-bef6ff4f7663
status: Done
priority:
  value: 2
  name: High
createdBy: Michael Garofalo
project: Receipts
team: mggarofalo
labels:
  - Bug
url: "https://linear.app/mggarofalo/issue/MGG-81/resolve-mapperly-nullable-mapping-warnings-for-id-properties"
gitBranchName: mggarofalo/mgg-81-resolve-mapperly-nullable-mapping-warnings-for-id-properties
createdAt: "2026-02-11T11:20:43.496Z"
updatedAt: "2026-02-12T11:47:38.650Z"
completedAt: "2026-02-11T13:33:58.979Z"
---

# Resolve Mapperly nullable mapping warnings for ID properties

## Problem

The LSP server is showing Mapperly warnings about nullable ID mappings:

> Mapping the nullable source property Id of Domain.Core.Account to the target property Id of Infrastructure.Entities.Core.AccountEntity which is not nullable

Similar warnings exist for all entity mappers (Receipt, ReceiptItem, Transaction).

## Root Cause

Domain entities have nullable ID properties (`Guid? Id`), but Infrastructure entities have non-nullable ID properties (`Guid Id`).

## Scope (Revised)

**Domain entities only.** ViewModels are being deprecated in favor of DTOs (see [MGG-82](./mgg-82-update-github-actions-workflow-to-net-10-and-add-comprehensive-ci-steps.md)), so we leave VM nullability as-is.

## Acceptance Criteria

- [ ] Change all Domain entity ID properties from `Guid?` to `Guid` (Account, Receipt, ReceiptItem, Transaction)
- [ ] Change constructor parameters from `Guid? id` to `Guid id`
- [ ] Use `Guid.Empty` as the sentinel for "new, not yet persisted" entities
- [ ] Update Domain unit tests: replace `Constructor_NullId_*` tests with `Constructor_EmptyId_*` tests
- [ ] Verify no Mapperly nullable mapping warnings remain in LSP
- [ ] Run all tests to ensure no regressions
- [ ] Do NOT modify ViewModels — they will be replaced by DTOs in [MGG-82](./mgg-82-update-github-actions-workflow-to-net-10-and-add-comprehensive-ci-steps.md)
