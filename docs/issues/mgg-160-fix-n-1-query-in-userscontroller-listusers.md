---
identifier: MGG-160
title: Fix N+1 query in UsersController.ListUsers
id: d474eba2-faaf-48b0-81da-2dee7b885d4c
status: Done
priority:
  value: 2
  name: High
createdBy: Michael Garofalo
project: Receipts
team: mggarofalo
labels:
  - backend
  - Bug
milestone: "Phase 4 [Frontend]"
url: "https://linear.app/mggarofalo/issue/MGG-160/fix-n1-query-in-userscontrollerlistusers"
gitBranchName: mggarofalo/mgg-160-fix-n1-query-in-userscontrollerlistusers
createdAt: "2026-02-22T01:07:12.619Z"
updatedAt: "2026-02-22T01:10:41.126Z"
completedAt: "2026-02-22T01:10:41.113Z"
---

# Fix N+1 query in UsersController.ListUsers

`UsersController.ListUsers` has an N+1 query: it calls `userManager.GetRolesAsync(user)` per user inside a `foreach` loop. With `pageSize=100`, that's 101 DB queries per request.\\n\\n**Fix:** Inject `ApplicationDbContext` directly and batch-query the `AspNetUserRoles` + `AspNetRoles` join table in a single query for all user IDs in the page, then map roles in-memory.\\n\\n**Current (N+1):**\\n`csharp\nforeach (ApplicationUser user in users)\n{\n    IList<string> roles = await userManager.GetRolesAsync(user);\n    items.Add(new UserSummaryResponse { ... });\n}\n`\\n\\n**Target (2 queries):**\\n1. Paginated user query\\n2. Single batch query for roles of all user IDs in the page\\n\\n**Introduced in:** MGG-159
