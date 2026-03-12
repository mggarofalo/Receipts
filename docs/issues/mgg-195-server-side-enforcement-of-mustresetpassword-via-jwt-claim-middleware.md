---
identifier: MGG-195
title: Server-side enforcement of MustResetPassword via JWT claim + middleware
id: be212560-be5f-492b-b39b-abe87d267597
status: Done
priority:
  value: 1
  name: Urgent
createdBy: Michael Garofalo
project: Receipts
team: mggarofalo
labels:
  - security
  - backend
milestone: "Phase 4 [Backend]"
url: "https://linear.app/mggarofalo/issue/MGG-195/server-side-enforcement-of-mustresetpassword-via-jwt-claim-middleware"
gitBranchName: mggarofalo/mgg-195-server-side-enforcement-of-mustresetpassword-via-jwt-claim
createdAt: "2026-02-26T03:32:18.394Z"
updatedAt: "2026-02-26T03:35:27.594Z"
completedAt: "2026-02-26T03:35:27.581Z"
---

# Server-side enforcement of MustResetPassword via JWT claim + middleware

## Summary

The current `MustResetPassword` enforcement is frontend-only. A `curl` user can log in with the default password and get a fully valid JWT that works on all API endpoints. We need server-side enforcement so that tokens issued to users with `MustResetPassword = true` are rejected on all endpoints except `/api/auth/change-password` and `/api/auth/logout`.

## Approach

Embed `must_reset_password` as a claim in the JWT. Add middleware after `UseAuthentication()` that reads the claim and returns 403 for all paths except the two whitelisted ones. No DB lookup, no authorization handler, no new DI — just a claim check and a path whitelist.

API key auth is unaffected because API key claims don't include `must_reset_password`.

## Tasks

- [ ] Add `bool mustResetPassword` param to `ITokenService.GenerateAccessToken`
- [ ] Add `must_reset_password` claim in `TokenService` when flag is true
- [ ] Pass flag from all `AuthController` call sites (Register=true, Login=user.MustResetPassword, Refresh=user.MustResetPassword, ChangePassword=false)
- [ ] Create `MustResetPasswordMiddleware` — returns 403 for non-whitelisted paths when claim is present
- [ ] Register middleware in `AuthConfiguration.UseAuthServices()` between authentication and authorization
- [ ] Fix any tests referencing `GenerateAccessToken`
