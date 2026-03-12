---
identifier: MGG-226
title: Include canonical ID in relevant API response headers
id: 884d29a3-c102-432f-a565-424cf447de7d
status: Done
priority:
  value: 4
  name: Low
assignee: Michael Garofalo
createdBy: Michael Garofalo
project: Receipts
team: mggarofalo
labels:
  - backend
  - Feature
url: "https://linear.app/mggarofalo/issue/MGG-226/include-canonical-id-in-relevant-api-response-headers"
gitBranchName: mggarofalo/mgg-226-include-canonical-id-in-relevant-api-response-headers
createdAt: "2026-03-04T17:36:25.307Z"
updatedAt: "2026-03-04T18:59:21.507Z"
completedAt: "2026-03-04T18:59:21.490Z"
attachments:
  - title: "feat(api): add X-Resource-Id response header (MGG-226)"
    url: "https://github.com/mggarofalo/Receipts/pull/74"
---

# Include canonical ID in relevant API response headers

## Summary

Add a canonical resource ID to API response headers for endpoints that create or return a single resource. This gives clients a consistent, header-level way to identify the resource without parsing the response body.

## Context

The API already sets `X-Correlation-ID` via middleware. A similar pattern can be used to surface the canonical resource identifier (e.g., `X-Resource-Id`) in response headers for relevant endpoints.

## Requirements

* Add a response header (e.g., `X-Resource-Id`) containing the resource's canonical ID to:
  * All single-resource GET endpoints (`GET /api/accounts/{id}`, etc.)
  * All POST (create) endpoints — return the newly created resource's ID
  * All PUT (update) endpoints — echo the resource's ID
* Document the header in `openapi/spec.yaml` using a reusable `components/headers` definition
* Implement via middleware or action filter so individual controllers don't need manual header manipulation
* Add unit tests for the header presence and correctness

## Out of Scope

* List/collection endpoints (pagination headers are separate)
* DELETE endpoints (no resource to identify)

## Implementation Notes

* Follow the existing `CorrelationIdMiddleware` pattern in `src/Presentation/API/Middleware/`
* An `IActionResult` action filter or `IAsyncResultFilter` may be more appropriate than middleware, since it needs access to the response body to extract the ID
* Consider a marker interface or convention (e.g., all response DTOs have an `Id` property) to generically extract the canonical ID
