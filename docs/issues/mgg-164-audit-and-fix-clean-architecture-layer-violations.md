---
identifier: MGG-164
title: Audit and fix Clean Architecture layer violations
id: 0ceeca25-a344-4b33-92ef-dd13621ddc19
status: Done
priority:
  value: 3
  name: Medium
createdBy: Michael Garofalo
project: Receipts
team: mggarofalo
labels:
  - cleanup
  - backend
milestone: "Phase 7: Correctness Hardening"
url: "https://linear.app/mggarofalo/issue/MGG-164/audit-and-fix-clean-architecture-layer-violations"
gitBranchName: mggarofalo/mgg-164-audit-and-fix-clean-architecture-layer-violations
createdAt: "2026-02-22T10:44:26.365Z"
updatedAt: "2026-02-27T15:09:24.771Z"
completedAt: "2026-02-27T15:09:24.750Z"
---

# Audit and fix Clean Architecture layer violations

## Problem

Controllers in the Presentation layer are directly injecting `ApplicationDbContext` and running raw EF Core queries, bypassing the repository/service layer. This violates Clean Architecture boundaries — Presentation should only access data through Application-layer interfaces implemented in Infrastructure.

### Known violation

* `UsersController.cs` injects `ApplicationDbContext` directly and runs a `UserRoles`+`Roles` join query (introduced in PR #22, MGG-160 fix)

### Scope

1. **Audit all controllers** for direct `DbContext` usage or other layer-skipping patterns
2. **Log each violation** with file, line, and what layer boundary it crosses
3. **Fix each violation** by moving data access into the appropriate repository/service in Infrastructure, exposing it via an Application-layer interface, and having the controller call through that interface
4. **Preserve** `UserManager<T>` usage — this is an Identity abstraction and is acceptable in controllers

## Acceptance Criteria

- [ ] All controllers audited for `ApplicationDbContext`, `DbContext`, or direct EF Core usage
- [ ] Each violation documented (file, line, what it does)
- [ ] Data access moved to Infrastructure repository/service behind an interface
- [ ] Controllers only depend on Application-layer interfaces and [ASP.NET](<http://ASP.NET>) Identity abstractions (`UserManager<T>`, `SignInManager<T>`)
- [ ] No regression — existing functionality and tests still pass
- [ ] Build succeeds with no new warnings
