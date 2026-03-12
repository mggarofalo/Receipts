---
identifier: MGG-172
title: Ctrl+Shift+N intercepted by Chrome (opens incognito)
id: e2b2d77a-c91e-48d3-a279-550d2d15d2ee
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
url: "https://linear.app/mggarofalo/issue/MGG-172/ctrlshiftn-intercepted-by-chrome-opens-incognito"
gitBranchName: mggarofalo/mgg-172-ctrlshiftn-intercepted-by-chrome-opens-incognito
createdAt: "2026-02-24T13:14:51.421Z"
updatedAt: "2026-03-04T17:01:17.393Z"
completedAt: "2026-02-25T10:45:39.245Z"
attachments:
  - title: "fix(client): replace Ctrl+Shift+N with Shift+N for new item shortcut (MGG-172)"
    url: "https://github.com/mggarofalo/Receipts/pull/30"
---

# Ctrl+Shift+N intercepted by Chrome (opens incognito)

## Bug

The `Ctrl+Shift+N` shortcut for "New Item" is intercepted by Chrome (and other browsers) before it reaches the app. Chrome uses Ctrl+Shift+N to open a new incognito window.

## Root Cause

`useGlobalShortcuts.ts:19` binds `mod+shift+n`, which maps to `Ctrl+Shift+N` on Windows/Linux and `Cmd+Shift+N` on macOS. Both are reserved browser shortcuts:

* Windows/Linux: Ctrl+Shift+N = New incognito window (Chrome/Edge)
* macOS: Cmd+Shift+N = New folder (Finder) / varies by browser

## Reproduction

1. Navigate to any CRUD page (e.g., Accounts)
2. Press Ctrl+Shift+N
3. Expected: Create dialog opens
4. Actual: Chrome opens a new incognito window

## Fix

Choose a keybinding that doesn't conflict with browser shortcuts. Suggestions:

* `Alt+N` — not reserved by major browsers
* `Ctrl+Shift+O` or another unused combo
* A single-key shortcut like `n` (when not focused in a form field)

## Files

* `src/client/src/hooks/useGlobalShortcuts.ts:19`
* `src/client/src/lib/shortcut-registry.ts`
