---
identifier: MGG-215
title: "Clean up REST API surface: extract non-collection path params to query params"
id: 82d90cbe-b668-4b9e-82e6-5b2b8c61be4d
status: Done
priority:
  value: 4
  name: Low
assignee: Michael Garofalo
createdBy: Michael Garofalo
project: Receipts
team: mggarofalo
labels:
  - codegen
  - cleanup
  - frontend
  - backend
url: "https://linear.app/mggarofalo/issue/MGG-215/clean-up-rest-api-surface-extract-non-collection-path-params-to-query"
gitBranchName: mggarofalo/mgg-215-clean-up-rest-api-surface-extract-non-collection-path-params
createdAt: "2026-03-03T12:36:28.450Z"
updatedAt: "2026-03-03T21:32:30.852Z"
completedAt: "2026-03-03T21:32:30.828Z"
attachments:
  - title: "refactor(api): extract path params to query params and canonical parent routes (MGG-215)"
    url: "https://github.com/mggarofalo/Receipts/pull/65"
  - title: "fix(frontend): resolve all TypeScript errors and test failures (MGG-220)"
    url: "https://github.com/mggarofalo/Receipts/pull/64"
---

# Clean up REST API surface: extract non-collection path params to query params

## Summary

Several endpoints use path segments for IDs that are **filter criteria**, not the resource's own primary key. REST convention: path params identify the resource (`/api/things/{id}`), query params filter/search (`/api/things?parentId=X`). This issue covers converting those endpoints, updating the OpenAPI spec, regenerating client types, and fixing client-side call sites.

---

## Category 1 — GET "by-foreign-key" lookups (path → query param)

These endpoints filter a collection by a related resource's ID. They should use query parameters instead of dedicated path segments.

| Current | Proposed |
| -- | -- |
| `GET /api/transactions/by-receipt-id/{receiptId}` | `GET /api/transactions?receiptId={id}` |
| `GET /api/receipt-items/by-receipt-id/{receiptId}` | `GET /api/receipt-items?receiptId={id}` |
| `GET /api/adjustments/by-receipt-id/{receiptId}` | `GET /api/adjustments?receiptId={id}` |
| `GET /api/trips/by-receipt-id/{receiptId}` | `GET /api/trips?receiptId={id}` |
| `GET /api/receipts-with-items/by-receipt-id/{receiptId}` | `GET /api/receipts-with-items?receiptId={id}` |
| `GET /api/transaction-accounts/by-transaction-id/{transactionId}` | `GET /api/transaction-accounts?transactionId={id}` |
| `GET /api/transaction-accounts/by-receipt-id/{receiptId}` | `GET /api/transaction-accounts?receiptId={id}` |
| `GET ~/api/categories/{categoryId}/subcategories` | `GET /api/subcategories?categoryId={id}` |

**8 endpoints total.**

## Category 2 — Audit controller lookups (path → query param)

Same pattern — these are filter-by queries, not resource identity:

| Current | Proposed |
| -- | -- |
| `GET /api/audit/entity/{entityType}/{entityId}` | `GET /api/audit?entityType={type}&entityId={id}` |
| `GET /api/audit/user/{userId}` | `GET /api/audit?userId={id}` |
| `GET /api/audit/apikey/{apiKeyId}` | `GET /api/audit?apiKeyId={id}` |
| `GET /api/auth/audit/me` | Keep as-is (no foreign key param) |
| `GET /api/auth/audit/recent` | Keep as-is |
| `GET /api/auth/audit/failed` | Keep as-is |

**3 endpoints to change, 3 to keep.**

## Category 3 — Mutation endpoints: canonical parent in path (Google AIP-124 style)

**Decision**: Use canonical parent (Receipt) in the URL path. For dual-parent resources (Transaction), the secondary parent (Account) moves to the request body. Updates use the entity's own ID in the path. This follows [Google AIP-124](<https://google.aip.dev/124>).

### Transaction (dual parent: Receipt + Account)

Creates nest under receipt; accountId moves to request body. Updates use entity's own ID.

| Current | Proposed |
| -- | -- |
| `POST /api/transactions/{receiptId}/{accountId}` | `POST /api/receipts/{receiptId}/transactions` (accountId in body) |
| `POST /api/transactions/{receiptId}/{accountId}/batch` | `POST /api/receipts/{receiptId}/transactions/batch` (accountId in each body item) |
| `PUT /api/transactions/{receiptId}/{accountId}` | `PUT /api/transactions/{id}` (accountId in body) |
| `PUT /api/transactions/{receiptId}/{accountId}/batch` | `PUT /api/transactions/batch` (accountId in each body item) |

