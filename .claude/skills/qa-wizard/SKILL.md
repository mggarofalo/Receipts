---
name: qa-wizard
description: >
  End-to-end test of the 4-step receipt creation wizard, admin flows
  (audit log, recycle bin, user management), and error handling.
  Uses agent-browser + Aspire MCP. Produces a comprehensive pass/fail report.
allowed-tools: Bash, Read, Grep, Glob, mcp__aspire__list_resources, mcp__aspire__list_console_logs
user_invocable: true
argument: ""
---

# QA Wizard Testing

End-to-end test of the receipt creation wizard, admin-only features, and error handling flows.

**Duration:** ~5 minutes

## Prerequisites

- Aspire must be running (F5 or `dotnet run --project src/Receipts.AppHost`)
- agent-browser must be installed globally
- At least one account and one category must exist (for the wizard to reference). If not, create them first via `/qa-crud accounts` and `/qa-crud categories`.

## References

Before starting, read the reference file:
- `references/qa-common.md` — Port discovery, login flow, form patterns

## Phase 1: Port Discovery + Auth

1. **Port Discovery:** Call `mcp__aspire__list_console_logs` for `frontend` resource, parse the base URL.
2. **Login:** Follow the login flow from `qa-common.md` (try `Admin123!@#` first, fall back to `QaTest2024!@#`, handle password change if needed).
3. **Verify:** Confirm dashboard loads.

## Phase 2: Wizard Happy Path

Navigate through all 4 steps of the receipt creation wizard.

### Step 1: Trip Details

1. Navigate to the wizard:
   ```
   agent-browser open ${BASE_URL}/receipts/new
   ```
2. Wait and snapshot:
   ```
   agent-browser wait --load networkidle
   agent-browser snapshot -i
   ```
3. Verify:
   - H1 says "New Receipt"
   - Card title says "Trip Details"
   - Stepper shows Step 1 as active

4. Fill **Location** (Combobox with `allowCustom`):
   - Click the Location combobox trigger button
   - Snapshot to see the popover
   - Type a custom value in the search input: `agent-browser fill @eN "QA Store"`
   - The combobox should show "QA Store" as a custom option or accept it
   - If there's a create/select option for "QA Store", click it; otherwise press Enter

5. Fill **Date** (DateInput):
   ```
   agent-browser fill @eN "2026-03-18"
   ```

6. Fill **Tax Amount** (CurrencyInput):
   - Try `agent-browser fill @eN "1.50"` first
   - If the value doesn't stick, use the nativeInputValueSetter pattern from `qa-common.md`

7. Click **Next** button:
   ```
   agent-browser click @eN
   ```

8. Wait and verify Step 2 loads:
   ```
   agent-browser wait --load networkidle
   agent-browser snapshot -i
   ```
   Verify card title says "Transactions".

9. Record: **Step 1 — PASS/FAIL**

### Step 2: Transactions

1. Fill **Account** (Combobox — select existing):
   - Click the Account combobox trigger
   - Snapshot to see options
   - Click the first available account option

2. Fill **Amount** (CurrencyInput):
   - Fill with `11.50`

3. Fill **Date** (DateInput — should be pre-filled with receipt date):
   - Verify it's pre-filled; if not, fill with `2026-03-18`

4. Click **Add Transaction** button:
   ```
   agent-browser click @eN
   ```

5. Verify the transaction appears in the table below the form:
   ```
   agent-browser snapshot -i
   ```
   Look for the account name and `$11.50` in the table.

6. Click **Next** button:
   ```
   agent-browser click @eN
   ```

7. Verify Step 3 loads (card title: "Line Items").

8. Record: **Step 2 — PASS/FAIL**

### Step 3: Line Items

1. Fill **Item Code** (optional, text input):
   ```
   agent-browser fill @eN "QA-1"
   ```

2. Fill **Description** (text input with autocomplete popover):
   ```
   agent-browser fill @eN "QA Test Item"
   ```
   - If a suggestions popover appears, dismiss it by pressing Escape or clicking elsewhere:
     ```
     agent-browser press Escape
     ```

3. Fill **Quantity** (number input — should default to 1):
   - Verify it shows `1`; if not, fill with `1`

