# API Guidelines

## OpenAPI Spec-First

The canonical API contract lives in `openapi/spec.yaml` (OpenAPI 3.1.0). All API changes follow a spec-first workflow: edit the spec, lint, build (regenerates DTOs), check drift.

**Key files:** `openapi/spec.yaml` (canonical spec), `.spectral.yaml` (lint rules), `scripts/check-drift.mjs` (drift detection), `scripts/check-breaking.mjs` (breaking change detection, CI only)

**npm scripts:** `npm run lint:spec`, `npm run check:drift`, `npm run check:breaking -- origin/main`

## Endpoint Return Types

Use `TypedResults` with concrete `Results<T1, T2, ...>` union return types on all endpoints (see MGG-227). This provides compile-time enforcement of response types and eliminates the need for `[ProducesResponseType]` attributes.

## Authentication Standards

Token-based authentication must conform to these RFCs:
- **RFC 6749** — OAuth 2.0 Authorization Framework: token issuance, response format, error codes
- **RFC 7662** — OAuth 2.0 Token Introspection: token validation endpoint semantics
- **RFC 7009** — OAuth 2.0 Token Revocation: revocation endpoint behavior and response codes
