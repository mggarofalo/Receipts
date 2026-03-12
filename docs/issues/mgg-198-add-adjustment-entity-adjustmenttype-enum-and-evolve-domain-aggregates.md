---
identifier: MGG-198
title: "Add Adjustment entity, AdjustmentType enum, and evolve domain aggregates"
id: 6b31b8f2-360b-4584-8cf1-97d733691eef
status: Done
priority:
  value: 1
  name: Urgent
assignee: Michael Garofalo
createdBy: Michael Garofalo
project: Receipts
team: mggarofalo
labels:
  - codegen
  - backend
  - Feature
milestone: "Phase 7: Correctness Hardening"
url: "https://linear.app/mggarofalo/issue/MGG-198/add-adjustment-entity-adjustmenttype-enum-and-evolve-domain-aggregates"
gitBranchName: mggarofalo/mgg-198-add-adjustment-entity-adjustmenttype-enum-and-evolve-domain
createdAt: "2026-02-27T14:55:06.392Z"
updatedAt: "2026-03-02T01:23:51.717Z"
completedAt: "2026-03-02T01:23:51.699Z"
attachments:
  - title: "feat(domain,infrastructure,api): add Adjustment entity with full CRUD stack (MGG-198)"
    url: "https://github.com/mggarofalo/Receipts/pull/44"
---

# Add Adjustment entity, AdjustmentType enum, and evolve domain aggregates

## Scope\\n\\nIntroduce typed adjustments (tip, discount, rounding, etc.) as a first-class entity, evolve the domain aggregates to support computed totals and a balance equation, and wire everything through the full stack (domain → infrastructure → API).\\n\\n## Design Decisions\\n\\n- **Aggregate model:** Proper aggregate root — `Trip` gets `Validate()` + computed properties\\n- **Reconciliation approach:** Typed adjustments (not a single reconciliation field)\\n- **Balance equation:** `sum(item.TotalAmount) + Receipt.TaxAmount + sum(adjustment.Amount) == sum(transaction.Amount)`\\n- Adjustments are signed: positive = paid more (tip, rounding up), negative = paid less (coupon, loyalty)\\n\\n## Tasks\\n\\n### Domain Layer\\n- \[ \] Create `AdjustmentType` enum in `src/Common/GlobalEnums.cs`: `Tip`, `Discount`, `Rounding`, `LoyaltyRedemption`, `Coupon`, `StoreCredit`, `Other`\\n- \[ \] Create `Adjustment` entity in `src/Domain/Core/Adjustment.cs`: Id, ReceiptId, Type (AdjustmentType), Amount (Money), Description?\\n- \[ \] Constructor invariants: Amount != 0, Description required when Type == Other\\n\\n### Aggregate Evolution\\n- \[ \] `ReceiptWithItems` → add computed `Subtotal` property: `Items.Sum(i => i.TotalAmount)` using Money arithmetic\\n- \[ \] Create new aggregate `ReceiptAggregate` (or evolve `Trip`): Receipt + Items + Adjustments + Transactions\\n- \[ \] Add computed `AdjustmentTotal`: `Adjustments.Sum(a => a.Amount)`\\n- \[ \] Add computed `ExpectedTotal`: `Subtotal + TaxAmount + AdjustmentTotal`\\n- \[ \] Add `Validate()` method returning validation result (used by C3)\\n\\n### Infrastructure Layer\\n- \[ \] Create `AdjustmentEntity` in `src/Infrastructure/Entities/Core/`\\n- \[ \] Add EF configuration + DbSet\\n- \[ \] Create migration (new `Adjustments` table)\\n- \[ \] Create Mapperly mappers (API + Infrastructure layers)\\n\\n### API Layer\\n- \[ \] Update `openapi/spec.yaml`: add Adjustment schemas (`CreateAdjustmentRequest`, `UpdateAdjustmentRequest`, `AdjustmentResponse`) + CRUD endpoints under `/api/Receipt/{receiptId}/Adjustment`\\n- \[ \] Add computed `subtotal`, `adjustmentTotal`, `expectedTotal` to `ReceiptWithItemsResponse` and `TripResponse`\\n- \[ \] Regenerate DTOs\\n- \[ \] Create `AdjustmentsController`\\n- \[ \] Add repository interface + implementation for Adjustments\\n\\n## Critical Files\\n\\n- `src/Domain/Core/Receipt.cs`, `src/Domain/Aggregates/Trip.cs`\\n- `openapi/spec.yaml`\\n- `src/Infrastructure/Entities/Core/` (new entity)\\n- `src/Common/GlobalEnums.cs`\\n\\n## Dependencies\\n\\n- **Blocks:** C3 (Hard invariants), C4 (Soft invariants), C5 (Frontend)\\n- **Blocked by:** Nothing — can start immediately
