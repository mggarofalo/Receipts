---
identifier: MGG-240
title: Forced password change can be bypassed by navigating away
id: c705398e-61c8-45f6-80e0-9c636deb97de
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
  - frontend
  - backend
url: "https://linear.app/mggarofalo/issue/MGG-240/forced-password-change-can-be-bypassed-by-navigating-away"
gitBranchName: mggarofalo/mgg-240-forced-password-change-can-be-bypassed-by-navigating-away
createdAt: "2026-03-05T12:01:42.310Z"
updatedAt: "2026-03-05T18:41:25.429Z"
completedAt: "2026-03-05T18:41:25.410Z"
attachments:
  - title: "fix(client): persist forced password change across refreshes (MGG-240)"
    url: "https://github.com/mggarofalo/Receipts/pull/86"
---

# Forced password change can be bypassed by navigating away

## Bug

When a user is required to change their password (e.g., first login with seeded admin credentials), they can bypass the forced change by simply navigating back to the main page. However, API calls correctly return 403 with "Password change required", so the user can't actually do anything meaningful — they just aren't redirected back to the change-password page.

## Steps to Reproduce

1. Log in as the seeded admin (`Admin123!@#`)
2. Get redirected to the change-password page
3. Attempt to reset password to the same seeded password → 400 (rejected as matching, which is correct)
4. Navigate back to the main page (browser back or direct URL)
5. **Result**: User lands on the main page but all API calls fail with 403 "Password change required"

## Additional Observation

* After logging out and logging back in with the old seeded password, the change-password page appears again (the `MustChangePassword` flag was never cleared, which is correct)
* The backend enforcement is working — this is a client-side routing issue only

## Expected Behavior

* The client should redirect back to the change-password page if `MustChangePassword` is still true, rather than allowing the user to sit on a broken main page

## Acceptance Criteria

- [ ] User cannot bypass the forced password change screen by navigating away
- [ ] Client redirects back to change-password when API returns 403 "Password change required"
- [ ] 400 response on password change (e.g., same password) keeps the user on the change-password page with a clear error message
