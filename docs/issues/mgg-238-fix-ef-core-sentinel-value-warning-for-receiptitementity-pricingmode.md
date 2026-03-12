---
identifier: MGG-238
title: Fix EF Core sentinel value warning for ReceiptItemEntity.PricingMode
id: d7a40670-f9c7-4797-9886-0938330023a1
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
  - Bug
url: "https://linear.app/mggarofalo/issue/MGG-238/fix-ef-core-sentinel-value-warning-for-receiptitementitypricingmode"
gitBranchName: mggarofalo/mgg-238-fix-ef-core-sentinel-value-warning-for
createdAt: "2026-03-05T11:46:21.759Z"
updatedAt: "2026-03-05T11:58:39.714Z"
completedAt: "2026-03-05T11:58:39.700Z"
attachments:
  - title: "fix(infrastructure): remove PricingMode database default to fix EF Core sentinel warning (MGG-238)"
    url: "https://github.com/mggarofalo/Receipts/pull/84"
---

# Fix EF Core sentinel value warning for ReceiptItemEntity.PricingMode

## Problem

EF Core emits a runtime warning on startup:

> The 'PricingMode' property on entity type 'ReceiptItemEntity' is configured with a database-generated default, but has no configured sentinel value. The database-generated default will always be used for inserts when the property has the value 'Quantity', since this is the CLR default for the 'PricingMode' type.

Because `PricingMode` is an enum and `Quantity` is its first member (value `0`), EF Core can't distinguish between "the user explicitly set `Quantity`" and "no value was set (CLR default)." This means:

* If someone intentionally sets `PricingMode = Quantity`, EF may ignore it and use the DB default instead
* The behavior is silently wrong — no error, just incorrect data

## Fix Options

- [ ] Investigate which approach fits best:
  1. **Add a sentinel value**: Add a `None = -1` or `Unspecified = 0` member to the enum and shift real values to start at 1, then configure `.HasSentinel(PricingMode.Unspecified)`
  2. **Use a nullable backing field**: Make the backing field `PricingMode?` so EF can distinguish null (unset) from `Quantity` (explicitly set)
  3. **Remove the database-generated default**: If the DB default isn't actually needed, drop it from the migration/configuration so EF always uses the application-supplied value
- [ ] Apply the fix
- [ ] Verify the warning no longer appears on startup
- [ ] Ensure existing data/migrations are handled (migration may be needed if enum values shift)

## References

* [EF Core docs: Default values](<https://aka.ms/efcore-docs-default-values>)
