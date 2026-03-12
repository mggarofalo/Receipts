---
identifier: MGG-31
title: Update Application.Tests — adopt FluentAssertions for query handler assertions
id: 5d1a542b-c030-49cb-955a-1af67439cd88
status: Done
priority:
  value: 2
  name: High
createdBy: Michael Garofalo
project: Receipts
team: mggarofalo
url: "https://linear.app/mggarofalo/issue/MGG-31/update-applicationtests-adopt-fluentassertions-for-query-handler"
gitBranchName: mggarofalo/mgg-31-update-applicationtests-adopt-fluentassertions-for-query
createdAt: "2026-02-11T04:59:58.253Z"
updatedAt: "2026-02-11T05:43:25.222Z"
completedAt: "2026-02-11T05:43:25.211Z"
---

# Update Application.Tests — adopt FluentAssertions for query handler assertions

## \~8-10 test files

Replace `Assert.Equal(expected, actual)` with `.Should().BeSameAs(expected)` in query handler tests (mock returns same instance).

**Query handler tests:**

* `tests/Application.Tests/Queries/Core/Account/GetAccountByIdQueryHandlerTests.cs`
* `tests/Application.Tests/Queries/Core/Receipt/GetReceiptByIdQueryHandlerTests.cs`
* `tests/Application.Tests/Queries/Core/ReceiptItem/GetReceiptItemByIdQueryHandlerTests.cs`
* `tests/Application.Tests/Queries/Core/Transaction/GetTransactionByIdQueryHandlerTests.cs`
* `tests/Application.Tests/Queries/Aggregates/ReceiptsWithItems/GetReceiptWithItemsByReceiptIdQueryHandlerTests.cs`
* `tests/Application.Tests/Queries/Aggregates/TransactionAccounts/GetTransactionAccountByTransactionIdQueryHandlerTests.cs`
* `tests/Application.Tests/Queries/Aggregates/TransactionAccounts/GetTransactionAccountsByReceiptIdQueryHandlerTests.cs`
* `tests/Application.Tests/Queries/Aggregates/Trips/GetTripByReceiptIdQueryHandler.cs`

For aggregate property comparisons, use `.Should().BeSameAs()` on sub-properties.

Commit: `test(application): adopt FluentAssertions for query handler assertions`
