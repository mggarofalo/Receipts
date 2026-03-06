---
identifier: MGG-246
title: "Fix `.map is not a function` on /trips and TransactionForm"
id: 2ce4a425-a5e4-45e4-8012-23a0037aa871
status: Done
priority:
  value: 1
  name: Urgent
assignee: Michael Garofalo
createdBy: Michael Garofalo
project: Receipts
team: mggarofalo
labels:
  - frontend
  - Feature
url: "https://linear.app/mggarofalo/issue/MGG-246/fix-map-is-not-a-function-on-trips-and-transactionform"
gitBranchName: mggarofalo/mgg-246-fix-map-is-not-a-function-on-trips-and-transactionform
createdAt: "2026-03-06T10:35:19.293Z"
updatedAt: "2026-03-06T10:37:23.321Z"
completedAt: "2026-03-06T10:37:23.298Z"
attachments:
  - title: Fix .map is not a function on /trips and TransactionForm (MGG-246)
    url: "https://github.com/mggarofalo/Receipts/pull/94"
---

# Fix `.map is not a function` on /trips and TransactionForm

## Bug

`(receipts ?? []).map is not a function` on the /trips page. The API hooks (`useReceipts`, `useAccounts`) return paginated response objects `{ data: T[], total, ... }`, but three locations incorrectly cast the full response object to an array and call `.map()` on it.

## Affected Files

* `src/client/src/pages/Trips.tsx:39` — `receipts` used instead of `receipts?.data`
* `src/client/src/components/TransactionForm.tsx:55` — `receipts` used instead of `receipts?.data`
* `src/client/src/components/TransactionForm.tsx:61` — `accounts` used instead of `accounts?.data`

## Correct Pattern (used elsewhere)

```typescript
const list = (receiptsResponse?.data as ReceiptResponse[] | undefined) ?? [];
```

## Checklist

- [ ] Fix `Trips.tsx` to access `receipts?.data` before mapping
- [ ] Fix `TransactionForm.tsx` receipts to access `receipts?.data` before mapping
- [ ] Fix `TransactionForm.tsx` accounts to access `accounts?.data` before mapping
- [ ] Verify no other instances of this pattern exist
