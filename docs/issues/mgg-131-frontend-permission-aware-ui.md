---
identifier: MGG-131
title: "Frontend: Permission-aware UI"
id: f8d831d3-62dc-4ad3-a5ab-0c1d3260fd32
status: Done
priority:
  value: 3
  name: Medium
createdBy: Michael Garofalo
project: Receipts
team: mggarofalo
labels:
  - security
  - frontend
  - Feature
milestone: "Phase 4 [Frontend]"
url: "https://linear.app/mggarofalo/issue/MGG-131/frontend-permission-aware-ui"
gitBranchName: mggarofalo/mgg-131-frontend-permission-aware-ui
createdAt: "2026-02-19T01:49:58.028Z"
updatedAt: "2026-02-21T20:45:26.078Z"
completedAt: "2026-02-21T20:45:26.063Z"
attachments:
  - title: openapi/spec.yaml — TokenResponse schema
    url: "https://github.com/mggarofalo/Receipts/blob/master/openapi/spec.yaml"
---

# Frontend: Permission-aware UI

Parse role claims from the JWT access token and conditionally render UI elements based on the authenticated user's roles.

## Background

The [`TokenResponse`](<https://github.com/mggarofalo/Receipts/blob/master/openapi/spec.yaml>) schema (`accessToken`, `refreshToken`, `expiresIn`) is defined in `openapi/spec.yaml` and the TypeScript client is generated from it. The access token is a HS256 JWT containing a `role` claim (array of strings, e.g. `["Admin", "User"]`) set by the backend.

## Tasks

### Role parsing

* Decode the JWT access token (without re-verifying the signature on the client — just read the payload)
* Extract `role` claim as `string[]` from the token payload
* Store roles alongside the auth state (Zustand / React Context)

### Permission primitives

```ts
// usePermission hook
function usePermission() {
  const { roles } = useAuthStore();
  return {
    hasRole: (role: string) => roles.includes(role),
    isAdmin: () => roles.includes('Admin'),
  };
}
```

### Conditional rendering

* `<AdminOnly>` guard component: renders children only when `isAdmin()` is true
* Hide/show navigation items, action buttons, and management pages based on role
* Admin-only sections: user role management UI (assign/remove roles per user, backed by the `/api/users/{userId}/roles` endpoints from MGG-130)

### Graceful degradation

* If token is missing or malformed, treat user as having no roles
* Non-admin users see a clean UI with admin controls simply absent (not disabled/greyed)
* Role state refreshes on token refresh (in case roles change mid-session)

## TypeScript client note

The generated TypeScript client (from `openapi/spec.yaml` via NSwag/openapi-typescript-codegen) exposes `TokenResponse` with camelCase property names matching the spec. Use the generated client types — do not hand-roll API types.

## Acceptance criteria

* `isAdmin()` returns `true` only for users with the `Admin` role in their JWT
* Admin-only UI elements are not rendered for `User`-role accounts
* Admin navigation/controls appear immediately after login for admin accounts
* Role state is preserved across page refresh (re-decoded from stored token)
* No role-check logic is duplicated — all checks go through `usePermission()`
