---
identifier: MGG-213
title: Add TypeScript and ESLint checks to pre-commit hooks for React client
id: ae0bdac7-879a-4b3b-948e-fd0277c52d46
status: Done
priority:
  value: 3
  name: Medium
assignee: Michael Garofalo
createdBy: Michael Garofalo
project: Receipts
team: mggarofalo
labels:
  - dx
  - frontend
url: "https://linear.app/mggarofalo/issue/MGG-213/add-typescript-and-eslint-checks-to-pre-commit-hooks-for-react-client"
gitBranchName: mggarofalo/mgg-213-add-typescript-and-eslint-checks-to-pre-commit-hooks-for
createdAt: "2026-03-02T03:39:26.586Z"
updatedAt: "2026-03-03T12:12:39.683Z"
completedAt: "2026-03-03T12:12:39.665Z"
attachments:
  - title: "chore(dx): add TypeScript and ESLint pre-commit checks for React client (MGG-213)"
    url: "https://github.com/mggarofalo/Receipts/pull/58"
---

# Add TypeScript and ESLint checks to pre-commit hooks for React client

## Problem\\n\\nThe pre-commit hooks only cover .NET backend checks (spectral lint, dotnet format, dotnet build, dotnet test). There are no TypeScript or ESLint checks for the React client (`src/client/`). This allowed undefined function references (`setEditReceiptId`, `setEditAccountId`) to be committed in `ReceiptItems.tsx` and `Transactions.tsx` without being caught.\\n\\n## Tasks\\n\\n- \[ \] Add `npx tsc --noEmit` (scoped to `src/client/`) to the pre-commit hook pipeline\\n- \[ \] Add `npx eslint .` (scoped to `src/client/`) to the pre-commit hook pipeline\\n- \[ \] Verify the hooks run correctly and catch TypeScript errors\\n\\n## Context\\n\\nDiscovered during [MGG-211](./mgg-211-review-and-decompose-large-tsx-page-components.md) (component decomposition) — undefined references were silently committed because no client-side type checking exists in the commit pipeline.
