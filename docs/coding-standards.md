# Coding Standards

## C# Coding Standards

- Use explicit type declarations instead of `var` (exception: when type is obvious and enhances readability)
- Use primary constructors where appropriate
- Use `new(..)` target-typed syntax: `DatabaseMigratorService service = new(...);`
- Unit tests: xUnit with Arrange/Act/Assert structure
- Use `expected` and `actual` as variable names in test assertions
- Avoid testing implementation details
- **Moq `It.IsAny<T>()` rule:** Only use `It.IsAny<T>()` when the value genuinely doesn't matter for the test's assertion (e.g., `CancellationToken`). For domain-meaningful parameters like IDs, entity names, or any value the system under test is expected to pass through correctly, use specific values. This catches argument-ordering bugs where two parameters share the same type (e.g., `receiptId` and `accountId` are both `Guid`).

## Mapperly Rules

- Use Mapperly, not AutoMapper
- Use concrete mapper instances in tests — never mock mappers
- Don't use `[UseMapper(typeof(...))]` — instantiate mapper dependencies as fields and call explicitly
- Use `[MapperIgnoreTarget(nameof(XResponse.AdditionalProperties))]` on generated DTO mappings
- For aggregates with value objects: create manual mapping methods that delegate to Core mappers
- Generated DTOs live in `src/Presentation/API/Generated/` (namespace `API.Generated.Dtos`) — these files are gitignored and regenerated from `openapi/spec.yaml` on every `dotnet build`
- Naming: `CreateXRequest`, `UpdateXRequest`, `XResponse`

## EF Core Query Guidelines

Prefer **narrow projections** — always select the minimum required fields:
- Use `.Select()` to project only the columns/properties needed by the caller
- Use `.IgnoreAutoIncludes()` when navigation properties aren't needed for the query
- Avoid loading full entities when only a subset of fields is required
- For delete/update-only operations, prefer `ExecuteUpdateAsync`/`ExecuteDeleteAsync` over materializing entities
- Eliminate N+1 query patterns — use joined queries or batch operations instead of loops

## Validation and Code Quality

### LSP Server Checks

Always check the LSP for warnings when validating code changes:

- **Mapperly nullable mapping warnings**: Indicate design mismatches that can cause runtime NullReferenceExceptions — do not ignore
- **Unused usings, variables, or parameters**: Often indicate incomplete refactoring
- **Potential null reference warnings**: Important with nullable reference types enabled

**Workflow:** Build → Test → Check LSP diagnostics → Address warnings → Create follow-up issues for non-trivial fixes.
