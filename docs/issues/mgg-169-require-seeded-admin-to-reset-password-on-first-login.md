---
identifier: MGG-169
title: Require seeded admin to reset password on first login
id: 687e107e-372d-4b58-b072-c9f358729161
status: Done
priority:
  value: 2
  name: High
createdBy: Michael Garofalo
project: Receipts
team: mggarofalo
labels:
  - security
  - frontend
  - backend
milestone: "Phase 4 [Backend]"
url: "https://linear.app/mggarofalo/issue/MGG-169/require-seeded-admin-to-reset-password-on-first-login"
gitBranchName: mggarofalo/mgg-169-require-seeded-admin-to-reset-password-on-first-login
createdAt: "2026-02-24T12:50:19.927Z"
updatedAt: "2026-02-26T03:34:09.420Z"
completedAt: "2026-02-26T03:25:21.264Z"
---

# Require seeded admin to reset password on first login

## Summary

The seeded admin account uses a well-known default password. Force the admin to change it on their first login to prevent use of the default credential in production.

## Tasks

### Backend

- [ ] Add a `MustResetPassword` flag (or use an Identity claim/stamp) on the seeded admin user
- [ ] On login, check whether the user's password must be reset — if so, return a response indicating a password change is required (e.g., a `mustResetPassword: true` field on the login response, or a dedicated HTTP 403 with a specific error code)
- [ ] Create a "force change password" endpoint that accepts old + new password, validates, updates the password, and clears the flag
- [ ] Ensure the flag is set during database seeding for the default admin account

### Frontend

- [ ] After login, if the response indicates `mustResetPassword`, redirect to a "Change Password" page instead of the dashboard
- [ ] Build a "Change Password" form (current password + new password + confirm)
- [ ] On successful password change, redirect to the dashboard
- [ ] Prevent navigation away from the change-password page while the flag is active

## Acceptance Criteria

- [ ] Seeded admin is forced to change their password on first login
- [ ] After changing the password, the flag is cleared and subsequent logins proceed normally
- [ ] Non-seeded users (created by admin) are not affected unless explicitly flagged
