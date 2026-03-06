---
identifier: MGG-27
title: "Update Domain.Tests — remove equality tests, adopt FluentAssertions"
id: cefe6384-e30a-4c4b-839e-27a12d0d6f5e
status: Done
priority:
  value: 2
  name: High
createdBy: Michael Garofalo
project: Receipts
team: mggarofalo
url: "https://linear.app/mggarofalo/issue/MGG-27/update-domaintests-remove-equality-tests-adopt-fluentassertions"
gitBranchName: mggarofalo/mgg-27-update-domaintests-remove-equality-tests-adopt
createdAt: "2026-02-11T04:59:31.730Z"
updatedAt: "2026-02-11T10:39:06.295Z"
completedAt: "2026-02-11T10:39:06.281Z"
---

# Update Domain.Tests — remove equality tests, adopt FluentAssertions

## 7 test files

**Domain Core (4 files):**

* `tests/Domain.Tests/Core/AccountTests.cs`
* `tests/Domain.Tests/Core/ReceiptTests.cs`
* `tests/Domain.Tests/Core/ReceiptItemTests.cs`
* `tests/Domain.Tests/Core/TransactionTests.cs`

Actions: Keep `Constructor_*` tests (they compare primitives). Delete all `Equals_*`, `GetHashCode_*`, `OperatorEqual_*`, `OperatorNotEqual_*` tests.

**Domain Aggregates (3 files):**

* `tests/Domain.Tests/Aggregates/ReceiptWithItemsTests.cs`
* `tests/Domain.Tests/Aggregates/TransactionAccountTests.cs`
* `tests/Domain.Tests/Aggregates/TripTests.cs`

Actions: Keep property tests, update `Assert.Equal(obj, obj)` on same reference to `.Should().BeSameAs()`. Delete all equality tests.

Commit: `test(domain): remove equality tests and adopt FluentAssertions`
