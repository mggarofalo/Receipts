#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
if [ -f "${SCRIPT_DIR}/../.env" ]; then
    set -a
    source "${SCRIPT_DIR}/../.env"
    set +a
fi

# Backup the Receipts PostgreSQL database.
# Usage: ./scripts/backup.sh [backup_dir]
# Keeps 7 days of backups by default.

BACKUP_DIR="${1:-./backups}"
RETENTION_DAYS=7
TIMESTAMP=$(date +%Y%m%d_%H%M%S)
BACKUP_FILE="${BACKUP_DIR}/receipts_${TIMESTAMP}.sql.gz"

mkdir -p "${BACKUP_DIR}"

echo "Backing up database to ${BACKUP_FILE}..."

docker compose exec -T db pg_dump \
    -U "${POSTGRES_USER:-receipts}" \
    -d "${POSTGRES_DB:-receipts}" \
    --clean --if-exists \
    | gzip > "${BACKUP_FILE}"

echo "Backup complete: ${BACKUP_FILE} ($(du -h "${BACKUP_FILE}" | cut -f1))"

# Prune old backups
PRUNED=$(find "${BACKUP_DIR}" -name "receipts_*.sql.gz" -mtime "+$((RETENTION_DAYS - 1))" -delete -print | wc -l)
if [ "${PRUNED}" -gt 0 ]; then
    echo "Pruned ${PRUNED} backup(s) older than ${RETENTION_DAYS} days"
fi
