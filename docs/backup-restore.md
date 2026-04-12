# Backup & Restore

The application supports portable SQLite backups for disaster recovery, migration, and offline archival. Both a REST API and a CLI tool are available.

## What gets backed up

All domain entities are included in the export:

- Accounts
- Categories
- Subcategories
- Item Templates
- Receipts
- Receipt Items
- Transactions
- Adjustments

Soft-deleted records are **excluded** from exports.

## REST API

Both endpoints require the **Admin** role.

### Export

```
POST /api/backup/export
Authorization: Bearer <token>
```

Returns a SQLite database file as `application/octet-stream` with filename `receipts-backup-{yyyyMMdd-HHmmss}.db`.

### Import

```
POST /api/backup/import
Content-Type: multipart/form-data
Authorization: Bearer <token>
```

Accepts a single file upload (`.sqlite`, `.sqlite3`, or `.db`, max 100 MB). The import uses **upsert** semantics: existing records are updated by primary key, new records are created. Previously soft-deleted records are restored if they appear in the backup.

Response:

```json
{
  "accountsCreated": 2, "accountsUpdated": 1,
  "categoriesCreated": 3, "categoriesUpdated": 0,
  "subcategoriesCreated": 5, "subcategoriesUpdated": 2,
  "itemTemplatesCreated": 4, "itemTemplatesUpdated": 1,
  "receiptsCreated": 10, "receiptsUpdated": 0,
  "receiptItemsCreated": 30, "receiptItemsUpdated": 5,
  "transactionsCreated": 10, "transactionsUpdated": 0,
  "adjustmentsCreated": 3, "adjustmentsUpdated": 0,
  "totalCreated": 67, "totalUpdated": 9
}
```

## CLI tool (DbExporter)

For scripted or cron-based backups without the web API:

```bash
# Default output path (temp directory)
dotnet run --project src/Tools/DbExporter

# Custom output path
dotnet run --project src/Tools/DbExporter -- /backups/receipts.db
```

Requires database connection via `POSTGRES_*` environment variables or Aspire connection string.

## UI

The **Backup & Restore** page is available at `/admin/backup` (admin users only). It provides:

- **Export**: One-click download with progress spinner
- **Import**: File picker with size display, confirmation dialog, and per-entity result summary

## Operational notes

- Backups are self-contained SQLite files — no external dependencies needed to read them
- Import is transactional: if any step fails, the entire import is rolled back
- The CLI tool is useful for automated backup schedules (e.g., cron on the host machine)
- Store backups off-device for true disaster recovery
