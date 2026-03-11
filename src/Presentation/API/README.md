# API

ASP.NET Core Web API — the HTTP boundary of the application.

## Structure

- **`Controllers/Core/`** — REST endpoints for individual entities (CRUD + soft-delete/restore + batch)
- **`Controllers/Aggregates/`** — REST endpoints for composite views (ReceiptWithItems, TransactionAccount, Trip)
- **`Mapping/`** — Mapperly mappers (Domain <-> generated DTOs)
- **`Generated/`** — NSwag-generated Request/Response DTOs from `openapi/spec.yaml` (gitignored, regenerated on build)
- **`Validators/`** — FluentValidation validators for business rules
- **`Filters/`** — Action filters (e.g., `FluentValidationActionFilter`)
- **`Middleware/`** — Request pipeline middleware (e.g., `ValidationExceptionMiddleware`, rate limiting)
- **`Authentication/`** — JWT Bearer + API Key dual-scheme authentication
- **`Hubs/`** — SignalR hub for real-time entity change notifications
- **`Configuration/`** — Service registration extension methods
- **`Services/`** — API-layer services

## Key Patterns

- **Spec-first development:** All API changes start in `openapi/spec.yaml`. DTOs are generated from the spec via NSwag on every `dotnet build`.
- **Generated DTO namespace:** `API.Generated.Dtos` — naming convention: `CreateXRequest`, `UpdateXRequest`, `XResponse`.
- **`[MapperIgnoreTarget(nameof(XResponse.AdditionalProperties))]`** is required on all response mappers (NSwag generates this property).
- **TypedResults:** Endpoints use `Results<T1, T2, ...>` union return types for compile-time response type enforcement.
- **Dual auth:** JWT Bearer tokens for browser clients, API keys for programmatic access. Both schemes are valid on all protected endpoints.
