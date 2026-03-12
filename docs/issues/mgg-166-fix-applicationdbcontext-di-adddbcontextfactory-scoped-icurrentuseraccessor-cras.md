---
identifier: MGG-166
title: "Fix ApplicationDbContext DI: AddDbContextFactory + scoped ICurrentUserAccessor crash at startup"
id: 8c698022-8cbd-4016-b89e-f91b5c2010a3
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
milestone: "Phase 4 [Backend]"
url: "https://linear.app/mggarofalo/issue/MGG-166/fix-applicationdbcontext-di-adddbcontextfactory-scoped"
gitBranchName: mggarofalo/mgg-166-fix-applicationdbcontext-di-adddbcontextfactory-scoped
createdAt: "2026-02-24T02:17:05.445Z"
updatedAt: "2026-02-24T10:36:16.661Z"
completedAt: "2026-02-24T03:01:53.752Z"
---

# Fix ApplicationDbContext DI: AddDbContextFactory + scoped ICurrentUserAccessor crash at startup

## Problem

`AddDbContextFactory<ApplicationDbContext>` registers a singleton factory that creates `ApplicationDbContext` using `ActivatorUtilities.CreateInstance` from the **root** `IServiceProvider`. The `[ActivatorUtilitiesConstructor]` attribute on the 2-param constructor forces DI to resolve `ICurrentUserAccessor`, which is registered as **scoped** — causing `InvalidOperationException: Cannot resolve scoped service from root provider` at startup.

### Two crash sites at startup:

1. `Program.cs` migration — `GetRequiredService<ApplicationDbContext>()` from a scope still delegates to the factory, which uses root provider
2. `SeedRolesAndAdminAsync` — `UserManager`/`RoleManager` depend on `ApplicationDbContext` through Identity EF stores, same factory resolution path

### Additional issue

EF Core 10 raises `PendingModelChangesWarning` as an exception during `MigrateAsync()` when the model has pending changes (pre-existing on milestone branch).

## Root Cause

* `AddDbContextFactory` creates a **singleton** factory
* The factory's lambda uses `ActivatorUtilities.CreateInstance` with the **root** `IServiceProvider`
* `[ActivatorUtilitiesConstructor]` on `ApplicationDbContext(options, ICurrentUserAccessor)` forces DI to always pick this constructor
* `ICurrentUserAccessor` is registered as scoped → root provider can't resolve it

## Files Involved

* `src/Infrastructure/ApplicationDbContext.cs` — dual constructors + `[ActivatorUtilitiesConstructor]`
* `src/Infrastructure/Services/InfrastructureService.cs` — `AddDbContextFactory` registration
* `src/Presentation/API/Program.cs` — startup migration + seed
* `src/Presentation/API/Configuration/AuthConfiguration.cs` — `SeedRolesAndAdminAsync`
* `src/Infrastructure/Services/NullCurrentUserAccessor.cs` — existing fallback (scoped, not available at root)
* `src/Receipts.AppHost/AppHost.cs` — also has a separate bug: duplicate endpoint name (`http`) from `AddViteApp` + `WithHttpEndpoint` (needs `name: "vite"` parameter)

## Design Considerations

* All repositories and services use `IDbContextFactory<ApplicationDbContext>` — switching to `AddDbContext` would break them all
* Adding a scoped `AddDbContext` alongside the factory could cause shared context issues
* Removing `[ActivatorUtilitiesConstructor]` might work if `ActivatorUtilities` falls back to the 1-param constructor, but needs verification
* Alternatively: make the factory lambda manually construct `ApplicationDbContext` with the options-only constructor and avoid `ActivatorUtilities`
* The `PendingModelChangesWarning` during migration should be suppressed in the startup migration code path

## Acceptance Criteria

- [ ] API starts without `ICurrentUserAccessor` scoped service crash
- [ ] Database migrations run at startup
- [ ] Role/admin seeding runs at startup
- [ ] `ICurrentUserAccessor` still works correctly in request-scoped contexts (audit logging, soft delete)
- [ ] No shared context or lifecycle issues introduced
- [ ] AppHost `WithHttpEndpoint` uses explicit endpoint name to avoid collision
