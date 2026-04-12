# Reports

The application includes a suite of financial reports accessible from the Reports view in the UI and via REST endpoints under `/api/reports`. All endpoints require authentication.

## Dashboard

The dashboard (`/api/dashboard`) provides at-a-glance metrics:

| Endpoint | Description |
|----------|-------------|
| `GET /summary` | Total spending, receipt count, average per receipt for a date range |
| `GET /spending-over-time` | Time-series spending with configurable granularity (daily/monthly/quarterly/yearly) |
| `GET /spending-by-category` | Top N categories by spend |
| `GET /spending-by-account` | Spending breakdown by payment account |
| `GET /spending-by-store` | Spending breakdown by store/location |
| `GET /earliest-receipt-year` | Earliest data point (used for date range selectors) |

All dashboard endpoints accept `startDate` and `endDate` query parameters.

## Available reports

### Out-of-Balance Receipts

```
GET /api/reports/out-of-balance
```

Finds receipts where items + tax + adjustments do not equal the transaction total. Useful for catching data entry errors.

**Parameters:** `sort` (date, difference), `page`, `pageSize`

### Spending by Location

```
GET /api/reports/spending-by-location
```

Aggregates spending by store location with visit count and average per visit.

**Parameters:** `startDate`, `endDate`, `sort` (location, visits, total, averagePerVisit), `page`, `pageSize`

### Item Similarity

```
GET /api/reports/item-similarity
```

Groups items with similar descriptions using trigram matching. Helps identify inconsistent naming (e.g., "Milk 2%" vs "2% Milk").

**Parameters:** `threshold` (0.3-0.95, default 0.7), `sort` (canonicalName, occurrences, maxSimilarity), `page`, `pageSize`

Bulk rename is available via `POST /api/reports/item-similarity/rename` with `itemIds` and `newDescription`.

### Item Cost Over Time

```
GET /api/reports/item-cost-over-time
```

Tracks price changes for a specific item or category over time.

**Parameters:** `description` or `category` (one required), `startDate`, `endDate`, `granularity` (exact, monthly, yearly)

### Duplicate Receipt Detection

```
GET /api/reports/duplicates
```

Detects potential double entries based on configurable matching criteria.

**Parameters:** `matchOn` (DateAndLocation, DateAndTotal, DateAndLocationAndTotal), `locationTolerance` (exact, normalized), `totalTolerance` (numeric threshold)

### Category Trends

```
GET /api/reports/category-trends
```

Time-series spending broken down by category, showing how spending patterns shift over time.

**Parameters:** `startDate`, `endDate`, `granularity` (daily, monthly, quarterly, yearly), `topN` (1-50, default 7)

### Uncategorized Items

```
GET /api/reports/uncategorized-items
```

Lists items still assigned to the default "Uncategorized" category. Supports bulk categorization from the UI.

**Parameters:** `sort` (description, total, itemCode), `page`, `pageSize`

### Item Descriptions (Autocomplete)

```
GET /api/reports/item-descriptions
```

Provides autocomplete suggestions for item descriptions during data entry.

**Parameters:** `search` (min 2 chars), `categoryOnly` (bool), `limit` (1-50, default 20)
