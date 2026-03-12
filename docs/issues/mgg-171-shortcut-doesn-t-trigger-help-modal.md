---
identifier: MGG-171
title: "? shortcut doesn't trigger help modal"
id: b9af5468-b470-4e0d-9b38-db32b8b7959a
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
url: "https://linear.app/mggarofalo/issue/MGG-171/shortcut-doesnt-trigger-help-modal"
gitBranchName: mggarofalo/mgg-171-shortcut-doesnt-trigger-help-modal
createdAt: "2026-02-24T13:14:44.406Z"
updatedAt: "2026-03-04T17:01:17.440Z"
completedAt: "2026-02-25T10:45:38.568Z"
attachments:
  - title: "fix(client): bind help modal shortcut to ? instead of shift+/ (MGG-171)"
    url: "https://github.com/mggarofalo/Receipts/pull/29"
---

# ? shortcut doesn't trigger help modal

## Bug

The `?` keyboard shortcut (bound as `shift+/` in `react-hotkeys-hook`) does not open the ShortcutsHelp modal in real browsers.

## Root Cause

The `useHotkeys("shift+/", ...)` binding in `useGlobalShortcuts.ts:10` likely has a key-matching mismatch. When a user presses Shift+/ on a US keyboard layout, the browser emits `key: "?"` with `shiftKey: true`. The `react-hotkeys-hook` library may not correctly map `shift+/` to match this event.

## Reproduction

1. Navigate to any page in the app
2. Ensure no form element has focus
3. Press `?` (Shift+/)
4. Expected: ShortcutsHelp modal opens
5. Actual: Nothing happens

## Fix Options

* Change binding from `shift+/` to `?` (if react-hotkeys-hook supports it)
* Use `useKeyboardShortcut` (plain `addEventListener`) instead of `useHotkeys` for this binding
* Add a second binding: `useHotkeys("shift+/, ?", ...)`

## Files

* `src/client/src/hooks/useGlobalShortcuts.ts:10`
* `src/client/src/components/ShortcutsHelp.tsx`
