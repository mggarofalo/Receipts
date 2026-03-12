---
identifier: MGG-128
title: Permission System
id: 509c8392-f272-4047-b487-6f0700ec58ed
status: Done
priority:
  value: 1
  name: Urgent
assignee: Michael Garofalo
createdBy: Michael Garofalo
project: Receipts
team: mggarofalo
labels:
  - epic
  - security
  - frontend
  - backend
milestone: "Phase 4 [Frontend]"
url: "https://linear.app/mggarofalo/issue/MGG-128/permission-system"
gitBranchName: mggarofalo/mgg-128-permission-system
createdAt: "2026-02-19T01:49:06.474Z"
updatedAt: "2026-02-21T20:45:32.900Z"
completedAt: "2026-02-21T20:45:32.863Z"
attachments:
  - title: "Permission system: role-based UI & admin manager (MGG-128)"
    url: "https://github.com/mggarofalo/Receipts/pull/22"
---

# Permission System

Role-based authorization system spanning backend (role definitions, JWT role claims, per-endpoint policies) and frontend (conditional UI based on user roles/permissions).

Built on top of the JWT + API key auth foundation from MGG-34. Distinct from authentication (who you are) — this is authorization (what you can do).

**Children:**

* BE: Protect existing API endpoints with \[Authorize\]
* BE: Role-based authorization policies
* FE: Permission-aware UI

**MVP — required before any role-gated features.**
