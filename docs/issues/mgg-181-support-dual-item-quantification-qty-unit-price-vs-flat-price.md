---
identifier: MGG-181
title: "Support dual item quantification: qty × unit price vs flat price"
id: 8912f63f-d36b-4697-9da2-55ac94ff23c0
status: Done
priority:
  value: 3
  name: Medium
assignee: Michael Garofalo
createdBy: Michael Garofalo
project: Receipts
team: mggarofalo
labels:
  - frontend
  - backend
  - Feature
milestone: "Phase 4 [Frontend]"
url: "https://linear.app/mggarofalo/issue/MGG-181/support-dual-item-quantification-qty-unit-price-vs-flat-price"
gitBranchName: mggarofalo/mgg-181-support-dual-item-quantification-qty-×-unit-price-vs-flat
createdAt: "2026-02-25T10:45:31.027Z"
updatedAt: "2026-02-26T13:30:40.132Z"
completedAt: "2026-02-26T13:15:03.394Z"
attachments:
  - title: "feat: add pricing mode to receipt items (MGG-181)"
    url: "https://github.com/mggarofalo/Receipts/pull/38"
---

# Support dual item quantification: qty × unit price vs flat price

Receipt items should support two mutually exclusive ways of specifying the amount:

1. **Quantity × Unit Price** — e.g., 3 × $4.99 = $14.97
2. **Flat Item Price** — e.g., $14.97 entered directly

## Requirements

* Toggle or auto-detection between the two modes per line item
* When using qty × unit price: quantity and unit price fields visible, total computed automatically
* When using flat price: single price field, quantity defaults to 1
* Backend must store whichever mode was used (or normalize consistently)
* Validation: prevent conflicting data (e.g., qty=3, unit=$5, but total=$20)
