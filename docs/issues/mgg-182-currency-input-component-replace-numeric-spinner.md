---
identifier: MGG-182
title: Currency input component (replace numeric spinner)
id: cf3a209d-62d6-42c3-9075-6fa532e3362c
status: Done
priority:
  value: 3
  name: Medium
assignee: Michael Garofalo
createdBy: Michael Garofalo
project: Receipts
team: mggarofalo
labels:
  - frontend
  - Feature
milestone: "Phase 4 [Frontend]"
url: "https://linear.app/mggarofalo/issue/MGG-182/currency-input-component-replace-numeric-spinner"
gitBranchName: mggarofalo/mgg-182-currency-input-component-replace-numeric-spinner
createdAt: "2026-02-25T10:45:34.493Z"
updatedAt: "2026-02-27T11:25:58.592Z"
completedAt: "2026-02-27T11:25:58.530Z"
---

# Currency input component (replace numeric spinner)

All monetary input fields (unit price, item price, totals, etc.) should use a clean currency input component instead of a raw numeric input with spinner arrows.

## Requirements

* No spinner arrows
* Currency symbol prefix (e.g., `$`)
* Auto-format to 2 decimal places on blur
* Accept only valid currency values (digits + single decimal point)
* Right-aligned text (standard for currency)
* Apply to all monetary fields: unit price, item price, receipt total, transaction amount, etc.
