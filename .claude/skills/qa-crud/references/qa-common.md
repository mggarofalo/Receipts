# QA Common Reference

Shared patterns for all QA skills. Each skill embeds auth inline since agent-browser sessions don't persist across skill invocations.

## Port Discovery

Aspire assigns random ports — never hardcode. Discover the frontend URL before any browser interaction.

### Recipe

1. Call `mcp__aspire__list_console_logs` for the `frontend` resource
2. Parse logs for the line containing `Local:   http://localhost:XXXX`
3. Extract the full URL (e.g., `http://localhost:5173`)
4. Store as `BASE_URL` for the session

If `list_console_logs` returns no output or the frontend resource isn't found, the app is not running. Abort with: "Aspire is not running. Start with F5 or `dotnet run --project src/Receipts.AppHost`."

## Login Flow

### Credentials

| Role  | Email                    | Initial Password | Post-Change Password |
|-------|--------------------------|------------------|----------------------|
| Admin | `admin@receipts.local`   | `Admin123!@#`    | `QaTest2024!@#`      |

### Steps

1. Open login page:
   ```
   agent-browser open ${BASE_URL}/login
   ```
2. Wait for page load:
   ```
   agent-browser wait --load networkidle
   ```
3. Take snapshot to get form refs:
   ```
   agent-browser snapshot -i
   ```
4. Fill email field (find the `[input type="email"]` ref):
   ```
   agent-browser fill @eN "admin@receipts.local"
   ```
5. Fill password field (find the `[input type="password"]` ref):
   ```
   agent-browser fill @eN "Admin123!@#"
   ```
6. Click the "Sign In" button:
   ```
   agent-browser click @eN
   ```
7. Wait for navigation:
   ```
   agent-browser wait --load networkidle
   ```
8. Check current URL:
   ```
   agent-browser get url
   ```

### Password Change Handling

If the URL after login is `/change-password`, the account requires a password reset:

1. Take snapshot to get change-password form refs:
   ```
   agent-browser snapshot -i
   ```
2. Fill "Current Password":
   ```
   agent-browser fill @eN "Admin123!@#"
   ```
3. Fill "New Password":
   ```
   agent-browser fill @eN "QaTest2024!@#"
   ```
4. Fill "Confirm New Password":
   ```
   agent-browser fill @eN "QaTest2024!@#"
   ```
5. Click "Change Password" button:
   ```
   agent-browser click @eN
   ```
6. Wait for redirect to dashboard:
   ```
   agent-browser wait --load networkidle
   ```

After password change, use `QaTest2024!@#` for all subsequent logins. If initial login fails with `Admin123!@#`, retry with `QaTest2024!@#` (password may have been changed in a prior session).

### Verifying Auth Success

After login (and optional password change), verify the dashboard loads:
```
agent-browser get url
```
Expected: URL ends with `/` (dashboard root).

Take a screenshot to confirm:
```
agent-browser screenshot "$TEMP/qa-login-success.png"
```

## Form Fill Patterns

### Standard Text Inputs

Use `agent-browser fill` with the snapshot ref:
```
agent-browser fill @eN "value"
```

### React Controlled Inputs (CurrencyInput, etc.)

If `agent-browser fill` doesn't work (value reverts after React re-render), use the nativeInputValueSetter fallback:
```
agent-browser eval "const input = document.querySelector('CSS_SELECTOR'); const nativeSetter = Object.getOwnPropertyDescriptor(window.HTMLInputElement.prototype, 'value').set; nativeSetter.call(input, 'VALUE'); input.dispatchEvent(new Event('input', { bubbles: true }));"
```

### Combobox Fields (Combobox component)

Comboboxes use a trigger button + popover. To select an option:

1. Click the combobox trigger button (shows as `[button]` with placeholder text):
   ```
   agent-browser click @eN
   ```
2. Wait for popover to open, then snapshot:
   ```
   agent-browser snapshot -i
   ```
3. Click the desired option (`[option]` or `[menuitem]` in the list):
   ```
   agent-browser click @eN
   ```

For comboboxes with `allowCustom`, you can type a custom value in the search input that appears after clicking the trigger.

### Date Inputs (DateInput component)

DateInput is a standard `<input type="date">`. Fill with ISO date format:
```
agent-browser fill @eN "2026-03-18"
```

### Checkbox/Switch Toggle

Click the checkbox or switch element:
```
agent-browser click @eN
```

### Form Submission

After filling all fields, click the submit button. Then wait for the response:
```
agent-browser click @eN
agent-browser wait --load networkidle
```

## Toast Verification

Success toasts appear as `[status]` or `[alert]` nodes in the accessibility tree. After a mutation:
```
agent-browser snapshot -i
```
Look for toast text like "created successfully" or "updated successfully" in the snapshot output.

## Error Checking

### Console Errors

Check for JavaScript errors:
```
agent-browser eval "window.__consoleErrors || []"
```

Note: This requires prior injection. As an alternative, check the page visually for error boundaries or error alerts.

### Visual Error Detection

Take a screenshot and look for:
- Red alert banners
- Error boundary fallback text ("Something went wrong")
- Blank/white pages (failed to render)
- Loading spinners that never resolve (take screenshot after waiting)

## Cleanup

Always close the browser at the end of a test run:
```
agent-browser close
```

## Important Rules

- **Separate Bash calls**: Every `agent-browser` command MUST be a separate Bash tool call. Never chain with `&&`.
- **Screenshots to $TEMP**: Always `agent-browser screenshot "$TEMP/filename.png"`, never to the working directory.
- **No hardcoded ports**: Always discover via `mcp__aspire__list_console_logs`.
- **Re-snapshot after navigation**: After any navigation or DOM change, take a fresh `agent-browser snapshot -i` to get updated refs.
