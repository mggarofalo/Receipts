# Development Guide

Local development uses **.NET Aspire** to orchestrate all services — API, PostgreSQL, and PgAdmin — with a single F5 press in VS Code.

## Prerequisites

| Tool | Version | Purpose |
|------|---------|---------|
| [.NET 10 SDK](https://dot.net) | 10.0+ | Build and run the API |
| [Docker Desktop](https://www.docker.com/products/docker-desktop/) | Any | PostgreSQL container (Aspire manages it) |
| [Node.js](https://nodejs.org) | 18+ | OpenAPI spec linting and drift detection |
| [VS Code](https://code.visualstudio.com) | Any | Recommended IDE |
| [C# Dev Kit](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csdevkit) | Any | VS Code C# support |

> **Docker is required** — Aspire provisions PostgreSQL as a container automatically. You don't need to manage the database manually.

## Initial Setup

```bash
# Clone the repository
git clone https://github.com/mggarofalo/Receipts.git
cd Receipts

# Restore .NET packages and install tools (also installs Husky pre-commit hooks)
dotnet restore Receipts.slnx

# Install Node dependencies (OpenAPI linting tools)
npm install
```

## F5 Debugging (Recommended)

1. Open the repository root in VS Code
2. Press **F5** (or **Run → Start Debugging**)
3. Select **"Launch Aspire AppHost"** if prompted
4. Wait ~30 seconds for all services to start

VS Code will automatically open the **Aspire Dashboard** in your browser.

### What Starts

| Service | URL | Description |
|---------|-----|-------------|
| Aspire Dashboard | http://localhost:15888 | Observability — logs, traces, metrics |
| API (HTTP) | http://localhost:5000 | REST API |
| API (HTTPS) | https://localhost:5001 | REST API (HTTPS) |
| API Docs (Scalar) | http://localhost:5000/scalar | Interactive API documentation |
| PgAdmin | auto-assigned | PostgreSQL admin UI |

### Debug Configurations

- **Launch Aspire AppHost** — Starts the entire stack (API + DB + Dashboard)
- **Attach to API** — Attach debugger to a running API process for breakpoints
- **Debug All (AppHost + API)** — Launch AppHost and immediately attach the debugger to the API

## Aspire Dashboard

The dashboard provides full observability without any external tooling:

| View | What You See |
|------|-------------|
| **Resources** | All running services with health status |
| **Traces** | Distributed request traces across API → DB |
| **Metrics** | Request rates, response times, runtime stats |
| **Logs** | Structured logs from all services, searchable |
| **Console** | Raw stdout/stderr from each service |

### Database Tracing

EF Core queries are automatically traced and visible in the Traces view. Each HTTP request shows the full span tree including SQL statements executed against PostgreSQL.

## Running Without Aspire

If you prefer to run the API directly (without Docker/Aspire):

```bash
# Set database environment variables
export POSTGRES_HOST=localhost
export POSTGRES_PORT=5432
export POSTGRES_USER=postgres
export POSTGRES_PASSWORD=yourpassword
export POSTGRES_DB=receiptsdb

# Run the API
dotnet run --project src/Presentation/API/API.csproj
```

EF Core migrations run automatically on startup when the database is configured.

## Build and Test

```bash
# Build entire solution
dotnet build Receipts.slnx

# Run all tests
dotnet test Receipts.slnx

# Run tests for a specific project
dotnet test tests/Application.Tests/Application.Tests.csproj
```

## Pre-commit Hooks

Every `git commit` runs the full quality pipeline automatically:

1. **OpenAPI spec lint** — `npx spectral lint openapi/spec.yaml`
2. **Code format check** — `dotnet format --verify-no-changes`
3. **Build with warnings-as-errors** — also regenerates `openapi/generated/API.json`
4. **Semantic drift check** — compares spec vs generated output for structural differences
5. **Tests** — `dotnet test --no-build`

To skip hooks temporarily (use sparingly):
```bash
git commit --no-verify -m "message"
```

## OpenAPI Spec-First Workflow

All API changes follow a spec-first workflow:

1. Edit `openapi/spec.yaml` — this is the single source of truth
2. `npm run lint:spec` — validate the spec
3. `dotnet build` — regenerates DTOs and the built output
4. `npm run check:drift` — verify spec and implementation stay in sync

See [AGENTS.md](AGENTS.md) for the full workflow details.

## Troubleshooting

### Port conflicts
If ports 5000/5001 are in use, Aspire will pick alternative ports. Check the Dashboard Resources view for the actual URLs.

### Docker not running
Aspire requires Docker to provision the PostgreSQL container. Start Docker Desktop before pressing F5.

### Database connection issues
The API waits for PostgreSQL to be healthy before starting (`.WaitFor(db)` in AppHost). If the API starts before the database is ready, Aspire restarts it automatically.

### Pre-commit hook failures
- **Spec lint fails** — fix the OpenAPI spec error reported by Spectral
- **Format fails** — run `dotnet format Receipts.slnx` to auto-fix
- **Drift check fails** — the spec and generated API are out of sync; update the spec or the implementation to match
- **Tests fail** — fix the failing tests before committing