4. Fill **Unit Price** (CurrencyInput):
   - Fill with `10.00`

5. Fill **Category** (Combobox):
   - Click the Category combobox trigger
   - Snapshot to see options
   - Click the first available category

6. Skip **Subcategory** (optional).

7. Click **Add Item** button:
   ```
   agent-browser click @eN
   ```

8. Verify the item appears in the table:
   ```
   agent-browser snapshot -i
   ```
   Look for "QA Test Item" and `$10.00` in the table.

9. Click **Next** button:
   ```
   agent-browser click @eN
   ```

10. Verify Step 4 loads (review step).

11. Record: **Step 3 — PASS/FAIL**

### Step 4: Review & Submit

1. Snapshot the review page:
   ```
   agent-browser snapshot -i
   ```

2. Verify review data:
   - Location: "QA Store"
   - Date: "2026-03-18" (or formatted equivalent)
   - Tax: "$1.50"
   - Transaction: account name, $11.50
   - Item: "QA Test Item", qty 1, $10.00

3. Click **Submit** (or "Create Receipt") button:
   ```
   agent-browser click @eN
   ```

4. Wait for redirect:
   ```
   agent-browser wait --load networkidle
   ```

5. Check URL:
   ```
   agent-browser get url
   ```
   Expected: URL contains `/receipt-detail?id=`

6. Verify receipt detail page loads:
   ```
   agent-browser snapshot
   ```
   Look for "Receipt Details" heading.

7. Screenshot:
   ```
   agent-browser screenshot "$TEMP/qa-wizard-receipt-created.png"
   ```

8. Record: **Step 4 / Submit — PASS/FAIL**

## Phase 3: Wizard Cancel/Discard

1. Navigate to wizard again:
   ```
   agent-browser open ${BASE_URL}/receipts/new
   ```

2. Wait and snapshot:
   ```
   agent-browser wait --load networkidle
   agent-browser snapshot -i
   ```

3. Fill the Location field with something (to trigger "has data" state):
   - Click Location combobox, type "Discard Test"

4. Click the **X** (close/cancel) button in the top-right:
   ```
   agent-browser snapshot -i
   agent-browser click @eN  (the X button with sr-only "Cancel")
   ```

5. Verify the discard confirmation dialog appears:
   ```
   agent-browser snapshot -i
   ```
   Look for "Discard receipt?" title and "Continue editing" / "Discard" buttons.

6. Click **Discard**:
   ```
   agent-browser click @eN
   ```

7. Wait and verify redirect to `/receipts`:
   ```
   agent-browser wait --load networkidle
   agent-browser get url
   ```
   Expected: URL ends with `/receipts`

8. Record: **Cancel/Discard — PASS/FAIL**

## Phase 4: Admin Flows

Test admin-only features.

### 4.1: Audit Log

1. Navigate to audit log:
   ```
   agent-browser open ${BASE_URL}/audit
   ```

2. Wait and snapshot:
   ```
   agent-browser wait --load networkidle
   agent-browser snapshot
   ```

3. Verify:
   - H1 says "Audit Log"
   - Table with audit entries renders
   - Look for entries related to the receipt created in Phase 2

4. Record: **Audit Log — PASS/FAIL**

### 4.2: Recycle Bin

1. First, soft-delete a receipt to test with. Navigate to the receipts list and find one with a delete option. If no delete option is available on the list page, navigate to a receipt detail and look for a delete button there.

   Alternative approach if direct delete isn't available:
   - Soft-delete a receipt item via `/receipt-items` (check a checkbox, click Delete, confirm)
   - Then verify it appears in the recycle bin

2. Navigate to recycle bin:
   ```
   agent-browser open ${BASE_URL}/trash
   ```

3. Wait and snapshot:
   ```
   agent-browser wait --load networkidle
   agent-browser snapshot
   ```

4. Verify:
   - H1 says "Recycle Bin"
   - Page renders with tabs for different entity types
   - Look for the soft-deleted item

5. If a deleted item is found, test restore:
   - Find the restore button for the item
   - Click it
   - Verify the item is removed from the trash list

6. Record: **Recycle Bin — PASS/FAIL**

### 4.3: User Management

1. Navigate to user management:
   ```
   agent-browser open ${BASE_URL}/admin/users
   ```

