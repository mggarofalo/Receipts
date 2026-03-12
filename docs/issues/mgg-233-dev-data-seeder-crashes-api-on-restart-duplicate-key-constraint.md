---
identifier: MGG-233
title: Dev data seeder crashes API on restart (duplicate key constraint)
id: 402834a1-6a34-4146-b6c2-766cbad3837b
status: Done
priority:
  value: 1
  name: Urgent
assignee: Michael Garofalo
createdBy: Michael Garofalo
project: Receipts
team: mggarofalo
labels:
  - dx
  - backend
  - Bug
url: "https://linear.app/mggarofalo/issue/MGG-233/dev-data-seeder-crashes-api-on-restart-duplicate-key-constraint"
gitBranchName: mggarofalo/mgg-233-dev-data-seeder-crashes-api-on-restart-duplicate-key
createdAt: "2026-03-05T11:16:58.065Z"
updatedAt: "2026-03-05T11:42:59.913Z"
completedAt: "2026-03-05T11:23:17.567Z"
attachments:
  - title: "refactor(infrastructure): migrate dev seed data to EF Core HasData (MGG-233)"
    url: "https://github.com/mggarofalo/Receipts/pull/83"
  - title: "refactor(infrastructure): migrate dev seed data to EF Core HasData (MGG-233)"
    url: "https://github.com/mggarofalo/Receipts/pull/82"
---

# Dev data seeder crashes API on restart (duplicate key constraint)

## Problem

The API crashes on startup with an unhandled `DbUpdateException` when `DevelopmentDataSeeder.SeedAsync()` tries to insert categories that already exist in the database.

**Error:** `23505: duplicate key value violates unique constraint "IX_Categories_Name"`

**Root cause:** The seeder's idempotency guard (`DevelopmentDataSeeder.cs:22`) only checks `Accounts.AnyAsync()`. If the Accounts table is empty but Categories already exist (e.g., from a partial previous seed, a DB state diverge between Aspire restarts, or manual data manipulation), the guard passes and the seeder attempts to re-insert categories, violating the unique constraint.

**Secondary issue:** The seeder's exception is unhandled and propagates to `Program.Main`, crashing the entire API process.

## Resolution

Replacing the custom `DevelopmentDataSeeder` entirely with EF Core's built-in `HasData()` mechanism in MGG-235. `HasData()` is idempotent by design — seed data is tracked via migrations, eliminating both the guard logic and the crash vector.

## Observed in

Aspire console logs for `api` resource — exit code `-532462766` (unhandled exception).
