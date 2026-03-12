---
identifier: MGG-189
title: Fix flash of white background on dark mode page load
id: 823d81f1-0283-4c02-80ea-2366d58bfcc4
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
url: "https://linear.app/mggarofalo/issue/MGG-189/fix-flash-of-white-background-on-dark-mode-page-load"
gitBranchName: mggarofalo/mgg-189-fix-flash-of-white-background-on-dark-mode-page-load
createdAt: "2026-02-25T13:58:23.057Z"
updatedAt: "2026-02-25T14:04:11.019Z"
completedAt: "2026-02-25T14:04:10.999Z"
---

# Fix flash of white background on dark mode page load

When loading the app in dark mode, there's a brief flash of white background before Tailwind CSS loads and applies `bg-background`. The inline script in `index.html` correctly adds `.dark` to `<html>`, but the body has no background color until CSS is parsed.

Fix: set `background-color` and `color-scheme` directly on `<html>` in the inline script to prevent the flash.
