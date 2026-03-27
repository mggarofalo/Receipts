# Group 4: Receipt Wizard E2E

**Session:** `qa-group-4`
**Dependencies:** Groups 2, 3 (needs accounts and categories to exist)
**Data created:** One complete receipt with trip, transaction, and line item

## Prerequisites

- Application running at `${BASE_URL}`
- Valid credentials: `${EMAIL}` / `${PASSWORD}`
- At least one account exists (created by Group 2: `QA-001`)
- At least one category exists (created by Group 3: `QA Category Updated`)
- API available at `${API_URL}` with `${ACCESS_TOKEN}` (fallback if wizard UI has issues)

## Login

Follow the login flow from qa-suite-common.md using SESSION, EMAIL, PASSWORD.

## Tests

### Test 4.1: Navigate to wizard

**Steps:**

1. `browser-use --session qa-group-4 open ${BASE_URL}/receipts/new`
2. `browser-use --session qa-group-4 wait text "New Receipt"`
3. `browser-use --session qa-group-4 state` -- verify "New Receipt" heading and stepper with 4 steps visible

**Pass criteria:** Page loads at `/receipts/new` with "New Receipt" heading. A 4-step stepper is visible (Trip Details, Transactions, Line Items, Review -- or similar step labels).

**On failure:** `browser-use --session qa-group-4 screenshot "$TEMP/qa-suite-group4-navigate-fail.png"`

---

### Test 4.2: Step 1 -- Trip Details

**Steps:**

1. `browser-use --session qa-group-4 state` -- find Location combobox, Date input, Tax Amount input, Next button indices
2. Click the Location combobox trigger:
   - `browser-use --session qa-group-4 click LOCATION_COMBOBOX_INDEX`
3. `browser-use --session qa-group-4 state` -- check if "QA Store" appears as an option, or if there is a text input to type a new location
4. If combobox allows typing: `browser-use --session qa-group-4 input LOCATION_INPUT_INDEX "QA Store"`
5. If combobox shows options: select "QA Store" or create it as needed
6. `browser-use --session qa-group-4 state` -- re-index after combobox interaction
7. Fill the Date field: `browser-use --session qa-group-4 input DATE_INDEX "2026-03-21"`
8. Fill the Tax Amount field: `browser-use --session qa-group-4 input TAX_INDEX "3.42"`
   - If the currency input doesn't accept plain `input`, use the React controlled input pattern from qa-suite-common.md (eval with nativeSetter)
9. `browser-use --session qa-group-4 state` -- find Next button index
10. `browser-use --session qa-group-4 click NEXT_INDEX` -- click Next

**Pass criteria:** Step 1 completes and the wizard advances to Step 2 (Transactions). The stepper shows step 2 as active.

**On failure:** `browser-use --session qa-group-4 screenshot "$TEMP/qa-suite-group4-step1-fail.png"`

---

### Test 4.3: Step 2 -- Transactions

**Steps:**

1. `browser-use --session qa-group-4 state` -- verify on step 2, find Add Transaction button or account combobox, amount input
2. Click the Account combobox trigger:
   - `browser-use --session qa-group-4 click ACCOUNT_COMBOBOX_INDEX`
3. `browser-use --session qa-group-4 state` -- find account options
4. `browser-use --session qa-group-4 click ACCOUNT_OPTION_INDEX` -- select the first available account (e.g., "QA-001 - QA Test Account Updated")
5. `browser-use --session qa-group-4 state` -- re-index after selection
6. Fill Amount: `browser-use --session qa-group-4 input AMOUNT_INDEX "31.88"`
   - If currency input doesn't accept plain `input`, use eval with nativeSetter pattern
7. If there is an "Add" button for the transaction row, click it
8. `browser-use --session qa-group-4 state` -- find Next button index
9. `browser-use --session qa-group-4 click NEXT_INDEX` -- click Next

**Pass criteria:** Step 2 completes with at least one transaction added (account selected, amount $31.88). Wizard advances to Step 3.

**On failure:** `browser-use --session qa-group-4 screenshot "$TEMP/qa-suite-group4-step2-fail.png"`

---

### Test 4.4: Step 3 -- Line Items

**Steps:**

1. `browser-use --session qa-group-4 state` -- verify on step 3, find description input, quantity input, unit price input, category combobox
2. Fill Description: `browser-use --session qa-group-4 input DESCRIPTION_INDEX "QA Test Item"`
3. Fill Quantity: `browser-use --session qa-group-4 input QUANTITY_INDEX "2"`
4. Fill Unit Price: `browser-use --session qa-group-4 input UNIT_PRICE_INDEX "14.23"`
   - If currency input doesn't accept plain `input`, use eval with nativeSetter pattern
5. Click the Category combobox trigger:
   - `browser-use --session qa-group-4 click CATEGORY_COMBOBOX_INDEX`
