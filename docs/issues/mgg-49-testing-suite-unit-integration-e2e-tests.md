---
identifier: MGG-49
title: "Testing Suite: Unit, Integration & E2E Tests"
id: 8263f709-1226-4db2-a1f8-246cd0d0c531
status: Done
priority:
  value: 3
  name: Medium
createdBy: Michael Garofalo
project: Receipts
team: mggarofalo
labels:
  - testing
  - frontend
milestone: "Phase 4 [Frontend]"
url: "https://linear.app/mggarofalo/issue/MGG-49/testing-suite-unit-integration-and-e2e-tests"
gitBranchName: mggarofalo/mgg-49-testing-suite-unit-integration-e2e-tests
createdAt: "2026-02-11T05:07:41.099Z"
updatedAt: "2026-02-26T13:06:25.956Z"
completedAt: "2026-02-26T13:06:12.104Z"
---

# Testing Suite: Unit, Integration & E2E Tests

## Objective

Establish E2E testing coverage and test data factories for the React frontend.

**Scope note:** Unit and integration tests (Vitest) are deferred to Phase 8 (MGG-126 + MGG-127). This issue focuses exclusively on Playwright E2E tests, MSW mock handlers, and reusable test data factories.

## Completed

- [X] Setup Playwright for E2E tests (`playwright.config.ts`, chromium-only)
- [X] Create MSW mock handlers (`e2e/mocks/handlers.ts`) for all API endpoints
- [X] Create Playwright test fixtures (`e2e/fixtures.ts`) with `mockApi`, `loginUser`, `loginAdmin`, `loginMustReset`
- [X] Create test data factories (`src/test/factories.ts`) for all API response types
- [X] Write E2E tests:
  - `auth.spec.ts` — Login validation, successful login, logout, register, must-reset-password, route protection (13 tests)
  - `accounts-crud.spec.ts` — List, create, edit, delete, search, select-all (8 tests)
  - `receipts-crud.spec.ts` — List, create, edit, delete, search (6 tests)
  - `keyboard-nav.spec.ts` — j/k navigation, Enter to open, Ctrl+K palette, ? help, Space toggle (6 tests)
  - `admin.spec.ts` — Admin access to users/audit/trash, non-admin redirect, unauthenticated redirect (7 tests)
  - `search-filter.spec.ts` — Ctrl+K search dialog, filtering, result count, no results (8 tests)
- [X] Add `frontend-e2e` CI job to GitHub Actions (Playwright with artifact upload)
- [X] Add `frontend-lint` CI job to GitHub Actions (ESLint + Prettier + TypeScript)

## Total: 48 E2E tests across 6 spec files

## Acceptance Criteria

* ~~> 70% code coverage~~ → Deferred to MGG-126
* All critical user journeys tested via E2E
* E2E tests pass consistently (48/48)
* Tests run in CI/CD (frontend-e2e job)
* Test data factories reusable for Phase 8 Vitest work
