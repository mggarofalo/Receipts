---
identifier: MGG-170
title: Remove registration and add admin-only user management
id: 281d500c-9762-452e-afb1-93393da72a4d
status: Done
priority:
  value: 2
  name: High
assignee: Michael Garofalo
createdBy: Michael Garofalo
project: Receipts
team: mggarofalo
labels:
  - security
  - frontend
  - backend
milestone: "Phase 4 [Frontend]"
url: "https://linear.app/mggarofalo/issue/MGG-170/remove-registration-and-add-admin-only-user-management"
gitBranchName: mggarofalo/mgg-170-remove-registration-and-add-admin-only-user-management
createdAt: "2026-02-24T12:50:30.747Z"
updatedAt: "2026-02-26T13:14:19.612Z"
completedAt: "2026-02-26T13:14:19.594Z"
attachments:
  - title: "Remove registration, add admin-only user management (MGG-170)"
    url: "https://github.com/mggarofalo/Receipts/pull/35"
---

# Remove registration and add admin-only user management

## Summary

Self-registration should not be allowed. Only admins can create new users. Remove the registration link/page from the frontend and add an admin-only user management page.

## Tasks

### Backend

- [ ] Remove or disable the public registration endpoint (or gate it behind an admin-only authorization policy)
- [ ] Create admin-only user management endpoints:
  - `GET /api/users` — list all users (paginated)
  - `POST /api/users` — create a new user (admin sets email, name, temporary password, role)
  - `PUT /api/users/{id}` — update user (name, email, role, active/disabled)
  - `DELETE /api/users/{id}` — deactivate/delete user
  - `POST /api/users/{id}/reset-password` — admin-initiated password reset (sets `MustResetPassword` flag)
- [ ] All user management endpoints require `Admin` role authorization
- [ ] Update OpenAPI spec with new endpoints

### Frontend

- [ ] Remove the "Register" / "Sign Up" link from the login page
- [ ] Remove the registration page/route
- [ ] Build an admin-only **User Management** settings page:
  - User list table (name, email, role, status, created date, last login)
  - "Create User" dialog/form (email, name, temporary password, role selection)
  - Edit user dialog (update name, email, role, enable/disable)
  - "Reset Password" action per user (triggers admin-initiated reset, sets `MustResetPassword` flag)
  - Delete/deactivate user with confirmation
- [ ] Add the User Management page to the settings/admin navigation (only visible to admin role)
- [ ] Protect the route with admin role check

## Acceptance Criteria

- [ ] No public registration endpoint or UI exists
- [ ] Only admin users can create new users
- [ ] Admin can list, create, edit, deactivate, and reset passwords for users
- [ ] User management page is only accessible to users with the Admin role
- [ ] Non-admin users cannot see the user management navigation item
- [ ] New users created by admin are prompted to change their temporary password on first login (ties into the `MustResetPassword` flag)
