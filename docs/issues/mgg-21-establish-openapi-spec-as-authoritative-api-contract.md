---
identifier: MGG-21
title: Establish OpenAPI spec as authoritative API contract
id: 8a5a333e-e1b0-4096-b505-cfc4e33e9d81
status: Done
priority:
  value: 1
  name: Urgent
assignee: Michael Garofalo
createdBy: Michael Garofalo
project: Receipts
team: mggarofalo
labels:
  - codegen
  - backend
milestone: "Phase 1: OpenAPI Spec-First"
url: "https://linear.app/mggarofalo/issue/MGG-21/establish-openapi-spec-as-authoritative-api-contract"
gitBranchName: mggarofalo/mgg-21-establish-openapi-spec-as-authoritative-api-contract
createdAt: "2026-01-14T10:52:12.835Z"
updatedAt: "2026-02-14T16:06:50.903Z"
completedAt: "2026-02-14T10:54:57.566Z"
attachments:
  - title: "feat(api): establish OpenAPI spec as authoritative API contract (MGG-21)"
    url: "https://github.com/mggarofalo/Receipts/pull/9"
---

# Establish OpenAPI spec as authoritative API contract

## Overview

Establish an OpenAPI 3.1 specification as the **single source of truth** for the API contract. Both the .NET server DTOs and the React/TypeScript client types will be **generated** from this spec, ensuring the contract can never drift between frontend and backend.

## Why Spec-First (not Code-First)

| Approach | Flow | Risk |
| -- | -- | -- |
| **Code-first** | Write C# DTOs → generate OpenAPI → generate TS types | .NET code is "truth" — TS types are second-class, drift risk when devs skip regen |
| **Spec-first** | Write OpenAPI spec → generate C# DTOs + TS types | Spec is truth — both sides are generated, impossible to drift |

**Spec-first wins** because:

* The spec is the contract, not an artifact of implementation
* Both .NET and TypeScript types are generated — neither can drift
* API reviews happen at the spec level, not buried in C# attributes
* Enables future clients (mobile, CLI) with zero additional work
* OpenAPI linting catches design issues early

## Notes from MGG-14 (Baseline)

### `Microsoft.Extensions.ApiDescription.Server` behavior

MGG-14 added this package for build-time spec export. It works by **bootstrapping the full application** (including DI container) to discover endpoints — it is *not* a static analysis tool. `InfrastructureService` and `Program.cs` were updated to handle missing DB env vars gracefully so the generator can run without a database.

### Decision point for this issue

Once `spec.yaml` is the canonical source of truth, decide whether to:

1. **Keep** `Microsoft.Extensions.ApiDescription.Server` for drift detection (compare generated spec vs canonical spec)
2. **Remove** it entirely — if the spec drives code generation, the code-first export becomes redundant and the drift check can be done at the test/CI level instead (e.g., integration tests that validate controller routes match the spec)

If kept for drift detection, the generated file should move from `openapi/API.json` to `openapi/generated/v1.json` (already reflected in the repository layout below). If removed, the `OpenApiGenerateDocuments` and `OpenApiDocumentsDirectory` MSBuild properties in `API.csproj` should be cleaned up along with the DB-missing graceful handling in `InfrastructureService`.

### No `GenerateDocumentationFile`

MGG-14 intentionally did not enable `<GenerateDocumentationFile>` because CS1591 warnings for every undocumented public type break the pre-commit hook (`TreatWarningsAsErrors=true`). MGG-93 was created to add `[EndpointSummary]`/`[EndpointDescription]` attributes instead, which flow into the OpenAPI spec without XML doc generation.

## Technology Stack

| Purpose | Tool | Package | Version |
| -- | -- | -- | -- |
| Runtime OpenAPI serving | `Microsoft.AspNetCore.OpenApi` | NuGet | 10.0.1+ |
| Build-time spec export | `Microsoft.Extensions.ApiDescription.Server` | NuGet | 10.0.2+ |
| API docs UI | `Scalar.AspNetCore` | NuGet | latest |
| Spec linting | `@stoplight/spectral-cli` | npm | 6.15.0+ |
| .NET DTO generation | `NSwag.MSBuild` | NuGet | 14.6.3+ |
| TypeScript type generation | `openapi-typescript` | npm | 7.12.0+ |
| TypeScript API client | `openapi-fetch` | npm | 0.13.0+ |
| Git hooks | `husky` | npm | 9.1.7+ |

## Repository Layout