6. `browser-use --session qa-group-4 state` -- find category options
7. `browser-use --session qa-group-4 click CATEGORY_OPTION_INDEX` -- select first available category
8. If there is an "Add" button for the line item row, click it
9. `browser-use --session qa-group-4 state` -- find Next button index
10. `browser-use --session qa-group-4 click NEXT_INDEX` -- click Next

**Pass criteria:** Step 3 completes with at least one line item added. Wizard advances to Step 4 (Review).

**On failure:** `browser-use --session qa-group-4 screenshot "$TEMP/qa-suite-group4-step3-fail.png"`

---

### Test 4.5: Step 4 -- Review & Submit

**Steps:**

1. `browser-use --session qa-group-4 state` -- verify on step 4 (Review), check summary content
2. Verify the summary shows:
   - Location: "QA Store"
   - Tax: "$3.42" (or `3.42`)
   - Transaction amount: "$31.88" (or `31.88`)
   - Item: "QA Test Item"
3. `browser-use --session qa-group-4 state` -- find Submit button index
4. `browser-use --session qa-group-4 click SUBMIT_INDEX` -- click Submit

**Pass criteria:** Review page displays the entered data correctly. Submit button is clickable.

**On failure:** `browser-use --session qa-group-4 screenshot "$TEMP/qa-suite-group4-step4-fail.png"`

---

### Test 4.6: Verify receipt created

**Steps:**

1. `browser-use --session qa-group-4 wait text "successfully"` -- wait for success indicator (toast or redirect)
2. `browser-use --session qa-group-4 eval "window.location.pathname"` -- check URL (should be `/receipts` or `/receipt-detail?id=...`)
3. `browser-use --session qa-group-4 state` -- verify the new receipt is visible

**Pass criteria:** After submission, the user is redirected to the receipt detail page or receipt list. The newly created receipt is visible (showing "QA Store" or the entered data).

**On failure:** `browser-use --session qa-group-4 screenshot "$TEMP/qa-suite-group4-verify-fail.png"`

## API Fallback

If the wizard UI has issues (known bug: NavigationMenu can steal focus from comboboxes), use the API to create the receipt instead:

```bash
curl -X POST "${API_URL}/api/receipts/complete" \
  -H "Authorization: Bearer ${ACCESS_TOKEN}" \
  -H "Content-Type: application/json" \
  -d '{
    "trip": {
      "location": "QA Store",
      "date": "2026-03-21",
      "taxAmount": 3.42
    },
    "transactions": [
      {
        "accountId": "<ACCOUNT_GUID>",
        "amount": 31.88,
        "date": "2026-03-21"
      }
    ],
    "receiptItems": [
      {
        "description": "QA Test Item",
        "quantity": 2,
        "unitPrice": 14.23,
        "categoryId": "<CATEGORY_GUID>"
      }
    ]
  }'
```

To get the account and category GUIDs:
- `curl -H "Authorization: Bearer ${ACCESS_TOKEN}" "${API_URL}/api/accounts"` -- find `QA-001` account ID
- `curl -H "Authorization: Bearer ${ACCESS_TOKEN}" "${API_URL}/api/categories"` -- find category ID

If using the API fallback, mark the wizard UI tests as FAIL with details, but still mark Test 4.6 as PASS if the API receipt creation succeeds and can be verified in the UI.

## Cleanup

```
browser-use --session qa-group-4 close
```

Note: The created receipt is intentionally left for use by Group 5 (Receipt List/Detail/Edit/Delete tests).

## Result

Output structured JSON per qa-suite-common.md format.

```json
{
  "group": 4,
  "name": "Receipt Wizard E2E",
  "status": "PASS|FAIL|BLOCKED|PARTIAL",
  "tests": [
    {
      "name": "Navigate to wizard",
      "status": "PASS|FAIL|SKIP",
      "details": "...",
      "screenshot": "$TEMP/qa-suite-group4-navigate-fail.png"
    },
    {
      "name": "Step 1 -- Trip Details",
      "status": "PASS|FAIL|SKIP",
      "details": "...",
      "screenshot": "$TEMP/qa-suite-group4-step1-fail.png"
    },
    {
      "name": "Step 2 -- Transactions",
      "status": "PASS|FAIL|SKIP",
      "details": "...",
      "screenshot": "$TEMP/qa-suite-group4-step2-fail.png"
    },
    {
      "name": "Step 3 -- Line Items",
      "status": "PASS|FAIL|SKIP",
      "details": "...",
      "screenshot": "$TEMP/qa-suite-group4-step3-fail.png"
    },
    {
      "name": "Step 4 -- Review & Submit",
      "status": "PASS|FAIL|SKIP",
      "details": "...",
      "screenshot": "$TEMP/qa-suite-group4-step4-fail.png"
    },
    {
      "name": "Verify receipt created",
      "status": "PASS|FAIL|SKIP",
      "details": "...",
      "screenshot": "$TEMP/qa-suite-group4-verify-fail.png"
    }
  ]
}
```
