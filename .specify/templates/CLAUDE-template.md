# Claude Code Context for Feature: [FEATURE-NUMBER]

This file provides context-specific instructions for Claude Code when working on feature [FEATURE-NUMBER].

## Feature Overview

[Brief description of the feature being implemented]

## Active Specification Documents

- **Specification**: `.specify/specs/[FEATURE-NUMBER]/spec.md`
- **Technical Plan**: `.specify/specs/[FEATURE-NUMBER]/plan.md`
- **Tasks**: `.specify/specs/[FEATURE-NUMBER]/tasks.md`

## Constitution Compliance

Before making any changes, ensure compliance with `.specify/memory/constitution.md`. Key articles to remember:

1. **Clean Architecture**: Dependencies flow inward only
2. **CQRS**: Use MediatR for all Commands and Queries
3. **Test-First**: Write tests before implementation
4. **Explicit Types**: Avoid `var` except for obvious types
5. **Domain Separation**: Keep Domain, Entity, and ViewModel separate

## Current Task Context

**Current Phase**: [Phase name from tasks.md]
**Current Task**: [Task number and description]
**Blocked By**: [Any blocking tasks]
**Can Parallelize**: [Yes/No - which tasks]

## Implementation Guidelines

### When Creating Domain Entities

```csharp
// Location: src/Domain/Core/[Entity].cs
public class EntityName
{
    public Guid Id { get; init; }
    public string Name { get; init; }

    public EntityName(Guid id, string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Id = id;
        Name = name;
    }
}
```

### When Creating Command Handlers

```csharp
// Location: src/Application/Commands/[Entity]/Create/
public record CreateEntityCommand(string Name) : IRequest<Entity>;

public class CreateEntityCommandHandler(IEntityService service)
    : IRequestHandler<CreateEntityCommand, Entity>
{
    public async Task<Entity> Handle(CreateEntityCommand request, CancellationToken cancellationToken)
    {
        return await service.CreateAsync(request.Name, cancellationToken);
    }
}
```

### When Creating Query Handlers

```csharp
// Location: src/Application/Queries/Core/[Entity]/
public record GetAllEntitiesQuery : IRequest<List<Entity>>;

public class GetAllEntitiesQueryHandler(IEntityService service)
    : IRequestHandler<GetAllEntitiesQuery, List<Entity>>
{
    public async Task<List<Entity>> Handle(GetAllEntitiesQuery request, CancellationToken cancellationToken)
    {
        return await service.GetAllAsync(cancellationToken);
    }
}
```

### When Creating Tests

```csharp
// Use Arrange/Act/Assert pattern
[Fact]
public async Task Handle_ShouldReturnAllEntities()
{
    // Arrange
    List<Entity> expected = EntityGenerator.GenerateList(3);
    Mock<IEntityService> mockService = new();
    mockService.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
        .ReturnsAsync(expected);

    GetAllEntitiesQueryHandler handler = new(mockService.Object);
    GetAllEntitiesQuery query = new();

    // Act
    List<Entity> actual = await handler.Handle(query, CancellationToken.None);

    // Assert
    Assert.Equal(expected.Count, actual.Count);
}
```

## Files Modified/Created in This Feature

Track files as you work on them:

### Created
- [ ] `src/Domain/Core/[Entity].cs`
- [ ] `src/Application/Commands/[Entity]/...`
- [ ] `src/Application/Queries/Core/[Entity]/...`
- [ ] `tests/Domain.Tests/Core/[Entity]Tests.cs`
- [ ] [Add more as needed]

### Modified
- [ ] `src/Infrastructure/InfrastructureServiceRegistration.cs`
- [ ] [Add more as needed]

## Build and Test Commands

```bash
# Build
dotnet build Receipts.sln

# Run all tests
dotnet test Receipts.sln

# Run specific test project
dotnet test tests/Application.Tests/Application.Tests.csproj

# Run tests matching pattern
dotnet test --filter "FullyQualifiedName~[Entity]"
```

## Commit Guidance

Follow Conventional Commits format:

```
feat(domain): add [Entity] domain model
test(domain): add [Entity] validation tests
feat(application): add [Entity] CQRS handlers
feat(infrastructure): add [Entity] repository
feat(api): add [Entity] REST endpoints
```

## Notes

[Add any feature-specific notes, decisions, or clarifications here]