**DTO changes:**

* `CreateTransactionRequest`: add `accountId` field
* `UpdateTransactionRequest`: add `accountId` field
* Controller creates: `[FromRoute] Guid receiptId` only (accountId from body)
* Controller updates: `[FromRoute] Guid id` (entity's own ID), accountId from body

### ReceiptItem (single parent: Receipt)

Creates nest under receipt. Updates use entity's own ID.

| Current | Proposed |
| -- | -- |
| `POST /api/receipt-items/{receiptId}` | `POST /api/receipts/{receiptId}/receipt-items` |
| `POST /api/receipt-items/{receiptId}/batch` | `POST /api/receipts/{receiptId}/receipt-items/batch` |
| `PUT /api/receipt-items/{receiptId}` | `PUT /api/receipt-items/{id}` |
| `PUT /api/receipt-items/{receiptId}/batch` | `PUT /api/receipt-items/batch` |

**DTO changes:** None — receiptId stays as `[FromRoute]` on creates, removed from updates.

### Adjustment (single parent: Receipt)

Creates nest under receipt. Updates use entity's own ID.

| Current | Proposed |
| -- | -- |
| `POST /api/adjustments/{receiptId}` | `POST /api/receipts/{receiptId}/adjustments` |
| `PUT /api/adjustments/{receiptId}` | `PUT /api/adjustments/{id}` |

**DTO changes:** None — receiptId stays as `[FromRoute]` on creates, removed from updates.

**10 endpoints total.**

---

## Checklist

- [ ] **Category 1**: Convert 8 GET endpoints from path segments to `[FromQuery]` params
- [ ] **Category 2**: Convert 3 audit GET endpoints from path segments to `[FromQuery]` params
- [ ] **Category 3 — Transactions**: Move creates under `/api/receipts/{receiptId}/transactions`, add `accountId` to DTOs, update PUT routes to use entity `{id}`
- [ ] **Category 3 — ReceiptItems**: Move creates under `/api/receipts/{receiptId}/receipt-items`, update PUT routes to use entity `{id}`
- [ ] **Category 3 — Adjustments**: Move creates under `/api/receipts/{receiptId}/adjustments`, update PUT routes to use entity `{id}`
- [ ] Update OpenAPI spec (regenerate from running app)
- [ ] Regenerate client TypeScript types (`openapi-typescript`)
- [ ] Update all React hooks that call changed endpoints
- [ ] Update MediatR commands if their signatures change (receiptId/accountId source)
- [ ] Update Mapperly mappers if DTO shapes change
- [ ] Run backend tests and fix any route-dependent assertions
- [ ] Run frontend type checks (`tsc --noEmit`) and fix
- [ ] Smoke test affected pages in browser

## Files to modify

**Backend controllers:**

* `src/Presentation/API/Controllers/Core/TransactionsController.cs`
* `src/Presentation/API/Controllers/Core/ReceiptItemsController.cs`
* `src/Presentation/API/Controllers/Core/AdjustmentsController.cs`
* `src/Presentation/API/Controllers/Core/SubcategoriesController.cs`
* `src/Presentation/API/Controllers/Core/AuditController.cs`
* `src/Presentation/API/Controllers/Aggregates/TripController.cs`
* `src/Presentation/API/Controllers/Aggregates/ReceiptWithItemsController.cs`
* `src/Presentation/API/Controllers/Aggregates/TransactionAccountController.cs`

**Spec + codegen:**

* OpenAPI spec (regenerate at runtime)
* `src/client/src/lib/api-types.ts` (regenerate)
* `CreateTransactionRequest` / `UpdateTransactionRequest` DTOs (add `accountId`)

**Frontend hooks:**

* `src/client/src/hooks/useTrips.ts`
* `src/client/src/hooks/useTransactions.ts`
* `src/client/src/hooks/useReceiptItems.ts`
* `src/client/src/hooks/useAdjustments.ts`
* `src/client/src/hooks/useAggregates.ts`
* `src/client/src/hooks/useAudit.ts`
* `src/client/src/hooks/useSubcategories.ts`

## Design references

* [Google AIP-124: Resource association](<https://google.aip.dev/124>) — canonical parent pattern
* [Stripe API: Charges](<https://docs.stripe.com/api/charges/create>) — flat body-only reference
* [HN discussion on nested resources](<https://news.ycombinator.com/item?id=32506784>) — community consensus on flat vs nested
