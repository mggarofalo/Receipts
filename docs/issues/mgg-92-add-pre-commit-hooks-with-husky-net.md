---
identifier: MGG-92
title: Add pre-commit hooks with Husky.NET
id: d8c398c0-aac5-4500-9f81-1804a6a697c1
status: Done
priority:
  value: 2
  name: High
createdBy: Michael Garofalo
project: Receipts
team: mggarofalo
labels:
  - dx
  - infra
milestone: "Phase 0: Housekeeping"
url: "https://linear.app/mggarofalo/issue/MGG-92/add-pre-commit-hooks-with-huskynet"
gitBranchName: mggarofalo/mgg-92-add-pre-commit-hooks-with-huskynet
createdAt: "2026-02-12T12:25:10.597Z"
updatedAt: "2026-02-12T12:40:32.703Z"
completedAt: "2026-02-12T12:40:32.687Z"
---

# Add pre-commit hooks with Husky.NET

## Summary

Add pre-commit hooks to enforce code quality checks locally before code reaches CI. Use [Husky.NET](<https://alirezanet.github.io/Husky.Net/>) as the hook manager since this is a .NET-only repo (no Node.js dependency needed).

## Why

* Catch formatting issues, build warnings, and test failures before push
* Faster feedback loop than waiting for CI
* Prevents "fix formatting" follow-up commits
* Prepares the repo for the spec-first workflow in Phase 1 (MGG-21) which will add spec linting and codegen staleness checks to the hook pipeline

## Pre-commit hook pipeline

```bash
# Step 1: Check code formatting
dotnet format --verify-no-changes

# Step 2: Build with warnings-as-errors
dotnet build -p:TreatWarningsAsErrors=true

# Step 3: Run tests
dotnet test --no-build
```

## Tasks

- [ ] Install [Husky.NET](<http://Husky.NET>) as a dotnet tool (`dotnet tool install husky`)
- [ ] Add `.config/dotnet-tools.json` manifest if not present
- [ ] Initialize Husky: `dotnet husky install`
- [ ] Create `.husky/pre-commit` hook with the pipeline above
- [ ] Add `husky install` to the repo's restore/setup flow so hooks auto-install on clone
- [ ] Verify hooks trigger on `git commit`
- [ ] Document the hook setup in [AGENTS.md](<http://AGENTS.md>)

## Acceptance Criteria

- [ ] Pre-commit hook runs format check, build, and tests
- [ ] Hook blocks commits that fail any step
- [ ] New clones auto-install hooks via `dotnet tool restore`
- [ ] [AGENTS.md](<http://AGENTS.md>) documents how to skip hooks when needed (`--no-verify`)
