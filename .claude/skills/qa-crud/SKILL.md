---
name: qa-crud
description: >
  Test CRUD operations for Receipts app entities. Verifies list, create,
  read, update, validation, and delete flows per entity using agent-browser
  + Aspire MCP. Produces a per-entity pass/fail report.
allowed-tools: Bash, Read, Grep, Glob, mcp__aspire__list_resources, mcp__aspire__list_console_logs
user_invocable: true
argument: "<entity> | --all — Entity name (accounts, categories, subcategories, item-templates, transactions, receipt-items) or --all for all entities"
---

# QA CRUD Testing

Test CRUD (Create, Read, Update, Delete) operations for one or all entities in the Receipts app.

**Duration:** ~3 min per entity, ~15 min for `--all`

## Prerequisites

- Aspire must be running (F5 or `dotnet run --project src/Receipts.AppHost`)
- agent-browser must be installed globally

## References

Before starting, read both reference files:
- `references/qa-common.md` — Port discovery, login flow, form patterns
- `references/qa-entity-matrix.md` — Entity fields, test data, dependency order

## Argument Parsing

Parse the argument to determine which entities to test:

| Argument | Entities |
|----------|----------|
| `accounts` | accounts only |
| `categories` | categories only |
| `subcategories` | subcategories only |
| `item-templates` | item-templates only |
| `transactions` | transactions only |
| `receipt-items` | receipt-items only |
| `--all` | All entities in dependency order |

**Dependency order** (for `--all`): accounts → categories → subcategories → item-templates → (skip receipts) → transactions → receipt-items

If an unknown entity name is given, list the valid options and abort.

## Phase 1: Port Discovery + Auth

Follow the same process as smoke-test:

1. **Port Discovery:** Call `mcp__aspire__list_console_logs` for `frontend` resource, parse the base URL.
2. **Login:** Follow the login flow from `qa-common.md` (try `Admin123!@#` first, fall back to `QaTest2024!@#`, handle password change if needed).
3. **Verify:** Confirm dashboard loads.

## Phase 2: Entity Resolution

Based on the parsed argument, build the list of entities to test. For `--all`, use the dependency order from `qa-entity-matrix.md`.

Warn the user if testing an entity with dependencies (e.g., `subcategories` requires categories to exist).

## Phase 3: CRUD Cycle (Per Entity)

For each entity, perform the following operations. Refer to `qa-entity-matrix.md` for entity-specific field details, test data, and validation expectations.

### 3.1: List (Navigate + Verify Table)

1. Navigate to the entity's route:
   ```
   agent-browser open ${BASE_URL}/${entity-route}
   ```
2. Wait for load:
   ```
   agent-browser wait --load networkidle
   ```
3. Snapshot and verify:
   - H1 matches expected heading
   - Table structure exists (look for `table`, `columnheader` nodes)
4. Record: **List — PASS/FAIL**

### 3.2: Create

1. Snapshot to find the "New {Entity}" button:
   ```
   agent-browser snapshot -i
   ```
2. Click the "New" button (e.g., "New Account", "New Category"):
   ```
   agent-browser click @eN
   ```
3. Wait for dialog to open, then snapshot:
   ```
   agent-browser snapshot -i
   ```
4. Verify dialog title matches expected (e.g., "Create Account").
5. Fill all required fields using test values from `qa-entity-matrix.md`:
   - For text inputs: `agent-browser fill @eN "value"`
   - For comboboxes: Click trigger → snapshot → click option
   - For currency inputs: Try `agent-browser fill` first; use nativeInputValueSetter fallback if needed
   - For date inputs: `agent-browser fill @eN "YYYY-MM-DD"`
6. Click the submit/save button:
   ```
   agent-browser click @eN
   ```
7. Wait for dialog to close:
   ```
   agent-browser wait --load networkidle
   ```
8. Snapshot and verify:
   - Dialog closed
   - Toast message appears (look for success text)
   - New row appears in table with expected values
9. Record: **Create — PASS/FAIL**

### 3.3: Read (Verify Created Data)

1. Snapshot the table:
   ```
   agent-browser snapshot -i
   ```
2. Search the snapshot output for the test values (e.g., "QA-001", "QA Test Account").
3. Verify the correct values appear in the correct columns.
4. Record: **Read — PASS/FAIL**

