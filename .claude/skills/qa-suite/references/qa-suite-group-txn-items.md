# Group 6: Transactions & Receipt Items CRUD

**Session:** `qa-group-6`
**Dependencies:** Group 4 (needs at least one receipt and one account to exist)
**Data created:** One transaction ($25.50, then updated to $30.00, then deleted). One receipt item (QA Test Item, then updated, then deleted).

## Prerequisites

- Application running at `${BASE_URL}`
- Valid credentials: `${EMAIL}` / `${PASSWORD}`
- At least one receipt must exist (created by Group 4 wizard or prior data)
- At least one account must exist (created by Group 2 or prior data)
- At least one category must exist (for receipt item creation)

## Login

Follow the login flow from qa-suite-common.md using SESSION, EMAIL, PASSWORD.

## Tests

### Test 6.1: Transaction List

**Steps:**

1. `browser-use --session qa-group-6 open ${BASE_URL}/transactions`
2. `browser-use --session qa-group-6 wait text "Transactions"`
3. `browser-use --session qa-group-6 state` -- verify H1 is "Transactions"

**Pass criteria:** Page loads at `/transactions` with H1 heading "Transactions" and the table renders (even if empty).

**On failure:** `browser-use --session qa-group-6 screenshot "$TEMP/qa-suite-group6-txn-list-fail.png"`

---

### Test 6.2: Transaction Create

**Steps:**

