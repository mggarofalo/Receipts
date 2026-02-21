[![.NET CI](https://github.com/mggarofalo/Receipts/actions/workflows/github-ci.yml/badge.svg)](https://github.com/mggarofalo/Receipts/actions/workflows/github-ci.yml)

# Receipts

A receipt management API built with .NET 10 and Clean Architecture. Tracks accounts, receipts, receipt line items, and transactions with full CRUD operations and aggregate views.

## Tech Stack

| Layer | Technology |
|---|---|
| Runtime | .NET 10 |
| API | ASP.NET Core Web API |
| Database | PostgreSQL + EF Core 10 |
| CQRS | MediatR 14 |
| Mapping | Mapperly (compile-time, zero-reflection) |
| Validation | FluentValidation |
| Logging | Serilog + OpenTelemetry |
| Real-time | SignalR |
| API Docs | Microsoft.AspNetCore.OpenApi + Scalar |
| Local Dev | .NET Aspire (orchestration + observability) |
| Testing | xUnit + FluentAssertions + Moq |

## Architecture

The solution follows Clean Architecture with strict dependency flow: Domain has no outward dependencies, Application depends only on Domain, and outer layers (Infrastructure, Presentation) depend inward.

```
src/
  Common/              Shared utilities, enums, extension methods
  Domain/
    Core/              Entities: Account, Receipt, ReceiptItem, Transaction
    Aggregates/        Composites: ReceiptWithItems, TransactionAccount, Trip
  Application/
    Commands/          Write operations (Create, Update, Delete per entity)
    Queries/           Read operations (Core + Aggregate queries)
    Interfaces/        Service contracts implemented by Infrastructure
  Infrastructure/
    Entities/          EF Core database entities (separate from Domain)
    Repositories/      Repository pattern over EF Core
    Services/          Service implementations
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

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (for local dev — Aspire provisions PostgreSQL as a container)
- [Node.js 18+](https://nodejs.org) (for OpenAPI tooling)

## Getting Started

See **[DEVELOPMENT.md](DEVELOPMENT.md)** for the full local development guide including F5 debugging with Aspire, the Aspire Dashboard, and troubleshooting.

**Quick start:**

```bash
git clone https://github.com/mggarofalo/Receipts.git
cd Receipts
dotnet restore Receipts.slnx
npm install
# Then press F5 in VS Code
```

Aspire orchestrates the entire stack (API + PostgreSQL + Dashboard) automatically. No manual database setup needed.

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

GitHub Actions runs three parallel jobs on every push to `master` and every PR:

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

All endpoints are documented with OpenAPI metadata (`[EndpointSummary]` / `[EndpointDescription]`). Run the API and visit `/swagger` for the interactive spec.

### Core Resources

| Method | Route | Description |
|---|---|---|
| GET | `/api/Accounts/{id}` | Get account by ID |
| GET | `/api/Accounts` | List all accounts |
| POST | `/api/Accounts` | Create accounts (batch) |
| PUT | `/api/Accounts` | Update accounts (batch) |
| DELETE | `/api/Accounts` | Delete accounts (batch) |
| GET | `/api/Receipts/{id}` | Get receipt by ID |
| GET | `/api/Receipts` | List all receipts |
| POST | `/api/Receipts` | Create receipts (batch) |
| PUT | `/api/Receipts` | Update receipts (batch) |
| DELETE | `/api/Receipts` | Delete receipts (batch) |
| GET | `/api/ReceiptItems/{id}` | Get receipt item by ID |
| GET | `/api/ReceiptItems` | List all receipt items |
| GET | `/api/ReceiptItems/by-receipt-id/{receiptId}` | Get items for a receipt |
| POST | `/api/ReceiptItems/{receiptId}` | Create items under a receipt |
| PUT | `/api/ReceiptItems/{receiptId}` | Update items under a receipt |
| DELETE | `/api/ReceiptItems` | Delete receipt items (batch) |
| GET | `/api/Transactions/{id}` | Get transaction by ID |
| GET | `/api/Transactions` | List all transactions |
| GET | `/api/Transactions/by-receipt-id/{receiptId}` | Get transactions for a receipt |
| POST | `/api/Transactions/{receiptId}/{accountId}` | Create transactions |
| PUT | `/api/Transactions/{receiptId}/{accountId}` | Update transactions |
| DELETE | `/api/Transactions` | Delete transactions (batch) |

### Aggregate Views

| Method | Route | Description |
|---|---|---|
| GET | `/api/ReceiptWithItems/by-receipt-id/{receiptId}` | Receipt + line items |
| GET | `/api/TransactionAccount/by-transaction-id/{transactionId}` | Transaction + account |
| GET | `/api/TransactionAccount/by-receipt-id/{receiptId}` | All transaction-accounts for a receipt |
| GET | `/api/Trip/by-receipt-id/{receiptId}` | Full trip aggregate |

### Other

| Method | Route | Description |
|---|---|---|
| GET | `/api/Health` | Health check |

## Branching Strategy

This project uses a two-tier branching model:

- **Milestone branches** (`milestone/phase-N`) collect issue work for a phase, then PR into `master` with CI gating
- **Issue branches** branch off the milestone branch and squash-merge locally back into it
- The `master` branch is always deployable
- Git worktrees (`.worktrees/`) are used for all branch work to keep the main repo on `master`

## License

Private repository. All rights reserved.
