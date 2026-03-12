---
identifier: MGG-194
title: "Remove unused frontend code: dead components, hooks, pages"
id: 8d9723ec-3fba-48b2-9c40-397868c30ccc
status: Done
priority:
  value: 4
  name: Low
assignee: Michael Garofalo
createdBy: Michael Garofalo
project: Receipts
team: mggarofalo
labels:
  - cleanup
  - frontend
url: "https://linear.app/mggarofalo/issue/MGG-194/remove-unused-frontend-code-dead-components-hooks-pages"
gitBranchName: mggarofalo/mgg-194-remove-unused-frontend-code-dead-components-hooks-pages
createdAt: "2026-02-26T03:19:17.601Z"
updatedAt: "2026-03-03T12:00:36.000Z"
completedAt: "2026-03-03T12:00:35.980Z"
attachments:
  - title: "chore(client): remove unused frontend code (MGG-194)"
    url: "https://github.com/mggarofalo/Receipts/pull/54"
---

# Remove unused frontend code: dead components, hooks, pages

## Summary

Full dead-code scan across both .NET backend and React frontend. The backend is clean — all services, repositories, controllers, DTOs, and configuration are actively used. The frontend has 4 items of dead code (1 component, 1 page, 2 hook functions).

---

## Dead Code to Remove

- [ ] `AdminOnly` component (`src/client/src/components/AdminOnly.tsx`)
  Wrapper that conditionally renders children based on admin role. Exported but never imported or used anywhere. Delete the file.
- [ ] `ServerError` page (`src/client/src/pages/ServerError.tsx`)
  A 500 error page component. Defined and exported but never imported in `App.tsx` or any other routing — no route points to it. Delete the file (or wire it up in the router if intended).
- [ ] `useAuditLogsByUser` hook (`src/client/src/hooks/useAudit.ts`)
  Calls `/api/audit/user/{userId}` but is never imported by any component. Remove the export.
- [ ] `useAuditLogsByApiKey` hook (`src/client/src/hooks/useAudit.ts`)
  Calls `/api/audit/apikey/{apiKeyId}` but is never imported by any component. Remove the export.

## Backend Assessment (No Action Needed)

| Area | Status |
| -- | -- |
| DI-registered services | All injected and used |
| Repository methods | All called from handlers/services |
| Controller endpoints | All routed and functional |
| Configuration keys | All consumed |
| Generated DTOs | Part of OpenAPI contract — all valid |

## Notes

* The `ServerError` page may have been written for future use. Decision: delete or integrate into error boundary routing.
* The two unused audit hooks correspond to real backend endpoints that work — the hooks were written but the UI pages that would use them were never built. If audit-by-user/by-API-key views are planned, keep them; otherwise delete.
