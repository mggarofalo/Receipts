---
identifier: MGG-99
title: Add runtime OpenAPI contract validation middleware (dev mode)
id: ffdd7661-d2ba-40a1-ad13-66ad32131f43
status: Done
priority:
  value: 4
  name: Low
createdBy: Michael Garofalo
project: Receipts
team: mggarofalo
labels:
  - testing
  - backend
milestone: "Phase 7: Correctness Hardening"
url: "https://linear.app/mggarofalo/issue/MGG-99/add-runtime-openapi-contract-validation-middleware-dev-mode"
gitBranchName: mggarofalo/mgg-99-add-runtime-openapi-contract-validation-middleware-dev-mode
createdAt: "2026-02-14T15:59:54.106Z"
updatedAt: "2026-02-27T15:22:13.235Z"
completedAt: "2026-02-27T15:22:13.226Z"
---

# Add runtime OpenAPI contract validation middleware (dev mode)

## Context

Build-time checks (drift detection, compilation, unit tests) catch most spec-vs-implementation mismatches, but they can't catch runtime-only issues:

* **Serialization settings** silently changing output (e.g., `JsonSerializerOptions` casing, null handling, enum serialization)
* **Middleware** modifying response bodies (exception handlers returning wrong shapes, response compression altering content-type)
* **Content negotiation** edge cases
* **Date/time format** mismatches between the spec and actual serialized output (e.g., `DateOnly` → string format)

These issues only manifest when the full HTTP pipeline processes a request, which unit tests bypass entirely.

## Goal

Add middleware that validates actual API responses against the OpenAPI spec **in development mode only**. When a response doesn't match the spec, it should log a warning (or throw, configurable).

## Implementation Options

| Approach | Pros | Cons |
| -- | -- | -- |
| Custom middleware reading spec + JSON Schema validation | Full control, lightweight | Must maintain |
| `Microsoft.AspNetCore.OpenApi` validation (if available in .NET 10) | First-party | May not exist yet |
| NSwag middleware | Already in the stack | Primarily for Swagger, not validation |

### Suggested Approach

Custom middleware registered only in Development environment:

```csharp
if (app.Environment.IsDevelopment())
{
    app.UseOpenApiResponseValidation("openapi/spec.yaml");
}
```

The middleware:

1. Intercepts responses after the controller writes them
2. Reads the response body
3. Looks up the matching operation in the spec (by path + method)
4. Validates the response body against the operation's response schema
5. Logs a warning (or throws) if validation fails

## Acceptance Criteria

- [ ] Middleware validates response bodies against spec schemas
- [ ] Only active in Development environment (zero overhead in production)
- [ ] Logs clear error messages identifying which property/field failed validation
- [ ] Catches serialization format mismatches (dates, enums, casing)
- [ ] Catches missing required fields in responses
- [ ] Configurable behavior: warn vs throw

## Dependencies

* Should follow [MGG-88](./mgg-88-generate-net-request-response-dtos-from-openapi-spec.md) (spec must be in Request/Response format)
* Benefits from semantic drift detection being in place
