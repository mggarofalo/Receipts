---
identifier: MGG-200
title: Add Tier 2 soft invariant warnings
id: 09fbaede-8628-451f-bb57-5459406f42c4
status: Done
priority:
  value: 2
  name: High
assignee: Michael Garofalo
createdBy: Michael Garofalo
project: Receipts
team: mggarofalo
labels:
  - codegen
  - backend
  - Improvement
milestone: "Phase 7: Correctness Hardening"
url: "https://linear.app/mggarofalo/issue/MGG-200/add-tier-2-soft-invariant-warnings"
gitBranchName: mggarofalo/mgg-200-add-tier-2-soft-invariant-warnings
createdAt: "2026-02-27T14:55:22.552Z"
updatedAt: "2026-03-02T02:07:40.385Z"
completedAt: "2026-03-02T02:07:40.361Z"
attachments:
  - title: "feat(domain): add Tier 2 soft invariant warnings (MGG-200)"
    url: "https://github.com/mggarofalo/Receipts/pull/48"
---

# Add Tier 2 soft invariant warnings

## Scope\\n\\nAdd soft validation warnings that flag suspicious but not necessarily invalid data. Warnings are returned in API responses but do not block saves.\\n\\n## Tasks\\n\\n- \[ \] Define `ValidationWarning` record (property, message, severity) in Application layer\\n- \[ \] **Tax reasonableness**: Flag if `TaxAmount / Subtotal` outside 0–25% (when Subtotal > 0)\\n- \[ \] **Adjustment reasonableness**: Flag if `|AdjustmentTotal| > 10%` of Subtotal\\n- \[ \] **Date consistency**: Flag if any `Transaction.Date < Receipt.Date`\\n- \[ \] Return warnings in Trip/ReceiptWithItems response bodies (add `warnings` array to OpenAPI spec)\\n- \[ \] Implement as a method on the aggregate (alongside `Validate()`)\\n- \[ \] Regenerate DTOs\\n\\n## Critical Files\\n\\n- `src/Domain/Aggregates/Trip.cs` (warning method)\\n- `openapi/spec.yaml` (warnings array)\\n\\n## Dependencies\\n\\n- **Blocks:** C5 (Frontend), C6 (Tests)\\n- **Blocked by:** C2 (Adjustment entity + aggregates)
