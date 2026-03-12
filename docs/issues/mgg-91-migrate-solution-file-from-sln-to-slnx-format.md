---
identifier: MGG-91
title: Migrate solution file from .sln to .slnx format
id: 9712b8b1-bd13-4478-a39e-b2f735a7371c
status: Done
priority:
  value: 2
  name: High
createdBy: Michael Garofalo
project: Receipts
team: mggarofalo
labels:
  - cleanup
  - infra
milestone: "Phase 0: Housekeeping"
url: "https://linear.app/mggarofalo/issue/MGG-91/migrate-solution-file-from-sln-to-slnx-format"
gitBranchName: mggarofalo/mgg-91-migrate-solution-file-from-sln-to-slnx-format
createdAt: "2026-02-12T12:24:51.026Z"
updatedAt: "2026-02-12T12:32:29.777Z"
completedAt: "2026-02-12T12:32:29.765Z"
---

# Migrate solution file from .sln to .slnx format

## Summary

Migrate from the legacy `Receipts.sln` format to the new XML-based `Receipts.slnx` format introduced in .NET 9 and fully supported in .NET 10.

## Why

* `.slnx` is the modern replacement — cleaner, smaller, and easier to read/diff
* Eliminates GUIDs, platform configuration blocks, and nested project boilerplate
* Better merge conflict resolution (XML vs opaque GUIDs)
* `.sln` is legacy and will eventually be deprecated

## Tasks

- [ ] Convert `Receipts.sln` to `Receipts.slnx` using `dotnet sln migrate`
- [ ] Verify the new `.slnx` loads correctly in VS Code / VS / Rider
- [ ] Verify `dotnet build Receipts.slnx` and `dotnet test Receipts.slnx` pass
- [ ] Update CI workflow (`.github/workflows/github-ci.yml`) to reference `Receipts.slnx`
- [ ] Update `AGENTS.md` build commands to reference `Receipts.slnx`
- [ ] Delete old `Receipts.sln`

## Acceptance Criteria

- [ ] `Receipts.slnx` is the only solution file
- [ ] All build, test, and format commands work with the new file
- [ ] CI passes
- [ ] No references to `Receipts.sln` remain in the repo
