# Route Matrix

All 19 routes in the Receipts app with their expected headings, access levels, and layout contexts.

## Public Routes (PublicLayout)

| # | Path | Page | Expected H1 | Access |
|---|------|------|-------------|--------|
| 1 | `/login` | Login | Sign In | Public |
| 2 | `/change-password` | ChangePassword | Change Password | Authenticated (must-reset only) |

## Protected Routes (Layout, requires auth)

| # | Path | Page | Expected H1 | Access |
|---|------|------|-------------|--------|
| 3 | `/` | Dashboard | Dashboard | User |
| 4 | `/accounts` | Accounts | Accounts | User |
| 5 | `/categories` | Categories | Categories | User |
| 6 | `/subcategories` | Subcategories | Subcategories | User |
| 7 | `/receipts` | Receipts | Receipts | User |
| 8 | `/receipts/new` | NewReceipt | New Receipt | User |
| 9 | `/receipt-items` | ReceiptItems | Receipt Items | User |
| 10 | `/transactions` | Transactions | Transactions | User |
| 11 | `/trips` | Trips | Trips | User |
| 12 | `/item-templates` | ItemTemplates | Item Templates | User |
| 13 | `/receipt-detail` | ReceiptDetail | Receipt Details | User |
| 14 | `/transaction-detail` | TransactionDetail | Transaction Details | User |
| 15 | `/api-keys` | ApiKeys | API Keys | User |
| 16 | `/security` | SecurityLog | Security Log | User |

## Admin Routes (AdminRoute, requires admin role)

| # | Path | Page | Expected H1 | Access |
|---|------|------|-------------|--------|
| 17 | `/audit` | AuditLog | Audit Log | Admin |
| 18 | `/trash` | RecycleBin | Recycle Bin | Admin |
| 19 | `/admin/users` | AdminUsers | User Management | Admin |

## Catch-All

| Path | Page | Expected H1 |
|------|------|-------------|
| `/*` (any unmatched) | NotFound | Page Not Found |

## Route Notes

- **`/change-password`** only renders the form when `mustResetPassword` is true. If the user doesn't need a password change, it redirects to `/`. For smoke testing, just verify the URL doesn't error — don't expect the form to be visible.
- **`/receipt-detail`** and **`/transaction-detail`** require query params (`?id=...`) to show data. Without params, they show an empty state or placeholder. Verify the page loads without errors.
- **`/receipts/new`** is the 4-step wizard. For smoke testing, just verify step 1 loads with the "Trip Details" card.
- **Admin routes** (`/audit`, `/trash`, `/admin/users`) require the logged-in user to have an admin role. The seeded `admin@receipts.local` user is an admin.
- **Auth guard behavior**: Unauthenticated access to protected routes redirects to `/login`. Non-admin access to admin routes redirects to `/` (dashboard).
