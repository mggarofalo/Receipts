---
identifier: MGG-187
title: "Accessibility follow-up: public route landmarks, page titles, heading semantics"
id: e153e90b-c1be-4e83-9021-a6cd16ad5405
status: Done
priority:
  value: 3
  name: Medium
createdBy: Michael Garofalo
project: Receipts
team: mggarofalo
labels:
  - frontend
  - Improvement
milestone: "Phase 4 [Frontend]"
url: "https://linear.app/mggarofalo/issue/MGG-187/accessibility-follow-up-public-route-landmarks-page-titles-heading"
gitBranchName: mggarofalo/mgg-187-accessibility-follow-up-public-route-landmarks-page-titles
createdAt: "2026-02-25T12:59:15.252Z"
updatedAt: "2026-02-25T13:19:50.239Z"
completedAt: "2026-02-25T13:19:50.216Z"
---

# Accessibility follow-up: public route landmarks, page titles, heading semantics

## Context

Follow-up from MGG-46 and MGG-47 code review. These gaps were discovered during QA of PR #27.

## Tasks

- [ ] Wrap public routes (Login, Register) in a `<main>` landmark — add `<main>` to public-route layouts
- [ ] Fix page title — `document.title` shows "client" instead of "Receipts" at runtime. Add per-route titles (e.g., "Sign In - Receipts") using `react-helmet-async` or `useEffect` in each page
- [ ] Ensure `<h1>` heading semantics — verify CardTitle renders a semantic heading element (`<h2>` or configurable), or confirm nested `<h1>` is properly exposed
- [ ] Add a minimal header/nav to public pages — even Login benefits from a banner landmark with app logo/name

## Acceptance Criteria

* Public routes (Login, Register) wrapped in `<main>` landmark
* Each route sets a meaningful `<title>` via `document.title`
* `<h1>` heading on every page is semantic (not just styled `<div>`)
* Public pages have at least a minimal `<header>` with app branding
