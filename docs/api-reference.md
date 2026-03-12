# API Reference

All endpoints are documented with OpenAPI metadata. Run the API and visit `/scalar` for the interactive API documentation.

## Core Resources

Each core resource supports CRUD, soft-delete/restore, and batch operations.

| Resource | Routes |
|---|---|
| **Accounts** | `GET\|PUT /api/accounts/{id}`, `GET\|POST\|DELETE /api/accounts`, `GET /api/accounts/deleted`, `POST /api/accounts/{id}/restore`, `POST\|PUT /api/accounts/batch` |
| **Categories** | `GET\|PUT /api/categories/{id}`, `GET\|POST\|DELETE /api/categories`, `GET /api/categories/deleted`, `POST /api/categories/{id}/restore`, `POST\|PUT /api/categories/batch`, `GET /api/categories/{categoryId}/subcategories` |
| **Subcategories** | `GET\|PUT /api/subcategories/{id}`, `GET\|POST\|DELETE /api/subcategories`, `GET /api/subcategories/deleted`, `POST /api/subcategories/{id}/restore`, `POST\|PUT /api/subcategories/batch` |
| **Item Templates** | `GET\|PUT /api/item-templates/{id}`, `GET\|POST\|DELETE /api/item-templates`, `GET /api/item-templates/deleted`, `POST /api/item-templates/{id}/restore` |
| **Receipts** | `GET\|PUT /api/receipts/{id}`, `GET\|POST\|DELETE /api/receipts`, `GET /api/receipts/deleted`, `POST /api/receipts/{id}/restore`, `POST\|PUT /api/receipts/batch` |
| **Receipt Items** | `GET\|POST\|PUT /api/receipt-items/{id}`, `GET\|DELETE /api/receipt-items`, `GET /api/receipt-items/deleted`, `POST /api/receipt-items/{id}/restore`, `GET /api/receipt-items/by-receipt-id/{receiptId}`, `POST\|PUT /api/receipt-items/{id}/batch` |
| **Transactions** | `GET /api/transactions/{id}`, `GET\|DELETE /api/transactions`, `GET /api/transactions/deleted`, `POST /api/transactions/{id}/restore`, `GET /api/transactions/by-receipt-id/{receiptId}`, `POST\|PUT /api/transactions/{receiptId}/{accountId}`, `POST\|PUT /api/transactions/{receiptId}/{accountId}/batch` |

## Aggregate Views

| Method | Route | Description |
|---|---|---|
| GET | `/api/receipts-with-items/by-receipt-id/{receiptId}` | Receipt + line items |
| GET | `/api/transaction-accounts/by-transaction-id/{transactionId}` | Transaction + account |
| GET | `/api/transaction-accounts/by-receipt-id/{receiptId}` | All transaction-accounts for a receipt |
| GET | `/api/trips/by-receipt-id/{receiptId}` | Full trip aggregate |

## Auth & Users

| Method | Route | Description |
|---|---|---|
| POST | `/api/auth/login` | Login with email and password |
| POST | `/api/auth/refresh` | Refresh access token |
| POST | `/api/auth/logout` | Logout and invalidate refresh token |
| POST | `/api/auth/change-password` | Change password |
| GET | `/api/auth/audit/me` | Current user's auth history |
| GET | `/api/auth/audit/recent` | Recent auth events (admin) |
| GET | `/api/auth/audit/failed` | Failed login attempts (admin) |
| GET\|POST | `/api/users` | List / create users |
| GET\|PUT\|DELETE | `/api/users/{userId}` | Get / update / deactivate user |
| POST | `/api/users/{userId}/reset-password` | Reset password (admin) |
| GET | `/api/users/{userId}/roles` | List roles |
| POST\|DELETE | `/api/users/{userId}/roles/{role}` | Assign / remove role |
| GET\|POST | `/api/apikeys` | List / create API keys |
| DELETE | `/api/apikeys/{id}` | Revoke API key |

## Audit & Trash

| Method | Route | Description |
|---|---|---|
| GET | `/api/audit/entity/{entityType}/{entityId}` | Audit history for an entity |
| GET | `/api/audit/recent` | Recent audit log entries |
| GET | `/api/audit/user/{userId}` | Audit logs by user |
| GET | `/api/audit/apikey/{apiKeyId}` | Audit logs by API key |
| POST | `/api/trash/purge` | Permanently delete all soft-deleted items |
| GET | `/api/health` | Health check |
