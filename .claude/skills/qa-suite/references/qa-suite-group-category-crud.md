# Group 3: Category & Subcategory CRUD

**Session:** `qa-group-3`
**Dependencies:** None
**Data created:** Category `QA Category` (updated to `QA Category Updated`), Subcategory `QA Subcategory` (updated to `QA Sub Updated`)

## Prerequisites

- Application running at `${BASE_URL}`
- Valid credentials: `${EMAIL}` / `${PASSWORD}`

## Login

Follow the login flow from qa-suite-common.md using SESSION, EMAIL, PASSWORD.

## Tests

### Test 3.1: Category list page

**Steps:**

1. `browser-use --session qa-group-3 open ${BASE_URL}/categories`
2. `browser-use --session qa-group-3 wait text "Categories"`
3. `browser-use --session qa-group-3 state` -- verify H1 is "Categories" and table/list renders

**Pass criteria:** Page loads at `/categories` with H1 "Categories". A "New Category" button is present.

**On failure:** `browser-use --session qa-group-3 screenshot "$TEMP/qa-suite-group3-catlist-fail.png"`

---

### Test 3.2: Category create

**Steps:**

1. `browser-use --session qa-group-3 state` -- find "New Category" button index
2. `browser-use --session qa-group-3 click NEW_CATEGORY_INDEX` -- click "New Category"
3. `browser-use --session qa-group-3 wait text "Create Category"` -- wait for dialog
4. `browser-use --session qa-group-3 state` -- find Name input, Description input, submit button indices
5. `browser-use --session qa-group-3 input NAME_INDEX "QA Category"`
6. `browser-use --session qa-group-3 input DESCRIPTION_INDEX "Test category for QA"`
7. `browser-use --session qa-group-3 click SUBMIT_INDEX` -- click submit/save button
8. `browser-use --session qa-group-3 wait text "successfully"` -- wait for success toast
9. `browser-use --session qa-group-3 state` -- verify "QA Category" appears in the table

**Pass criteria:** Success toast appears. Table contains a row with "QA Category" in the Name column.

**On failure:** `browser-use --session qa-group-3 screenshot "$TEMP/qa-suite-group3-catcreate-fail.png"`

---

### Test 3.3: Category update

**Steps:**

1. `browser-use --session qa-group-3 state` -- find the edit button (pencil icon, aria-label="Edit") in the QA Category row
2. `browser-use --session qa-group-3 click EDIT_INDEX` -- click edit button
3. `browser-use --session qa-group-3 wait text "Edit Category"` -- wait for edit dialog
4. `browser-use --session qa-group-3 state` -- find the Name input index
5. Clear the Name field and type new value:
   - `browser-use --session qa-group-3 eval "const input = document.querySelector('input[name=\"name\"]'); const nativeSetter = Object.getOwnPropertyDescriptor(window.HTMLInputElement.prototype, 'value').set; nativeSetter.call(input, ''); input.dispatchEvent(new Event('input', { bubbles: true }));"`
   - `browser-use --session qa-group-3 input NAME_INDEX "QA Category Updated"`
6. `browser-use --session qa-group-3 click SUBMIT_INDEX` -- click submit/save button
7. `browser-use --session qa-group-3 wait text "successfully"` -- wait for success toast
8. `browser-use --session qa-group-3 state` -- verify table shows "QA Category Updated"

**Pass criteria:** Success toast appears. Table row now shows "QA Category Updated" in the Name column.

**On failure:** `browser-use --session qa-group-3 screenshot "$TEMP/qa-suite-group3-catupdate-fail.png"`

---

### Test 3.4: Category validation

**Steps:**

1. `browser-use --session qa-group-3 state` -- find "New Category" button index
2. `browser-use --session qa-group-3 click NEW_CATEGORY_INDEX` -- open create dialog
3. `browser-use --session qa-group-3 wait text "Create Category"` -- wait for dialog
4. `browser-use --session qa-group-3 state` -- find submit button index
5. `browser-use --session qa-group-3 click SUBMIT_INDEX` -- submit empty form
6. `browser-use --session qa-group-3 state` -- check for validation error messages

**Pass criteria:** Validation error appears for the "Name" field (e.g., "required" or similar error text).

**On failure:** `browser-use --session qa-group-3 screenshot "$TEMP/qa-suite-group3-catvalidation-fail.png"`

**Restore:** Close the dialog (click X or press Escape) before continuing.

---

### Test 3.5: Subcategory list page

**Steps:**

1. `browser-use --session qa-group-3 open ${BASE_URL}/subcategories`
2. `browser-use --session qa-group-3 wait text "Subcategories"`
3. `browser-use --session qa-group-3 state` -- verify H1 is "Subcategories"

**Pass criteria:** Page loads at `/subcategories` with H1 "Subcategories". A "New Subcategory" button is present.

**On failure:** `browser-use --session qa-group-3 screenshot "$TEMP/qa-suite-group3-sublist-fail.png"`

---

### Test 3.6: Subcategory create

**Steps:**

