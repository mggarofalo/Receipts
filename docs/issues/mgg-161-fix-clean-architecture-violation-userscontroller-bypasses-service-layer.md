---
identifier: MGG-161
title: "Fix Clean Architecture violation: UsersController bypasses service layer"
id: 7c1290c9-92bc-4483-8b4f-9875c0d77d0f
status: Done
priority:
  value: 1
  name: Urgent
createdBy: Michael Garofalo
project: Receipts
team: mggarofalo
labels:
  - backend
  - Bug
milestone: "Phase 4 [Frontend]"
url: "https://linear.app/mggarofalo/issue/MGG-161/fix-clean-architecture-violation-userscontroller-bypasses-service"
gitBranchName: mggarofalo/mgg-161-fix-clean-architecture-violation-userscontroller-bypasses
createdAt: "2026-02-22T01:24:33.007Z"
updatedAt: "2026-02-22T01:50:28.698Z"
completedAt: "2026-02-22T01:50:28.679Z"
---

# Fix Clean Architecture violation: UsersController bypasses service layer

`UsersController` injects `ApplicationDbContext` directly, violating Clean Architecture — Presentation must never reference Infrastructure internals.\\n\\n**Fix:** Extract the batch role query into an `IUserService` interface (Application layer) with an `Infrastructure` implementation, and have the controller call through the interface.\\n\\n**Introduced in:** MGG-160
