# AGENTS.md

This file provides guidance to AI agents when working with code in this repository.

## Prerequisites

- **.NET 10 SDK** — build, test, and run the API
- **Node.js 18+** and **npm** — OpenAPI spec linting (`@stoplight/spectral-cli`) and semantic drift detection (`js-yaml`)
- **PostgreSQL** — runtime database (connection via environment variables)

After cloning, run `dotnet restore Receipts.slnx` then `npm install`.

## Workflow Rules

### Linear

All issue work is tracked in Linear. See **[docs/linear.md](docs/linear.md)** for workspace structure, milestone phases, priority semantics, label conventions, and the "what's next" decision process.

- Team: "Mggarofalo" (ID: `a4aff05d-41e6-45dc-b670-cdb485fef765`), Project: "Receipts"
- All issues assigned to a milestone (Phase 0-8) with at least one layer label (`backend`, `frontend`, `infra`, `docs`)
- Issues labeled `epic` are parent containers — skip and work their children

### Branching

Two-tier hierarchical model: milestone branches for CI/PR gating, issue branches for individual work. See **[docs/branching.md](docs/branching.md)** for full strategy, merge procedures, and directory isolation.

### Commit Convention

[Conventional Commits](https://www.conventionalcommits.org/) format: `<type>(<scope>): <description>`

| Types | `feat`, `fix`, `docs`, `refactor`, `test`, `chore` |
|-------|-----------------------------------------------------|
| Scopes | `api`, `client`, `domain`, `application`, `infrastructure`, `common`, `shared` |

### OpenAPI Spec-First

The canonical API contract lives in `openapi/spec.yaml` (OpenAPI 3.1.0). All API changes follow a spec-first workflow: edit the spec, lint, build (regenerates DTOs), check drift.

**Key files:** `openapi/spec.yaml` (canonical spec), `.spectral.yaml` (lint rules), `scripts/check-drift.mjs` (drift detection), `scripts/check-breaking.mjs` (breaking change detection, CI only)

**npm scripts:** `npm run lint:spec`, `npm run check:drift`, `npm run check:breaking -- origin/main`

## Build and Test Commands

```bash
dotnet build Receipts.slnx                                    # Build entire solution
dotnet test Receipts.slnx                                     # Run all tests
dotnet test tests/Application.Tests/Application.Tests.csproj  # Single project
dotnet test --filter "FullyQualifiedName~TestMethodName"       # Single test
dotnet run --project src/Presentation/API/API.csproj           # Run the API
dotnet ef migrations add MigrationName --project src/Infrastructure/Infrastructure.csproj --startup-project src/Presentation/API/API.csproj
```

## Pre-commit Hooks

Native Git hooks via `core.hooksPath`. Install automatically on `dotnet restore` (or `bash .githooks/setup.sh`).

**Pipeline (runs on every `git commit`):**
1. `npx spectral lint openapi/spec.yaml` — OpenAPI spec linting
2. `dotnet format --verify-no-changes` — code formatting check
3. `dotnet build -p:TreatWarningsAsErrors=true` — build (also generates `openapi/generated/API.json`)
4. `git diff --exit-code -- src/Presentation/API/Generated/` — DTO staleness check
5. `node scripts/check-drift.mjs` — semantic drift detection
6. `dotnet test --no-build` — run all tests

Skip with `git commit --no-verify -m "message"` (use sparingly).

## Architecture

.NET 10 Clean Architecture: Domain → Application → Infrastructure → Presentation. Uses CQRS with MediatR, Repository pattern, Mapperly for compile-time mapping, soft-delete with audit logging.

See **[docs/architecture.md](docs/architecture.md)** for full layer structure, key patterns, Mapperly code examples, and test project layout.

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
- Generated DTOs live in `src/Presentation/API/Generated/Dtos.g.cs` (namespace `API.Generated.Dtos`)
- Naming: `CreateXRequest`, `UpdateXRequest`, `XResponse`

## Agent Workflow Rules

### Tests and Code Review

**Never write tests or perform code review in the main conversation context.** Always spawn subagents for these tasks:
- Use the `test-runner` or equivalent subagent for running and writing tests
- Use `pr-review-toolkit:code-reviewer` or similar review agents for code review
- This keeps the main context focused on implementation and prevents context window bloat

### EF Core Query Guidelines

Prefer **narrow projections** — always select the minimum required fields:
- Use `.Select()` to project only the columns/properties needed by the caller
- Use `.IgnoreAutoIncludes()` when navigation properties aren't needed for the query
- Avoid loading full entities when only a subset of fields is required
- For delete/update-only operations, prefer `ExecuteUpdateAsync`/`ExecuteDeleteAsync` over materializing entities
- Eliminate N+1 query patterns — use joined queries or batch operations instead of loops

### API Endpoint Return Types

Use `TypedResults` with concrete `Results<T1, T2, ...>` union return types on all endpoints (see MGG-227). This provides compile-time enforcement of response types and eliminates the need for `[ProducesResponseType]` attributes.

### Authentication Standards

Token-based authentication must conform to these RFCs:
- **RFC 6749** — OAuth 2.0 Authorization Framework: token issuance, response format, error codes
- **RFC 7662** — OAuth 2.0 Token Introspection: token validation endpoint semantics
- **RFC 7009** — OAuth 2.0 Token Revocation: revocation endpoint behavior and response codes

## Validation and Code Quality

### LSP Server Checks

Always check the LSP for warnings when validating code changes:

- **Mapperly nullable mapping warnings**: Indicate design mismatches that can cause runtime NullReferenceExceptions — do not ignore
- **Unused usings, variables, or parameters**: Often indicate incomplete refactoring
- **Potential null reference warnings**: Important with nullable reference types enabled

**Workflow:** Build → Test → Check LSP diagnostics → Address warnings → Create follow-up issues for non-trivial fixes.
