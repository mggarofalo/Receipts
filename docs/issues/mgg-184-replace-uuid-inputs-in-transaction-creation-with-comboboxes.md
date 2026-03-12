---
identifier: MGG-184
title: Replace UUID inputs in Transaction creation with comboboxes
id: 63690924-9792-4ac2-bf39-08fcb5754919
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
url: "https://linear.app/mggarofalo/issue/MGG-184/replace-uuid-inputs-in-transaction-creation-with-comboboxes"
gitBranchName: mggarofalo/mgg-184-replace-uuid-inputs-in-transaction-creation-with-comboboxes
createdAt: "2026-02-25T10:45:47.008Z"
updatedAt: "2026-02-26T12:36:27.607Z"
completedAt: "2026-02-26T12:36:27.587Z"
---

# Replace UUID inputs in Transaction creation with comboboxes

Creating a transaction currently requires manually entering receipt and account UUIDs. This is incorrect — users don't know or work with UUIDs.

## Requirements

* **Account**: Replace UUID text field with a searchable combobox populated from the user's accounts (display account name, resolve to ID internally)
* **Receipt**: Transactions should ideally be created from within a receipt's context (so the receipt is implicit). If accessed standalone, provide a searchable receipt picker (by date, store, amount, etc.)
* No UUID fields should remain on this form