1. `browser-use --session qa-group-3 state` -- find "New Subcategory" button index
2. `browser-use --session qa-group-3 click NEW_SUBCATEGORY_INDEX` -- click "New Subcategory"
3. `browser-use --session qa-group-3 wait text "Create Subcategory"` -- wait for dialog
4. `browser-use --session qa-group-3 state` -- find Name input, Category combobox trigger, submit button indices
5. `browser-use --session qa-group-3 input NAME_INDEX "QA Subcategory"`
6. `browser-use --session qa-group-3 click CATEGORY_COMBOBOX_INDEX` -- click Category combobox trigger to open dropdown
7. `browser-use --session qa-group-3 state` -- find the category options in the opened popover
8. In the category popover, look for the option labeled "QA Category Updated" and click it. If not found, select the first available category as a fallback.
9. `browser-use --session qa-group-3 click SUBMIT_INDEX` -- click submit/save button
10. `browser-use --session qa-group-3 wait text "successfully"` -- wait for success toast
11. `browser-use --session qa-group-3 state` -- verify "QA Subcategory" appears (may need to click "Expand All" to see grouped subcategories)

**Pass criteria:** Success toast appears. "QA Subcategory" is visible in the subcategories list under its parent category group.

**On failure:** `browser-use --session qa-group-3 screenshot "$TEMP/qa-suite-group3-subcreate-fail.png"`

---

### Test 3.7: Subcategory update

**Steps:**

1. `browser-use --session qa-group-3 state` -- find the edit button (pencil icon) for the QA Subcategory row
2. `browser-use --session qa-group-3 click EDIT_INDEX` -- click edit button
3. `browser-use --session qa-group-3 wait text "Edit Subcategory"` -- wait for edit dialog
4. `browser-use --session qa-group-3 state` -- find Name input index
5. Clear the Name field and type new value:
   - `browser-use --session qa-group-3 eval "const input = document.querySelector('input[name=\"name\"]'); const nativeSetter = Object.getOwnPropertyDescriptor(window.HTMLInputElement.prototype, 'value').set; nativeSetter.call(input, ''); input.dispatchEvent(new Event('input', { bubbles: true }));"`
   - `browser-use --session qa-group-3 input NAME_INDEX "QA Sub Updated"`
6. `browser-use --session qa-group-3 click SUBMIT_INDEX` -- click submit/save button
7. `browser-use --session qa-group-3 wait text "successfully"` -- wait for success toast
8. `browser-use --session qa-group-3 state` -- verify "QA Sub Updated" appears

**Pass criteria:** Success toast appears. Subcategory name is now "QA Sub Updated" in the list.

**On failure:** `browser-use --session qa-group-3 screenshot "$TEMP/qa-suite-group3-subupdate-fail.png"`

---

### Test 3.8: Subcategory validation

**Steps:**

1. `browser-use --session qa-group-3 state` -- find "New Subcategory" button index
2. `browser-use --session qa-group-3 click NEW_SUBCATEGORY_INDEX` -- open create dialog
3. `browser-use --session qa-group-3 wait text "Create Subcategory"` -- wait for dialog
4. `browser-use --session qa-group-3 state` -- find submit button index
5. `browser-use --session qa-group-3 click SUBMIT_INDEX` -- submit empty form
6. `browser-use --session qa-group-3 state` -- check for validation error messages

**Pass criteria:** Validation errors appear for "Name" and "Category" fields (both are required).

**On failure:** `browser-use --session qa-group-3 screenshot "$TEMP/qa-suite-group3-subvalidation-fail.png"`

**Restore:** Close the dialog (click X or press Escape) before cleanup.

## Cleanup

```
browser-use --session qa-group-3 close
```

Note: Created data (`QA Category Updated`, `QA Sub Updated`) is intentionally left for use by downstream test groups (e.g., Group 4 wizard). Cleanup handled by orchestrator or teardown step.

## Result

Output structured JSON per qa-suite-common.md format.

```json
{
  "group": 3,
  "name": "Category & Subcategory CRUD",
  "status": "PASS|FAIL|BLOCKED|PARTIAL",
  "tests": [
    {
      "name": "Category list page",
      "status": "PASS|FAIL|SKIP",
      "details": "...",
      "screenshot": "$TEMP/qa-suite-group3-catlist-fail.png"
    },
    {
      "name": "Category create",
      "status": "PASS|FAIL|SKIP",
      "details": "...",
      "screenshot": "$TEMP/qa-suite-group3-catcreate-fail.png"
    },
    {
      "name": "Category update",
      "status": "PASS|FAIL|SKIP",
      "details": "...",
      "screenshot": "$TEMP/qa-suite-group3-catupdate-fail.png"
    },
    {
      "name": "Category validation",
      "status": "PASS|FAIL|SKIP",
      "details": "...",
      "screenshot": "$TEMP/qa-suite-group3-catvalidation-fail.png"
    },
    {
      "name": "Subcategory list page",
      "status": "PASS|FAIL|SKIP",
      "details": "...",
      "screenshot": "$TEMP/qa-suite-group3-sublist-fail.png"
    },
    {
      "name": "Subcategory create",
      "status": "PASS|FAIL|SKIP",
      "details": "...",
      "screenshot": "$TEMP/qa-suite-group3-subcreate-fail.png"
    },
    {
      "name": "Subcategory update",
      "status": "PASS|FAIL|SKIP",
      "details": "...",
      "screenshot": "$TEMP/qa-suite-group3-subupdate-fail.png"
    },
    {
      "name": "Subcategory validation",
      "status": "PASS|FAIL|SKIP",
      "details": "...",
      "screenshot": "$TEMP/qa-suite-group3-subvalidation-fail.png"
    }
  ]
}
```
