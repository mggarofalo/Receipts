---
identifier: MGG-29
title: "Update Presentation.Shared.Tests — remove equality tests, adopt FluentAssertions"
id: 685ccbb9-0a60-42f3-8541-3a41fb337f0a
status: Done
priority:
  value: 2
  name: High
createdBy: Michael Garofalo
project: Receipts
team: mggarofalo
url: "https://linear.app/mggarofalo/issue/MGG-29/update-presentationsharedtests-remove-equality-tests-adopt"
gitBranchName: mggarofalo/mgg-29-update-presentationsharedtests-remove-equality-tests-adopt
createdAt: "2026-02-11T04:59:49.000Z"
updatedAt: "2026-02-11T10:39:06.396Z"
completedAt: "2026-02-11T10:39:06.378Z"
---

# Update Presentation.Shared.Tests — remove equality tests, adopt FluentAssertions

## 14 test files

**ViewModel tests (7 files):** Keep `Constructor_*` tests, delete all `Equals_*`, `GetHashCode_*`, `OperatorEquals_*`, `OperatorNotEquals_*` tests. For aggregate property tests, update same-reference comparisons to `.Should().BeSameAs()`.

* `tests/Presentation.Shared.Tests/ViewModels/Core/AccountVMTests.cs`
* `tests/Presentation.Shared.Tests/ViewModels/Core/ReceiptVMTests.cs`
* `tests/Presentation.Shared.Tests/ViewModels/Core/ReceiptItemVMTests.cs`
* `tests/Presentation.Shared.Tests/ViewModels/Core/TransactionVMTests.cs`
* `tests/Presentation.Shared.Tests/ViewModels/Aggregates/ReceiptWithItemsVMTests.cs`
* `tests/Presentation.Shared.Tests/ViewModels/Aggregates/TransactionAccountVMTests.cs`
* `tests/Presentation.Shared.Tests/ViewModels/Aggregates/TripVMTests.cs`

**HttpClientApiExtensions tests (7 files):** Replace `Assert.Equal(object, object)` with `.Should().BeEquivalentTo()`. Replace `Assert.All`/`Assert.Contains` patterns on collections with `.Should().BeEquivalentTo()`.

* `tests/Presentation.Shared.Tests/HttpClientApiExtensions/Core/AccountClientTests.cs`
* `tests/Presentation.Shared.Tests/HttpClientApiExtensions/Core/ReceiptClientTests.cs`
* `tests/Presentation.Shared.Tests/HttpClientApiExtensions/Core/ReceiptItemClientTests.cs`
* `tests/Presentation.Shared.Tests/HttpClientApiExtensions/Core/TransactionClientTests.cs`
* `tests/Presentation.Shared.Tests/HttpClientApiExtensions/Aggregates/ReceiptWithItemsClientTests.cs`
* `tests/Presentation.Shared.Tests/HttpClientApiExtensions/Aggregates/TransactionAccountClientTests.cs`
* `tests/Presentation.Shared.Tests/HttpClientApiExtensions/Aggregates/TripClientTests.cs`

Commit: `test(shared): remove equality tests, adopt FluentAssertions`
