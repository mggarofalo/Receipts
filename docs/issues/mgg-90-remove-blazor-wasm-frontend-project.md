---
identifier: MGG-90
title: Remove Blazor WASM Frontend Project
id: 301a13ae-12c3-4833-923d-664964e6722f
status: Done
priority:
  value: 2
  name: High
createdBy: Michael Garofalo
project: Receipts
team: mggarofalo
labels:
  - cleanup
  - backend
milestone: "Phase 0: Housekeeping"
url: "https://linear.app/mggarofalo/issue/MGG-90/remove-blazor-wasm-frontend-project"
gitBranchName: mggarofalo/mgg-90-remove-blazor-wasm-frontend-project
createdAt: "2026-02-12T11:38:11.253Z"
updatedAt: "2026-02-16T11:04:36.986Z"
completedAt: "2026-02-12T12:23:10.388Z"
---

# Remove Blazor WASM Frontend Project

## Summary

Remove the existing Blazor WASM frontend (`src/Presentation/Client`) from the solution. The Blazor frontend is being replaced by a React/Vite SPA (MGG-32) and should be removed as a prerequisite to reduce solution complexity and build times.

## Scope

* Remove `src/Presentation/Client/` project directory
* Remove `src/Presentation/Client.Tests/` if it exists
* Remove project references from `Receipts.sln`
* Remove any Blazor-specific NuGet packages from `Directory.Packages.props` (MudBlazor, etc.)
* Remove Blazor WASM hosting middleware from the API if present
* Clean up any shared project references that were only used by the Blazor client
* Update CI/CD workflows if they reference the Client project

## Why Separate From the React Rewrite

This is pure removal — no new code, no new dependencies. It can and should be done independently before the React frontend epic begins, so that:

* The solution builds faster
* There's no dead code confusing contributors or agents
* The React project starts from a clean slate

## Acceptance Criteria

- [ ] Blazor Client project fully removed from solution
- [ ] Solution builds and all tests pass
- [ ] No orphaned Blazor-specific packages remain
- [ ] CI pipeline passes
