---
identifier: MGG-241
title: Navbar dropdown menus don't align to their trigger element
id: 11a98e37-01b0-4545-877f-281d9b7cd6dc
status: Done
priority:
  value: 4
  name: Low
assignee: Michael Garofalo
createdBy: Michael Garofalo
project: Receipts
team: mggarofalo
labels:
  - frontend
url: "https://linear.app/mggarofalo/issue/MGG-241/navbar-dropdown-menus-dont-align-to-their-trigger-element"
gitBranchName: mggarofalo/mgg-241-navbar-dropdown-menus-dont-align-to-their-trigger-element
createdAt: "2026-03-05T12:03:40.622Z"
updatedAt: "2026-03-05T14:01:51.813Z"
completedAt: "2026-03-05T14:01:51.801Z"
attachments:
  - title: "fix(client): align navbar dropdown menus to trigger elements (MGG-241)"
    url: "https://github.com/mggarofalo/Receipts/pull/85"
---

# Navbar dropdown menus don't align to their trigger element

## Bug

All navbar dropdown menus render at the same fixed position regardless of which menu trigger (e.g., "Data", "Admin") is hovered. The dropdown should be positioned relative to the hovered trigger element.

## Expected Behavior

* Each dropdown menu should appear directly beneath (or aligned to) its trigger element
* Hovering "Data" should open the dropdown anchored to the "Data" nav item
* Hovering "Admin" should open the dropdown anchored to the "Admin" nav item

## Acceptance Criteria

- [ ] Dropdown menus are positioned dynamically relative to their trigger element
- [ ] Dropdowns remain visually consistent across viewport sizes
