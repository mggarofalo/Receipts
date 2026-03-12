---
identifier: MGG-93
title: Add OpenAPI endpoint metadata attributes to all controllers
id: 34b257e3-cbb4-494a-8e45-bee168372ec6
status: Done
priority:
  value: 1
  name: Urgent
createdBy: Michael Garofalo
project: Receipts
team: mggarofalo
labels:
  - dx
  - backend
milestone: "Phase 1: OpenAPI Spec-First"
url: "https://linear.app/mggarofalo/issue/MGG-93/add-openapi-endpoint-metadata-attributes-to-all-controllers"
gitBranchName: mggarofalo/mgg-93-add-openapi-endpoint-metadata-attributes-to-all-controllers
createdAt: "2026-02-13T10:47:53.604Z"
updatedAt: "2026-02-14T15:17:26.067Z"
completedAt: "2026-02-14T15:17:26.038Z"
---

# Add OpenAPI endpoint metadata attributes to all controllers

## Summary

Add `[EndpointSummary]` and `[EndpointDescription]` attributes to all controller actions so that the build-time OpenAPI spec includes human-readable documentation for every endpoint.

## Context

MGG-14 replaced Swashbuckle with `Microsoft.AspNetCore.OpenApi` and Scalar. The existing controllers have `/// <summary>` XML doc comments on some actions, but `GenerateDocumentationFile` was intentionally not enabled because it produces CS1591 warnings for every undocumented public type, breaking the pre-commit hook (`TreatWarningsAsErrors=true`).

Instead, the .NET 10 approach is to use endpoint metadata attributes which flow directly into the OpenAPI spec without requiring XML documentation generation:

```csharp
[HttpGet("{id}")]
[EndpointSummary("Get an account by ID")]
[EndpointDescription("Returns a single account matching the provided GUID.")]
[ProducesResponseType<AccountVM>(StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public async Task<IActionResult> GetById(Guid id) { ... }
```

## Tasks

1. Audit all controller actions across `Controllers/Core/` and `Controllers/Aggregates/`
2. Add `[EndpointSummary]` to every action (short one-line description)
3. Add `[EndpointDescription]` where the endpoint behavior needs more explanation
4. Verify `[ProducesResponseType]` attributes are present and accurate for all actions
5. Rebuild and verify the generated `openapi/API.json` includes the new metadata
6. Remove the now-redundant `/// <summary>` XML doc comments from controller actions

## Acceptance Criteria

- [ ] Every controller action has `[EndpointSummary]`
- [ ] Complex actions have `[EndpointDescription]`
- [ ] All `[ProducesResponseType]` attributes are accurate
- [ ] Generated `openapi/API.json` shows summaries/descriptions for all endpoints
- [ ] No `/// <summary>` XML comments remain on controller actions (replaced by attributes)
