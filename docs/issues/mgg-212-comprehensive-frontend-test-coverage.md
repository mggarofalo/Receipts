---
identifier: MGG-212
title: Comprehensive frontend test coverage
id: 1fc3d0fb-6610-476f-8e07-ea3aa3130286
status: Done
priority:
  value: 4
  name: Low
assignee: Michael Garofalo
createdBy: Michael Garofalo
project: Receipts
team: mggarofalo
labels:
  - testing
  - frontend
url: "https://linear.app/mggarofalo/issue/MGG-212/comprehensive-frontend-test-coverage"
gitBranchName: mggarofalo/mgg-212-comprehensive-frontend-test-coverage
createdAt: "2026-03-02T03:30:07.293Z"
updatedAt: "2026-03-03T14:01:46.887Z"
completedAt: "2026-03-03T14:01:46.860Z"
attachments:
  - title: "test(frontend): comprehensive test coverage for React client (MGG-212)"
    url: "https://github.com/mggarofalo/Receipts/pull/60"
---

# Comprehensive frontend test coverage

## Problem

Frontend CI reports 83/83 lines and 47/47 branches — 100% of what's measured, but only **10 out of \~128 source files** have tests (7.8% coverage). Vitest only instruments files imported by tests, so the vast majority of the codebase is invisible to coverage.

## Scope

Add tests for untested source files across all categories:

* **Pages**: 21 files, 0 tested
* **Feature components**: \~28 files, 1 tested (ValidationWarnings)
* **UI components**: \~27 files, 0 tested
* **Hooks**: 26 files, 3 tested (useAuth, usePagination, usePermission)
* **Lib utilities**: 16 files, 2 tested (problem-details, auth)
* **Contexts**: 4 files, 0 tested

## Additional work

* Configure Vitest `coverage.all: true` so untested files appear in reports
* Set a minimum coverage threshold in CI
