---
identifier: MGG-224
title: "Improve client unit test coverage for unhealthy packages (src, src.pages)"
id: 5ba5cd28-f517-4035-99fe-cf9bee336372
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
url: "https://linear.app/mggarofalo/issue/MGG-224/improve-client-unit-test-coverage-for-unhealthy-packages-src-srcpages"
gitBranchName: mggarofalo/mgg-224-improve-client-unit-test-coverage-for-unhealthy-packages-src
createdAt: "2026-03-03T21:40:57.593Z"
updatedAt: "2026-03-04T16:48:17.760Z"
completedAt: "2026-03-04T16:48:17.742Z"
attachments:
  - title: "test(client): improve unit test coverage for src and src/pages packages (MGG-224)"
    url: "https://github.com/mggarofalo/Receipts/pull/70"
---

# Improve client unit test coverage for unhealthy packages (src, src.pages)

## Summary

The client unit test coverage report (from PR #66 CI) flags two packages as unhealthy (below the 80% minimum line rate):

| Package | Line Rate | Branch Rate | Health |
| -- | -- | -- | -- |
| `src` | 0% | 100% | ❌ |
| `src.pages` | 72% | 62% | ❌ |

## Acceptance criteria

- [ ] `src` package line rate meets or exceeds the 80% minimum threshold
- [ ] `src.pages` package line rate meets or exceeds the 80% minimum threshold
- [ ] No existing tests are broken

## Context

* The overall client coverage is 82% (1878/2290 lines), so the project-wide threshold is met, but these two packages individually fall below the 80% minimum
* `src` at 0% likely means root-level source files (e.g., `App.tsx`, `main.tsx`, entry points) have no test coverage
* `src.pages` at 72% line / 62% branch means page-level components need additional test cases
* Identified in PR #66 CI coverage comment: [https://github.com/mggarofalo/Receipts/pull/66#issuecomment-3993705142](<https://github.com/mggarofalo/Receipts/pull/66#issuecomment-3993705142>)
