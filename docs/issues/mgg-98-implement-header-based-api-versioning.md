---
identifier: MGG-98
title: Implement header-based API versioning
id: 812c8112-7e62-4eea-9507-084008677612
status: Done
priority:
  value: 4
  name: Low
createdBy: Michael Garofalo
project: Receipts
team: mggarofalo
labels:
  - backend
  - Feature
milestone: "Phase 7: Correctness Hardening"
url: "https://linear.app/mggarofalo/issue/MGG-98/implement-header-based-api-versioning"
gitBranchName: mggarofalo/mgg-98-implement-header-based-api-versioning
createdAt: "2026-02-14T15:59:41.673Z"
updatedAt: "2026-02-27T15:32:42.239Z"
completedAt: "2026-02-27T15:32:42.224Z"
---

# Implement header-based API versioning

## Context

With spec-first API development and breaking change detection in CI, the project needs a formal API versioning strategy to handle intentional breaking changes. Header-based versioning keeps URLs clean and is the preferred approach for modern APIs.

## Goal

Implement header-based API versioning using the `api-version` header (or `Api-Version` — to be decided). The API should support multiple concurrent versions to enable gradual client migration.

## Design Considerations

### Versioning Strategy

* **Header:** `api-version: 1.0` (or `Api-Version: 2024-02-14` — date-based)
* **Default behavior:** If no header is provided, use the latest version (or return 400 — to be decided)
* **Supported versions:** Maintain N-1 at minimum (current + previous)

### .NET Implementation Options

1. **Asp.Versioning.Http** (Microsoft's official package) — supports header, query, URL segment, and media type versioning
2. **Custom middleware** — simple header check with version-specific controller routing
3. **Content negotiation** — version in Accept header (`application/vnd.receipts.v1+json`)

**Recommendation:** `Asp.Versioning.Http` — it's the official Microsoft solution, well-maintained, and supports all versioning schemes.

### OpenAPI Spec Impact

* Separate spec files per version (e.g., `openapi/v1/spec.yaml`, `openapi/v2/spec.yaml`)
* Or a single spec with version tags/extensions
* NSwag/Kiota generates DTOs per version

### Breaking Change Workflow

1. Developer wants to make a breaking change
2. Create new API version (e.g., v2)
3. New version gets the breaking change; old version remains
4. CI breaking change detection compares within the same version (not cross-version)
5. Old version deprecated after clients migrate
6. Old version removed after deprecation period

## Tasks

- [ ] Add `Asp.Versioning.Http` package
- [ ] Configure header-based versioning in `Program.cs`
- [ ] Add version attributes to controllers
- [ ] Define versioning strategy for the OpenAPI spec
- [ ] Update NSwag/Kiota config for versioned DTOs
- [ ] Document the versioning policy
- [ ] Update CI breaking change detection to be version-aware

## Acceptance Criteria

- [ ] API responds to `api-version` header
- [ ] Unversioned requests handled gracefully (default version or 400)
- [ ] Multiple versions can coexist
- [ ] OpenAPI spec reflects versioning
- [ ] Breaking change CI integrates with versioning

## Dependencies

* Should follow breaking change detection in CI (that issue motivates this one)
* Impacts spec structure and DTO generation pipeline
