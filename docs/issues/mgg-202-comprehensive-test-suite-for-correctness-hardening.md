---
identifier: MGG-202
title: Comprehensive test suite for correctness hardening
id: 0184e90c-78b7-41be-9742-2ba48def67f7
status: Done
priority:
  value: 3
  name: Medium
assignee: Michael Garofalo
createdBy: Michael Garofalo
project: Receipts
team: mggarofalo
labels:
  - testing
  - backend
milestone: "Phase 7: Correctness Hardening"
url: "https://linear.app/mggarofalo/issue/MGG-202/comprehensive-test-suite-for-correctness-hardening"
gitBranchName: mggarofalo/mgg-202-comprehensive-test-suite-for-correctness-hardening
createdAt: "2026-02-27T14:55:37.482Z"
updatedAt: "2026-03-02T03:24:25.553Z"
completedAt: "2026-03-02T03:07:17.853Z"
attachments:
  - title: "test: close coverage gaps for Phase 7 (MGG-202)"
    url: "https://github.com/mggarofalo/Receipts/pull/52"
  - title: "test: comprehensive test suite for correctness hardening (MGG-202)"
    url: "https://github.com/mggarofalo/Receipts/pull/50"
---

# Comprehensive test suite for correctness hardening

## Scope\\n\\nComprehensive test coverage for all correctness hardening features across all layers.\\n\\n## Tasks\\n\\n### Domain Tests\\n- \[ \] Adjustment constructor (valid, Amount == 0 rejected, Description required for Type == Other)\\n- \[ \] ReceiptItem UnitPrice > 0 guard\\n- \[ \] Line-item tolerance (within ±$0.01, outside tolerance rejected)\\n- \[ \] Computed Subtotal, AdjustmentTotal, ExpectedTotal\\n- \[ \] Trip.Validate() with valid data, unbalanced data, edge cases\\n\\n### Application Tests\\n- \[ \] ValidationBehavior pipeline behavior (valid request passes through, invalid request throws)\\n- \[ \] Balance equation edge cases (exact match, within tolerance, outside tolerance)\\n\\n### API Tests\\n- \[ \] 400 ProblemDetails responses for validation failures\\n- \[ \] Adjustment CRUD endpoints\\n- \[ \] Warning responses included in Trip/receipt responses\\n\\n### Mapper Tests\\n- \[ \] Adjustment round-trips through API + Infrastructure mappers\\n\\n### Frontend Tests\\n- \[ \] Adjustment form interactions\\n- \[ \] Validation error display\\n- \[ \] Warning rendering\\n\\n## Dependencies\\n\\n- **Blocks:** Nothing\\n- **Blocked by:** C3 (Hard invariants), C4 (Soft invariants), C5 (Frontend)
