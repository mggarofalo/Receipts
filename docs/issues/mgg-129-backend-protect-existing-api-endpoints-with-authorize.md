---
identifier: MGG-129
title: "Backend: Protect existing API endpoints with [Authorize]"
id: 8dc1b75b-c6a1-4de6-91ca-4651db9911ef
status: Done
priority:
  value: 1
  name: Urgent
createdBy: Michael Garofalo
project: Receipts
team: mggarofalo
labels:
  - security
  - backend
milestone: "Phase 4 [Backend]"
url: "https://linear.app/mggarofalo/issue/MGG-129/backend-protect-existing-api-endpoints-with-authorize"
gitBranchName: mggarofalo/mgg-129-backend-protect-existing-api-endpoints-with-authorize
createdAt: "2026-02-19T01:49:18.116Z"
updatedAt: "2026-02-20T10:39:58.712Z"
completedAt: "2026-02-20T10:39:58.696Z"
attachments:
  - title: openapi/spec.yaml
    url: "https://github.com/mggarofalo/Receipts/blob/master/openapi/spec.yaml"
  - title: scripts/check-drift.mjs
    url: "https://github.com/mggarofalo/Receipts/blob/master/scripts/check-drift.mjs"
---

# Backend: Protect existing API endpoints with [Authorize]

Add `[Authorize(Policy = "ApiOrJwt")]` to all existing controllers and document `security:` requirements on all protected paths in the OpenAPI spec.

## Controllers to update

All controllers except `HealthController`:

| Controller | Route |
| -- | -- |
| `AccountsController` | `/api/accounts` |
| `ReceiptsController` | `/api/receipts` |
| `TransactionsController` | `/api/transactions` |
| `ReceiptItemsController` | `/api/receiptitems` |
| `ReceiptWithItemsController` | `/api/receipts/{id}/...` |
| `TransactionAccountController` | `/api/transactionaccounts` |
| `TripController` | `/api/trips` |

## OpenAPI spec changes

Add a `security:` block to every protected operation in [`openapi/spec.yaml`](<https://github.com/mggarofalo/Receipts/blob/master/openapi/spec.yaml>):

```yaml
security:
  - BearerAuth: []
  - ApiKey: []
```

## Drift detector note

[`scripts/check-drift.mjs`](<https://github.com/mggarofalo/Receipts/blob/master/scripts/check-drift.mjs>) does **not** validate `security:` requirements on individual operations — only schema names, properties, request/response types, path params, and status codes are checked. Spec changes here are for documentation and Scalar UI only; they will not cause drift failures.

## Acceptance criteria

* All listed controllers return `401` when called without credentials
* `/api/health` remains unauthenticated
* All protected operations in `openapi/spec.yaml` have `security: [{BearerAuth: []}, {ApiKey: []}]`
* All existing tests still pass (`dotnet test`)
