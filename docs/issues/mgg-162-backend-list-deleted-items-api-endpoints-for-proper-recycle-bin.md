---
identifier: MGG-162
title: "Backend: List deleted items API endpoints for proper recycle bin"
id: ae066668-8887-4122-ba51-d9adbe742352
status: Done
priority:
  value: 4
  name: Low
createdBy: Michael Garofalo
project: Receipts
team: mggarofalo
labels:
  - backend
  - Feature
milestone: "Phase 4 [Frontend]"
url: "https://linear.app/mggarofalo/issue/MGG-162/backend-list-deleted-items-api-endpoints-for-proper-recycle-bin"
gitBranchName: mggarofalo/mgg-162-backend-list-deleted-items-api-endpoints-for-proper-recycle
createdAt: "2026-02-22T02:39:23.638Z"
updatedAt: "2026-02-22T03:10:10.694Z"
completedAt: "2026-02-22T03:10:10.679Z"
---

# Backend: List deleted items API endpoints for proper recycle bin

The current recycle bin UI (MGG-69) uses the audit log as a workaround to show deleted items. Proper "list deleted items" endpoints are needed for each entity type to support pagination, filtering, and showing full entity details (not just entity IDs).

Endpoints to add:

* `GET /api/accounts/deleted` → list soft-deleted accounts
* `GET /api/receipts/deleted` → list soft-deleted receipts
* `GET /api/receipt-items/deleted` → list soft-deleted receipt items
* `GET /api/transactions/deleted` → list soft-deleted transactions

Each should support pagination and return full entity data.
