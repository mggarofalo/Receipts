---
identifier: MGG-173
title: "Enter on focused row: edit dialog opens then immediately closes"
id: 1bcd1ca7-3d48-4044-9d3e-88c0c52fbe2a
status: Done
priority:
  value: 2
  name: High
assignee: Michael Garofalo
createdBy: Michael Garofalo
project: Receipts
team: mggarofalo
labels:
  - frontend
  - Bug
url: "https://linear.app/mggarofalo/issue/MGG-173/enter-on-focused-row-edit-dialog-opens-then-immediately-closes"
gitBranchName: mggarofalo/mgg-173-enter-on-focused-row-edit-dialog-opens-then-immediately
createdAt: "2026-02-24T13:15:00.810Z"
updatedAt: "2026-03-04T17:01:17.341Z"
completedAt: "2026-02-25T10:45:39.842Z"
attachments:
  - title: "fix(client): prevent Enter key from propagating into edit dialog (MGG-173)"
    url: "https://github.com/mggarofalo/Receipts/pull/31"
---

# Enter on focused row: edit dialog opens then immediately closes

## Bug

When a row is focused via j/k navigation and Enter is pressed, the edit dialog opens but immediately closes, and an "Updated" toast appears — as if the form inside the dialog was submitted instantly.

## Root Cause

The `Enter` keydown event propagates into the newly-opened dialog. The edit form's submit button (or the form element itself) receives the same keydown event, triggering an immediate form submission with unchanged values. This causes:

1. Dialog opens (shortcut handler fires)
2. Same Enter keydown event reaches the form inside the dialog
3. Form submits with current values → "Updated" toast
4. Dialog closes on successful submit

## Reproduction

1. Navigate to Accounts page (or any CRUD list page with data)
2. Press `j` to focus a row
3. Press `Enter`
4. Expected: Edit dialog opens and stays open for editing
5. Actual: Edit dialog flashes open then immediately closes. "Updated" toast appears.

## Fix

In the `Enter` hotkey handler in `useListKeyboardNav.ts:69-81`, either:

* Use `setTimeout(() => onOpen(items[focusedIndex]), 0)` to defer dialog opening to the next tick
* Call `event.preventDefault()` and `event.stopPropagation()` before opening
* In the form component, ignore Enter events that arrive within the first \~100ms of dialog mount

## Files

* `src/client/src/hooks/useListKeyboardNav.ts:69-81` (Enter handler)
* Form components that handle Ctrl+Enter submit
