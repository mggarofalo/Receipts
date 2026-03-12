---
identifier: MGG-186
title: Overhaul User Management to show user list instead of UUID lookup
id: 042ff57e-b7fa-4286-8615-80dd1a8af896
status: Done
priority:
  value: 3
  name: Medium
createdBy: Michael Garofalo
project: Receipts
team: mggarofalo
labels:
  - frontend
  - Feature
milestone: "Phase 4 [Frontend]"
url: "https://linear.app/mggarofalo/issue/MGG-186/overhaul-user-management-to-show-user-list-instead-of-uuid-lookup"
gitBranchName: mggarofalo/mgg-186-overhaul-user-management-to-show-user-list-instead-of-uuid
createdAt: "2026-02-25T10:45:54.268Z"
updatedAt: "2026-02-26T12:36:29.119Z"
completedAt: "2026-02-26T12:36:29.100Z"
---

# Overhaul User Management to show user list instead of UUID lookup

User management currently only presents a UUID lookup field, which is unhelpful — an admin doesn't know user UUIDs.

## Requirements

* Show a paginated/searchable list of all users with key details (name, email, role, created date, etc.)
* Clicking a user row navigates to their detail/edit view
* Remove the UUID lookup field entirely
* Management actions (edit role, disable, delete) accessible from the list or detail view
