# Task Breakdown: [FEATURE-NUMBER] - [Feature Title]

> **Specification**: [Link to spec.md]
> **Technical Plan**: [Link to plan.md]
> **Created**: [DATE]
> **Last Updated**: [DATE]

## Overview

This document breaks down the technical plan into ordered, executable tasks. Tasks are organized by user story and respect dependencies between them.

## Legend

- `[P]` - Can be parallelized with other `[P]` tasks in the same group
- `[S]` - Sequential; must complete before next task
- `[B]` - Blocked by specified task(s)

---

## Phase 1: Domain Layer

### Task 1.1: Create Domain Entity [S]

**Description**: Create the [Entity] domain model with validation logic.

**Files**:
- `src/Domain/Core/[Entity].cs`

**Acceptance Criteria**:
- [ ] Entity has all required properties
- [ ] Constructor validates required fields
- [ ] Equality members implemented (if needed)

**Tests Required**:
- `tests/Domain.Tests/Core/[Entity]Tests.cs`

**Estimated Complexity**: Low | Medium | High

---

### Task 1.2: Create Domain Entity Tests [S] [B: 1.1]

**Description**: Write unit tests for [Entity] domain model.

**Files**:
- `tests/Domain.Tests/Core/[Entity]Tests.cs`

**Acceptance Criteria**:
- [ ] Constructor validation tests
- [ ] Property assignment tests
- [ ] Equality tests (if applicable)

---

## Phase 2: Application Layer

### Task 2.1: Create Service Interface [S]

**Description**: Define the I[Entity]Service interface in Application layer.

**Files**:
- `src/Application/Interfaces/Services/I[Entity]Service.cs`

**Acceptance Criteria**:
- [ ] CRUD method signatures defined
- [ ] Proper async patterns with CancellationToken

---

### Task 2.2: Create Commands [P]

**Description**: Create Create, Update, Delete commands and handlers.

**Files**:
- `src/Application/Commands/[Entity]/Create/Create[Entity]Command.cs`
- `src/Application/Commands/[Entity]/Create/Create[Entity]CommandHandler.cs`
- `src/Application/Commands/[Entity]/Update/Update[Entity]Command.cs`
- `src/Application/Commands/[Entity]/Update/Update[Entity]CommandHandler.cs`
- `src/Application/Commands/[Entity]/Delete/Delete[Entity]Command.cs`
- `src/Application/Commands/[Entity]/Delete/Delete[Entity]CommandHandler.cs`

**Acceptance Criteria**:
- [ ] Commands implement IRequest<T>
- [ ] Handlers implement IRequestHandler<TRequest, TResponse>
- [ ] Handlers use injected service interface

---

### Task 2.3: Create Queries [P]

**Description**: Create GetAll and GetById queries and handlers.

**Files**:
- `src/Application/Queries/Core/[Entity]/GetAll[Entity]Query.cs`
- `src/Application/Queries/Core/[Entity]/GetAll[Entity]QueryHandler.cs`
- `src/Application/Queries/Core/[Entity]/Get[Entity]ByIdQuery.cs`
- `src/Application/Queries/Core/[Entity]/Get[Entity]ByIdQueryHandler.cs`

**Acceptance Criteria**:
- [ ] Queries implement IRequest<T>
- [ ] Handlers implement IRequestHandler<TRequest, TResponse>
- [ ] Proper null handling for GetById

---

### Task 2.4: Create Command/Query Handler Tests [S] [B: 2.2, 2.3]

**Description**: Write unit tests for all command and query handlers.

**Files**:
- `tests/Application.Tests/Commands/[Entity]/Create[Entity]CommandHandlerTests.cs`
- `tests/Application.Tests/Commands/[Entity]/Update[Entity]CommandHandlerTests.cs`
- `tests/Application.Tests/Commands/[Entity]/Delete[Entity]CommandHandlerTests.cs`
- `tests/Application.Tests/Queries/Core/[Entity]/GetAll[Entity]QueryHandlerTests.cs`
- `tests/Application.Tests/Queries/Core/[Entity]/Get[Entity]ByIdQueryHandlerTests.cs`

**Acceptance Criteria**:
- [ ] Mock service dependencies
- [ ] Test success and failure scenarios
- [ ] Use SampleData generators for test data

---

## Phase 3: Infrastructure Layer

### Task 3.1: Create Infrastructure Entity [S]

**Description**: Create the EF Core entity for database mapping.

**Files**:
- `src/Infrastructure/Entities/[Entity]Entity.cs`

