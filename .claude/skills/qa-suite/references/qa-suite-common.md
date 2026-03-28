# QA Suite Common Reference

Shared patterns for all QA test group subagents. Every subagent MUST read this file before starting.

## Browser-Use CLI Commands

All browser interaction uses `browser-use` CLI with a named `--session` for isolation.

### Command Reference

```
browser-use --session SESSION open URL         # Navigate to URL
browser-use --session SESSION state            # Get interactive elements (compact text)
browser-use --session SESSION click INDEX      # Click element by numeric index
browser-use --session SESSION input INDEX "t"  # Click + type text into element
browser-use --session SESSION screenshot PATH  # Capture screenshot (ALWAYS $TEMP)
browser-use --session SESSION wait text "str"  # Wait for text to appear on page
browser-use --session SESSION eval "js"        # Execute JavaScript in page context
browser-use --session SESSION close            # Close browser session
```

### Critical Rules

1. **Each `browser-use` command MUST be a SEPARATE Bash tool call** -- never chain with `&&`
2. **Screenshots go to `$TEMP` ONLY** -- e.g., `browser-use --session S screenshot "$TEMP/qa-suite-groupN-testname.png"`
3. **NEVER read screenshot files back into context** -- they are for human review only
4. **NEVER use `browser-use get html`** -- DOM output is too large, will blow context
5. **Always re-run `state` after any click, navigation, or DOM mutation** -- element indices change
6. **Use `state` as primary inspection** -- it returns a compact listing of interactive elements

## Credential Matrix

| Target | Email | Password | MustResetPassword |
|--------|-------|----------|-------------------|
| local (fresh seed) | `admin@receipts.local` | `Admin123!@#` | Yes -- preflight changes to `QaTest2024!@#` |
| local (already changed) | `admin@receipts.local` | `QaTest2024!@#` | No |
| prod | `claude@code.com` | `Password123!@#` | No |

The orchestrator resolves credentials and passes them to you. Use whatever `EMAIL` and `PASSWORD` values you receive.

## Login Flow

1. `browser-use --session SESSION open ${BASE_URL}/login`
2. `browser-use --session SESSION wait text "Sign In"`
3. `browser-use --session SESSION state` -- find email input, password input, Sign In button indices
4. `browser-use --session SESSION input EMAIL_INDEX "${EMAIL}"`
5. `browser-use --session SESSION input PASSWORD_INDEX "${PASSWORD}"`
6. `browser-use --session SESSION click SUBMIT_INDEX`
7. `browser-use --session SESSION wait text "Dashboard"` -- if this times out, check for password change
8. `browser-use --session SESSION eval "window.location.pathname"` -- verify URL

**If redirected to `/change-password`:**
1. `browser-use --session SESSION state` -- find current/new/confirm password fields
2. Fill current password: `${PASSWORD}`
3. Fill new password: `QaTest2024!@#`
4. Fill confirm password: `QaTest2024!@#`
5. Click "Change Password" button
6. Wait for dashboard

**If login fails (error message appears):**
- Try fallback password `QaTest2024!@#` (password may have been changed in prior run)

## Form Fill Patterns

### Standard Text Inputs
```
browser-use --session S input INDEX "value"
```

### React Controlled Inputs (CurrencyInput, etc.)
If `browser-use input` doesn't work (value reverts after React re-render), use eval:
```
browser-use --session S eval "const input = document.querySelector('CSS_SELECTOR'); const nativeSetter = Object.getOwnPropertyDescriptor(window.HTMLInputElement.prototype, 'value').set; nativeSetter.call(input, 'VALUE'); input.dispatchEvent(new Event('input', { bubbles: true }));"
```

### Combobox Fields
1. Click the combobox trigger button
2. Run `state` to see the opened popover with options
3. Click the desired option by index

### Date Inputs
```
browser-use --session S input INDEX "2026-03-21"
```

### Form Submission
After filling all fields, click the submit button, then:
```
browser-use --session S wait text "successfully"  # Wait for success toast
browser-use --session S state                      # Verify result
```

## Toast Verification

After create/update/delete operations, look for toast text in `state` output:
- Create: "created successfully"
- Update: "updated successfully"
- Delete: "deleted successfully"

## Structured Result Format

At the END of your test run, output this JSON block (fenced in triple backticks with `json` language tag). The orchestrator parses this to build the report.

```json
{
  "group": N,
  "name": "Group Name",
  "status": "PASS|FAIL|BLOCKED|PARTIAL",
  "tests": [
    {
      "name": "Test name",
      "status": "PASS|FAIL|SKIP",
      "details": "Brief description of result or error",
      "screenshot": "$TEMP/qa-suite-groupN-testname.png"
    }
  ]
}
```

**Status rules:**
- `PASS` -- all tests passed
- `FAIL` -- at least one test failed
- `BLOCKED` -- could not run tests (auth failure, page not loading, etc.)
- `PARTIAL` -- some tests ran, others could not

## Cleanup

Always close your browser session when done:
```
browser-use --session SESSION close
```

## Test Data Naming Convention

All test data MUST use the `QA` prefix for easy identification and cleanup:
- Account code: `QA-001`, `QA-001-UPD`
- Account name: `QA Test Account`, `QA Test Account Updated`
- Category: `QA Category`, `QA Category Updated`
- Subcategory: `QA Subcategory`, `QA Sub Updated`
- Item template: `QA Template`, `QA Template Updated`
- Receipt location: `QA Store`
- Transaction/item descriptions: `QA Test Item`, `QA Item Updated`
