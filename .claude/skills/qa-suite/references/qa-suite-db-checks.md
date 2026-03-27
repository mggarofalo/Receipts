# DB Integrity Checks

API-based data verification using `curl` with Bearer token. Run after all browser test waves complete.

All checks use:
```
curl -s -H "Authorization: Bearer ${ACCESS_TOKEN}" ${API_URL}/api/...
```

## Check 1: Account Integrity

**Endpoint:** `GET /api/accounts`

**Verify:**
- Response is valid JSON with `data` array
- No accounts with empty `accountCode` or `name`
- If CRUD group ran: QA-001 account exists

**Pass criteria:** All accounts have non-empty accountCode and name.

## Check 2: Category Tree Validity

**Endpoints:** `GET /api/categories`, `GET /api/subcategories`

**Verify:**
- All subcategories reference a valid `categoryId` that exists in the categories response
- No orphaned subcategories pointing to non-existent categories

**Pass criteria:** Every subcategory's categoryId matches an existing category.

## Check 3: Receipt Item Integrity

**Endpoint:** `GET /api/receipt-items`

**Verify:**
- All receipt items have a non-null `receiptId`
- All receipt items have a non-empty `description`

**Pass criteria:** No orphaned or incomplete receipt items.

## Check 4: Transaction Integrity

**Endpoint:** `GET /api/transactions`

**Verify:**
- All transactions have a non-null `receiptId` and `accountId`
- All transactions have a non-zero `amount`

**Pass criteria:** No orphaned or zero-amount transactions.

## Check 5: QA Data Presence (conditional)

Only run if CRUD/wizard groups were included in this test run.

**Verify (after CRUD groups):**
- `GET /api/accounts` -- look for accountCode containing "QA"
- `GET /api/categories` -- look for name containing "QA"

**Verify (after wizard group):**
- `GET /api/receipts` -- look for location containing "QA"

**Pass criteria:** Expected QA test data was created by browser tests.

## Check 6: API Health

**Endpoint:** `GET /api/health`

**Verify:**
- Response status 200
- Database connectivity confirmed

**Pass criteria:** API reports healthy status.
