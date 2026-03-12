---
identifier: MGG-199
title: Enforce Tier 1 hard invariants
id: 77677648-e268-4b23-892a-91c417cb90a7
status: Done
priority:
  value: 2
  name: High
assignee: Michael Garofalo
createdBy: Michael Garofalo
project: Receipts
team: mggarofalo
labels:
  - backend
  - Feature
milestone: "Phase 7: Correctness Hardening"
url: "https://linear.app/mggarofalo/issue/MGG-199/enforce-tier-1-hard-invariants"
gitBranchName: mggarofalo/mgg-199-enforce-tier-1-hard-invariants
createdAt: "2026-02-27T14:55:16.062Z"
updatedAt: "2026-03-02T02:05:07.514Z"
completedAt: "2026-03-02T02:05:07.498Z"
attachments:
  - title: "feat(domain): enforce Tier 1 hard invariants (MGG-199)"
    url: "https://github.com/mggarofalo/Receipts/pull/47"
---

# Enforce Tier 1 hard invariants

## Scope\\n\\nEnforce write-time validation for the core business invariants that must never be violated. Invalid data gets rejected with structured 400 ProblemDetails responses.\\n\\n## Balance Equation\\n\\n`\nsum(item.TotalAmount) + Receipt.TaxAmount + sum(adjustment.Amount) == sum(transaction.Amount)\n`\\n\\n## Tasks\\n\\n- \[ \] **Balance equation**: In `Trip.Validate()` — reject if `ExpectedTotal != sum(transaction.Amount)`\\n- \[ \] **Non-negative UnitPrice**: Add `UnitPrice.Amount > 0` guard to `ReceiptItem` constructor + FluentValidation rule on `CreateReceiptItemRequest` / `UpdateReceiptItemRequest`\\n- \[ \] **Line-item tolerance**: `|TotalAmount - (Quantity * UnitPrice)| <= $0.01` — enforce in `ReceiptItem` constructor\\n- \[ \] **Mutation-time enforcement**: In command handlers for create/update Transaction, load full Trip aggregate, call `Validate()`, reject if invalid\\n- \[ \] Return structured 400 ProblemDetails with per-field errors\\n\\n## Rounding Tolerance\\n\\nFixed ±$0.01 per line item (user-confirmed design decision).\\n\\n## Critical Files\\n\\n- `src/Domain/Core/ReceiptItem.cs` (UnitPrice guard, tolerance check)\\n- `src/Domain/Aggregates/Trip.cs` (Validate method)\\n- Transaction command handlers in `src/Application/`\\n\\n## Dependencies\\n\\n- **Blocks:** C6 (Tests)\\n- **Blocked by:** C1 (Validation pipeline), C2 (Adjustment entity + aggregates)