2. Wait and snapshot:
   ```
   agent-browser wait --load networkidle
   agent-browser snapshot
   ```

3. Verify:
   - H1 says "User Management"
   - Table of users renders
   - Admin user (`admin@receipts.local`) appears in the list

4. Record: **User Management — PASS/FAIL**

## Phase 5: Error Handling

### 5.1: 404 Page

1. Navigate to a non-existent route:
   ```
   agent-browser open ${BASE_URL}/this-does-not-exist
   ```

2. Wait and snapshot:
   ```
   agent-browser wait --load networkidle
   agent-browser snapshot
   ```

3. Verify:
   - "Page Not Found" heading appears
   - Page renders without JS errors

4. Record: **404 Page — PASS/FAIL**

### 5.2: Unauthorized Redirect

1. Clear auth:
   ```
   agent-browser eval "localStorage.clear(); sessionStorage.clear();"
   ```

2. Navigate to protected route:
   ```
   agent-browser open ${BASE_URL}/receipts
   ```

3. Wait and check:
   ```
   agent-browser wait --load networkidle
   agent-browser get url
   ```

4. Verify redirect to `/login`.

5. Record: **Unauth Redirect — PASS/FAIL**

### 5.3: Wizard Validation

1. Log back in (follow login flow from `qa-common.md` using `QaTest2024!@#` or `Admin123!@#`).

2. Navigate to wizard:
   ```
   agent-browser open ${BASE_URL}/receipts/new
   ```

3. Wait and snapshot:
   ```
   agent-browser wait --load networkidle
   agent-browser snapshot -i
   ```

4. Without filling any fields, click **Next**:
   ```
   agent-browser click @eN  (the Next button)
   ```

5. Snapshot and verify validation errors appear:
   ```
   agent-browser snapshot -i
   ```
   Look for "Location is required", "Date is required" error messages.

6. Record: **Wizard Validation — PASS/FAIL**

## Phase 6: Report

Generate and display a comprehensive markdown report:

```markdown
# QA Wizard Report

**Date:** YYYY-MM-DD  |  **Base URL:** {BASE_URL}  |  **Result:** {PASS/FAIL}

## Wizard Happy Path

| Phase | Status | Details | Screenshot |
|-------|--------|---------|------------|
| Step 1: Trip Details | PASS | Location, date, tax filled | — |
| Step 2: Transactions | PASS | Transaction added ($11.50) | — |
| Step 3: Line Items | PASS | Item added (QA Test Item, $10.00) | — |
| Step 4: Review & Submit | PASS | Receipt created, redirected to detail | $TEMP/qa-wizard-receipt-created.png |

## Wizard Cancel/Discard

| Test | Status | Details |
|------|--------|---------|
| Cancel with data | PASS | Discard dialog shown, redirected to /receipts |

## Admin Flows

| Feature | Status | Details |
|---------|--------|---------|
| Audit Log | PASS | Entries rendered, wizard actions found |
| Recycle Bin | PASS | Deleted items shown, restore works |
| User Management | PASS | User list rendered, admin visible |

## Error Handling

| Test | Status | Details |
|------|--------|---------|
| 404 Page | PASS | "Page Not Found" rendered |
| Unauth Redirect | PASS | Redirected to /login |
| Wizard Validation | PASS | Required field errors shown |

## Summary

- Wizard: {N}/{N} steps passed
- Admin: {N}/{N} flows passed
- Error handling: {N}/{N} tests passed

**Overall: {PASS/FAIL}**
```

## Phase 7: Cleanup

```
agent-browser close
```

Print summary:
```
QA wizard: {steps_passed} wizard steps, {admin_passed} admin flows, {error_passed} error tests. Overall: {PASS/FAIL}
```

## Important Rules

- Every `agent-browser` command is a **separate Bash tool call** (no `&&` chaining).
- Screenshots go to **`$TEMP`** only.
- Ports are **never hardcoded** — discover via Aspire MCP.
- After any navigation or form interaction, take a fresh snapshot.
- The wizard creates real data (receipts, transactions, items). This is expected for QA testing.
- If the wizard submit fails due to missing accounts/categories, note it in the report and suggest running `/qa-crud accounts` and `/qa-crud categories` first.
