---
identifier: MGG-159
title: "Backend: GET /api/users endpoint for admin user listing"
id: 2b5ebf86-fe63-4360-b5bb-eb5df97e42d8
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
milestone: "Phase 4 [Frontend]"
url: "https://linear.app/mggarofalo/issue/MGG-159/backend-get-apiusers-endpoint-for-admin-user-listing"
gitBranchName: mggarofalo/mgg-159-backend-get-apiusers-endpoint-for-admin-user-listing
createdAt: "2026-02-21T20:45:45.184Z"
updatedAt: "2026-02-21T21:00:25.728Z"
completedAt: "2026-02-21T21:00:25.715Z"
---

# Backend: GET /api/users endpoint for admin user listing

Add a `GET /api/users` endpoint (admin-only) that returns a list of users with their IDs, emails, and assigned roles.

**Context:** The Admin Permissions Manager page (MGG-132) currently uses a userId-input lookup pattern because no user listing endpoint exists. Once this endpoint is built, the admin page can be upgraded to show a searchable/paginated user list instead of requiring manual UUID entry.

**Scope:**

* Add `GET /api/users` to the OpenAPI spec
* Implement the endpoint in a `UsersController` (or extend the existing auth controller)
* Protect with `RequireAdmin` policy
* Return `UserResponse[]` with `id`, `email`, `roles`
* Support optional query parameters for pagination (`page`, `pageSize`)

**Follow-up from:** [MGG-132](./mgg-132-frontend-admin-permissions-manager.md) (Frontend: Admin Permissions Manager)
