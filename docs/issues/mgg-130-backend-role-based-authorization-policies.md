---
identifier: MGG-130
title: "Backend: Role-based authorization policies"
id: 0f1a6e6f-7f0b-4da1-8548-4dc72ba4905a
status: Done
priority:
  value: 2
  name: High
createdBy: Michael Garofalo
project: Receipts
team: mggarofalo
labels:
  - security
  - backend
  - Feature
milestone: "Phase 4 [Backend]"
url: "https://linear.app/mggarofalo/issue/MGG-130/backend-role-based-authorization-policies"
gitBranchName: mggarofalo/mgg-130-backend-role-based-authorization-policies
createdAt: "2026-02-19T01:49:37.904Z"
updatedAt: "2026-02-20T10:55:11.606Z"
completedAt: "2026-02-20T10:55:11.593Z"
attachments:
  - title: scripts/check-drift.mjs
    url: "https://github.com/mggarofalo/Receipts/blob/master/scripts/check-drift.mjs"
  - title: openapi/spec.yaml
    url: "https://github.com/mggarofalo/Receipts/blob/master/openapi/spec.yaml"
---

# Backend: Role-based authorization policies

Define roles (`Admin`, `User`), seed them on startup, include role claims in JWT tokens, and expose role-management endpoints for admins.

## Tasks

### Role setup

* Define `Admin` and `User` roles as constants (avoid magic strings)
* Seed roles into `AspNetRoles` table on application startup
* Seed a configurable default admin user (credentials via `appsettings` / user-secrets)
* Assign `User` role to all newly registered users automatically in `AuthController.Register`

### JWT role claims

`TokenService.GenerateAccessToken` already accepts `IList<string> roles` and adds `ClaimTypes.Role` — ensure `AuthController` calls `userManager.GetRolesAsync(user)` and passes roles through. Verify roles appear in decoded JWT.

### Authorization policies

Add named policies to `AuthConfiguration.AddAuthServices`:

```csharp
options.AddPolicy("RequireAdmin", policy =>
{
    policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme, ApiKeyAuthenticationDefaults.AuthenticationScheme);
    policy.RequireAuthenticatedUser();
    policy.RequireRole("Admin");
});
```

### Role management endpoints (admin only)

New endpoints under `/api/users/{userId}/roles`:

| Method | Path | Description |
| -- | -- | -- |
| `GET` | `/api/users/{userId}/roles` | List roles for a user |
| `POST` | `/api/users/{userId}/roles/{role}` | Assign role to user |
| `DELETE` | `/api/users/{userId}/roles/{role}` | Remove role from user |

All three require `[Authorize(Policy = "RequireAdmin")]`.

## OpenAPI spec changes

Add all three endpoints to [`openapi/spec.yaml`](<https://github.com/mggarofalo/Receipts/blob/master/openapi/spec.yaml>) with `security: [{BearerAuth: []}, {ApiKey: []}]` and appropriate request/response schemas. The [`scripts/check-drift.mjs`](<https://github.com/mggarofalo/Receipts/blob/master/scripts/check-drift.mjs>) will validate the new operations and schemas against the generated `openapi/generated/API.json` — spec must be authored first, then implementation must match it exactly.

## Acceptance criteria

* `Admin` and `User` roles exist in the database after first startup
* Newly registered users automatically get the `User` role
* Decoded JWT contains a `role` claim with the user's roles
* `/api/users/{userId}/roles` endpoints work and are restricted to `Admin`
* Non-admin users receive `403` on role-management endpoints
* `dotnet test` passes, `node scripts/check-drift.mjs` reports no drift