### 3.4: Update

1. Find the Edit button (pencil icon, `aria-label="Edit"`) for the created row:
   ```
   agent-browser snapshot -i
   ```
   Look for the edit button in the row containing the test data.
2. Click the Edit button:
   ```
   agent-browser click @eN
   ```
3. Wait for edit dialog, then snapshot:
   ```
   agent-browser snapshot -i
   ```
4. Verify the dialog shows "Edit {Entity}" title and pre-filled values.
5. Change one field to the update value from `qa-entity-matrix.md`:
   - Clear and fill: `agent-browser fill @eN "new value"`
6. Click submit/save.
7. Wait and verify:
   - Dialog closed
   - Table row shows updated value
8. Record: **Update — PASS/FAIL**

### 3.5: Validation

1. Click the "New" button again to open create dialog.
2. Without filling any fields, click submit immediately.
3. Snapshot and verify:
   - Form validation errors appear (look for error messages like "required", "is required")
   - Dialog stays open (not submitted)
4. Close the dialog (click Cancel or press Escape):
   ```
   agent-browser press Escape
   ```
5. Record: **Validation — PASS/FAIL**

### 3.6: Delete (If Applicable)

Only for entities with delete support (item-templates, transactions, receipt-items). See `qa-entity-matrix.md` for which entities support delete.

1. Find the checkbox for the created/updated item:
   ```
   agent-browser snapshot -i
   ```
2. Check the checkbox:
   ```
   agent-browser click @eN
   ```
3. Click the "Delete (1)" button:
   ```
   agent-browser click @eN
   ```
4. Confirm in the delete dialog:
   ```
   agent-browser snapshot -i
   agent-browser click @eN  (the confirm/delete button)
   ```
5. Wait and verify:
   - Item removed from table
   - Success toast appears
6. Record: **Delete — PASS/FAIL**

For entities without delete (accounts, categories, subcategories), record: **Delete — N/A**

### 3.7: On Failure

When any operation fails:
1. Take a screenshot:
   ```
   agent-browser screenshot "$TEMP/qa-crud-{entity}-{operation}-fail.png"
   ```
2. Continue to the next operation (don't abort the entire entity).
3. If Create fails, skip Read/Update/Delete (they depend on the created entity).

## Phase 4: Report

Generate and display a markdown report:

```markdown
# QA CRUD Report

**Date:** YYYY-MM-DD  |  **Base URL:** {BASE_URL}  |  **Result:** {PASS/FAIL}

## Entity: Accounts

| Operation | Status | Details | Screenshot |
|-----------|--------|---------|------------|
| List | PASS | H1 "Accounts" found, table rendered | — |
| Create | PASS | Created QA-001 / QA Test Account | — |
| Read | PASS | Values verified in table | — |
| Update | PASS | Updated to QA-001-UPD | — |
| Validation | PASS | Required field errors shown | — |
| Delete | N/A | Entity does not support delete | — |

## Entity: Categories

| Operation | Status | Details | Screenshot |
|-----------|--------|---------|------------|
| ... | ... | ... | ... |

---

## Summary

| Entity | List | Create | Read | Update | Validation | Delete | Result |
|--------|------|--------|------|--------|------------|--------|--------|
| accounts | PASS | PASS | PASS | PASS | PASS | N/A | PASS |
| categories | PASS | PASS | PASS | PASS | PASS | N/A | PASS |
| ... | ... | ... | ... | ... | ... | ... | ... |

**Overall: {PASS/FAIL}**
```

Any single operation FAIL = entity FAIL. Any entity FAIL = overall FAIL.

## Phase 5: Cleanup

```
agent-browser close
```

Print summary:
```
CRUD tested {N} entities. {P} passed, {F} failed. Overall: {PASS/FAIL}
```

## Important Rules

- Every `agent-browser` command is a **separate Bash tool call** (no `&&` chaining).
- Screenshots go to **`$TEMP`** only.
- Ports are **never hardcoded** — discover via Aspire MCP.
- After any navigation or dialog open/close, take a fresh snapshot.
- Use test data prefixed with "QA" to make cleanup easy and avoid conflicts with real data.
- If a combobox fill doesn't work with `agent-browser fill`, use the click-trigger → snapshot → click-option pattern from `qa-common.md`.
