---
identifier: MGG-178
title: Click-to-select row in data tables
id: 4b84002b-cd9d-4fc2-8d70-1a6530a9d12e
status: Done
priority:
  value: 3
  name: Medium
createdBy: Michael Garofalo
project: Receipts
team: mggarofalo
labels:
  - frontend
  - Feature
milestone: "Phase 4 [Frontend]"
url: "https://linear.app/mggarofalo/issue/MGG-178/click-to-select-row-in-data-tables"
gitBranchName: mggarofalo/mgg-178-click-to-select-row-in-data-tables
createdAt: "2026-02-25T10:45:17.910Z"
updatedAt: "2026-02-26T02:35:52.173Z"
completedAt: "2026-02-26T02:35:52.151Z"
---

# Click-to-select row in data tables

Clicking a table row should select it, equivalent to using arrow keys for navigation. This enables a click → spacebar workflow for row selection/action.

## Requirements

* Clicking anywhere on a row marks it as the active/selected row
* Visual selection indicator matches keyboard-based selection
* Spacebar triggers the same action on a clicked-selected row as on a keyboard-selected row
* Should not interfere with inline action buttons within the row
