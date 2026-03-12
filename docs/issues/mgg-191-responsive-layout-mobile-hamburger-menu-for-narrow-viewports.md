---
identifier: MGG-191
title: "Responsive layout: mobile hamburger menu for narrow viewports"
id: 42bfcc3f-7ceb-4c85-a6a5-f0a38aa1e14c
status: Done
priority:
  value: 1
  name: Urgent
createdBy: Michael Garofalo
project: Receipts
team: mggarofalo
labels:
  - frontend
  - Improvement
url: "https://linear.app/mggarofalo/issue/MGG-191/responsive-layout-mobile-hamburger-menu-for-narrow-viewports"
gitBranchName: mggarofalo/mgg-191-responsive-layout-mobile-hamburger-menu-for-narrow-viewports
createdAt: "2026-02-25T14:10:07.303Z"
updatedAt: "2026-02-25T14:12:48.327Z"
completedAt: "2026-02-25T14:12:48.306Z"
---

# Responsive layout: mobile hamburger menu for narrow viewports

The Layout header has 7-10 nav links in a fixed horizontal row that forces a minimum page width. At narrower viewports the page doesn't reflow.

Fix: add a hamburger menu (Sheet drawer) for mobile/tablet, hide desktop nav links below `lg` breakpoint. Move search button, SignalR status, theme toggle, and user dropdown into the mobile menu as well.
