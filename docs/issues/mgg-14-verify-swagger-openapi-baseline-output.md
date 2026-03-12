---
identifier: MGG-14
title: Verify Swagger/OpenAPI baseline output
id: 1faa615b-f6f2-43e7-b757-91f897190443
status: Done
priority:
  value: 2
  name: High
createdBy: Michael Garofalo
project: Receipts
team: mggarofalo
labels:
  - testing
  - backend
milestone: "Phase 1: OpenAPI Spec-First"
url: "https://linear.app/mggarofalo/issue/MGG-14/verify-swaggeropenapi-baseline-output"
gitBranchName: mggarofalo/mgg-14-verify-swaggeropenapi-baseline-output
createdAt: "2026-01-14T10:25:21.137Z"
updatedAt: "2026-02-13T10:53:28.788Z"
completedAt: "2026-02-13T10:53:28.777Z"
---

# Verify Swagger/OpenAPI baseline output

## Summary

Verify that the existing Swagger/OpenAPI configuration produces a correct and complete spec for all current API endpoints. This establishes the **baseline** before moving to a spec-first approach (MGG-21).

## What Was Done

### Removed Swashbuckle

* Removed `Swashbuckle.AspNetCore` from `Directory.Packages.props` and `API.csproj`
* Removed `AddSwaggerGen()` and `AddEndpointsApiExplorer()` from `ProgramService.cs`
* Deleted `SwaggerConfiguration.cs`

### Added Microsoft.AspNetCore.OpenApi + Scalar

* Created `OpenApiConfiguration.cs` with `AddOpenApi()` (built-in) + `MapOpenApi()` + `MapScalarApiReference()`
* Added `Scalar.AspNetCore` 2.12.39 for interactive docs UI (replaces Swagger UI)
* Updated `launchSettings.json` to launch `scalar/v1` instead of `swagger`

### Build-Time Spec Export

* Added `Microsoft.Extensions.ApiDescription.Server` 10.0.3 with `PrivateAssets=all`
* Configured `OpenApiGenerateDocuments=true` and `OpenApiDocumentsDirectory` pointing to `openapi/` at repo root
* Made `InfrastructureService.RegisterInfrastructureServices()` handle missing DB env vars gracefully (so the build-time doc generator can boot the app without a database)
* Made `Program.cs` skip migrations when DB is not configured
* Added `openapi/API.json` to `.gitignore`

### Decision: No `GenerateDocumentationFile`

* Enabling `GenerateDocumentationFile` produces CS1591 warnings for every undocumented public type, which breaks the pre-commit hook (`TreatWarningsAsErrors=true`)
* Instead, endpoint documentation should use `[EndpointSummary]` and `[EndpointDescription]` attributes (or `[ProducesResponseType]` etc.) which flow into the OpenAPI spec without requiring XML doc comments
* A follow-up issue was created for adding endpoint metadata attributes to all controllers

### Generated Spec

* `openapi/API.json` generated at build time (OpenAPI 3.1.1)
* Contains 27 endpoint operations across all controllers
* Schemas include all ViewModels (AccountVM, ReceiptVM, ReceiptItemVM, TransactionVM, etc.)

## Acceptance Criteria

- [X] Swashbuckle fully removed
- [X] `Microsoft.AspNetCore.OpenApi` serving `/openapi/v1.json`
- [X] Scalar API docs UI accessible in development
- [X] Build-time `openapi/API.json` generated to `openapi/` directory
- [X] All endpoints documented with correct request/response schemas
- [ ] Gap analysis documented → deferred to [MGG-21](./mgg-21-establish-openapi-spec-as-authoritative-api-contract.md) (spec authoring)

## Blocking

This issue blocks [MGG-21](./mgg-21-establish-openapi-spec-as-authoritative-api-contract.md) (Establish OpenAPI spec as authoritative API contract)
