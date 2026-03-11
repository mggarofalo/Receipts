# Receipts.AppHost

.NET Aspire orchestration project. Defines and coordinates all services for local development.

## What It Does

The AppHost configures and starts all services with a single F5 press:

- **API** — the ASP.NET Core Web API
- **PostgreSQL** — database container (provisioned automatically via Docker)
- **PgAdmin** — database admin UI
- **React dev server** — Vite dev server for the client SPA
- **Aspire Dashboard** — observability UI (logs, traces, metrics)

## Key File

- **`AppHost.cs`** — resource definitions and service wiring (database connection strings, service dependencies, health checks)

## Usage

```bash
# Via Aspire CLI
aspire run

# Via VS Code (recommended)
# Press F5 → select "Launch Aspire AppHost"
```

See [docs/development.md](../../docs/development.md) for the full local development guide.
