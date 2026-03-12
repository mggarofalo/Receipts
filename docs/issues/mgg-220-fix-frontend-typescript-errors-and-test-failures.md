---
identifier: MGG-220
title: Fix frontend TypeScript errors and test failures
id: 702685ba-b526-4d1a-a0de-e295665bca28
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
  - frontend
  - Bug
url: "https://linear.app/mggarofalo/issue/MGG-220/fix-frontend-typescript-errors-and-test-failures"
gitBranchName: mggarofalo/mgg-220-fix-frontend-typescript-errors-and-test-failures
createdAt: "2026-03-03T21:17:55.130Z"
updatedAt: "2026-03-03T21:32:26.580Z"
completedAt: "2026-03-03T21:32:26.560Z"
attachments:
  - title: "fix(frontend): resolve all TypeScript errors and test failures (MGG-220)"
    url: "https://github.com/mggarofalo/Receipts/pull/64"
---

# Fix frontend TypeScript errors and test failures

## Problem

Several frontend TypeScript errors and test failures exist that need to be resolved:

* `vite.config.ts` import uses wrong module — should use `vitest/config` so the `test` property is recognized
* `localStorage` polyfill missing from test setup for jsdom 28.x compatibility (causes 36 test failures)
* `ItemTemplateForm.tsx` uses wrong prop name (`onChange` → `onValueChange`) for `Combobox`
* Partial `UseQueryResult` mocks across 15 test files need intermediate casts
* `useSignalR.test.ts` uses manual cast instead of `vi.mocked()`
* Incomplete `FetchResponse` type casts in tests

## Tasks

- [ ] Fix `vite.config.ts` import to use `vitest/config`
- [ ] Add `localStorage` polyfill to test setup for jsdom 28.x compatibility
- [ ] Fix `ItemTemplateForm.tsx` prop name (`onChange` → `onValueChange`)
- [ ] Add `as unknown as` intermediate cast to partial `UseQueryResult` mocks across test files
- [ ] Use `vi.mocked()` instead of manual cast in `useSignalR.test.ts`
- [ ] Fix incomplete `FetchResponse` type casts
