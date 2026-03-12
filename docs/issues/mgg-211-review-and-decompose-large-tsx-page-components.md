---
identifier: MGG-211
title: Review and decompose large TSX page components
id: 6934c989-568b-4d23-855c-9b867c00417f
status: Done
priority:
  value: 4
  name: Low
assignee: Michael Garofalo
createdBy: Michael Garofalo
project: Receipts
team: mggarofalo
labels:
  - cleanup
  - frontend
url: "https://linear.app/mggarofalo/issue/MGG-211/review-and-decompose-large-tsx-page-components"
gitBranchName: mggarofalo/mgg-211-review-and-decompose-large-tsx-page-components
createdAt: "2026-03-02T02:20:39.694Z"
updatedAt: "2026-03-02T03:53:11.237Z"
completedAt: "2026-03-02T03:53:11.222Z"
attachments:
  - title: "refactor(client): decompose large page components (MGG-211)"
    url: "https://github.com/mggarofalo/Receipts/pull/53"
---

# Review and decompose large TSX page components

## Scope\\n\\nAudit all page-level TSX components for size and complexity. Extract reusable sub-components from pages that exceed \~150 lines or contain multiple logical sections.\\n\\n## Motivation\\n\\nSeveral page components (e.g., `Accounts.tsx`, `ReceiptItems.tsx`, `Transactions.tsx`) contain inline CRUD dialogs, table rendering, search/filter logic, and pagination all in one file. Extracting these into focused sub-components improves readability, testability, and reuse.\\n\\n## Tasks\\n\\n- \[ \] Audit all files in `src/client/src/pages/` for line count and complexity\\n- \[ \] Identify pages with >150 lines or >3 logical sections\\n- \[ \] Extract inline table sections into reusable Card components (following `ReceiptItemsCard` / `AdjustmentsCard` pattern)\\n- \[ \] Extract CRUD dialog groups into self-contained components that own their mutation hooks\\n- \[ \] Ensure no regressions — all existing tests pass, TypeScript and ESLint clean\\n\\n## Reference\\n\\nThe `ReceiptDetail.tsx` decomposition in [MGG-201](./mgg-201-frontend-adjustment-management-and-validation-display.md) established the pattern:\\n- `BalanceSummaryCard` — pure presentational summary\\n- `ReceiptItemsCard` — table with keyboard nav\\n- `AdjustmentsCard` — table + CRUD dialogs + mutations\\n- `ValidationWarnings` — alert display"
