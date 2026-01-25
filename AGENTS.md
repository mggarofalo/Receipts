# AGENTS.md

This file provides guidance to AI agents when working with code in this repository.

## Development Workflow

When working on tasks that are expected to result in code changes, follow this standard process:

1. **Linear Issue Management**
   - Check if a Linear issue exists for the work
   - If no issue exists, create one with:
     - Clear title describing the work
     - Description with acceptance criteria
     - Appropriate labels and team assignment
   - Link the issue ID to your work

2. **Branch-Based Development**
   - Create a feature branch from `master` using the Linear issue identifier
   - Branch naming convention: Use the suggested git branch name from Linear (usually `{team-key}-{issue-number}-{slug}`)
   - Example: `REC-123-add-receipt-export`

3. **Pull Request Process**
   - When work in a branch is complete, create a PR against `master`
   - PR title should reference the Linear issue (e.g., "REC-123: Add receipt export endpoint")
   - Include a summary of changes and link to the Linear issue
   - Ensure all tests pass and the build succeeds before requesting review

4. **Direct Commits to Master**
   - Only use for trivial changes like typo fixes or documentation updates
   - When in doubt, create a branch and PR

## Build and Test Commands

```bash
# Build entire solution
dotnet build Receipts.sln

# Run all tests
dotnet test Receipts.sln

# Run tests for a specific project
dotnet test tests/Application.Tests/Application.Tests.csproj

# Run a single test by name
dotnet test --filter "FullyQualifiedName~TestMethodName"

# Run the API
dotnet run --project src/Presentation/API/API.csproj

# Add EF Core migration (run from Infrastructure project)
dotnet ef migrations add MigrationName --project src/Infrastructure/Infrastructure.csproj --startup-project src/Presentation/API/API.csproj
```

## Architecture

This is a .NET 10 Clean Architecture solution for a receipt management application. It uses central package management via `Directory.Packages.props`.

### Layer Structure

- **Common** - Shared utilities, extension methods, and configuration variable constants
- **Domain** - Core domain models with no dependencies on other layers
  - `Core/` - Entity classes (Account, Receipt, ReceiptItem, Transaction)
  - `Aggregates/` - Composite domain objects (ReceiptWithItems, TransactionAccount, Trip)
- **Application** - Business logic using CQRS pattern with MediatR
  - `Commands/{Entity}/Create|Update|Delete/` - Command + Handler pairs for write operations
  - `Queries/Core/{Entity}/` - Query + Handler pairs for read operations
  - `Queries/Aggregates/` - Complex queries joining multiple entities
  - `Interfaces/Services/` - Service interfaces implemented by Infrastructure
- **Infrastructure** - Data access with PostgreSQL via EF Core
  - `Entities/` - Database entity classes (separate from Domain)
  - `Repositories/` - Repository pattern implementation
  - `Services/` - Service implementations
  - `Mapping/` - AutoMapper profiles (Domain <-> Entity)
- **Presentation**
  - **API** - ASP.NET Core Web API with SignalR hub for real-time updates
    - `Controllers/Core/` and `Controllers/Aggregates/` - REST endpoints
    - `Mapping/` - AutoMapper profiles (Domain <-> ViewModel)
    - `Configuration/` - Service registration extension methods
    - `Hubs/ReceiptsHub.cs` - SignalR hub
  - **Client** - Blazor WebAssembly frontend using MudBlazor
  - **Shared** - ViewModels and HTTP clients shared between API and Client
    - `ViewModels/` - DTOs for API communication
    - `HttpClientApiExtensions/` - Typed HTTP clients
    - `Validators/` - FluentValidation validators

### Key Patterns

- **CQRS**: Commands and Queries are separate with dedicated handlers
- **Mediator Pattern**: MediatR dispatches commands/queries to handlers
- **Repository Pattern**: Infrastructure repositories abstract EF Core
- **Mapping**: AutoMapper handles Domain <-> Entity (Infrastructure) and Domain <-> ViewModel (API)
- **Service Registration**: Each layer has a static extension method (`RegisterApplicationServices`, `RegisterInfrastructureServices`) for DI setup

### Database

PostgreSQL with EF Core. Connection configured via environment variables:
- `POSTGRES_HOST`, `POSTGRES_PORT`, `POSTGRES_USER`, `POSTGRES_PASSWORD`, `POSTGRES_DB`

Migrations run automatically on API startup via `IDatabaseMigratorService`.

## C# Coding Standards

- Use explicit type declarations instead of `var` (exception: when type is obvious and enhances readability)
- Use primary constructors where appropriate
- Use `new(..)` target-typed syntax: `DatabaseMigratorService service = new(...);`
- Unit tests: xUnit with Arrange/Act/Assert structure
- Use `expected` and `actual` as variable names in test assertions
- Avoid testing implementation details

## Test Project Structure

Tests mirror src structure. `SampleData` project provides shared test fixtures across test projects.

## Commit Message Convention

Use [Conventional Commits](https://www.conventionalcommits.org/) format:

```
<type>(<scope>): <description>

[optional body]
```

**Types:**
- `feat` - New feature
- `fix` - Bug fix
- `docs` - Documentation only
- `refactor` - Code change that neither fixes a bug nor adds a feature
- `test` - Adding or updating tests
- `chore` - Maintenance tasks, dependencies, build config

**Scopes** (optional): `api`, `client`, `domain`, `application`, `infrastructure`, `common`, `shared`

**Examples:**
- `feat(api): add receipt export endpoint`
- `fix(client): correct date picker timezone handling`
- `refactor(infrastructure): simplify repository base class`
- `docs: update agent guidance documentation`

## Spec-Driven Development (SpecKit)

This project uses [GitHub Spec Kit](https://github.com/github/spec-kit) for specification-driven development. The `.specify/` directory contains:

### Directory Structure

```
.specify/
├── memory/
│   └── constitution.md      # Non-negotiable project principles
├── specs/
│   └── [FEATURE-NUMBER]/    # Feature-specific artifacts
│       ├── spec.md          # Functional specification
│       ├── plan.md          # Technical implementation plan
│       └── tasks.md         # Ordered task breakdown
└── templates/
    ├── spec-template.md     # Specification template
    ├── plan-template.md     # Technical plan template
    ├── tasks-template.md    # Task breakdown template
    └── CLAUDE-template.md   # Claude Code context template
```

### Constitution

The constitution (`.specify/memory/constitution.md`) defines mandatory principles:

1. **Clean Architecture Compliance** - Strict layer separation
2. **CQRS Pattern Enforcement** - Commands and Queries via MediatR
3. **Test-First Development** - Tests before implementation
4. **Explicit Type Declarations** - Avoid `var` except for obvious types
5. **Domain Separation** - Separate Domain, Entity, and ViewModel
6. **Database Access Patterns** - Repository pattern with EF Core
7. **API Design Standards** - REST conventions with FluentValidation
8. **Simplicity Over Cleverness** - No over-engineering
9. **Security and Configuration** - No hardcoded secrets
10. **Conventional Commits** - Standardized commit messages

### Workflow Phases

1. **Specify** - Define functional requirements using `spec-template.md`
2. **Plan** - Create technical plan using `plan-template.md`
3. **Tasks** - Break down into ordered tasks using `tasks-template.md`
4. **Implement** - Execute tasks following test-first principles

### Creating a New Feature

1. Create a new directory: `.specify/specs/[FEATURE-NUMBER]/`
2. Copy templates and fill in feature-specific content
3. Review specification against constitution
4. Implement following the task breakdown
