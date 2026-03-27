# Group 9: Cross-Cutting

**Session:** `qa-group-9`
**Dependencies:** None
**Data created:** None (read-only tests)

## Prerequisites

- Application running at `${BASE_URL}`
- Valid credentials: `${EMAIL}` / `${PASSWORD}`

## Login

Follow the login flow from qa-suite-common.md using SESSION, EMAIL, PASSWORD.

## Tests

### Test 9.1: Navigation Structure

**Steps:**

1. `browser-use --session qa-group-9 open ${BASE_URL}/`
2. `browser-use --session qa-group-9 wait text "Dashboard"`
3. `browser-use --session qa-group-9 state` -- map all navigation links visible in the sidebar/header
4. Identify the navigation groups: look for "Data" dropdown (Receipts, Transactions, Receipt Items, Trips), "Manage" dropdown (Accounts, Categories, Subcategories, Item Templates), "Admin" dropdown (User Management, Audit Log, Recycle Bin)

**Pass criteria:** Navigation contains at least three dropdown groups (Data, Manage, Admin) with the expected sub-links. All major routes from the route matrix are represented.

**On failure:** `browser-use --session qa-group-9 screenshot "$TEMP/qa-suite-group9-nav-structure-fail.png"`

---

### Test 9.2: Navigate All Sections

**Steps:**

For each of the following routes, navigate and verify the expected H1 heading per the route matrix:

1. `browser-use --session qa-group-9 open ${BASE_URL}/accounts` then `browser-use --session qa-group-9 wait text "Accounts"`
2. `browser-use --session qa-group-9 open ${BASE_URL}/categories` then `browser-use --session qa-group-9 wait text "Categories"`
3. `browser-use --session qa-group-9 open ${BASE_URL}/subcategories` then `browser-use --session qa-group-9 wait text "Subcategories"`
4. `browser-use --session qa-group-9 open ${BASE_URL}/receipts` then `browser-use --session qa-group-9 wait text "Receipts"`
5. `browser-use --session qa-group-9 open ${BASE_URL}/transactions` then `browser-use --session qa-group-9 wait text "Transactions"`
6. `browser-use --session qa-group-9 open ${BASE_URL}/receipt-items` then `browser-use --session qa-group-9 wait text "Receipt Items"`
7. `browser-use --session qa-group-9 open ${BASE_URL}/trips` then `browser-use --session qa-group-9 wait text "Trips"`
8. `browser-use --session qa-group-9 open ${BASE_URL}/item-templates` then `browser-use --session qa-group-9 wait text "Item Templates"`
9. `browser-use --session qa-group-9 open ${BASE_URL}/api-keys` then `browser-use --session qa-group-9 wait text "API Keys"`
10. `browser-use --session qa-group-9 open ${BASE_URL}/security` then `browser-use --session qa-group-9 wait text "Security Log"`
11. `browser-use --session qa-group-9 open ${BASE_URL}/admin/users` then `browser-use --session qa-group-9 wait text "User Management"`
12. `browser-use --session qa-group-9 open ${BASE_URL}/audit` then `browser-use --session qa-group-9 wait text "Audit Log"`
13. `browser-use --session qa-group-9 open ${BASE_URL}/trash` then `browser-use --session qa-group-9 wait text "Recycle Bin"`

**Pass criteria:** Every route loads successfully and displays its expected H1 heading. No JavaScript errors or blank pages.

**On failure:** `browser-use --session qa-group-9 screenshot "$TEMP/qa-suite-group9-nav-sections-fail.png"` -- note which specific route(s) failed.

---

### Test 9.3: Search

**Steps:**

1. `browser-use --session qa-group-9 open ${BASE_URL}/accounts`
2. `browser-use --session qa-group-9 wait text "Accounts"`
3. `browser-use --session qa-group-9 state` -- find search/filter input on the page
4. `browser-use --session qa-group-9 input SEARCH_INDEX "test"` -- type a search query
5. `browser-use --session qa-group-9 state` -- verify the table content updates (rows filter or "no results" message appears)

**Pass criteria:** Typing in the search input triggers filtering of the table rows. The table either shows filtered results matching "test" or displays a "no results" message.

**On failure:** `browser-use --session qa-group-9 screenshot "$TEMP/qa-suite-group9-search-fail.png"`

---

### Test 9.4: Theme Toggle

**Steps:**

