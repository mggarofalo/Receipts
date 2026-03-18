---
name: smoke-test
description: >
  Smoke test every route in the Receipts app. Verifies pages load,
  expected headings exist, no JS errors, and auth guards work.
  Uses agent-browser + Aspire MCP. Produces a markdown pass/fail report.
allowed-tools: Bash, Read, Grep, Glob, mcp__aspire__list_resources, mcp__aspire__list_console_logs
user_invocable: true
argument: ""
---

# Smoke Test

Verify every route in the Receipts app loads correctly, displays the expected heading, and has no JavaScript errors. Also test auth guards (unauthenticated redirect, non-admin redirect).

**Duration:** ~2 minutes

## Prerequisites

- Aspire must be running (F5 or `dotnet run --project src/Receipts.AppHost`)
- agent-browser must be installed globally (`agent-browser --version`)

## References

Before starting, read both reference files:
- `references/qa-common.md` — Port discovery, login flow, form patterns
- `references/qa-routes.md` — Complete route matrix with expected headings

## Phase 1: Port Discovery

1. Call `mcp__aspire__list_console_logs` for the `frontend` resource.
2. Parse the logs for `Local:   http://localhost:XXXX` to get the base URL.
3. If no frontend resource or no URL found, abort: "Aspire is not running."
4. Store the URL as `BASE_URL`.

## Phase 2: Auth Bootstrap

Follow the login flow from `qa-common.md`:

1. Open `${BASE_URL}/login`
2. Wait for network idle
3. Snapshot to get form refs
4. Fill email: `admin@receipts.local`
5. Fill password: `Admin123!@#` (fall back to `QaTest2024!@#` if login fails)
6. Click "Sign In"
7. Wait for navigation
8. Check URL — if `/change-password`, follow the password change flow from `qa-common.md`
9. Verify dashboard loads (URL ends with `/`)
10. Screenshot: `$TEMP/smoke-login-success.png`

## Phase 3: Route Crawl

For each of the 19 routes listed in `qa-routes.md` (skip `/login` and `/change-password` — already tested in Phase 2), perform these steps:

### Per-Route Steps

1. **Navigate:**
   ```
   agent-browser open ${BASE_URL}${path}
   ```

2. **Wait for load:**
   ```
   agent-browser wait --load networkidle
   ```

3. **Snapshot** (full accessibility tree, no `-i`):
   ```
   agent-browser snapshot
   ```

4. **Check heading:** Search the snapshot output for the expected H1 text from `qa-routes.md`. Mark PASS if found, FAIL if not.

5. **Check for errors:** Look for error indicators in the snapshot:
   - Error boundary text ("Something went wrong")
   - Blank page (very few nodes in tree)
   - Alert with "error" or "failed" text

6. **On FAIL:** Take a screenshot for debugging:
   ```
   agent-browser screenshot "$TEMP/smoke-fail-{path-slug}.png"
   ```

### Route-Specific Notes

- **`/` (Dashboard):** May take a moment to load data. Wait for networkidle.
- **`/receipts/new`:** Verify "Trip Details" card title appears (it's the Step 1 heading).
- **`/receipt-detail`:** Without `?id=` param, verify page loads without crashing. May show empty state.
- **`/transaction-detail`:** Same as receipt-detail — verify no crash without params.
- **`/audit`, `/trash`, `/admin/users`:** These are admin routes. The seeded admin user should have access.

### Tracking

Maintain a results list:
```
results = [{ route, status, headingFound, errors, screenshot }]
```

## Phase 4: Auth Guard Tests

### Test 1: Unauthenticated Access

1. Clear auth state by evaluating:
   ```
   agent-browser eval "localStorage.clear(); sessionStorage.clear();"
   ```

2. Navigate to a protected route:
   ```
   agent-browser open ${BASE_URL}/accounts
   ```

3. Wait and check URL:
   ```
   agent-browser wait --load networkidle
   agent-browser get url
   ```

4. **PASS** if redirected to `/login`. **FAIL** if the accounts page loads.

### Test 2: Non-Admin Access to Admin Route

This test is only possible if a non-admin user exists. If the seeded admin is the only user, skip this test and note it in the report as "SKIPPED — no non-admin user available."

If a non-admin user exists:
1. Log in as the non-admin user
2. Navigate to `${BASE_URL}/audit`
3. Verify redirect to `/` (dashboard)

## Phase 5: Report

Generate and display a markdown report:

```markdown
# Smoke Test Report

**Date:** YYYY-MM-DD  |  **Base URL:** {BASE_URL}  |  **Result:** {PASS/FAIL}

## Route Results

| # | Route | Status | Heading | Errors | Screenshot |
|---|-------|--------|---------|--------|------------|
| 1 | / | PASS | Dashboard | None | — |
| 2 | /accounts | PASS | Accounts | None | — |
| ... | ... | ... | ... | ... | ... |

## Auth Guard Results

| Test | Status | Details |
|------|--------|---------|
| Unauth → /accounts | PASS | Redirected to /login |
| Non-admin → /audit | SKIPPED | No non-admin user |

## Summary

- Routes tested: {N}
- Passed: {N}
- Failed: {N}
- Auth guard tests: {N passed} / {N total}

**Overall: {PASS/FAIL}**
```

A single route failure or auth guard failure = overall FAIL.

## Phase 6: Cleanup

```
agent-browser close
```

Print the summary line:
```
Smoke tested {N} routes. {P} passed, {F} failed. Auth guards: {pass/fail}. Overall: {PASS/FAIL}
```

## Important Rules

- Every `agent-browser` command is a **separate Bash tool call** (no `&&` chaining).
- Screenshots go to **`$TEMP`** only, never the working directory.
- Ports are **never hardcoded** — always discover via Aspire MCP.
- After any navigation, take a fresh snapshot before checking content.
- Do not modify any application data during smoke testing.
