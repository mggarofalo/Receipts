---
identifier: MGG-258
title: "fix: API key creation fails with 500 — non-UTC DateTimeOffset rejected by Npgsql"
id: a101a608-2875-4788-a431-7a118be6f49d
status: Done
priority:
  value: 2
  name: High
assignee: Michael Garofalo
createdBy: Michael Garofalo
project: Receipts
team: mggarofalo
labels:
  - backend
url: "https://linear.app/mggarofalo/issue/MGG-258/fix-api-key-creation-fails-with-500-non-utc-datetimeoffset-rejected-by"
gitBranchName: mggarofalo/mgg-258-fix-api-key-creation-fails-with-500-non-utc-datetimeoffset
createdAt: "2026-03-06T11:07:41.513Z"
updatedAt: "2026-03-06T11:23:13.113Z"
completedAt: "2026-03-06T11:23:13.091Z"
attachments:
  - title: "fix: normalize DateTimeOffset to UTC via global EF Core converter (MGG-258)"
    url: "https://github.com/mggarofalo/Receipts/pull/97"
---

# fix: API key creation fails with 500 — non-UTC DateTimeOffset rejected by Npgsql

## Bug

`POST /api/apikeys` returns **500 Internal Server Error** when the client sends an `ExpiresAt` value with a non-UTC offset.

## Root Cause

Npgsql requires `DateTimeOffset` values written to `timestamp with time zone` columns to have offset 0 (UTC). When the browser sends a local-time offset (e.g., `-05:00` for EST), the INSERT fails with:

> `System.ArgumentException: Cannot write DateTimeOffset with Offset=-05:00:00 to PostgreSQL type 'timestamp with time zone', only offset 0 (UTC) is supported.`

The immediate trigger is `ApiKeyService.CreateApiKeyAsync` (line 24 of `src/Infrastructure/Services/ApiKeyService.cs`) passing `expiresAt` from the client request directly to the entity without UTC conversion. But the underlying problem is that **there is no global safety net** — any future `DateTimeOffset` property accepted from external input would hit the same issue.

## Stack Trace (from Aspire structured logs)

```
DbUpdateException → ArgumentException
  at Npgsql.Internal.Converters.DateTimeOffsetConverter.WriteCore
  → NpgsqlCommand.ExecuteReader
  → RelationalCommand.ExecuteReaderAsync
  → ReaderModificationCommandBatch.ExecuteAsync
  → BatchExecutor.ExecuteAsync
  → RelationalDatabase.SaveChangesAsync
```

Trace ID: `a71bb68`, action: `API.Controllers.ApiKeyController.CreateApiKey`

## Fix — Global EF Core Value Converter

Add a `DateTimeOffset` → UTC value converter in `ApplicationDbContext.LoopPropertiesAndSetColumnTypes()`. This method already loops all entity properties and sets column types + enum converters. Adding a UTC-normalizing converter for `DateTimeOffset` and `DateTimeOffset?` will future-proof every entity:

```csharp
// In LoopPropertiesAndSetColumnTypes, after setting the column type for DateTimeOffset:
if (baseType == typeof(DateTimeOffset))
{
    property.SetValueConverter(new ValueConverter<DateTimeOffset, DateTimeOffset>(
        v => v.ToUniversalTime(),
        v => v.ToUniversalTime()));
}
```

This is consistent with how the method already applies `EnumToStringConverter` for enum properties.

## Affected Code

* `src/Infrastructure/ApplicationDbContext.cs` — `LoopPropertiesAndSetColumnTypes()` (add converter)
* `src/Infrastructure/Services/ApiKeyService.cs` line 24 — immediate trigger (will be fixed by the global converter)

## Checklist

- [ ] Add global `DateTimeOffset` → UTC value converter in `ApplicationDbContext.LoopPropertiesAndSetColumnTypes()`
- [ ] Handle both `DateTimeOffset` and `DateTimeOffset?` (nullable) via the base type unwrap already in `GetColumnType`
- [ ] Verify the converter is skipped for InMemory provider (same as existing column type logic)
- [ ] Add integration or unit test: persist a `DateTimeOffset` with non-UTC offset, assert it round-trips as UTC
- [ ] Verify `POST /api/apikeys` with non-UTC `ExpiresAt` succeeds after the fix
