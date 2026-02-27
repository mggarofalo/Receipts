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
| Database | PostgreSQL + EF Core 10 |
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
  AppHost/             .NET Aspire orchestration
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
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (for local dev — Aspire provisions PostgreSQL as a container)
- [Node.js 18+](https://nodejs.org) (for OpenAPI tooling and React client)

## Getting Started

See **[docs/development.md](docs/development.md)** for the full local development guide including F5 debugging with Aspire, the Aspire Dashboard, and troubleshooting.

**Quick start:**

```bash
git clone https://github.com/mggarofalo/Receipts.git
cd Receipts
dotnet restore Receipts.slnx
npm install
# Then press F5 in VS Code
```

Aspire orchestrates the entire stack (API + PostgreSQL + React dev server + Dashboard) automatically. No manual database setup needed.

## Development

### Build

```bash
dotnet build Receipts.slnx
```

### Run Tests

```bash
# All tests
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

Native Git hooks (`.githooks/`) run a six-stage pipeline on every `git commit`:

1. **OpenAPI spec lint** - `npx spectral lint openapi/spec.yaml`
2. **Format check** - `dotnet format --verify-no-changes`
3. **Build** - `dotnet build -p:TreatWarningsAsErrors=true`
4. **DTO staleness check** - `git diff --exit-code -- src/Presentation/API/Generated/`
5. **Spec drift check** - `node scripts/check-drift.mjs`
6. **Test** - `dotnet test --no-build`

Hooks install automatically on `dotnet restore`. To skip temporarily:

```bash
git commit --no-verify -m "message"
```

## CI/CD

GitHub Actions runs three parallel jobs on every push to `main` and every PR:

| Job | What it does |
|---|---|
| **Build & Test** | `dotnet build` with warnings-as-errors, then `dotnet test` |
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

## API Endpoints

All endpoints are documented with OpenAPI metadata. Run the API and visit `/scalar` for the interactive API documentation.

### Core Resources

Each core resource supports CRUD, soft-delete/restore, and batch operations.

| Resource | Routes |
|---|---|
| **Accounts** | `GET\|PUT /api/accounts/{id}`, `GET\|POST\|DELETE /api/accounts`, `GET /api/accounts/deleted`, `POST /api/accounts/{id}/restore`, `POST\|PUT /api/accounts/batch` |
| **Categories** | `GET\|PUT /api/categories/{id}`, `GET\|POST\|DELETE /api/categories`, `GET /api/categories/deleted`, `POST /api/categories/{id}/restore`, `POST\|PUT /api/categories/batch`, `GET /api/categories/{categoryId}/subcategories` |
| **Subcategories** | `GET\|PUT /api/subcategories/{id}`, `GET\|POST\|DELETE /api/subcategories`, `GET /api/subcategories/deleted`, `POST /api/subcategories/{id}/restore`, `POST\|PUT /api/subcategories/batch` |
| **Item Templates** | `GET\|PUT /api/item-templates/{id}`, `GET\|POST\|DELETE /api/item-templates`, `GET /api/item-templates/deleted`, `POST /api/item-templates/{id}/restore` |
| **Receipts** | `GET\|PUT /api/receipts/{id}`, `GET\|POST\|DELETE /api/receipts`, `GET /api/receipts/deleted`, `POST /api/receipts/{id}/restore`, `POST\|PUT /api/receipts/batch` |
| **Receipt Items** | `GET\|POST\|PUT /api/receipt-items/{id}`, `GET\|DELETE /api/receipt-items`, `GET /api/receipt-items/deleted`, `POST /api/receipt-items/{id}/restore`, `GET /api/receipt-items/by-receipt-id/{receiptId}`, `POST\|PUT /api/receipt-items/{id}/batch` |
| **Transactions** | `GET /api/transactions/{id}`, `GET\|DELETE /api/transactions`, `GET /api/transactions/deleted`, `POST /api/transactions/{id}/restore`, `GET /api/transactions/by-receipt-id/{receiptId}`, `POST\|PUT /api/transactions/{receiptId}/{accountId}`, `POST\|PUT /api/transactions/{receiptId}/{accountId}/batch` |

### Aggregate Views

| Method | Route | Description |
|---|---|---|
| GET | `/api/receipts-with-items/by-receipt-id/{receiptId}` | Receipt + line items |
| GET | `/api/transaction-accounts/by-transaction-id/{transactionId}` | Transaction + account |
| GET | `/api/transaction-accounts/by-receipt-id/{receiptId}` | All transaction-accounts for a receipt |
| GET | `/api/trips/by-receipt-id/{receiptId}` | Full trip aggregate |

### Auth & Users

| Method | Route | Description |
|---|---|---|
| POST | `/api/auth/login` | Login with email and password |
| POST | `/api/auth/refresh` | Refresh access token |
| POST | `/api/auth/logout` | Logout and invalidate refresh token |
| POST | `/api/auth/change-password` | Change password |
| GET | `/api/auth/audit/me` | Current user's auth history |
| GET | `/api/auth/audit/recent` | Recent auth events (admin) |
| GET | `/api/auth/audit/failed` | Failed login attempts (admin) |
| GET\|POST | `/api/users` | List / create users |
| GET\|PUT\|DELETE | `/api/users/{userId}` | Get / update / deactivate user |
| POST | `/api/users/{userId}/reset-password` | Reset password (admin) |
| GET | `/api/users/{userId}/roles` | List roles |
| POST\|DELETE | `/api/users/{userId}/roles/{role}` | Assign / remove role |
| GET\|POST | `/api/apikeys` | List / create API keys |
| DELETE | `/api/apikeys/{id}` | Revoke API key |

### Audit & Trash

| Method | Route | Description |
|---|---|---|
| GET | `/api/audit/entity/{entityType}/{entityId}` | Audit history for an entity |
| GET | `/api/audit/recent` | Recent audit log entries |
| GET | `/api/audit/user/{userId}` | Audit logs by user |
| GET | `/api/audit/apikey/{apiKeyId}` | Audit logs by API key |
| POST | `/api/trash/purge` | Permanently delete all soft-deleted items |
| GET | `/api/health` | Health check |

## Branching Strategy

See **[docs/branching.md](docs/branching.md)** for the full two-tier branching model with milestone branches, epic parent branches, issue branches, and directory isolation for parallel work.

## License

Private repository. All rights reserved.
