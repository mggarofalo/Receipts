---
identifier: MGG-23
title: Fix build warnings and test failures
id: 48ba5ec3-2fb7-4a08-8316-85b415dd5dd0
status: Done
createdBy: Michael Garofalo
project: Receipts
team: mggarofalo
url: "https://linear.app/mggarofalo/issue/MGG-23/fix-build-warnings-and-test-failures"
gitBranchName: mggarofalo/mgg-23-fix-build-warnings-and-test-failures
createdAt: "2026-01-14T13:11:47.446Z"
updatedAt: "2026-02-11T05:50:20.200Z"
completedAt: "2026-02-11T05:50:20.183Z"
---

# Fix build warnings and test failures

## Summary

Running `dotnet build` and `dotnet test` reveals multiple issues that need to be addressed:

## Issues

### 1\. MediatR 14.0.0 Licensing (1 test failure)

**Error:** `System.InvalidOperationException: No constructor for type 'MediatR.Licensing.LicenseAccessor' can be instantiated using services from the service container`

MediatR 14.0.0 introduced a licensing model that requires configuration. The `ApplicationServiceTests.RegisterApplicationServices_RegistersRequiredServices` test fails because the MediatR licensing infrastructure cannot be instantiated.

**Solution:** Downgrade MediatR to version 12.4.1 (last version without licensing requirements)

### 2\. Entity Framework Version Mismatch (22 build warnings)

**Warning:** `MSB3277: Found conflicts between different versions of "Microsoft.EntityFrameworkCore" that could not be resolved. Version 10.0.0 vs 10.0.1`

`Npgsql.EntityFrameworkCore.PostgreSQL` is at 10.0.0 which depends on EF Core 10.0.0, but other EF packages (`Microsoft.EntityFrameworkCore.InMemory`, `Microsoft.EntityFrameworkCore.Tools`) are at 10.0.1.

**Solution:** Downgrade EF packages to 10.0.0 to match Npgsql

### 3\. Test Data Generators Create Non-Unique Objects (55 test failures)

Multiple equality tests fail because test data generators (e.g., `AccountEntityGenerator.Generate()`) create objects with identical property values (except `Id`), while the `Equals` methods don't include `Id` in comparisons.

**Example:**

* Test: `Equals_DifferentAccountEntity_ReturnsFalse`
* Expected: Two generated accounts should not be equal
* Actual: They ARE equal because `AccountCode`, `Name`, `IsActive` are all identical

**Affected test suites:**

* `Domain.Tests`: 21 failures
* `Infrastructure.Tests`: 9 failures
* `Presentation.Shared.Tests`: 25 failures

**Solution:** Update generators to create objects with unique property values (use counters/unique suffixes)

## Acceptance Criteria

- [ ] Build succeeds with 0 warnings
- [ ] All tests pass
- [ ] No breaking changes to public APIs
