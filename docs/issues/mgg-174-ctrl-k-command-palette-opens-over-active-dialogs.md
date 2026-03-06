---
identifier: MGG-174
title: Ctrl+K command palette opens over active dialogs
id: 83047ad1-f7e9-4323-adaa-48544f2e6dfb
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
url: "https://linear.app/mggarofalo/issue/MGG-174/ctrlk-command-palette-opens-over-active-dialogs"
gitBranchName: mggarofalo/mgg-174-ctrlk-command-palette-opens-over-active-dialogs
createdAt: "2026-02-24T13:15:08.094Z"
updatedAt: "2026-03-04T17:01:17.484Z"
completedAt: "2026-02-25T10:45:40.694Z"
attachments:
  - title: "fix(client): suppress keyboard shortcuts when a dialog is open (MGG-174)"
    url: "https://github.com/mggarofalo/Receipts/pull/32"
---

# Ctrl+K command palette opens over active dialogs

## Bug

When a dialog (e.g., Create Account) is open, pressing Ctrl+K still opens the command palette/search dialog on top of it. Global shortcuts should be suppressed when any dialog is active.

## Root Cause

`GlobalSearchDialog.tsx:86` uses `useKeyboardShortcut({ key: "k", handler: toggleOpen })` which attaches a plain `document.addEventListener("keydown")` listener. This listener doesn't check whether any dialog is currently open.

In contrast, `useListKeyboardNav.ts` uses `react-hotkeys-hook` with `enableOnFormTags: false`, which correctly suppresses shortcuts when focus is inside a form element (like the dialog's form fields).

## Reproduction

1. Navigate to Accounts page
2. Click "New Account" to open the create dialog
3. Press Ctrl+K
4. Expected: Nothing happens (shortcut suppressed while dialog is open)
5. Actual: Command palette opens over the create dialog

## Fix

Add a dialog-open check to `useKeyboardShortcut`:

* Check `document.querySelector('[role=dialog]')` before firing
* Or pass an `enabled` prop that checks a "dialog open" context
* Or move Ctrl+K to `react-hotkeys-hook` with scope-based suppression

## Files

* `src/client/src/hooks/useKeyboardShortcut.ts`
* `src/client/src/components/GlobalSearchDialog.tsx:86`
