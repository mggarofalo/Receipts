---
identifier: MGG-185
title: Replace UUID input in Trip search with contextual picker
id: 8bbd53d1-c4d7-4d2d-a43b-639ec159a466
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
url: "https://linear.app/mggarofalo/issue/MGG-185/replace-uuid-input-in-trip-search-with-contextual-picker"
gitBranchName: mggarofalo/mgg-185-replace-uuid-input-in-trip-search-with-contextual-picker
createdAt: "2026-02-25T10:45:50.109Z"
updatedAt: "2026-02-26T12:36:28.368Z"
completedAt: "2026-02-26T12:36:28.343Z"
---

# Replace UUID input in Trip search with contextual picker

Trip search currently requires entering a receipt UUID to find trips. Users don't work with UUIDs.

## Requirements

* Replace receipt UUID text field with a searchable receipt picker (search by date, store name, amount, etc.)
* Or: allow browsing trips directly and filtering by attributes, without needing to reference a receipt at all
* No UUID fields should remain on this form
