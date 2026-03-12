---
identifier: MGG-167
title: Add EF migration for AuditLogs and AuthAuditLogs tables
id: baed1e92-5b5e-4e11-ad11-b4e09c6abb34
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
url: "https://linear.app/mggarofalo/issue/MGG-167/add-ef-migration-for-auditlogs-and-authauditlogs-tables"
gitBranchName: mggarofalo/mgg-167-add-ef-migration-for-auditlogs-and-authauditlogs-tables
createdAt: "2026-02-24T10:36:16.240Z"
updatedAt: "2026-02-24T10:56:22.222Z"
completedAt: "2026-02-24T10:56:22.202Z"
---

# Add EF migration for AuditLogs and AuthAuditLogs tables

## Problem

The `ApplicationDbContext` defines `DbSet<AuditLogEntity>` and `DbSet<AuthAuditLogEntity>` with table configuration, but no EF migration exists to create the `AuditLogs` and `AuthAuditLogs` tables. The API crashes at startup during admin user seeding because `SaveChangesAsync` audit logging tries to INSERT into `AuditLogs` which doesn't exist.

```
Npgsql.PostgresException: 42P01: relation "AuditLogs" does not exist
```

## Fix

Scaffolded EF migration `20260224104302_AddAuditTables` and excluded [ASP.NET](<http://ASP.NET>) Identity internal entities from audit logging (they use composite keys without an `Id` property).

## Acceptance Criteria

- [X] `AuditLogs` table created with indexes on `(EntityType, EntityId)`, `ChangedAt`, `ChangedByUserId`, `ChangedByApiKeyId`
- [X] `AuthAuditLogs` table created with indexes on `(UserId, Timestamp)`, `(EventType, Timestamp)`, `Timestamp`, `IpAddress`
- [X] API starts successfully and admin seeding completes
