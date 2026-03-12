---
identifier: MGG-94
title: Correctness Hardening
id: 887ab900-03ad-4f81-8b57-61f940e3baaf
status: Done
priority:
  value: 4
  name: Low
assignee: Michael Garofalo
createdBy: Michael Garofalo
project: Receipts
team: mggarofalo
labels:
  - epic
  - frontend
  - backend
milestone: "Phase 7: Correctness Hardening"
url: "https://linear.app/mggarofalo/issue/MGG-94/correctness-hardening"
gitBranchName: mggarofalo/mgg-94-correctness-hardening
createdAt: "2026-02-14T01:09:42.627Z"
updatedAt: "2026-03-02T03:27:01.708Z"
completedAt: "2026-03-02T03:27:01.693Z"
attachments:
  - title: "Phase 7: Correctness Hardening (MGG-94)"
    url: "https://github.com/mggarofalo/Receipts/pull/51"
---

# Correctness Hardening

## Correctness Hardening — Epic

The API currently accepts receipt payloads where the numbers don't add up. Phase 7 enforces cross-entity invariants, adds typed adjustments, and surfaces soft warnings.

### Design Decisions

| Decision | Choice |
| -- | -- |
| Aggregate model | Proper aggregate root — `Trip` gets `Validate()` + computed properties |
| Rounding tolerance | Fixed ±$0.01 per line item |
| Reconciliation | Typed adjustments — new `Adjustment` entity with `AdjustmentType` enum |
| Balance timing | Enforce on mutation (write-time validation) |

### Balance Equation

```
sum(item.TotalAmount) + Receipt.TaxAmount + sum(adjustment.Amount) == sum(transaction.Amount)
```

### Children

| \# | Issue | Title | Priority | Blocked by |
| -- | -- | -- | -- | -- |
| C1 | [MGG-197](./mgg-197-wire-up-fluentvalidation-mediatr-validation-pipeline.md) | Wire up FluentValidation + MediatR validation pipeline | Urgent | — |
| C2 | [MGG-198](./mgg-198-add-adjustment-entity-adjustmenttype-enum-and-evolve-domain-aggregates.md) | Add Adjustment entity, AdjustmentType enum, and evolve domain aggregates | Urgent | — |
| C3 | [MGG-199](./mgg-199-enforce-tier-1-hard-invariants.md) | Enforce Tier 1 hard invariants | High | C1, C2 |
| C4 | [MGG-200](./mgg-200-add-tier-2-soft-invariant-warnings.md) | Add Tier 2 soft invariant warnings | High | C2 |
| C5 | [MGG-201](./mgg-201-frontend-adjustment-management-and-validation-display.md) | Frontend — Adjustment management and validation display | Medium | C2, C4 |
| C6 | [MGG-202](./mgg-202-comprehensive-test-suite-for-correctness-hardening.md) | Comprehensive test suite for correctness hardening | Medium | C3, C4, C5 |
| C7 | [MGG-203](./mgg-203-update-documentation-and-linear-workspace-for-phase-7.md) | Update documentation and Linear workspace for Phase 7 | Low | — |

### Execution Waves

* **Wave 1 (parallel):** C1, C2, C7
* **Wave 2 (after C1+C2):** C3, C4
* **Wave 3 (after C2+C4):** C5
* **Wave 4 (after C3+C4+C5):** C6

### Dependency Graph

```
C1 (Validation pipeline) ──────────┐
                                    ├──> C3 (Hard invariants) ──┐
C2 (Adjustment entity + model) ────┤                            ├──> C6 (Tests)
                                    ├──> C4 (Soft invariants) ──┤
                                    │         │                  │
                                    │         └──> C5 (Frontend) ┘
                                    │              ▲
                                    └──────────────┘

C7 (Docs) — no dependencies, anytime
```

---

*Original design spec preserved below for reference.*

---

## Problem

The API currently accepts receipt payloads where the numbers don't add up. Individual fields are validated (non-empty strings, positive quantities, future dates), but **no cross-entity invariants are enforced**. You can save a receipt where items total $98 but transactions sum to $103, and nobody complains.

### Current State

| Layer | What's validated | What's missing |
| -- | -- | -- |
| **Domain constructors** | Location non-empty, date ≤ today, quantity > 0, transaction amount ≠ 0 | No cross-entity checks |
| **FluentValidation** | Required fields, max lengths, date ≤ today | No aggregate-level rules |
| **ReceiptItemMapper** | TotalAmount is *calculated* server-side (`qty × unitPrice`, floored to 2dp) | Good — client can't send wrong totals |
| **ReceiptWithItems aggregate** | Nothing — pure data bag | No business logic at all |

### Missing Invariants

**1. No receipt-level total field**
`Receipt` has `TaxAmount` but no `Total` or `Subtotal`. There's no anchor to validate items against.

**2. Transaction amounts are uncorrelated to receipt**
`Transaction.Amount` can be anything. No validation that `sum(transactions) == subtotal + tax`.

**3. UnitPrice has no floor**
`ReceiptItem.UnitPrice` can be zero or negative — only `Quantity` is validated as > 0.

**4. TaxAmount has no bounds**
`Receipt.TaxAmount` can be negative, zero, or wildly disproportionate to item totals.

### Proposed Validation Rules

**Tier 1 — Hard invariants (reject if violated)**

* Balance equation: `sum(item.TotalAmount) + TaxAmount + sum(adjustment.Amount) == sum(transaction.Amount)`
* Non-negative prices: `UnitPrice > 0`
* Line-item totals: Within ±$0.01 of `qty × unitPrice`

**Tier 2 — Soft invariants (warn, don't reject)**

* Tax reasonableness: `TaxAmount / Subtotal` within 0–25%
* Adjustment reasonableness: `|AdjustmentTotal| > 10%` of Subtotal
* Date consistency: Transaction dates on or after receipt date
