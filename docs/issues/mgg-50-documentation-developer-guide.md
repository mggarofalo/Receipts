---
identifier: MGG-50
title: "Documentation & Developer Guide"
id: c1369eb8-0917-4682-be9e-1c9647937988
status: Done
priority:
  value: 3
  name: Medium
createdBy: Michael Garofalo
project: Receipts
team: mggarofalo
labels:
  - docs
  - frontend
milestone: "Phase 4 [Frontend]"
url: "https://linear.app/mggarofalo/issue/MGG-50/documentation-and-developer-guide"
gitBranchName: mggarofalo/mgg-50-documentation-developer-guide
createdAt: "2026-02-11T05:07:45.335Z"
updatedAt: "2026-02-26T13:06:19.624Z"
completedAt: "2026-02-26T13:06:19.591Z"
---

# Documentation & Developer Guide

## Objective

Create comprehensive documentation for the React frontend.

## Completed

- [X] Write README with:
  - Project overview
  - Tech stack table
  - Setup instructions (standalone + Aspire)
  - Environment variables
  - Available npm scripts
  - Annotated project structure
  - Testing guide (E2E + factories)
- [X] Document component API (`docs/components.md` — form patterns, auth guards, app shell, search/filter, shared UI)
- [X] Create Architecture Decision Records (`docs/adr/`):
  - 001: openapi-fetch with generated types
  - 002: TanStack Query for server state
  - 003: React Hook Form + Zod v4
  - 004: shadcn/ui as vendored component library
  - 005: Client-side JWT auth with automatic token refresh
  - 006: SignalR for real-time cache invalidation
- [X] Document API client usage (`docs/api-client.md` — architecture, auth middleware, TanStack Query patterns, query keys, error handling)
- [X] Write keyboard shortcuts guide (`docs/keyboard-shortcuts.md` — complete reference table)
- [X] Create contributing guide (`CONTRIBUTING.md` — dev setup, code style, adding CRUD modules, testing, PR process)
- [X] Document environment variables (in README)
- [X] Add inline JSDoc comments for complex logic:
  - `src/lib/api-client.ts` — auth middleware, token refresh deduplication
  - `src/hooks/useSignalR.ts` — connection lifecycle
  - `src/hooks/useListKeyboardNav.ts` — keyboard nav state machine
  - `src/lib/search.ts` — `applyFilters` multi-type filtering
- [ ] ~~Create Storybook for UI components~~ (optional, not implemented)

## Acceptance Criteria

* New developer can setup project from README alone ✅
* All major features documented ✅
* Component usage clear ✅
* API documented ✅
