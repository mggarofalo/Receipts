# Group 10: Validation & Error Handling

**Session:** `qa-group-10`
**Dependencies:** None
**Data created:** None (read-only tests -- forms are submitted empty and rejected)

## Prerequisites

- Application running at `${BASE_URL}`
- Valid credentials: `${EMAIL}` / `${PASSWORD}`

## Login

Follow the login flow from qa-suite-common.md using SESSION, EMAIL, PASSWORD.

## Tests

### Test 10.1: Login Validation

**Steps:**

1. `browser-use --session qa-group-10 eval "localStorage.clear(); sessionStorage.clear();"` -- clear auth state
2. `browser-use --session qa-group-10 open ${BASE_URL}/login`
3. `browser-use --session qa-group-10 wait text "Sign In"`
4. `browser-use --session qa-group-10 state` -- find email input, password input, Sign In button
5. `browser-use --session qa-group-10 click SUBMIT_INDEX` -- submit empty form
6. `browser-use --session qa-group-10 state` -- check for validation error messages on email/password fields
7. `browser-use --session qa-group-10 input EMAIL_INDEX "${EMAIL}"` -- enter valid email
8. `browser-use --session qa-group-10 input PASSWORD_INDEX "WrongPassword999!"` -- enter wrong password
9. `browser-use --session qa-group-10 click SUBMIT_INDEX` -- submit with wrong password
10. `browser-use --session qa-group-10 state` -- check for error message (e.g., "Invalid credentials" or similar)

**Pass criteria:** Empty form submission shows validation errors. Wrong password submission shows an authentication error message. Neither submission navigates away from `/login`.

**On failure:** `browser-use --session qa-group-10 screenshot "$TEMP/qa-suite-group10-login-validation-fail.png"`

**Restore:** Log back in with correct credentials using the login flow from qa-suite-common.md before continuing.

> **Note:** Entity-level validation tests (accounts, categories, subcategories, receipt items, transactions) are covered by their respective CRUD groups (2, 3, 6). This group focuses on login validation, error boundaries, and missing-ID edge cases.

---

### Test 10.2: Error Boundary (formerly 10.7)

**Steps:**

1. `browser-use --session qa-group-10 open ${BASE_URL}/`
2. `browser-use --session qa-group-10 wait text "Dashboard"`
3. `browser-use --session qa-group-10 eval "!!document.querySelector('[data-sentry-component]') || !!window.__SENTRY__ || document.documentElement.innerHTML.includes('ErrorBoundary')"` -- check for error boundary or Sentry integration

**Pass criteria:** The eval returns evidence that an error boundary or Sentry error tracking is present in the application. Note: this may return `false` if the boundary only renders on error -- that is acceptable. Record the result as-is.

**On failure:** `browser-use --session qa-group-10 screenshot "$TEMP/qa-suite-group10-errorboundary-fail.png"`

---

### Test 10.3: Detail Without ID

**Steps:**

1. `browser-use --session qa-group-10 open ${BASE_URL}/receipt-detail`
2. `browser-use --session qa-group-10 state` -- check page behavior
3. `browser-use --session qa-group-10 eval "window.location.pathname"` -- check current URL

**Pass criteria:** Navigating to `/receipt-detail` without an `?id=` query parameter either redirects to `/receipts` or shows a graceful empty/error state. The page does NOT crash or show an unhandled error.

**On failure:** `browser-use --session qa-group-10 screenshot "$TEMP/qa-suite-group10-receipt-noid-fail.png"`

---

### Test 10.4: Transaction Detail Without ID

**Steps:**

1. `browser-use --session qa-group-10 open ${BASE_URL}/transaction-detail`
2. `browser-use --session qa-group-10 state` -- check page behavior
3. `browser-use --session qa-group-10 eval "window.location.pathname"` -- check current URL

**Pass criteria:** Navigating to `/transaction-detail` without an `?id=` query parameter is handled gracefully -- either redirects to `/transactions`, shows an empty state, or displays a user-friendly message. The page does NOT crash or show an unhandled error.

**On failure:** `browser-use --session qa-group-10 screenshot "$TEMP/qa-suite-group10-txn-noid-fail.png"`

## Cleanup

```
browser-use --session qa-group-10 close
```

## Result

Output structured JSON per qa-suite-common.md format.

```json
{
  "group": 10,
  "name": "Validation & Error Handling",
  "status": "PASS|FAIL|BLOCKED|PARTIAL",
  "tests": [
    {
      "name": "Login Validation",
      "status": "PASS|FAIL|SKIP",
      "details": "...",
      "screenshot": "$TEMP/qa-suite-group10-login-validation-fail.png"
    },
    {
      "name": "Error Boundary",
      "status": "PASS|FAIL|SKIP",
      "details": "...",
      "screenshot": "$TEMP/qa-suite-group10-errorboundary-fail.png"
    },
    {
      "name": "Detail Without ID",
      "status": "PASS|FAIL|SKIP",
      "details": "...",
      "screenshot": "$TEMP/qa-suite-group10-receipt-noid-fail.png"
    },
    {
      "name": "Transaction Detail Without ID",
      "status": "PASS|FAIL|SKIP",
      "details": "...",
      "screenshot": "$TEMP/qa-suite-group10-txn-noid-fail.png"
    }
  ]
}
```
