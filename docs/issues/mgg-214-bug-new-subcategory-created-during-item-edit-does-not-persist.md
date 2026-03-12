---
identifier: MGG-214
title: "Bug: new subcategory created during item edit does not persist"
id: 3d222b78-8f71-4155-a7dd-e3d2abc9eaff
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
  - Bug
url: "https://linear.app/mggarofalo/issue/MGG-214/bug-new-subcategory-created-during-item-edit-does-not-persist"
gitBranchName: mggarofalo/mgg-214-bug-new-subcategory-created-during-item-edit-does-not
createdAt: "2026-03-02T03:57:08.594Z"
updatedAt: "2026-03-03T12:15:46.925Z"
completedAt: "2026-03-03T12:15:46.912Z"
attachments:
  - title: "fix(client): persist subcategories created during item edit (MGG-214)"
    url: "https://github.com/mggarofalo/Receipts/pull/59"
---

# Bug: new subcategory created during item edit does not persist

## Bug

When editing a receipt item and creating a new subcategory inline, the subcategory does not persist. Editing another item afterward shows the newly created subcategory is missing from the dropdown.

## Steps to Reproduce

1. Edit a receipt item
2. Create a new subcategory during the edit flow
3. Save the item
4. Edit a different receipt item
5. Open the subcategory dropdown — the new subcategory is not listed

## Expected

The new subcategory should persist and appear in the subcategory dropdown for subsequent edits.

## Likely Cause

The subcategory creation may only be updating local state / React Query cache without actually calling the backend API, or the mutation's `onSuccess` isn't invalidating the subcategories query cache.