1. `browser-use --session qa-group-9 open ${BASE_URL}/`
2. `browser-use --session qa-group-9 wait text "Dashboard"`
3. `browser-use --session qa-group-9 eval "document.documentElement.className"` -- capture initial theme class
4. `browser-use --session qa-group-9 state` -- find theme toggle dropdown button (sun/moon icon in header)
5. `browser-use --session qa-group-9 click THEME_TOGGLE_INDEX` -- click to open theme dropdown
6. `browser-use --session qa-group-9 state` -- find "Light" option in the dropdown
7. `browser-use --session qa-group-9 click LIGHT_OPTION_INDEX` -- select "Light"
8. `browser-use --session qa-group-9 eval "document.documentElement.className"` -- capture new theme class

**Pass criteria:** After selecting "Light", the `class` attribute on `<html>` changes to include "light" (or removes "dark"). The two captured class values must be different.

**On failure:** `browser-use --session qa-group-9 screenshot "$TEMP/qa-suite-group9-theme-fail.png"`

---

### Test 9.5: Command Palette

**Steps:**

1. `browser-use --session qa-group-9 open ${BASE_URL}/`
2. `browser-use --session qa-group-9 wait text "Dashboard"`
3. `browser-use --session qa-group-9 eval "document.dispatchEvent(new KeyboardEvent('keydown', { key: 'k', ctrlKey: true, bubbles: true }))"` -- dispatch Ctrl+K
4. `browser-use --session qa-group-9 state` -- look for a dialog/modal with a search input (command palette)

**Pass criteria:** A command palette dialog opens with a search input. The dialog is visible in the state output.

**On failure:** `browser-use --session qa-group-9 screenshot "$TEMP/qa-suite-group9-cmdpalette-fail.png"`

**Restore:** Close the dialog (press Escape) before continuing.

---

### Test 9.6: 404 Page

**Steps:**

1. `browser-use --session qa-group-9 open ${BASE_URL}/nonexistent-page`
2. `browser-use --session qa-group-9 state` -- check the page content
3. `browser-use --session qa-group-9 eval "window.location.pathname"` -- check current URL

**Pass criteria:** Page shows "Page Not Found" heading, OR (known issue) redirects to dashboard for authenticated users. Note the actual behavior in the details.

**On failure:** `browser-use --session qa-group-9 screenshot "$TEMP/qa-suite-group9-404-fail.png"`

---

### Test 9.7: Breadcrumbs

**Steps:**

1. `browser-use --session qa-group-9 open ${BASE_URL}/accounts`
2. `browser-use --session qa-group-9 wait text "Accounts"`
3. `browser-use --session qa-group-9 state` -- look for breadcrumb navigation elements (nav with aria-label="breadcrumb" or breadcrumb-like links)
4. Verify breadcrumb trail renders with at least a link back to the parent section

**Pass criteria:** Breadcrumb navigation is visible on the page, showing the current page in the trail.

**On failure:** `browser-use --session qa-group-9 screenshot "$TEMP/qa-suite-group9-breadcrumbs-fail.png"`

## Cleanup

```
browser-use --session qa-group-9 close
```

## Result

Output structured JSON per qa-suite-common.md format.

```json
{
  "group": 9,
  "name": "Cross-Cutting",
  "status": "PASS|FAIL|BLOCKED|PARTIAL",
  "tests": [
    {
      "name": "Navigation Structure",
      "status": "PASS|FAIL|SKIP",
      "details": "...",
      "screenshot": "$TEMP/qa-suite-group9-nav-structure-fail.png"
    },
    {
      "name": "Navigate All Sections",
      "status": "PASS|FAIL|SKIP",
      "details": "...",
      "screenshot": "$TEMP/qa-suite-group9-nav-sections-fail.png"
    },
    {
      "name": "Search",
      "status": "PASS|FAIL|SKIP",
      "details": "...",
      "screenshot": "$TEMP/qa-suite-group9-search-fail.png"
    },
    {
      "name": "Theme Toggle",
      "status": "PASS|FAIL|SKIP",
      "details": "...",
      "screenshot": "$TEMP/qa-suite-group9-theme-fail.png"
    },
    {
      "name": "Command Palette",
      "status": "PASS|FAIL|SKIP",
      "details": "...",
      "screenshot": "$TEMP/qa-suite-group9-cmdpalette-fail.png"
    },
    {
      "name": "404 Page",
      "status": "PASS|FAIL|SKIP",
      "details": "...",
      "screenshot": "$TEMP/qa-suite-group9-404-fail.png"
    },
    {
      "name": "Breadcrumbs",
      "status": "PASS|FAIL|SKIP",
      "details": "...",
      "screenshot": "$TEMP/qa-suite-group9-breadcrumbs-fail.png"
    }
  ]
}
```
