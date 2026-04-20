[![.NET CI](https://github.com/mggarofalo/Receipts/actions/workflows/github-ci.yml/badge.svg)](https://github.com/mggarofalo/Receipts/actions/workflows/github-ci.yml)

# Receipts

A full-stack receipt management application built with .NET 10 Clean Architecture (API) and React (SPA). Tracks accounts, receipts, receipt line items, transactions, categories, and item templates with full CRUD operations, soft-delete/restore, audit logging, and aggregate views.

## Tech Stack

| Layer | Technology |
|---|---|
| Runtime | .NET 10 |
| API | ASP.NET Core Web API |
| Frontend | React 19, Vite, TypeScript |
| UI | Tailwind CSS 4, shadcn/ui, Radix UI |
| State & Routing | TanStack Query, React Router |
| Forms | React Hook Form, Zod |
| Database | PostgreSQL + EF Core 10 + pgvector |
| Embeddings | bge-large-en-v1.5 via ONNX Runtime (local, no API key, 1024-dim, CLS pooling) |
| CQRS | MediatR 14 |
| Mapping | Mapperly (compile-time, zero-reflection) |
| Validation | FluentValidation |
| Auth | JWT Bearer + API Key (dual scheme) |
| Logging | Serilog + OpenTelemetry |
| Real-time | SignalR |
| API Docs | Microsoft.AspNetCore.OpenApi + Scalar |
| Local Dev | .NET Aspire (orchestration + observability) |
| Testing | xUnit + FluentAssertions + Moq (API), Vitest + Playwright (client) |

## Architecture

The solution follows Clean Architecture with strict dependency flow: Domain has no outward dependencies, Application depends only on Domain, and outer layers (Infrastructure, Presentation) depend inward.

```
src/
  Common/              Shared utilities, enums, extension methods
  Domain/
    Core/              Entities: Account, Receipt, ReceiptItem, Transaction,
                       Category, Subcategory, ItemTemplate
    Aggregates/        Composites: ReceiptWithItems, TransactionAccount, Trip
  Application/
    Commands/          Write operations (Create, Update, Delete per entity)
    Queries/           Read operations (Core + Aggregate queries)
    Interfaces/        Service contracts implemented by Infrastructure
  Infrastructure/
    Entities/          EF Core database entities (separate from Domain)
    Repositories/      Repository pattern over EF Core
    Services/          Service implementations (incl. audit logging)
    Mapping/           Mapperly mappers (Domain <-> Entity)
    Migrations/        EF Core migrations
  Presentation/
    API/
      Controllers/     REST endpoints (Core/ and Aggregates/)
      Configuration/   Service registration extensions
      Mapping/         Mapperly mappers (Domain <-> generated DTOs)
      Generated/       NSwag-generated Request/Response DTOs from OpenAPI spec
      Validators/      FluentValidation validators (business rules only)
      Hubs/            SignalR hub for real-time updates
  client/              React/Vite SPA (TypeScript)
  Receipts.AppHost/    .NET Aspire orchestration
tests/
  Common.Tests/
  Domain.Tests/
  Application.Tests/
  Infrastructure.Tests/
  Presentation.API.Tests/
  SampleData/          Shared test fixtures across all test projects
```

### Key Patterns

- **CQRS**: Commands and queries are separate types with dedicated MediatR handlers
- **Repository Pattern**: Infrastructure repositories abstract EF Core from the Application layer
- **Compile-time Mapping**: Mapperly generates mapping code at build time (no reflection, fully debuggable)
- **Service Registration**: Each layer exposes a static DI extension method (`RegisterApplicationServices`, `RegisterInfrastructureServices`, etc.)
- **Central Package Management**: All NuGet versions defined in `Directory.Packages.props`
- **Soft Delete**: Entities support soft delete with restore capabilities and trash purge
- **Audit Logging**: All mutations are logged with user/API key attribution

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Aspire CLI](https://aspire.dev/get-started/install-cli/) — `dotnet tool install --global Aspire.Cli`
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (for local dev — Aspire provisions PostgreSQL as a container)
- [Node.js 18+](https://nodejs.org) (for OpenAPI tooling and React client)

## Getting Started

See **[docs/development.md](docs/development.md)** for the full local development guide including F5 debugging with Aspire, the Aspire Dashboard, and troubleshooting.

**Quick start:**

```bash
git clone https://github.com/mggarofalo/Receipts.git
cd Receipts
dotnet tool install --global Aspire.Cli  # if not already installed
dotnet restore Receipts.slnx
npm install
# Then press F5 in VS Code, or: aspire run
```

Aspire orchestrates the entire stack (API + PostgreSQL + React dev server + Dashboard) automatically. No manual database setup needed.

## Docker Setup

The repo includes a self-contained `docker-compose.yml` — no `.env` file or secret generation needed. Secrets are auto-generated on first run.

```bash
# Start the stack (secrets are generated automatically)
docker compose up -d

# Get the auto-generated admin password
docker compose exec app cat /secrets/admin_password

# View logs
docker compose logs -f app

# Stop
docker compose down
```

The app is available at `http://localhost:8080`. To customize PUID/PGID, timezone, or admin email, edit the values directly in `docker-compose.yml`. See **[docs/deployment.md](docs/deployment.md)** for the full deployment guide including HTTPS, backup/restore, updates, and troubleshooting.

## Development

### Build

```bash
dotnet build Receipts.slnx
```

### Run Tests

```bash
# Unit tests only (same as CI)
dotnet test Receipts.slnx --filter "Category!=Integration"

# All tests including integration (requires ONNX model)
dotnet test Receipts.slnx

# Single project
dotnet test tests/Application.Tests/Application.Tests.csproj

# Single test by name
dotnet test --filter "FullyQualifiedName~TestMethodName"
```

### Code Formatting

The project enforces consistent formatting via `dotnet format`. Check without modifying:

```bash
dotnet format Receipts.slnx --verify-no-changes
```

Fix formatting:

```bash
dotnet format Receipts.slnx
```

### EF Core Migrations

```bash
dotnet ef migrations add MigrationName \
  --project src/Infrastructure/Infrastructure.csproj \
  --startup-project src/Presentation/API/API.csproj
```

### OpenAPI Spec

The OpenAPI spec is generated at build time to `openapi/generated/API.json`. This file is gitignored (build artifact) but can be used for client generation or documentation.

## Pre-commit Hooks

Native Git hooks (`.githooks/`) run an eight-step pipeline on every `git commit`:

0. **Prerequisites** - `dotnet run scripts/worktree-setup.cs -- --check`
1. **OpenAPI spec lint** - `npx spectral lint openapi/spec.yaml`
2. **Format check** - `dotnet format --verify-no-changes`
3. **Build** - `dotnet build -p:TreatWarningsAsErrors=true` (also regenerates DTOs and `openapi/generated/API.json`)
4. **Spec drift check** - `node scripts/check-drift.mjs`
5. **Test** - `dotnet test --no-build --filter "Category!=Integration"`
6. **TypeScript types** - `npx tsc --noEmit`
7. **ESLint** - `npx eslint src/client/src`

Hooks install automatically on `dotnet restore`. For faster iteration, use quick mode (runs only steps 0, 2, 6, 7):

```bash
PRECOMMIT_QUICK=1 git commit -m "message"
```

## CI/CD

GitHub Actions runs three parallel jobs on every push to `main` and every PR:

| Job | What it does |
|---|---|
| **Build & Test** | `dotnet build` with warnings-as-errors, then `dotnet test` (unit tests only; integration tests excluded) |
| **Code Formatting** | `dotnet format --verify-no-changes` |
| **Vulnerability Scan** | `dotnet list package --vulnerable` with fail-on-detection |

NuGet packages are cached between runs. See `.github/workflows/github-ci.yml` for details.

## Testing Strategy

Tests mirror the source project structure with one test project per source layer. All test projects share the same stack:

| Package | Purpose |
|---|---|
| xUnit | Test framework |
| FluentAssertions | Readable assertion syntax |
| Moq | Mocking dependencies |
| coverlet.collector | Code coverage |

### Conventions

- **Arrange/Act/Assert** structure in every test
- **`expected`/`actual`** variable names for assertion clarity
- **Concrete Mapperly instances** in tests (not mocked) - tests exercise real mapping logic
- **In-memory EF Core** for Infrastructure tests (`Microsoft.EntityFrameworkCore.InMemory`)
- **SampleData project** provides shared test fixtures so entity construction is consistent across all test projects
- **No testing of implementation details** - tests verify behavior, not internal mechanics
- **Integration tests** tagged with `[Trait("Category", "Integration")]` for tests requiring external resources (ONNX model, real database). Excluded from CI via `--filter "Category!=Integration"`

## API Endpoints

The API provides full CRUD with soft-delete/restore and batch operations for 7 core resources (Accounts, Categories, Subcategories, Item Templates, Receipts, Receipt Items, Transactions), aggregate views, auth/user management, and audit logging.

All endpoints are documented with OpenAPI metadata. Run the API and visit `/scalar` for the interactive API documentation.

See **[docs/api-reference.md](docs/api-reference.md)** for the complete endpoint reference.

## Branching Strategy

See **[docs/branching.md](docs/branching.md)** for the full two-tier branching model with milestone branches, epic parent branches, issue branches, and directory isolation for parallel work.

## License

Private repository. All rights reserved.
