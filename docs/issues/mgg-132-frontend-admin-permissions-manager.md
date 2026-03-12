---
identifier: MGG-132
title: "Frontend: Admin Permissions Manager"
id: 1a1bd1af-34ba-420d-98dc-c3099869c8e7
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
url: "https://linear.app/mggarofalo/issue/MGG-132/frontend-admin-permissions-manager"
gitBranchName: mggarofalo/mgg-132-frontend-admin-permissions-manager
createdAt: "2026-02-19T01:57:45.424Z"
updatedAt: "2026-02-21T20:45:27.427Z"
completedAt: "2026-02-21T20:45:27.414Z"
---

# Frontend: Admin Permissions Manager

Admin-only UI for managing users and their role assignments.\\n\\n**Scope:**\\n- View all users with their currently assigned roles\\n- Assign or revoke roles from individual users\\n- Browse/review available roles and their associated permission policies\\n- Protect the page behind an admin-only route guard\\n\\nBuilt on top of the RBAC endpoints from [MGG-130](./mgg-130-backend-role-based-authorization-policies.md). Distinct from [MGG-131](./mgg-131-frontend-permission-aware-ui.md) (permission-aware UI), which controls what regular users can *see* — this page controls the *assignments* themselves.\\n\\n**Blocked by:** [MGG-129](./mgg-129-backend-protect-existing-api-endpoints-with-authorize.md) (endpoint protection) and [MGG-130](./mgg-130-backend-role-based-authorization-policies.md) (role policies must exist before they can be assigned).
