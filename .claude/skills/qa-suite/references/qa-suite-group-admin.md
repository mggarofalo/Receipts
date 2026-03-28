# Group 8: Admin Features

**Session:** `qa-group-8`
**Dependencies:** None
**Data created:** One item template ("QA Template", then deleted)

## Prerequisites

- Application running at `${BASE_URL}`
- Valid credentials: `${EMAIL}` / `${PASSWORD}`
- Logged-in user must have admin role (seeded `admin@receipts.local` has admin role)

## Login

Follow the login flow from qa-suite-common.md using SESSION, EMAIL, PASSWORD.

## Tests

### Test 8.1: User Management

**Steps:**

1. `browser-use --session qa-group-8 open ${BASE_URL}/admin/users`
2. `browser-use --session qa-group-8 wait text "User Management"`
3. `browser-use --session qa-group-8 state` -- verify H1 is "User Management"
4. Look for the admin user (`admin@receipts.local`) in the user table

**Pass criteria:** Page loads at `/admin/users` with H1 heading "User Management". A table of users renders and includes the admin user.

**On failure:** `browser-use --session qa-group-8 screenshot "$TEMP/qa-suite-group8-usermgmt-fail.png"`

---

### Test 8.2: Audit Log

**Steps:**

1. `browser-use --session qa-group-8 open ${BASE_URL}/audit`
2. `browser-use --session qa-group-8 wait text "Audit Log"`
3. `browser-use --session qa-group-8 state` -- verify H1 is "Audit Log"
4. Look for audit entries with timestamps in the table/list

**Pass criteria:** Page loads at `/audit` with H1 heading "Audit Log". Audit entries are visible with timestamps.

**On failure:** `browser-use --session qa-group-8 screenshot "$TEMP/qa-suite-group8-auditlog-fail.png"`

---

### Test 8.3: Recycle Bin

**Steps:**

1. `browser-use --session qa-group-8 open ${BASE_URL}/trash`
2. `browser-use --session qa-group-8 wait text "Recycle Bin"`
3. `browser-use --session qa-group-8 state` -- verify H1 is "Recycle Bin"
4. Note the contents (may be empty or contain previously deleted items)

**Pass criteria:** Page loads at `/trash` with H1 heading "Recycle Bin". The page renders without errors (contents may be empty).

**On failure:** `browser-use --session qa-group-8 screenshot "$TEMP/qa-suite-group8-recyclebin-fail.png"`

---

### Test 8.4: Item Templates List

**Steps:**

1. `browser-use --session qa-group-8 open ${BASE_URL}/item-templates`
2. `browser-use --session qa-group-8 wait text "Item Templates"`
3. `browser-use --session qa-group-8 state` -- verify H1 is "Item Templates"

**Pass criteria:** Page loads at `/item-templates` with H1 heading "Item Templates" and the table renders (even if empty).

**On failure:** `browser-use --session qa-group-8 screenshot "$TEMP/qa-suite-group8-templates-list-fail.png"`

---

### Test 8.5: Item Template Create

**Steps:**

1. `browser-use --session qa-group-8 state` -- find "New Template" button
2. `browser-use --session qa-group-8 click NEW_BUTTON_INDEX` -- click "New Template"
3. `browser-use --session qa-group-8 wait text "Create Item Template"`
4. `browser-use --session qa-group-8 state` -- find Name input and submit button
5. `browser-use --session qa-group-8 input NAME_INDEX "QA Template"` -- enter Name
6. `browser-use --session qa-group-8 click SUBMIT_INDEX` -- submit form
7. `browser-use --session qa-group-8 wait text "successfully"` -- wait for success toast
8. `browser-use --session qa-group-8 state` -- verify "QA Template" appears in the table

**Pass criteria:** Table shows new row with "QA Template" in the Name column. Success toast appeared.

**On failure:** `browser-use --session qa-group-8 screenshot "$TEMP/qa-suite-group8-template-create-fail.png"`

---

### Test 8.6: Item Template Delete

**Steps:**

1. `browser-use --session qa-group-8 state` -- find checkbox for the "QA Template" row
2. `browser-use --session qa-group-8 click CHECKBOX_INDEX` -- check the checkbox
3. `browser-use --session qa-group-8 state` -- find "Delete (1)" button
4. `browser-use --session qa-group-8 click DELETE_BUTTON_INDEX` -- click Delete
5. `browser-use --session qa-group-8 state` -- find confirmation dialog with confirm button
6. `browser-use --session qa-group-8 click CONFIRM_INDEX` -- confirm deletion
7. `browser-use --session qa-group-8 wait text "successfully"` -- wait for success toast
8. `browser-use --session qa-group-8 state` -- verify "QA Template" row is removed from the table

**Pass criteria:** "QA Template" row no longer appears in the table. Success toast appeared.

**On failure:** `browser-use --session qa-group-8 screenshot "$TEMP/qa-suite-group8-template-delete-fail.png"`

## Cleanup

```
browser-use --session qa-group-8 close
```

## Result

Output structured JSON per qa-suite-common.md format.

```json
{
  "group": 8,
  "name": "Admin Features",
  "status": "PASS|FAIL|BLOCKED|PARTIAL",
  "tests": [
    {
      "name": "User Management",
      "status": "PASS|FAIL|SKIP",
      "details": "...",
      "screenshot": "$TEMP/qa-suite-group8-usermgmt-fail.png"
    },
    {
      "name": "Audit Log",
      "status": "PASS|FAIL|SKIP",
      "details": "...",
      "screenshot": "$TEMP/qa-suite-group8-auditlog-fail.png"
    },
    {
      "name": "Recycle Bin",
      "status": "PASS|FAIL|SKIP",
      "details": "...",
      "screenshot": "$TEMP/qa-suite-group8-recyclebin-fail.png"
    },
    {
      "name": "Item Templates List",
      "status": "PASS|FAIL|SKIP",
      "details": "...",
      "screenshot": "$TEMP/qa-suite-group8-templates-list-fail.png"
    },
    {
      "name": "Item Template Create",
      "status": "PASS|FAIL|SKIP",
      "details": "...",
      "screenshot": "$TEMP/qa-suite-group8-template-create-fail.png"
    },
    {
      "name": "Item Template Delete",
      "status": "PASS|FAIL|SKIP",
      "details": "...",
      "screenshot": "$TEMP/qa-suite-group8-template-delete-fail.png"
    }
  ]
}
```
