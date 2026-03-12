---
identifier: MGG-192
title: Fix focus outline on table row click
id: 6e87b6fb-9c5e-49c9-9295-b2344bb5acc8
status: Done
priority:
  value: 1
  name: Urgent
createdBy: Michael Garofalo
project: Receipts
team: mggarofalo
labels:
  - cleanup
  - frontend
milestone: "Phase 4 [Frontend]"
url: "https://linear.app/mggarofalo/issue/MGG-192/fix-focus-outline-on-table-row-click"
gitBranchName: mggarofalo/mgg-192-fix-focus-outline-on-table-row-click
createdAt: "2026-02-26T02:38:44.711Z"
updatedAt: "2026-02-26T02:50:48.360Z"
completedAt: "2026-02-26T02:50:48.343Z"
---

# Fix focus outline on table row click

After [MGG-178](./mgg-178-click-to-select-row-in-data-tables.md) added click-to-select on table rows, clicking a row shows a large browser focus outline/ring. The outline should be suppressed since the `bg-accent` highlight already provides visual feedback for the selected state.\\n\\n## Requirements\\n\\n\* Remove the default browser focus outline on clicked table rows\\n\* Keep the existing `bg-accent` selection highlight as the visual indicator\\n\* Ensure keyboard focus indicators are still accessible (only suppress for mouse clicks)
