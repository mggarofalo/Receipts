# Group 5: Receipt List/Detail/Edit

**Session:** `qa-group-5`
**Dependencies:** Group 4 (needs at least one receipt to exist)
**Data created:** None (may modify an existing receipt if edit is available)

## Prerequisites

- Application running at `${BASE_URL}`
- Valid credentials: `${EMAIL}` / `${PASSWORD}`
- At least one receipt exists in the system (created by Group 4 wizard or API fallback)

## Login

Follow the login flow from qa-suite-common.md using SESSION, EMAIL, PASSWORD.

## Tests

### Test 5.1: Receipt list page

**Steps:**

1. `browser-use --session qa-group-5 open ${BASE_URL}/receipts`
2. `browser-use --session qa-group-5 wait text "Receipts"`
3. `browser-use --session qa-group-5 state` -- verify H1 is "Receipts" and table renders with columns

**Pass criteria:** Page loads at `/receipts` with H1 "Receipts". A data table is visible with columns (e.g., Location, Date, Tax Amount, or similar). At least one receipt row is present (from Group 4).

**On failure:** `browser-use --session qa-group-5 screenshot "$TEMP/qa-suite-group5-list-fail.png"`

---

### Test 5.2: Receipt detail page

**Steps:**

1. `browser-use --session qa-group-5 state` -- find a clickable receipt row in the table
2. `browser-use --session qa-group-5 click RECEIPT_ROW_INDEX` -- click the first receipt row
3. `browser-use --session qa-group-5 wait text "Receipt Details"` -- wait for detail page
4. `browser-use --session qa-group-5 eval "window.location.pathname + window.location.search"` -- verify URL is `/receipt-detail?id=...`
5. `browser-use --session qa-group-5 state` -- verify detail page content

**Pass criteria:** Receipt detail page loads with "Receipt Details" heading. The page shows:
- A balance summary section (tax amount, totals)
- A receipt items section (line items table or list)
- A transactions section (transaction entries)

The URL should be `/receipt-detail?id=<guid>`.

**On failure:** `browser-use --session qa-group-5 screenshot "$TEMP/qa-suite-group5-detail-fail.png"`

---

### Test 5.3: Receipt edit (if available)

**Steps:**

1. `browser-use --session qa-group-5 state` -- look for an Edit button on the receipt detail page
2. If an Edit button exists:
   a. `browser-use --session qa-group-5 click EDIT_INDEX` -- click Edit
   b. `browser-use --session qa-group-5 state` -- verify edit form or mode appears
   c. Modify one field (e.g., change the tax amount or a line item description)
   d. `browser-use --session qa-group-5 click SAVE_INDEX` -- click Save
   e. `browser-use --session qa-group-5 wait text "successfully"` -- wait for success toast
   f. `browser-use --session qa-group-5 state` -- verify the change is reflected
3. If no Edit button exists: mark this test as SKIP with details "No edit button found on receipt detail page"

**Pass criteria:** If edit is available: the field is successfully modified and the updated value is visible after save. If edit is not available: test is SKIP (not FAIL).

**On failure:** `browser-use --session qa-group-5 screenshot "$TEMP/qa-suite-group5-edit-fail.png"`

---

### Test 5.4: Back navigation

**Steps:**

1. `browser-use --session qa-group-5 eval "window.location.pathname"` -- confirm currently on `/receipt-detail`
2. `browser-use --session qa-group-5 state` -- find a Back button (could be labeled "Back", "Back to Receipts", or be an arrow icon)
3. `browser-use --session qa-group-5 click BACK_INDEX` -- click Back
4. `browser-use --session qa-group-5 wait text "Receipts"` -- wait for list page
5. `browser-use --session qa-group-5 eval "window.location.pathname"` -- verify URL is `/receipts`

**Pass criteria:** Clicking the back button on the receipt detail page navigates back to `/receipts` and the receipt list is visible with H1 "Receipts".

**On failure:** `browser-use --session qa-group-5 screenshot "$TEMP/qa-suite-group5-backnav-fail.png"`

---

### Test 5.5: Empty detail graceful handling

**Steps:**

1. `browser-use --session qa-group-5 open ${BASE_URL}/receipt-detail` -- navigate to detail page without `?id` param
2. `browser-use --session qa-group-5 eval "window.location.pathname"` -- check if redirected
3. `browser-use --session qa-group-5 state` -- inspect page content

**Pass criteria:** One of the following graceful behaviors occurs:
- Redirects to `/receipts` (the list page)
- Shows an empty state or placeholder message (not a crash/error page)
- Shows "Receipts" heading (if redirected to list)

The page must NOT show an unhandled exception, blank white screen, or JavaScript error.

**On failure:** `browser-use --session qa-group-5 screenshot "$TEMP/qa-suite-group5-emptydetail-fail.png"`

## Cleanup

```
browser-use --session qa-group-5 close
```

## Result

Output structured JSON per qa-suite-common.md format.

```json
{
  "group": 5,
  "name": "Receipt List/Detail/Edit",
  "status": "PASS|FAIL|BLOCKED|PARTIAL",
  "tests": [
    {
      "name": "Receipt list page",
      "status": "PASS|FAIL|SKIP",
      "details": "...",
      "screenshot": "$TEMP/qa-suite-group5-list-fail.png"
    },
    {
      "name": "Receipt detail page",
      "status": "PASS|FAIL|SKIP",
      "details": "...",
      "screenshot": "$TEMP/qa-suite-group5-detail-fail.png"
    },
    {
      "name": "Receipt edit",
      "status": "PASS|FAIL|SKIP",
      "details": "...",
      "screenshot": "$TEMP/qa-suite-group5-edit-fail.png"
    },
    {
      "name": "Back navigation",
      "status": "PASS|FAIL|SKIP",
      "details": "...",
      "screenshot": "$TEMP/qa-suite-group5-backnav-fail.png"
    },
    {
      "name": "Empty detail graceful handling",
      "status": "PASS|FAIL|SKIP",
      "details": "...",
      "screenshot": "$TEMP/qa-suite-group5-emptydetail-fail.png"
    }
  ]
}
```
