---
identifier: MGG-235
title: Migrate dev seed data to EF Core HasData
id: b31a61a0-b724-4396-b54a-0e45589df1b2
status: Done
priority:
  value: 1
  name: Urgent
createdBy: Michael Garofalo
project: Receipts
team: mggarofalo
labels:
  - dx
  - backend
  - Improvement
url: "https://linear.app/mggarofalo/issue/MGG-235/migrate-dev-seed-data-to-ef-core-hasdata"
gitBranchName: mggarofalo/mgg-235-migrate-dev-seed-data-to-ef-core-hasdata
createdAt: "2026-03-05T11:23:02.987Z"
updatedAt: "2026-03-05T13:52:43.237Z"
completedAt: "2026-03-05T13:52:43.213Z"
---

# Migrate dev seed data to EF Core HasData

## Summary

Replace the custom `DevelopmentDataSeeder` service with EF Core's built-in `HasData()` seed mechanism. Seed data becomes part of the schema via migrations — idempotent by design, no custom guard logic needed.

## Motivation

MGG-233 identified that the `DevelopmentDataSeeder` crashes the API on restart due to fragile idempotency logic. Rather than patching the seeder, this replaces the entire approach with the canonical EF Core pattern.

## Checklist

- [ ] Add `HasData()` calls with fixed GUIDs to all 8 entity configuration methods in `ApplicationDbContext.cs`
  - Accounts (5), Categories (5), Subcategories (13), ItemTemplates (4), Receipts (6), ReceiptItems (11), Adjustments (3), Transactions (6)
- [ ] Generate EF migration (`SeedDevelopmentData`)
- [ ] Delete `DevelopmentDataSeeder.cs`
- [ ] Remove DI registration from `InfrastructureService.cs`
- [ ] Remove seeder invocation from `Program.cs`
- [ ] Build passes with `TreatWarningsAsErrors=true`
- [ ] All tests pass
- [ ] API starts cleanly on restart (verify via Aspire console logs)
