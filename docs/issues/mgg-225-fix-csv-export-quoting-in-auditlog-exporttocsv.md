---
identifier: MGG-225
title: Fix CSV export quoting in AuditLog exportToCsv
id: 7b33f47a-4bc0-4c38-8840-66f3753288e4
status: Done
priority:
  value: 4
  name: Low
assignee: Michael Garofalo
createdBy: Michael Garofalo
project: Receipts
team: mggarofalo
url: "https://linear.app/mggarofalo/issue/MGG-225/fix-csv-export-quoting-in-auditlog-exporttocsv"
gitBranchName: mggarofalo/mgg-225-fix-csv-export-quoting-in-auditlog-exporttocsv
createdAt: "2026-03-04T16:37:03.529Z"
updatedAt: "2026-03-04T18:38:55.648Z"
completedAt: "2026-03-04T18:38:55.624Z"
attachments:
  - title: Fix CSV export quoting in AuditLog exportToCsv (MGG-225)
    url: "https://github.com/mggarofalo/Receipts/pull/72"
---

# Fix CSV export quoting in AuditLog exportToCsv

## Bug

In `src/client/src/pages/AuditLog.tsx`, the `exportToCsv` function only applies RFC-4180 quoting to the `changesJson` field, while all other fields (timestamp, user, action, entityType, entityId) are emitted raw without quoting or escaping.

If any of those fields contain commas, double quotes, or newlines, the resulting CSV will be malformed.

## Expected Behavior

All fields in the CSV output should be properly quoted per RFC-4180 to handle special characters.

## Current Behavior

Only `changesJson` is wrapped in quotes with internal quotes escaped. Other fields are concatenated directly with comma separators.
