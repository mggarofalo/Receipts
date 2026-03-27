# Group 1: Auth & Session Management

**Session:** `qa-group-1`
**Dependencies:** None
**Data created:** None

## Prerequisites

- Application running at `${BASE_URL}`
- Valid credentials: `${EMAIL}` / `${PASSWORD}`
- Admin user (seeded `admin@receipts.local` has admin role)

## Login

Follow the login flow from qa-suite-common.md using SESSION, EMAIL, PASSWORD.

## Tests

### Test 1.1: Login with valid credentials

**Steps:**

1. `browser-use --session qa-group-1 open ${BASE_URL}/login`
2. `browser-use --session qa-group-1 wait text "Sign In"`
3. `browser-use --session qa-group-1 state` -- find email input, password input, Sign In button indices
4. `browser-use --session qa-group-1 input EMAIL_INDEX "${EMAIL}"`
5. `browser-use --session qa-group-1 input PASSWORD_INDEX "${PASSWORD}"`
6. `browser-use --session qa-group-1 click SUBMIT_INDEX`
7. `browser-use --session qa-group-1 wait text "Dashboard"`
8. `browser-use --session qa-group-1 eval "window.location.pathname"` -- verify URL is `/`

**Pass criteria:** Page shows "Dashboard" heading and URL is `/`. If redirected to `/change-password`, follow the password change flow from qa-suite-common.md, then verify dashboard loads.

**On failure:** `browser-use --session qa-group-1 screenshot "$TEMP/qa-suite-group1-login-fail.png"`

---

### Test 1.2: Auth guard redirect

**Steps:**

1. `browser-use --session qa-group-1 close` -- close authenticated session
2. `browser-use --session qa-group-1 open ${BASE_URL}/accounts` -- navigate to protected route without auth
3. `browser-use --session qa-group-1 wait text "Sign In"` -- should redirect to login
4. `browser-use --session qa-group-1 eval "window.location.pathname"` -- verify URL is `/login`

**Pass criteria:** Unauthenticated navigation to `/accounts` redirects to `/login` and shows the "Sign In" heading.

**On failure:** `browser-use --session qa-group-1 screenshot "$TEMP/qa-suite-group1-authguard-fail.png"`

**Restore:** Re-authenticate using the login flow from qa-suite-common.md before continuing to Test 1.3.

---

### Test 1.3: Admin guard

**Steps:**

1. `browser-use --session qa-group-1 open ${BASE_URL}/admin/users`
2. `browser-use --session qa-group-1 wait text "User Management"`
3. `browser-use --session qa-group-1 state` -- verify H1 is "User Management"
4. `browser-use --session qa-group-1 eval "window.location.pathname"` -- verify URL is `/admin/users`

**Pass criteria:** Page loads with "User Management" heading at `/admin/users`. The seeded admin user has the admin role and should not be redirected.

**On failure:** `browser-use --session qa-group-1 screenshot "$TEMP/qa-suite-group1-adminguard-fail.png"`

---

### Test 1.4: Logout

**Steps:**

1. `browser-use --session qa-group-1 open ${BASE_URL}/` -- ensure on dashboard
2. `browser-use --session qa-group-1 wait text "Dashboard"`
3. `browser-use --session qa-group-1 state` -- find user menu / avatar button in the header
4. `browser-use --session qa-group-1 click USER_MENU_INDEX` -- click user menu trigger
5. `browser-use --session qa-group-1 state` -- find "Log Out" or "Sign Out" menu item
6. `browser-use --session qa-group-1 click LOGOUT_INDEX` -- click logout option
7. `browser-use --session qa-group-1 wait text "Sign In"` -- should redirect to login
8. `browser-use --session qa-group-1 eval "window.location.pathname"` -- verify URL is `/login`

**Pass criteria:** After clicking logout, user is redirected to `/login` and sees the "Sign In" heading.

**On failure:** `browser-use --session qa-group-1 screenshot "$TEMP/qa-suite-group1-logout-fail.png"`

**Restore:** Re-authenticate using the login flow from qa-suite-common.md before continuing to Test 1.5.

---

## Cleanup

```
browser-use --session qa-group-1 close
```

## Result

Output structured JSON per qa-suite-common.md format.

```json
{
  "group": 1,
  "name": "Auth & Session Management",
  "status": "PASS|FAIL|BLOCKED|PARTIAL",
  "tests": [
    {
      "name": "Login with valid credentials",
      "status": "PASS|FAIL|SKIP",
      "details": "...",
      "screenshot": "$TEMP/qa-suite-group1-login-fail.png"
    },
    {
      "name": "Auth guard redirect",
      "status": "PASS|FAIL|SKIP",
      "details": "...",
      "screenshot": "$TEMP/qa-suite-group1-authguard-fail.png"
    },
    {
      "name": "Admin guard",
      "status": "PASS|FAIL|SKIP",
      "details": "...",
      "screenshot": "$TEMP/qa-suite-group1-adminguard-fail.png"
    },
    {
      "name": "Logout",
      "status": "PASS|FAIL|SKIP",
      "details": "...",
      "screenshot": "$TEMP/qa-suite-group1-logout-fail.png"
    },
  ]
}
```
