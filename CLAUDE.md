# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

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

This is a .NET 9 Clean Architecture solution for a receipt management application. It uses central package management via `Directory.Packages.props`.

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
- `docs: add CLAUDE.md for AI assistant guidance`