```
Receipts/
├── openapi/
│   ├── spec.yaml              ← canonical, hand-authored OpenAPI 3.1 spec
│   └── generated/
│       └── v1.json            ← build-time export from API (for drift detection)
├── src/
│   ├── Presentation/
│   │   └── API/
│   │       └── Generated/
│   │           └── Dtos.g.cs  ← NSwag-generated C# DTOs from spec.yaml
│   └── client/                ← React app (under MGG-33)
│       └── src/
│           └── generated/
│               └── api.d.ts   ← openapi-typescript-generated TS types
├── .spectral.yaml             ← Spectral linting rules
├── .husky/
│   └── pre-commit             ← orchestrates all validation
└── nswag.json                 ← NSwag configuration
```

## Implementation Plan

### Phase 1: Baseline (MGG-14) — DONE

* Replaced Swashbuckle with `Microsoft.AspNetCore.OpenApi` + Scalar
* Added `Microsoft.Extensions.ApiDescription.Server` for build-time export
* Exported `openapi/API.json` as starting point (gitignored — build artifact only)
* Made app startup resilient to missing DB config for spec generation

### Phase 2: Author the canonical spec

* Create `openapi/spec.yaml` from the exported baseline
* Define all endpoints, request/response schemas, error responses
* Use `$ref` for shared schema components
* Add `.spectral.yaml` ruleset

### Phase 3: Spec linting with Spectral

**Install:**

```bash
npm install -D @stoplight/spectral-cli
```

**Configure** `.spectral.yaml`:

```yaml
extends:
  - "spectral:oas"

rules:
  operation-operationId: error     # every operation needs an operationId
  oas3-api-servers: warn
  info-contact: off
```

**Lint command:**

```bash
npx spectral lint openapi/spec.yaml --fail-severity warn
```

### Phase 4: Drift detection

After spec.yaml is canonical, validate that the running API matches it:

```bash
# Build exports the API's actual spec to openapi/generated/v1.json
dotnet build src/Presentation/API/API.csproj

# Diff canonical spec against build-time export
# (use a semantic OpenAPI diff tool, or normalize + json diff)
npx @useoptic/optic diff openapi/spec.yaml openapi/generated/v1.json
```

Alternative: use OpenAPI validation middleware in the API itself to reject requests/responses that don't match the spec at runtime (development mode only).

### Phase 5: Pre-commit hook workflow

**Install husky:**

```bash
npm install -D husky
npx husky init
```

`.husky/pre-commit` — runs in this order, each step gates the next:

```bash
#!/usr/bin/env sh

# === Step 1: Lint the OpenAPI spec ===
echo "Linting OpenAPI spec..."
npx spectral lint openapi/spec.yaml --fail-severity warn

# === Step 2: Regenerate .NET DTOs from spec ===
echo "Regenerating .NET DTOs from spec..."
dotnet build src/Presentation/API/API.csproj -t:NSwag --no-restore -v quiet

# === Step 3: Regenerate TypeScript types from spec ===
echo "Regenerating TypeScript types from spec..."
npx openapi-typescript openapi/spec.yaml -o src/client/src/generated/api.d.ts

# === Step 4: Check for uncommitted changes in generated files ===
echo "Checking generated files are up to date..."
git diff --exit-code -- \
  "src/Presentation/API/Generated/" \
  "src/client/src/generated/" \
  || (echo "ERROR: Generated files are stale. Stage the regenerated files." && exit 1)

# === Step 5: Run unit tests ===
echo "Running .NET tests..."
dotnet test Receipts.sln --no-build --verbosity quiet

# === Step 6: Coverage check (if configured) ===
# dotnet test --collect:"XPlat Code Coverage" -- threshold enforced by CI
```

**Hook execution order:**

1. **Spectral lint** — catches spec authoring errors immediately
2. **Regenerate .NET DTOs** — ensures NSwag output matches current spec
3. **Regenerate TS types** — ensures openapi-typescript output matches current spec
4. **Stale file check** — `git diff --exit-code` fails if generated files changed (developer forgot to regenerate after editing the spec)
5. **Unit tests** — run against the freshly-regenerated types
6. **Coverage** — enforced in CI (too slow for pre-commit, but can be added)

## Acceptance Criteria

- [ ] Canonical OpenAPI 3.1 `spec.yaml` exists in `openapi/` directory
- [ ] `.spectral.yaml` config with enforced rules
- [ ] `npx spectral lint` passes with zero warnings
- [ ] NSwag configured to generate .NET DTOs from spec (see MGG-88)
- [ ] openapi-typescript configured to generate TS types from spec (see MGG-36)
- [ ] Husky pre-commit hook orchestrates: lint → regen → stale check → test
- [ ] Build-time drift detection (exported spec vs canonical spec)
- [ ] Developer documentation explains the spec-first workflow and hook pipeline

## Blocking

This issue blocks:

* MGG-88 (Generate .NET DTOs from spec)
* MGG-36 (Generate TypeScript types from spec)
* MGG-83 (Replace ViewModels epic)
