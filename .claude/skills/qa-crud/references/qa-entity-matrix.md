# Entity CRUD Matrix

Defines the test data, form fields, dependency order, and expected behavior for each entity's CRUD operations.

## Dependency Order

Entities must be created in this order (each depends on prior entities existing):

1. **accounts** — no dependencies
2. **categories** — no dependencies
3. **subcategories** — depends on categories
4. **item-templates** — no dependencies
5. **receipts** — read-only in list view (created via wizard), skip CRUD
6. **transactions** — depends on receipts + accounts
7. **receipt-items** — depends on receipts

When running `--all`, process in this order. When running a single entity, warn if dependencies may not exist.

## Entity Details

### 1. Accounts (`/accounts`)

| Property | Value |
|----------|-------|
| H1 | Accounts |
| New Button | "New Account" |
| Dialog Title (Create) | Create Account |
| Dialog Title (Edit) | Edit Account |
| Has Delete | No (soft-delete via isActive toggle) |
| Has Select/Bulk | No |

**Create Fields:**

| Label | Type | Test Value | Update Value |
|-------|------|-----------|--------------|
| Account Code | text input | `QA-001` | `QA-001-UPD` |
| Name | text input | `QA Test Account` | `QA Test Account Updated` |
| Active | checkbox/switch | checked (default) | — |

**Validation:** Submit empty form → expect "Account Code" and "Name" required errors.

**Verify Create:** New row shows `QA-001` in Account Code column and `QA Test Account` in Name column.

**Verify Update:** Row shows updated values.

---

### 2. Categories (`/categories`)

| Property | Value |
|----------|-------|
| H1 | Categories |
| New Button | "New Category" |
| Dialog Title (Create) | Create Category |
| Dialog Title (Edit) | Edit Category |
| Has Delete | No |
| Has Select/Bulk | No |

**Create Fields:**

| Label | Type | Test Value | Update Value |
|-------|------|-----------|--------------|
| Name | text input | `QA Category` | `QA Category Updated` |
| Description (optional) | text input | `Test category for QA` | `Updated description` |

**Validation:** Submit empty form → expect "Name" required error.

**Verify Create:** New row shows `QA Category` in Name column.

---

### 3. Subcategories (`/subcategories`)

| Property | Value |
|----------|-------|
| H1 | Subcategories |
| New Button | "New Subcategory" |
| Dialog Title (Create) | Create Subcategory |
| Dialog Title (Edit) | Edit Subcategory |
| Has Delete | No |
| Has Select/Bulk | No |

**Create Fields:**

| Label | Type | Test Value | Update Value |
|-------|------|-----------|--------------|
| Name | text input | `QA Subcategory` | `QA Sub Updated` |
| Category | combobox (select existing) | select first available | — |
| Description (optional) | text input | `Test subcategory` | `Updated sub desc` |

**Combobox Interaction (Category):**
1. Click the Category combobox trigger
2. Snapshot to see options
3. Click the first option in the list

**Validation:** Submit empty form → expect "Name" and "Category" required errors.

**Verify Create:** Subcategory appears under its category group (may need to click "Expand All" first).

---

### 4. Item Templates (`/item-templates`)

| Property | Value |
|----------|-------|
| H1 | Item Templates |
| New Button | "New Template" |
| Dialog Title (Create) | Create Item Template |
| Dialog Title (Edit) | Edit Item Template |
| Has Delete | Yes (bulk select + "Delete" button) |
| Has Select/Bulk | Yes (checkboxes) |

**Create Fields:**

| Label | Type | Test Value | Update Value |
|-------|------|-----------|--------------|
| Name | text input | `QA Template` | `QA Template Updated` |
| Description (optional) | text input | `Test template` | — |
| Default Category (optional) | combobox | — (leave empty) | — |
| Default Subcategory (optional) | combobox | — (leave empty) | — |
| Default Unit Price (optional) | currency input | `9.99` | `12.50` |
| Default Pricing Mode (optional) | combobox | — (leave empty) | — |
| Default Item Code (optional) | text input | `QA-TMPL` | — |

