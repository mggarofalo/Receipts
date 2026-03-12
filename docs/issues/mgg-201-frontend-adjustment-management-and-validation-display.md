---
identifier: MGG-201
title: Frontend — Adjustment management and validation display
id: b8161675-6fd1-45ae-a8f3-ce09a7b173f8
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
  - Feature
milestone: "Phase 7: Correctness Hardening"
url: "https://linear.app/mggarofalo/issue/MGG-201/frontend-adjustment-management-and-validation-display"
gitBranchName: mggarofalo/mgg-201-frontend-adjustment-management-and-validation-display
createdAt: "2026-02-27T14:55:29.765Z"
updatedAt: "2026-03-02T02:20:05.758Z"
completedAt: "2026-03-02T02:20:05.741Z"
attachments:
  - title: "feat(client): adjustment CRUD, validation warnings, balance display (MGG-201)"
    url: "https://github.com/mggarofalo/Receipts/pull/49"
---

# Frontend — Adjustment management and validation display

## Scope\\n\\nAdd Adjustment CRUD UI, display computed balance equation fields, handle 400 validation errors, and display soft warnings in the React frontend.\\n\\n## Tasks\\n\\n- \[ \] Add Adjustment CRUD UI within receipt detail views (add/edit/remove adjustments with type dropdown + amount)\\n- \[ \] Display computed Subtotal, AdjustmentTotal, ExpectedTotal in Trip views with balance equation breakdown\\n- \[ \] Handle 400 validation errors: parse ProblemDetails, display per-field errors in forms\\n- \[ \] Display soft warnings using shadcn Alert (warning variant) in Trip/receipt detail views\\n- \[ \] Regenerated TypeScript types from OpenAPI spec changes in C2/C4\\n\\n## Critical Files\\n\\n- `src/client/src/components/ReceiptForm.tsx`\\n- `src/client/src/pages/Trips.tsx` / receipt detail pages\\n- `src/client/src/api/` (generated client)\\n\\n## Dependencies\\n\\n- **Blocks:** C6 (Tests)\\n- **Blocked by:** C2 (Adjustment entity), C4 (Soft invariant warnings)
