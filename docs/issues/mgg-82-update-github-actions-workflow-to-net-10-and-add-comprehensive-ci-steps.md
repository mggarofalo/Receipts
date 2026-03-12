---
identifier: MGG-82
title: Update GitHub Actions workflow to .NET 10 and add comprehensive CI steps
id: a67b7516-da73-458e-a707-e0e8a52eedf8
status: Done
priority:
  value: 1
  name: Urgent
createdBy: Michael Garofalo
project: Receipts
team: mggarofalo
labels:
  - infra
milestone: "Phase 0: Housekeeping"
url: "https://linear.app/mggarofalo/issue/MGG-82/update-github-actions-workflow-to-net-10-and-add-comprehensive-ci"
gitBranchName: mggarofalo/mgg-82-update-github-actions-workflow-to-net-10-and-add
createdAt: "2026-02-11T11:21:47.355Z"
updatedAt: "2026-02-12T12:19:47.131Z"
completedAt: "2026-02-12T12:19:47.113Z"
---

# Update GitHub Actions workflow to .NET 10 and add comprehensive CI steps

## Problem

The current GitHub Actions workflow (`.github/workflows/github-ci.yml`) is very basic and uses .NET 8 instead of .NET 10. It only includes restore, build, and test steps with no additional quality checks or security scanning.

## Current State

```yaml
- Setup .NET 8.0.x
- Restore dependencies
- Build
- Test
```

## Proposed Improvements

### 1\. Update to .NET 10

* Change `dotnet-version: 8.0.x` to `dotnet-version: 10.0.x`

### 2\. Add Additional CI Steps

**Code Quality:**

- [ ] Add code formatting check: `dotnet format --verify-no-changes`
- [ ] Add code analysis/linting warnings check
- [ ] Consider adding StyleCop or similar static analysis

**Security:**

- [ ] Add dependency vulnerability scanning: `dotnet list package --vulnerable`
- [ ] Consider adding CodeQL analysis (GitHub Security)
- [ ] Check for outdated packages: `dotnet list package --outdated`

**Build Improvements:**

- [ ] Add build warnings-as-errors flag: `dotnet build --no-restore /warnaserror`
- [ ] Add release build configuration test: `dotnet build -c Release`
- [ ] Cache NuGet packages between runs for faster builds

**Testing Enhancements:**

- [ ] Add test coverage reporting (e.g., Coverlet)
- [ ] Publish test results as artifacts
- [ ] Add integration tests if applicable

**Documentation:**

- [ ] Add workflow status badge to [README.md](<http://README.md>)

### 3\. Consider Multi-Job Workflow

Split into separate jobs for better parallelization and clarity:

* **build**: Restore and build
* **test**: Run unit tests with coverage
* **lint**: Code formatting and analysis
* **security**: Vulnerability scanning

## Acceptance Criteria

- [ ] Workflow uses .NET 10.0.x
- [ ] At minimum: code formatting check, vulnerability scan, and warnings-as-errors are added
- [ ] All checks pass on current codebase
- [ ] Workflow runs on both push and PR to master
- [ ] Consider caching strategy for faster builds

## References

* [GitHub Actions .NET documentation](<https://docs.github.com/en/actions/automating-builds-and-tests/building-and-testing-net>)
* [dotnet format documentation](<https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-format>)
* Current workflow: `.github/workflows/github-ci.yml`
