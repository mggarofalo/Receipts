---
identifier: MGG-28
title: "Update Infrastructure.Tests — remove equality tests, adopt FluentAssertions"
id: 0b6d6119-d8c2-4643-ab09-b72e4fda56da
status: Done
priority:
  value: 2
  name: High
createdBy: Michael Garofalo
project: Receipts
team: mggarofalo
url: "https://linear.app/mggarofalo/issue/MGG-28/update-infrastructuretests-remove-equality-tests-adopt"
gitBranchName: mggarofalo/mgg-28-update-infrastructuretests-remove-equality-tests-adopt
createdAt: "2026-02-11T04:59:38.092Z"
updatedAt: "2026-02-11T10:39:06.341Z"
completedAt: "2026-02-11T10:39:06.326Z"
---

# Update Infrastructure.Tests — remove equality tests, adopt FluentAssertions

## 12 test files

**Entity tests (4 files):** Keep `Constructor_*` tests, delete all equality tests.

* `tests/Infrastructure.Tests/Entities/Core/AccountEntityTests.cs`
* `tests/Infrastructure.Tests/Entities/Core/ReceiptEntityTests.cs`
* `tests/Infrastructure.Tests/Entities/Core/ReceiptItemEntityTests.cs`
* `tests/Infrastructure.Tests/Entities/Core/TransactionEntityTests.cs`

**Mapping tests (4 files):** Replace `Assert.Equal(expected, actual)` with `actual.Should().BeEquivalentTo(expected)`.

* `tests/Infrastructure.Tests/Mapping/AccountMappingProfileTests.cs`
* `tests/Infrastructure.Tests/Mapping/ReceiptMappingProfileTests.cs`
* `tests/Infrastructure.Tests/Mapping/ReceiptItemMappingProfileTests.cs`
* `tests/Infrastructure.Tests/Mapping/TransactionMappingProfileTests.cs`

**Repository tests (4 files):** Replace `Assert.Equal(entity, actual)` with `.Should().BeEquivalentTo()`. For `CreateAsync` tests, replace `Id = Guid.Empty` mutation pattern with `.BeEquivalentTo(entities, opt => opt.Excluding(x => x.Id))`.

* `tests/Infrastructure.Tests/Repositories/AccountRepositoryTests.cs`
* `tests/Infrastructure.Tests/Repositories/ReceiptRepositoryTests.cs`
* `tests/Infrastructure.Tests/Repositories/ReceiptItemRepositoryTests.cs`
* `tests/Infrastructure.Tests/Repositories/TransactionRepositoryTests.cs`

**Service tests (4 files):** Replace `Assert.Equal(expected, actual)` with `.Should().BeSameAs(expected)` (mock returns same instance).

* `tests/Infrastructure.Tests/Services/AccountServiceTests.cs`
* `tests/Infrastructure.Tests/Services/ReceiptServiceTests.cs`
* `tests/Infrastructure.Tests/Services/ReceiptItemServiceTests.cs`
* `tests/Infrastructure.Tests/Services/TransactionServiceTests.cs`

Commit: `test(infrastructure): remove equality tests, adopt FluentAssertions`