1. `browser-use --session qa-group-6 state` -- find "New Transaction" button
2. `browser-use --session qa-group-6 click NEW_BUTTON_INDEX` -- click "New Transaction"
3. `browser-use --session qa-group-6 wait text "Create Transaction"`
4. `browser-use --session qa-group-6 state` -- find Receipt combobox, Account combobox, Amount input, Date input, submit button
5. Click the Receipt combobox trigger
6. `browser-use --session qa-group-6 state` -- see options in the popover
7. `browser-use --session qa-group-6 click FIRST_RECEIPT_OPTION_INDEX` -- select first receipt
8. Click the Account combobox trigger
9. `browser-use --session qa-group-6 state` -- see options in the popover
10. `browser-use --session qa-group-6 click FIRST_ACCOUNT_OPTION_INDEX` -- select first account
11. `browser-use --session qa-group-6 input AMOUNT_INDEX "25.50"` -- enter Amount $25.50 (if value doesn't stick, use the nativeInputValueSetter eval pattern from qa-suite-common.md)
12. `browser-use --session qa-group-6 input DATE_INDEX "2026-03-21"` -- enter Date
13. `browser-use --session qa-group-6 click SUBMIT_INDEX` -- submit form
14. `browser-use --session qa-group-6 wait text "successfully"` -- wait for success toast
15. `browser-use --session qa-group-6 state` -- verify new row in table

**Pass criteria:** Table shows new transaction with amount `$25.50` and the selected account/receipt. Success toast appeared.

**On failure:** `browser-use --session qa-group-6 screenshot "$TEMP/qa-suite-group6-txn-create-fail.png"`

---

### Test 6.3: Transaction Update

**Steps:**

1. `browser-use --session qa-group-6 state` -- find the edit (pencil icon) button on the newly created transaction row
2. `browser-use --session qa-group-6 click EDIT_BUTTON_INDEX` -- click edit
3. `browser-use --session qa-group-6 wait text "Edit Transaction"`
4. `browser-use --session qa-group-6 state` -- find Amount input
5. Clear the Amount field and enter new value: `browser-use --session qa-group-6 input AMOUNT_INDEX "30.00"` (use triple-click to select all first, or use eval nativeInputValueSetter pattern)
6. `browser-use --session qa-group-6 click SUBMIT_INDEX` -- submit form
7. `browser-use --session qa-group-6 wait text "successfully"` -- wait for success toast
8. `browser-use --session qa-group-6 state` -- verify row updated

**Pass criteria:** Transaction row now shows `$30.00` instead of `$25.50`. Success toast appeared.

**On failure:** `browser-use --session qa-group-6 screenshot "$TEMP/qa-suite-group6-txn-update-fail.png"`

---

### Test 6.4: Transaction Delete

**Steps:**

1. `browser-use --session qa-group-6 state` -- find checkbox for the updated transaction row
2. `browser-use --session qa-group-6 click CHECKBOX_INDEX` -- check the checkbox
3. `browser-use --session qa-group-6 state` -- find "Delete (1)" button
4. `browser-use --session qa-group-6 click DELETE_BUTTON_INDEX` -- click Delete
5. `browser-use --session qa-group-6 state` -- find confirmation dialog with confirm button
6. `browser-use --session qa-group-6 click CONFIRM_INDEX` -- confirm deletion
7. `browser-use --session qa-group-6 wait text "successfully"` -- wait for success toast
8. `browser-use --session qa-group-6 state` -- verify the row is removed from the table

**Pass criteria:** Transaction row with `$30.00` no longer appears in the table. Success toast appeared.

**On failure:** `browser-use --session qa-group-6 screenshot "$TEMP/qa-suite-group6-txn-delete-fail.png"`

---

### Test 6.5: Transaction Validation

**Steps:**

1. `browser-use --session qa-group-6 state` -- find "New Transaction" button
2. `browser-use --session qa-group-6 click NEW_BUTTON_INDEX` -- click "New Transaction"
3. `browser-use --session qa-group-6 wait text "Create Transaction"`
4. `browser-use --session qa-group-6 state` -- find the submit button
5. `browser-use --session qa-group-6 click SUBMIT_INDEX` -- submit empty form
6. `browser-use --session qa-group-6 state` -- check for validation error messages

**Pass criteria:** Validation error messages appear for required fields (Receipt, Account, Amount, Date). Form is NOT submitted.

**On failure:** `browser-use --session qa-group-6 screenshot "$TEMP/qa-suite-group6-txn-validation-fail.png"`

**Restore:** Close the dialog (click X or press Escape) before continuing.

---

### Test 6.6: Receipt Item List

**Steps:**

1. `browser-use --session qa-group-6 open ${BASE_URL}/receipt-items`
2. `browser-use --session qa-group-6 wait text "Receipt Items"`
3. `browser-use --session qa-group-6 state` -- verify H1 is "Receipt Items"

**Pass criteria:** Page loads at `/receipt-items` with H1 heading "Receipt Items" and the table renders (even if empty).

**On failure:** `browser-use --session qa-group-6 screenshot "$TEMP/qa-suite-group6-item-list-fail.png"`

---

### Test 6.7: Receipt Item Create

**Steps:**

1. `browser-use --session qa-group-6 state` -- find "New Item" button
2. `browser-use --session qa-group-6 click NEW_BUTTON_INDEX` -- click "New Item"
3. `browser-use --session qa-group-6 wait text "Create Receipt Item"`
4. `browser-use --session qa-group-6 state` -- find Receipt combobox, Item Code input, Description input, Pricing Mode combobox, Quantity input, Unit Price input, Category combobox, submit button
5. Click the Receipt combobox trigger
6. `browser-use --session qa-group-6 state` -- see options
7. `browser-use --session qa-group-6 click FIRST_RECEIPT_OPTION_INDEX` -- select first receipt
8. `browser-use --session qa-group-6 input ITEM_CODE_INDEX "QA-ITEM-1"` -- enter Item Code
9. `browser-use --session qa-group-6 input DESCRIPTION_INDEX "QA Test Item"` -- enter Description (if autocomplete popover appears, press Escape to dismiss)
10. `browser-use --session qa-group-6 input QUANTITY_INDEX "2"` -- enter Quantity
11. `browser-use --session qa-group-6 input UNIT_PRICE_INDEX "10.00"` -- enter Unit Price (use nativeInputValueSetter eval pattern if value doesn't stick)
12. Click the Category combobox trigger
13. `browser-use --session qa-group-6 state` -- see options
14. `browser-use --session qa-group-6 click FIRST_CATEGORY_OPTION_INDEX` -- select first category
15. `browser-use --session qa-group-6 click SUBMIT_INDEX` -- submit form
16. `browser-use --session qa-group-6 wait text "successfully"` -- wait for success toast
17. `browser-use --session qa-group-6 state` -- verify new row in table

**Pass criteria:** Table shows new receipt item with Description "QA Test Item", Unit Price `$10.00`, and Quantity `2`. Success toast appeared.

**On failure:** `browser-use --session qa-group-6 screenshot "$TEMP/qa-suite-group6-item-create-fail.png"`

---

### Test 6.8: Receipt Item Update

**Steps:**

1. `browser-use --session qa-group-6 state` -- find the edit (pencil icon) button on the "QA Test Item" row
2. `browser-use --session qa-group-6 click EDIT_BUTTON_INDEX` -- click edit
3. `browser-use --session qa-group-6 wait text "Edit Receipt Item"`
4. `browser-use --session qa-group-6 state` -- find Description input
5. Clear the Description field and enter new value: `browser-use --session qa-group-6 input DESCRIPTION_INDEX "QA Item Updated"`
6. `browser-use --session qa-group-6 click SUBMIT_INDEX` -- submit form
7. `browser-use --session qa-group-6 wait text "successfully"` -- wait for success toast
8. `browser-use --session qa-group-6 state` -- verify row updated

**Pass criteria:** Receipt item row now shows "QA Item Updated" in the Description column. Success toast appeared.

**On failure:** `browser-use --session qa-group-6 screenshot "$TEMP/qa-suite-group6-item-update-fail.png"`

---

### Test 6.9: Receipt Item Delete

**Steps:**

1. `browser-use --session qa-group-6 state` -- find checkbox for the "QA Item Updated" row
2. `browser-use --session qa-group-6 click CHECKBOX_INDEX` -- check the checkbox
3. `browser-use --session qa-group-6 state` -- find "Delete (1)" button
4. `browser-use --session qa-group-6 click DELETE_BUTTON_INDEX` -- click Delete
5. `browser-use --session qa-group-6 state` -- find confirmation dialog with confirm button
6. `browser-use --session qa-group-6 click CONFIRM_INDEX` -- confirm deletion
7. `browser-use --session qa-group-6 wait text "successfully"` -- wait for success toast
8. `browser-use --session qa-group-6 state` -- verify the row is removed from the table

**Pass criteria:** Receipt item row with "QA Item Updated" no longer appears in the table. Success toast appeared.

**On failure:** `browser-use --session qa-group-6 screenshot "$TEMP/qa-suite-group6-item-delete-fail.png"`

---

### Test 6.10: Receipt Item Validation

**Steps:**

1. `browser-use --session qa-group-6 state` -- find "New Item" button
2. `browser-use --session qa-group-6 click NEW_BUTTON_INDEX` -- click "New Item"
3. `browser-use --session qa-group-6 wait text "Create Receipt Item"`
4. `browser-use --session qa-group-6 state` -- find the submit button
5. `browser-use --session qa-group-6 click SUBMIT_INDEX` -- submit empty form
6. `browser-use --session qa-group-6 state` -- check for validation error messages

**Pass criteria:** Validation error messages appear for required fields (Description, Category, Receipt). Form is NOT submitted.

**On failure:** `browser-use --session qa-group-6 screenshot "$TEMP/qa-suite-group6-item-validation-fail.png"`

## Cleanup

```
browser-use --session qa-group-6 close
```

## Result

Output structured JSON per qa-suite-common.md format.

```json
{
  "group": 6,
  "name": "Transactions & Receipt Items CRUD",
  "status": "PASS|FAIL|BLOCKED|PARTIAL",
  "tests": [
    {
      "name": "Transaction List",
      "status": "PASS|FAIL|SKIP",
      "details": "...",
      "screenshot": "$TEMP/qa-suite-group6-txn-list-fail.png"
    },
    {
      "name": "Transaction Create",
      "status": "PASS|FAIL|SKIP",
      "details": "...",
      "screenshot": "$TEMP/qa-suite-group6-txn-create-fail.png"
    },
    {
      "name": "Transaction Update",
      "status": "PASS|FAIL|SKIP",
      "details": "...",
      "screenshot": "$TEMP/qa-suite-group6-txn-update-fail.png"
    },
    {
      "name": "Transaction Delete",
      "status": "PASS|FAIL|SKIP",
      "details": "...",
      "screenshot": "$TEMP/qa-suite-group6-txn-delete-fail.png"
    },
    {
      "name": "Transaction Validation",
      "status": "PASS|FAIL|SKIP",
      "details": "...",
      "screenshot": "$TEMP/qa-suite-group6-txn-validation-fail.png"
    },
    {
      "name": "Receipt Item List",
      "status": "PASS|FAIL|SKIP",
      "details": "...",
      "screenshot": "$TEMP/qa-suite-group6-item-list-fail.png"
    },
    {
      "name": "Receipt Item Create",
      "status": "PASS|FAIL|SKIP",
      "details": "...",
      "screenshot": "$TEMP/qa-suite-group6-item-create-fail.png"
    },
    {
      "name": "Receipt Item Update",
      "status": "PASS|FAIL|SKIP",
      "details": "...",
      "screenshot": "$TEMP/qa-suite-group6-item-update-fail.png"
    },
    {
      "name": "Receipt Item Delete",
      "status": "PASS|FAIL|SKIP",
      "details": "...",
      "screenshot": "$TEMP/qa-suite-group6-item-delete-fail.png"
    },
    {
      "name": "Receipt Item Validation",
      "status": "PASS|FAIL|SKIP",
      "details": "...",
      "screenshot": "$TEMP/qa-suite-group6-item-validation-fail.png"
    }
  ]
}
```
