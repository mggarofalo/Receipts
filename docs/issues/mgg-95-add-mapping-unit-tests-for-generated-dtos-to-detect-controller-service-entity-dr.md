---
identifier: MGG-95
title: Add mapping unit tests for generated DTOs to detect controller/service/entity drift
id: 9951d046-b2f1-4074-a134-2c0597907f7f
status: Done
priority:
  value: 3
  name: Medium
createdBy: Michael Garofalo
project: Receipts
team: mggarofalo
labels:
  - testing
  - backend
milestone: "Phase 2: Backend DTO Generation"
url: "https://linear.app/mggarofalo/issue/MGG-95/add-mapping-unit-tests-for-generated-dtos-to-detect"
gitBranchName: mggarofalo/mgg-95-add-mapping-unit-tests-for-generated-dtos-to-detect
createdAt: "2026-02-14T15:45:14.414Z"
updatedAt: "2026-02-15T11:48:54.099Z"
completedAt: "2026-02-15T11:48:54.081Z"
---

# Add mapping unit tests for generated DTOs to detect controller/service/entity drift

## Context

After [MGG-88](./mgg-88-generate-net-request-response-dtos-from-openapi-spec.md) replaces ViewModels with NSwag-generated DTOs from the OpenAPI spec, the mapping layer between Domain entities and generated DTOs becomes a critical integration point. If the spec changes (adding/removing/renaming fields), the generated DTOs change automatically — but the Mapperly mappers and underlying controllers/services/entities may silently fall out of sync.

## Goal

Add comprehensive mapping unit tests that exercise every property path between Domain objects and generated DTOs. These tests serve as **canary tests** — when the spec or domain model changes, these tests break immediately and pinpoint exactly which mappings need attention.

## Scope

### Core Entity Mapping Tests (4 entities × 3 directions)

For each core entity (Account, Receipt, ReceiptItem, Transaction):

1. **CreateRequest → Domain**: Verify all request fields map correctly to domain properties. Verify ID is `Guid.Empty` for create requests.
2. **UpdateRequest → Domain**: Verify all request fields map correctly including the required ID.
3. **Domain → Response**: Verify all domain properties map correctly to response fields. Verify Money value objects flatten to decimal correctly.

### Aggregate Response Mapping Tests (3 aggregates)

For each aggregate (ReceiptWithItems, TransactionAccount, Trip):

1. **Domain → Response**: Verify nested objects compose correctly (e.g., `ReceiptWithItemsResponse` contains correct `ReceiptResponse` + `List<ReceiptItemResponse>`).

### Test Patterns

Each mapping test should:

* Create a fully-populated source object with **distinct, non-default values** for every property
* Map it through the Mapperly mapper
* Assert **every property** on the target matches the expected value
* Use explicit property-by-property assertions (not `BeEquivalentTo`) so failures pinpoint the exact broken field

```csharp
[Fact]
public void ToDomain_FromCreateAccountRequest_MapsAllProperties()
{
    // Arrange
    CreateAccountRequest request = new()
    {
        AccountCode = "TEST-001",
        Name = "Test Account",
        IsActive = true
    };

    // Act
    Account actual = _mapper.ToDomain(request);

    // Assert
    Assert.Equal(Guid.Empty, actual.Id);
    Assert.Equal("TEST-001", actual.AccountCode);
    Assert.Equal("Test Account", actual.Name);
    Assert.True(actual.IsActive);
}

[Fact]
public void ToResponse_FromDomain_MapsAllProperties()
{
    // Arrange
    Guid expectedId = Guid.NewGuid();
    Account account = new(expectedId, "ACC-001", "Savings", true);

    // Act
    AccountResponse actual = _mapper.ToResponse(account);

    // Assert
    Assert.Equal(expectedId, actual.Id);
    Assert.Equal("ACC-001", actual.AccountCode);
    Assert.Equal("Savings", actual.Name);
    Assert.True(actual.IsActive);
}
```

### Special Cases to Cover

* **Money value objects**: Verify `Money(100.50, Currency.USD)` maps to `decimal 100.50` in responses and vice versa
* **ReceiptItem TotalAmount calculation**: Verify `Quantity * UnitPrice` calculation in `ToDomain` with floor rounding
* **DateOnly mapping**: Verify `DateOnly` round-trips correctly through NSwag-generated types
* **Null vs default**: Verify optional/nullable fields handle null inputs correctly

## Test File Structure

```
tests/Presentation.API.Tests/Mapping/
├── Core/
│   ├── AccountDtoMapperTests.cs
│   ├── ReceiptDtoMapperTests.cs
│   ├── ReceiptItemDtoMapperTests.cs
│   └── TransactionDtoMapperTests.cs
└── Aggregates/
    ├── ReceiptWithItemsResponseMapperTests.cs
    ├── TransactionAccountResponseMapperTests.cs
    └── TripResponseMapperTests.cs
```

## Acceptance Criteria

- [ ] Every property on every generated DTO has at least one test asserting correct mapping
- [ ] Money value object decomposition/reconstruction is tested
- [ ] ReceiptItem TotalAmount calculation is tested
- [ ] All tests use explicit property assertions (not `BeEquivalentTo`)
- [ ] Tests use distinct non-default values to catch accidentally-passing defaults
- [ ] Adding a new required field to the spec (and regenerating DTOs) causes a compile error or test failure in the mapper tests
- [ ] All tests pass with zero warnings

## Dependencies

* **Blocked by**: [MGG-88](./mgg-88-generate-net-request-response-dtos-from-openapi-spec.md) (generated DTOs and new mappers must exist first)
