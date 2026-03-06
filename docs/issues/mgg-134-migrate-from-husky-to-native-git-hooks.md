---
identifier: MGG-134
title: Migrate from Husky to native Git hooks
id: 83141343-3be8-47f4-b6dc-0616d1d52257
status: Done
priority:
  value: 4
  name: Low
assignee: Michael Garofalo
createdBy: Michael Garofalo
project: Receipts
team: mggarofalo
labels:
  - dx
  - cleanup
url: "https://linear.app/mggarofalo/issue/MGG-134/migrate-from-husky-to-native-git-hooks"
gitBranchName: mggarofalo/mgg-134-migrate-from-husky-to-native-git-hooks
createdAt: "2026-02-21T11:49:41.655Z"
updatedAt: "2026-02-21T12:28:22.884Z"
completedAt: "2026-02-21T12:28:22.867Z"
attachments:
  - title: "chore: migrate from Husky.NET to native Git hooks (MGG-134)"
    url: "https://github.com/mggarofalo/Receipts/pull/20"
---

# Migrate from Husky to native Git hooks

Replace Husky with native Git hooks to reduce the dependency footprint.

### Why

* Husky adds an npm dependency for functionality that Git provides natively
* Native hooks are simpler to maintain and have no external dependencies

### Scope

* Remove Husky package and `.husky/` directory
* Set up equivalent native Git hooks (e.g., pre-commit, commit-msg)
* Update any CI or onboarding docs that reference Husky
* Ensure hooks are easily shareable (e.g., via a setup script or `.githooks/` directory with `core.hooksPath`)
