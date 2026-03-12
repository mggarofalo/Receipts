---
identifier: MGG-179
title: Reorganize top nav bar with grouped hover menus
id: 441822ba-5009-40c8-a053-f3a2b38fa241
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
url: "https://linear.app/mggarofalo/issue/MGG-179/reorganize-top-nav-bar-with-grouped-hover-menus"
gitBranchName: mggarofalo/mgg-179-reorganize-top-nav-bar-with-grouped-hover-menus
createdAt: "2026-02-25T10:45:23.862Z"
updatedAt: "2026-02-25T14:21:10.502Z"
completedAt: "2026-02-25T14:21:10.473Z"
---

# Reorganize top nav bar with grouped hover menus

The top navigation bar should be reorganized into logical groups, with each group expanding its children on hover.

## Requirements

* Group related nav items into categories (e.g., "Data" → Receipts/Transactions/Trips, "Settings" → Users/Categories/Accounts, etc.)
* Hover over a group label expands a dropdown/flyout with the child items
* Reduce top-level clutter while keeping all pages accessible within one hover
* Responsive behavior for smaller viewports
* Active page indicator should still be visible at the group level
