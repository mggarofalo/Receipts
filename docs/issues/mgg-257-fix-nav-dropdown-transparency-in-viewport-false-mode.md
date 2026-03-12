---
identifier: MGG-257
title: Fix nav dropdown transparency in viewport=false mode
id: 0b9cbc86-ae0f-4d4b-bc2c-6cfbe7f93789
status: Done
priority:
  value: 1
  name: Urgent
assignee: Michael Garofalo
createdBy: Michael Garofalo
project: Receipts
team: mggarofalo
labels:
  - frontend
url: "https://linear.app/mggarofalo/issue/MGG-257/fix-nav-dropdown-transparency-in-viewportfalse-mode"
gitBranchName: mggarofalo/mgg-257-fix-nav-dropdown-transparency-in-viewportfalse-mode
createdAt: "2026-03-06T10:59:17.518Z"
updatedAt: "2026-03-06T10:59:17.518Z"
completedAt: "2026-03-06T10:59:17.576Z"
attachments:
  - title: "PR #96: fix(client): add z-50 to nav dropdown"
    url: "https://github.com/mggarofalo/Receipts/pull/96"
---

# Fix nav dropdown transparency in viewport=false mode

## Problem

Navigation menu dropdowns (Data, Manage, Admin) appear transparent/see-through in dark mode. Page content bleeds through the dropdown overlay.

## Root Cause

`NavigationMenuContent` in `viewport={false}` mode had no `z-index`. The viewport wrapper (`NavigationMenuViewport`) provides `z-50`, but when `viewport={false}` the content renders directly — without a stacking context. Since `<main>` follows `<header>` in DOM order, it paints on top of the absolutely-positioned dropdown.

Every other shadcn overlay component (dropdown-menu, popover, dialog, tooltip) already uses `z-50`.

## Fix

Added `group-data-[viewport=false]/navigation-menu:z-50` to `NavigationMenuContent` styles in `src/client/src/components/ui/navigation-menu.tsx`.

## PR

[https://github.com/mggarofalo/Receipts/pull/96](<https://github.com/mggarofalo/Receipts/pull/96>)
