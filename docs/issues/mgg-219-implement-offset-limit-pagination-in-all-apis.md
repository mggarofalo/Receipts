---
identifier: MGG-219
title: Implement offset/limit pagination in all APIs
id: 4b25d007-de07-4d34-a267-c86fd53d0e51
status: Done
priority:
  value: 3
  name: Medium
assignee: Michael Garofalo
createdBy: Michael Garofalo
project: Receipts
team: mggarofalo
labels:
  - frontend
  - backend
  - Feature
url: "https://linear.app/mggarofalo/issue/MGG-219/implement-offsetlimit-pagination-in-all-apis"
gitBranchName: mggarofalo/mgg-219-implement-offsetlimit-pagination-in-all-apis
createdAt: "2026-03-03T17:50:46.137Z"
updatedAt: "2026-03-04T17:26:11.341Z"
completedAt: "2026-03-04T17:26:11.316Z"
attachments:
  - title: "feat: implement offset/limit pagination across all APIs (MGG-219)"
    url: "https://github.com/mggarofalo/Receipts/pull/71"
---

# Implement offset/limit pagination in all APIs

## Summary

Add offset/limit pagination support to all list endpoints in the API. Currently, endpoints return all matching records in a single response, which will not scale as data grows.

## Requirements

* Add `offset` and `limit` query parameters to all list/collection endpoints
* Update the OpenAPI spec (`openapi/spec.yaml`) with pagination parameters and response envelope
* Define a consistent pagination response envelope (e.g., `{ data: [...], total: number, offset: number, limit: number }`)
* Set sensible defaults (e.g., `offset=0`, `limit=50`) and maximum limits
* Implement pagination at the repository/query level (SQL `OFFSET`/`LIMIT` or keyset)
* Regenerate DTOs from the updated spec
* Update frontend API client to pass pagination parameters
* Add unit and integration tests for pagination behavior (boundary cases, out-of-range offset, etc.)

## Acceptance Criteria

- [ ] OpenAPI spec updated with pagination parameters on all list endpoints
- [ ] Spec linting passes (`npm run lint:spec`)
- [ ] Backend implements pagination for all list endpoints
- [ ] Response includes total count for client-side pagination controls
- [ ] Default and max limit enforced
- [ ] Frontend updated to consume paginated responses
- [ ] Drift check passes (`npm run check:drift`)
- [ ] Tests cover pagination edge cases
