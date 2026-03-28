# Group 7: Dashboard & Analytics

**Session:** `qa-group-7`
**Dependencies:** None
**Data created:** None (read-only tests)

## Prerequisites

- Application running at `${BASE_URL}`
- Valid credentials: `${EMAIL}` / `${PASSWORD}`

## Login

Follow the login flow from qa-suite-common.md using SESSION, EMAIL, PASSWORD.

## Tests

### Test 7.1: Dashboard Load

**Steps:**

1. `browser-use --session qa-group-7 open ${BASE_URL}/`
2. `browser-use --session qa-group-7 wait text "Dashboard"`
3. `browser-use --session qa-group-7 state` -- verify H1 is "Dashboard"
4. `browser-use --session qa-group-7 eval "window.location.pathname"` -- verify URL is `/`

**Pass criteria:** Page loads at `/` with H1 heading "Dashboard".

**On failure:** `browser-use --session qa-group-7 screenshot "$TEMP/qa-suite-group7-dashboard-load-fail.png"`

---

### Test 7.2: Summary Cards

**Steps:**

1. `browser-use --session qa-group-7 state` -- inspect all interactive elements and text on the dashboard
2. Look for summary cards containing: "Total Receipts", "Total Spent", "Avg Trip Amount", "Top Category"

**Pass criteria:** At least the following summary card labels are visible in the state output: "Total Receipts", "Total Spent", "Avg Trip Amount", "Top Category". Each card should display a value (number, currency, or text).

**On failure:** `browser-use --session qa-group-7 screenshot "$TEMP/qa-suite-group7-summary-cards-fail.png"`

---

### Test 7.3: Charts

**Steps:**

1. `browser-use --session qa-group-7 state` -- inspect the page for chart section headings
2. Look for text: "Spending Over Time", "Spending by Category", "Spending by Account"

**Pass criteria:** All three chart sections exist on the dashboard: "Spending Over Time", "Spending by Category", "Spending by Account".

**On failure:** `browser-use --session qa-group-7 screenshot "$TEMP/qa-suite-group7-charts-fail.png"`

---

### Test 7.4: Time Range Filters

**Steps:**

1. `browser-use --session qa-group-7 state` -- look for time range filter buttons
2. Verify the following filter buttons exist: "7d", "30d", "90d", "YTD", "All Time", "Custom"
3. `browser-use --session qa-group-7 click FILTER_30D_INDEX` -- click "30d" filter
4. `browser-use --session qa-group-7 state` -- verify the button appears selected/active (check for aria-pressed, data-state, or visual class change)

**Pass criteria:** All six time range filter buttons are present. Clicking "30d" activates it (visual state change or aria attribute update).

**On failure:** `browser-use --session qa-group-7 screenshot "$TEMP/qa-suite-group7-timerange-fail.png"`

---

### Test 7.5: Trips Page

**Steps:**

1. `browser-use --session qa-group-7 open ${BASE_URL}/trips`
2. `browser-use --session qa-group-7 wait text "Trips"`
3. `browser-use --session qa-group-7 state` -- verify H1 is "Trips" and table renders

**Pass criteria:** Page loads at `/trips` with H1 heading "Trips". A table or list view is present (even if empty).

**On failure:** `browser-use --session qa-group-7 screenshot "$TEMP/qa-suite-group7-trips-fail.png"`

---

### Test 7.6: API Keys Page

**Steps:**

1. `browser-use --session qa-group-7 open ${BASE_URL}/api-keys`
2. `browser-use --session qa-group-7 wait text "API Keys"`
3. `browser-use --session qa-group-7 state` -- verify H1 is "API Keys"

**Pass criteria:** Page loads at `/api-keys` with H1 heading "API Keys".

**On failure:** `browser-use --session qa-group-7 screenshot "$TEMP/qa-suite-group7-apikeys-fail.png"`

---

### Test 7.7: Security Log

**Steps:**

1. `browser-use --session qa-group-7 open ${BASE_URL}/security`
2. `browser-use --session qa-group-7 wait text "Security Log"`
3. `browser-use --session qa-group-7 state` -- verify H1 is "Security Log"

**Pass criteria:** Page loads at `/security` with H1 heading "Security Log".

**On failure:** `browser-use --session qa-group-7 screenshot "$TEMP/qa-suite-group7-security-fail.png"`

## Cleanup

```
browser-use --session qa-group-7 close
```

## Result

Output structured JSON per qa-suite-common.md format.

```json
{
  "group": 7,
  "name": "Dashboard & Analytics",
  "status": "PASS|FAIL|BLOCKED|PARTIAL",
  "tests": [
    {
      "name": "Dashboard Load",
      "status": "PASS|FAIL|SKIP",
      "details": "...",
      "screenshot": "$TEMP/qa-suite-group7-dashboard-load-fail.png"
    },
    {
      "name": "Summary Cards",
      "status": "PASS|FAIL|SKIP",
      "details": "...",
      "screenshot": "$TEMP/qa-suite-group7-summary-cards-fail.png"
    },
    {
      "name": "Charts",
      "status": "PASS|FAIL|SKIP",
      "details": "...",
      "screenshot": "$TEMP/qa-suite-group7-charts-fail.png"
    },
    {
      "name": "Time Range Filters",
      "status": "PASS|FAIL|SKIP",
      "details": "...",
      "screenshot": "$TEMP/qa-suite-group7-timerange-fail.png"
    },
    {
      "name": "Trips Page",
      "status": "PASS|FAIL|SKIP",
      "details": "...",
      "screenshot": "$TEMP/qa-suite-group7-trips-fail.png"
    },
    {
      "name": "API Keys Page",
      "status": "PASS|FAIL|SKIP",
      "details": "...",
      "screenshot": "$TEMP/qa-suite-group7-apikeys-fail.png"
    },
    {
      "name": "Security Log",
      "status": "PASS|FAIL|SKIP",
      "details": "...",
      "screenshot": "$TEMP/qa-suite-group7-security-fail.png"
    }
  ]
}
```
