---
identifier: MGG-229
title: "Implement token auth per RFC 6749, introspection per RFC 7662, and revocation per RFC 7009"
id: c3cbc2dd-cc4c-4189-9b8d-3f4424e27944
status: Done
priority:
  value: 3
  name: Medium
assignee: Michael Garofalo
createdBy: Michael Garofalo
project: Receipts
team: mggarofalo
labels:
  - security
  - backend
url: "https://linear.app/mggarofalo/issue/MGG-229/implement-token-auth-per-rfc-6749-introspection-per-rfc-7662-and"
gitBranchName: mggarofalo/mgg-229-implement-token-auth-per-rfc-6749-introspection-per-rfc-7662
createdAt: "2026-03-04T18:49:37.659Z"
updatedAt: "2026-03-04T20:23:53.069Z"
completedAt: "2026-03-04T20:23:53.051Z"
attachments:
  - title: "feat(api): RFC-compliant token auth, introspection, and revocation (MGG-229)"
    url: "https://github.com/mggarofalo/Receipts/pull/78"
---

# Implement token auth per RFC 6749, introspection per RFC 7662, and revocation per RFC 7009

## Context

The auth system (AuthController, token refresh, API key management) should align with the relevant OAuth 2.0 RFCs for token-based authentication.

## Applicable Standards

| RFC | Title | Applies To |
| -- | -- | -- |
| **RFC 6749** | The OAuth 2.0 Authorization Framework | Token issuance, grant types, token response format, error codes |
| **RFC 7662** | OAuth 2.0 Token Introspection | Token validation endpoint — allows resource servers to query token validity/metadata |
| **RFC 7009** | OAuth 2.0 Token Revocation | Token revocation endpoint — logout, key rotation, compromised token invalidation |

## Goal

Audit the existing auth implementation against these RFCs and bring it into compliance:

1. **RFC 6749**: Ensure token response format, error responses, and grant flow follow the spec
2. **RFC 7662**: Add a token introspection endpoint (`POST /introspect`) that returns token metadata (active, scope, exp, iat, sub, client_id)
3. **RFC 7009**: Ensure the revocation endpoint (`POST /revoke`) follows the spec — accepts token + token_type_hint, returns 200 even for invalid tokens

## Acceptance Criteria

* Token responses conform to RFC 6749 Section 5.1 (access_token, token_type, expires_in, refresh_token, scope)
* Error responses conform to RFC 6749 Section 5.2 (error, error_description, error_uri)
* `/introspect` endpoint per RFC 7662 Section 2
* `/revoke` endpoint per RFC 7009 Section 2
* All endpoints documented in OpenAPI spec
* All tests pass