**Acceptance Criteria**:
- [ ] All database columns defined
- [ ] Proper EF Core attributes/configuration

---

### Task 3.2: Create AutoMapper Profile (Infrastructure) [S] [B: 3.1]

**Description**: Create mapping between Domain and Infrastructure entities.

**Files**:
- `src/Infrastructure/Mapping/[Entity]MappingProfile.cs`

**Acceptance Criteria**:
- [ ] Bidirectional mapping configured
- [ ] All properties mapped correctly

---

### Task 3.3: Create Repository [S] [B: 3.1, 3.2]

**Description**: Implement the repository for [Entity].

**Files**:
- `src/Infrastructure/Repositories/[Entity]Repository.cs`

**Acceptance Criteria**:
- [ ] Implements I[Entity]Service
- [ ] Uses AutoMapper for conversions
- [ ] Proper async/await patterns

---

### Task 3.4: Create Database Migration [S] [B: 3.1]

**Description**: Generate EF Core migration for new entity.

**Command**:
```bash
dotnet ef migrations add Add[Entity]Table --project src/Infrastructure/Infrastructure.csproj --startup-project src/Presentation/API/API.csproj
```

**Acceptance Criteria**:
- [ ] Migration creates correct table structure
- [ ] Foreign keys properly defined (if any)

---

### Task 3.5: Register Infrastructure Services [S] [B: 3.3]

**Description**: Register repository in DI container.

**Files**:
- `src/Infrastructure/InfrastructureServiceRegistration.cs`

**Acceptance Criteria**:
- [ ] Service registered with Scoped lifetime

---

## Phase 4: Presentation Layer

### Task 4.1: Create ViewModel [P]

**Description**: Create the API ViewModel/DTO.

**Files**:
- `src/Presentation/Shared/ViewModels/[Entity]ViewModel.cs`

**Acceptance Criteria**:
- [ ] All required properties defined
- [ ] Proper nullability annotations

---

### Task 4.2: Create ViewModel Validator [P]

**Description**: Create FluentValidation validator for ViewModel.

**Files**:
- `src/Presentation/Shared/Validators/[Entity]ViewModelValidator.cs`

**Acceptance Criteria**:
- [ ] All business rules validated
- [ ] Clear error messages

---

### Task 4.3: Create AutoMapper Profile (API) [S] [B: 4.1]

**Description**: Create mapping between Domain and ViewModel.

**Files**:
- `src/Presentation/API/Mapping/[Entity]MappingProfile.cs`

**Acceptance Criteria**:
- [ ] Bidirectional mapping configured

---

### Task 4.4: Create API Controller [S] [B: 4.1, 4.2, 4.3]

**Description**: Create REST API controller with CRUD endpoints.

**Files**:
- `src/Presentation/API/Controllers/Core/[Entity]Controller.cs`

**Acceptance Criteria**:
- [ ] All CRUD endpoints implemented
- [ ] MediatR used for all operations
- [ ] Proper HTTP status codes returned
- [ ] SignalR notifications sent after writes (if applicable)

---

## Phase 5: Test Data Generators

### Task 5.1: Create SampleData Generators [P]

**Description**: Create test data generators for all test projects.

**Files**:
- `tests/SampleData/Domain/Core/[Entity]Generator.cs`
- `tests/SampleData/Entities/[Entity]EntityGenerator.cs`
- `tests/SampleData/ViewModels/[Entity]ViewModelGenerator.cs`

**Acceptance Criteria**:
- [ ] Generate() returns single instance
- [ ] GenerateList(count) returns collection
- [ ] Generated data is valid per business rules

---

## Phase 6: Integration & Verification

### Task 6.1: Run All Tests [S]

**Command**:
```bash
dotnet test Receipts.sln
```

**Acceptance Criteria**:
- [ ] All tests pass
- [ ] No build warnings

---

### Task 6.2: Manual Verification [S] [B: 6.1]

**Description**: Verify feature works end-to-end.

**Acceptance Criteria**:
- [ ] API endpoints respond correctly
- [ ] Data persists to database
- [ ] Real-time updates work (if applicable)

---

## Summary

| Phase | Tasks | Parallelizable |
|-------|-------|----------------|
| Domain | 2 | No |
| Application | 4 | Tasks 2.2, 2.3 |
| Infrastructure | 5 | No |
| Presentation | 4 | Tasks 4.1, 4.2 |
| Test Data | 1 | Yes |
| Integration | 2 | No |

**Total Tasks**: 18
