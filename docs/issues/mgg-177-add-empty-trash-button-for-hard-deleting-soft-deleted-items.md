---
identifier: MGG-177
title: Add Empty Trash button for hard-deleting soft-deleted items
id: ee0fc484-e510-492d-97e1-f9456eb87011
status: Done
priority:
  value: 3
  name: Medium
createdBy: Michael Garofalo
project: Receipts
team: mggarofalo
labels:
  - frontend
  - backend
  - Feature
milestone: "Phase 4 [Frontend]"
url: "https://linear.app/mggarofalo/issue/MGG-177/add-empty-trash-button-for-hard-deleting-soft-deleted-items"
gitBranchName: mggarofalo/mgg-177-add-empty-trash-button-for-hard-deleting-soft-deleted-items
createdAt: "2026-02-25T10:45:14.402Z"
updatedAt: "2026-02-26T03:16:20.994Z"
completedAt: "2026-02-26T03:16:20.981Z"
---

# Add Empty Trash button for hard-deleting soft-deleted items

Add an "Empty Trash" action that permanently hard-deletes all soft-deleted items.

## Requirements

* Button/action visible in the trash/deleted items view
* Confirmation dialog before executing (destructive and irreversible)
* Calls backend hard-delete endpoints for all soft-deleted entities
* Clear success/error feedback after operation
