---
identifier: MGG-13
title: Refactor TotalAmount calculation to AutoMapper
id: 11f25c9b-c551-41d8-8a37-7ea5f42c1b2f
status: Done
assignee: Michael Garofalo
createdBy: Michael Garofalo
project: Receipts
team: mggarofalo
url: "https://linear.app/mggarofalo/issue/MGG-13/refactor-totalamount-calculation-to-automapper"
gitBranchName: mggarofalo/mgg-13-refactor-totalamount-calculation-to-automapper
createdAt: "2026-01-14T10:25:20.426Z"
updatedAt: "2026-02-11T23:46:04.512Z"
completedAt: "2026-02-11T23:46:04.498Z"
attachments:
  - title: "refactor(shared): remove TotalAmount from ReceiptItemVM, calculate in mapper (MGG-13)"
    url: "https://github.com/mggarofalo/Receipts/pull/5"
---

# Refactor TotalAmount calculation to AutoMapper

## Summary

There's a TODO comment indicating TotalAmount should be calculated in the mapper instead of being a stored property.

## Details

* **File:** `src/Presentation/Shared/ViewModels/Core/ReceiptItemVM.cs` (line 10)
* **TODO:** `// TODO: Remove this property and calculate it in the mapper instead`

## Tasks

1. Remove `TotalAmount` property from `ReceiptItemVM.cs`
2. Update AutoMapper profile in Infrastructure to calculate during mapping
3. Update AutoMapper profile in API to calculate during mapping
4. Update any client code that depends on TotalAmount
5. Update tests

## Acceptance Criteria

- [ ] TotalAmount calculated as `Quantity * UnitPrice` in mapper
- [ ] Property removed from view model
- [ ] All tests pass
- [ ] Client displays correct calculated totals
