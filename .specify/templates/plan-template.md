# Technical Plan: [FEATURE-NUMBER] - [Feature Title]

> **Specification**: [Link to spec.md]
> **Status**: Draft | In Review | Approved
> **Created**: [DATE]
> **Last Updated**: [DATE]

## Architecture Overview

[High-level description of how this feature fits into the existing architecture. Include a diagram if helpful.]

## Constitution Compliance

This plan adheres to the following constitution articles:

- [x] Article I: Clean Architecture Compliance
- [x] Article II: CQRS Pattern Enforcement
- [x] Article III: Test-First Development
- [x] Article IV: Explicit Type Declarations
- [x] Article V: Domain Separation
- [x] Article VI: Database Access Patterns
- [x] Article VII: API Design Standards
- [x] Article VIII: Simplicity Over Cleverness
- [x] Article IX: Security and Configuration
- [x] Article X: Conventional Commits

## Technology Stack

| Component | Technology | Rationale |
|-----------|------------|-----------|
| [Component] | [Technology] | [Why this choice] |

## Layer Changes

### Domain Layer

#### New Entities

```
Domain/Core/[EntityName].cs
```

[Description of entity and its properties]

#### Entity Modifications

[Any changes to existing domain entities]

### Application Layer

#### Commands

| Command | Handler | Description |
|---------|---------|-------------|
| `Create[Entity]Command` | `Create[Entity]CommandHandler` | [Description] |
| `Update[Entity]Command` | `Update[Entity]CommandHandler` | [Description] |
| `Delete[Entity]Command` | `Delete[Entity]CommandHandler` | [Description] |

#### Queries

| Query | Handler | Description |
|-------|---------|-------------|
| `GetAll[Entity]Query` | `GetAll[Entity]QueryHandler` | [Description] |
| `Get[Entity]ByIdQuery` | `Get[Entity]ByIdQueryHandler` | [Description] |

#### Service Interfaces

```
Application/Interfaces/Services/I[Service]Service.cs
```

[Description of service interface methods]

### Infrastructure Layer

#### Database Entities

```
Infrastructure/Entities/[EntityName]Entity.cs
```

[Description of EF Core entity mapping]

#### Repository Implementation

```
Infrastructure/Repositories/[Entity]Repository.cs
```

[Description of repository methods]

#### Migrations

[Description of database migrations needed]

### Presentation Layer

#### API Endpoints

| Method | Route | Handler | Description |
|--------|-------|---------|-------------|
| GET | `/api/[entity]` | `GetAll[Entity]` | [Description] |
| GET | `/api/[entity]/{id}` | `Get[Entity]ById` | [Description] |
| POST | `/api/[entity]` | `Create[Entity]` | [Description] |
| PUT | `/api/[entity]/{id}` | `Update[Entity]` | [Description] |
| DELETE | `/api/[entity]/{id}` | `Delete[Entity]` | [Description] |

#### ViewModels

```
Shared/ViewModels/[EntityName]ViewModel.cs
```

[Description of ViewModel properties]

#### Validators

```
Shared/Validators/[EntityName]ViewModelValidator.cs
```

[Description of validation rules]

#### Client Components (if applicable)

[Description of Blazor components needed]

## Data Model

### Entity Relationships

[Describe relationships between entities]

### Database Schema Changes

```sql
-- Migration description
ALTER TABLE [table]...
```

## API Contracts

### Request/Response Examples

**POST /api/[entity]**

Request:
```json
{
  "field1": "value1",
  "field2": "value2"
}
```

Response (201 Created):
```json
{
  "id": "guid",
  "field1": "value1",
  "field2": "value2"
}
```

## AutoMapper Profiles

### Domain <-> Entity Mapping

```
Infrastructure/Mapping/[Entity]MappingProfile.cs
```

### Domain <-> ViewModel Mapping

```
Presentation/API/Mapping/[Entity]MappingProfile.cs
```

## Testing Strategy

### Unit Tests

| Test Class | Coverage |
|------------|----------|
| `[Entity]Tests` | Domain entity validation and equality |
| `Create[Entity]CommandHandlerTests` | Command handler behavior |
| `Get[Entity]QueryHandlerTests` | Query handler behavior |
| `[Entity]ViewModelValidatorTests` | ViewModel validation rules |

### Integration Tests

[Description of integration test scenarios]

### Test Data Generators

```
SampleData/Domain/Core/[Entity]Generator.cs
SampleData/Entities/[Entity]EntityGenerator.cs
SampleData/ViewModels/[Entity]ViewModelGenerator.cs
```

## SignalR Integration

[Description of real-time update notifications, if applicable]

## Security Considerations

- [Security consideration 1]
- [Security consideration 2]

## Performance Considerations

- [Performance consideration 1]
- [Performance consideration 2]

## Risks and Mitigations

| Risk | Impact | Mitigation |
|------|--------|------------|
| [Risk] | [Impact] | [Mitigation strategy] |

---

## Review Checklist

Before marking this plan as approved:

- [ ] All constitution articles are satisfied
- [ ] Layer boundaries are respected
- [ ] CQRS pattern is correctly applied
- [ ] Test strategy covers all new code
- [ ] API contracts are clearly defined
- [ ] AutoMapper profiles are identified
- [ ] No security vulnerabilities introduced