**Validation:** Submit empty form → expect "Name" required error.

**Verify Create:** New row shows `QA Template` in Name column.

**Delete Flow:**
1. Check the checkbox for the created template
2. Click "Delete (1)" button
3. Confirm in the delete dialog
4. Verify row is removed from table

---

### 5. Receipts (`/receipts`) — READ ONLY

Receipts are created via the wizard (`/receipts/new`), not through the list page. The receipts list page is read-only.

**Smoke Check:**
- Navigate to `/receipts`, verify the table renders
- Verify the H1 says "Receipts"
- If receipts exist, verify rows show location, date, and tax amount

---

### 6. Transactions (`/transactions`)

| Property | Value |
|----------|-------|
| H1 | Transactions |
| New Button | "New Transaction" |
| Dialog Title (Create) | Create Transaction |
| Dialog Title (Edit) | Edit Transaction |
| Has Delete | Yes (bulk select + "Delete" button) |
| Has Select/Bulk | Yes (checkboxes) |

**Prerequisites:** At least one receipt and one account must exist.

**Create Fields:**

| Label | Type | Test Value | Update Value |
|-------|------|-----------|--------------|
| Receipt | combobox | select first available | — |
| Account | combobox | select first available | — |
| Amount | currency input | `25.50` | `30.00` |
| Date | date input | today's date (YYYY-MM-DD) | — |

**Combobox Interaction (Receipt & Account):** Same pattern as subcategory — click trigger, snapshot, click option.

**Validation:** Submit empty form → expect required field errors.

**Verify Create:** New row shows amount `$25.50` and the selected account/receipt.

**Delete Flow:**
1. Check the checkbox for the created transaction
2. Click "Delete (1)" button
3. Confirm deletion
4. Verify row removed

---

### 7. Receipt Items (`/receipt-items`)

| Property | Value |
|----------|-------|
| H1 | Receipt Items |
| New Button | "New Item" |
| Dialog Title (Create) | Create Receipt Item |
| Dialog Title (Edit) | Edit Receipt Item |
| Has Delete | Yes (bulk select + "Delete" button) |
| Has Select/Bulk | Yes (checkboxes) |

**Prerequisites:** At least one receipt must exist.

**Create Fields:**

| Label | Type | Test Value | Update Value |
|-------|------|-----------|--------------|
| Receipt | combobox | select first available | — |
| Item Code | text input | `QA-ITEM-1` | `QA-ITEM-UPD` |
| Description | text input | `QA Test Item` | `QA Item Updated` |
| Pricing Mode | combobox | `Quantity` | — |
| Quantity | number input | `2` | `3` |
| Unit Price | currency input | `10.00` | `15.00` |
| Category | combobox | select first available | — |
| Subcategory | combobox (optional) | — (leave empty) | — |

**Validation:** Submit empty form → expect "Description", "Category", "Receipt" required errors.

**Verify Create:** New row shows `QA Test Item` in Description, `$10.00` in Unit Price, `2` in Qty.

**Delete Flow:**
1. Check the checkbox for the created item
2. Click "Delete (1)" button
3. Confirm deletion
4. Verify row removed

## Common Patterns

### Opening Create Dialog

All entity pages use a "New {Entity}" button in the top-right area:
```
agent-browser snapshot -i
```
Find the button with text like "New Account", "New Category", etc. Click it.

### Edit Flow

1. Find the pencil icon button (aria-label="Edit") in the row
2. Click it to open the Edit dialog
3. Modify one field
4. Click the submit button (usually "Save" or "Update")
5. Verify the table row updates

### Verifying Table Content

After create/update, the table should reflect changes. Take a snapshot:
```
agent-browser snapshot -i
```
Search the output for the expected values (e.g., "QA-001", "QA Test Account").

### Toast Messages

After successful create/update/delete, look for toast messages in the snapshot:
- Create: "created successfully" or similar
- Update: "updated successfully" or similar
- Delete: "deleted successfully" or similar
