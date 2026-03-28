# Group 2: Account CRUD

**Session:** `qa-group-2`
**Dependencies:** None
**Data created:** Account with code `QA-001` / name `QA Test Account` (later updated to `QA Test Account Updated`)

## Prerequisites

- Application running at `${BASE_URL}`
- Valid credentials: `${EMAIL}` / `${PASSWORD}`

## Login

Follow the login flow from qa-suite-common.md using SESSION, EMAIL, PASSWORD.

## Tests

### Test 2.1: Account list page

**Steps:**

1. `browser-use --session qa-group-2 open ${BASE_URL}/accounts`
2. `browser-use --session qa-group-2 wait text "Accounts"`
3. `browser-use --session qa-group-2 state` -- verify H1 is "Accounts" and table renders

**Pass criteria:** Page loads at `/accounts` with H1 "Accounts" and a data table is visible (even if empty). A "New Account" button is present.

**On failure:** `browser-use --session qa-group-2 screenshot "$TEMP/qa-suite-group2-list-fail.png"`

---

### Test 2.2: Create account

**Steps:**

1. `browser-use --session qa-group-2 state` -- find "New Account" button index
2. `browser-use --session qa-group-2 click NEW_ACCOUNT_INDEX` -- click "New Account"
3. `browser-use --session qa-group-2 wait text "Create Account"` -- wait for dialog
4. `browser-use --session qa-group-2 state` -- find Account Code input, Name input, submit button indices
5. `browser-use --session qa-group-2 input ACCOUNT_CODE_INDEX "QA-001"`
6. `browser-use --session qa-group-2 input NAME_INDEX "QA Test Account"`
7. `browser-use --session qa-group-2 click SUBMIT_INDEX` -- click submit/save button
8. `browser-use --session qa-group-2 wait text "successfully"` -- wait for success toast
9. `browser-use --session qa-group-2 state` -- verify table now contains the new row

**Pass criteria:** Success toast appears. Table contains a row with "QA-001" in the Account Code column and "QA Test Account" in the Name column.

**On failure:** `browser-use --session qa-group-2 screenshot "$TEMP/qa-suite-group2-create-fail.png"`

---

### Test 2.3: Read / verify account

**Steps:**

1. `browser-use --session qa-group-2 open ${BASE_URL}/accounts` -- ensure on accounts page
2. `browser-use --session qa-group-2 wait text "Accounts"`
3. `browser-use --session qa-group-2 state` -- scan table for QA-001 row

**Pass criteria:** The `state` output contains a row with "QA-001" and "QA Test Account". The Active status shows as checked/active (default).

**On failure:** `browser-use --session qa-group-2 screenshot "$TEMP/qa-suite-group2-read-fail.png"`

---

### Test 2.4: Update account

**Steps:**

1. `browser-use --session qa-group-2 state` -- find the edit button (pencil icon, aria-label="Edit") in the QA-001 row
2. `browser-use --session qa-group-2 click EDIT_INDEX` -- click edit button
3. `browser-use --session qa-group-2 wait text "Edit Account"` -- wait for edit dialog
4. `browser-use --session qa-group-2 state` -- find the Name input index
5. Clear the Name field and type new value:
   - `browser-use --session qa-group-2 eval "const input = document.querySelector('input[name=\"name\"]'); const nativeSetter = Object.getOwnPropertyDescriptor(window.HTMLInputElement.prototype, 'value').set; nativeSetter.call(input, ''); input.dispatchEvent(new Event('input', { bubbles: true }));"`
   - `browser-use --session qa-group-2 input NAME_INDEX "QA Test Account Updated"`
6. `browser-use --session qa-group-2 click SUBMIT_INDEX` -- click submit/save button
7. `browser-use --session qa-group-2 wait text "successfully"` -- wait for success toast
8. `browser-use --session qa-group-2 state` -- verify table shows updated name

**Pass criteria:** Success toast appears. Table row for QA-001 now shows "QA Test Account Updated" in the Name column.

**On failure:** `browser-use --session qa-group-2 screenshot "$TEMP/qa-suite-group2-update-fail.png"`

---

### Test 2.5: Validation errors

**Steps:**

1. `browser-use --session qa-group-2 state` -- find "New Account" button index
2. `browser-use --session qa-group-2 click NEW_ACCOUNT_INDEX` -- open create dialog
3. `browser-use --session qa-group-2 wait text "Create Account"` -- wait for dialog
4. `browser-use --session qa-group-2 state` -- find submit button index
5. `browser-use --session qa-group-2 click SUBMIT_INDEX` -- submit empty form
6. `browser-use --session qa-group-2 state` -- check for validation error messages

**Pass criteria:** Validation errors appear for both "Account Code" and "Name" fields (e.g., "required", "is required", or similar error text near each field).

**On failure:** `browser-use --session qa-group-2 screenshot "$TEMP/qa-suite-group2-validation-fail.png"`

**Restore:** Close the dialog (click X or press Escape) before cleanup.

## Cleanup

```
browser-use --session qa-group-2 close
```

Note: The created account (`QA-001` / `QA Test Account Updated`) is intentionally left in the database. Other test groups (e.g., Group 4 wizard) may depend on it. Cleanup of QA test data should be handled by the orchestrator or a dedicated teardown step.

## Result

Output structured JSON per qa-suite-common.md format.

```json
{
  "group": 2,
  "name": "Account CRUD",
  "status": "PASS|FAIL|BLOCKED|PARTIAL",
  "tests": [
    {
      "name": "Account list page",
      "status": "PASS|FAIL|SKIP",
      "details": "...",
      "screenshot": "$TEMP/qa-suite-group2-list-fail.png"
    },
    {
      "name": "Create account",
      "status": "PASS|FAIL|SKIP",
      "details": "...",
      "screenshot": "$TEMP/qa-suite-group2-create-fail.png"
    },
    {
      "name": "Read / verify account",
      "status": "PASS|FAIL|SKIP",
      "details": "...",
      "screenshot": "$TEMP/qa-suite-group2-read-fail.png"
    },
    {
      "name": "Update account",
      "status": "PASS|FAIL|SKIP",
      "details": "...",
      "screenshot": "$TEMP/qa-suite-group2-update-fail.png"
    },
    {
      "name": "Validation errors",
      "status": "PASS|FAIL|SKIP",
      "details": "...",
      "screenshot": "$TEMP/qa-suite-group2-validation-fail.png"
    }
  ]
}
```
