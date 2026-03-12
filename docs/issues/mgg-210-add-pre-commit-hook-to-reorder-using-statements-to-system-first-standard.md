---
identifier: MGG-210
title: Add pre-commit hook to reorder using statements to system-first standard
id: a191229c-f3b8-46e1-b515-b0d61c16eb55
status: Done
priority:
  value: 3
  name: Medium
assignee: Michael Garofalo
createdBy: Michael Garofalo
project: Receipts
team: mggarofalo
labels:
  - dx
  - cleanup
url: "https://linear.app/mggarofalo/issue/MGG-210/add-pre-commit-hook-to-reorder-using-statements-to-system-first"
gitBranchName: mggarofalo/mgg-210-add-pre-commit-hook-to-reorder-using-statements-to-system
createdAt: "2026-03-02T01:24:42.596Z"
updatedAt: "2026-03-03T12:03:24.698Z"
completedAt: "2026-03-03T12:03:24.674Z"
attachments:
  - title: "style: reorder using statements to system-first standard (MGG-210)"
    url: "https://github.com/mggarofalo/Receipts/pull/55"
---

# Add pre-commit hook to reorder using statements to system-first standard

## Scope

The `.editorconfig` currently has `dotnet_sort_system_directives_first = false`, which sorts all `using` statements alphabetically without prioritizing `System.*` namespaces. This is non-standard — the common C# convention is `System.*` first, then third-party, then project namespaces.

## Tasks

- [ ] Change `.editorconfig` to `dotnet_sort_system_directives_first = true`
- [ ] Add a pre-commit hook step that runs `dotnet format` import ordering check (or integrate into the existing format check)
- [ ] Run `dotnet format` across the solution to reorder all existing `using` statements
- [ ] Verify CI passes with the new ordering

## Notes

* The existing pre-commit hook already runs `dotnet format --verify-no-changes` — changing the editorconfig setting should be sufficient to enforce the new ordering going forward
* This is a bulk formatting change, so it should be done on its own branch to avoid noisy diffs in feature PRs
