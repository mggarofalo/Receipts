---
identifier: MGG-30
title: Update Presentation.API.Tests — adopt FluentAssertions for mapping tests
id: f4bf4e67-0c67-4417-8556-0bbbb97e764d
status: Done
priority:
  value: 2
  name: High
createdBy: Michael Garofalo
project: Receipts
team: mggarofalo
url: "https://linear.app/mggarofalo/issue/MGG-30/update-presentationapitests-adopt-fluentassertions-for-mapping-tests"
gitBranchName: mggarofalo/mgg-30-update-presentationapitests-adopt-fluentassertions-for
createdAt: "2026-02-11T04:59:52.631Z"
updatedAt: "2026-02-11T10:39:06.442Z"
completedAt: "2026-02-11T10:39:06.428Z"
---

# Update Presentation.API.Tests — adopt FluentAssertions for mapping tests

## 7 test files

Replace `Assert.Equal(expected, actual)` with `actual.Should().BeEquivalentTo(expected)` in all mapping profile tests.

**Core (4 files):**

* `tests/Presentation.API.Tests/Mapping/Core/AccountMappingProfileTests.cs`
* `tests/Presentation.API.Tests/Mapping/Core/ReceiptMappingProfileTests.cs`
* `tests/Presentation.API.Tests/Mapping/Core/ReceiptItemMappingProfileTests.cs`
* `tests/Presentation.API.Tests/Mapping/Core/TransactionMappingProfileTests.cs`

**Aggregates (3 files):**

* `tests/Presentation.API.Tests/Mapping/Aggregates/ReceiptWithItemsMappingProfileTests.cs`
* `tests/Presentation.API.Tests/Mapping/Aggregates/TransactionAccountMappingProfileTests.cs`
* `tests/Presentation.API.Tests/Mapping/Aggregates/TripMappingProfileTests.cs`

Commit: `test(api): adopt FluentAssertions for mapping test assertions`
