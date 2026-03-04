#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
if [ -f "${SCRIPT_DIR}/../.env" ]; then
    set -a
    source "${SCRIPT_DIR}/../.env"
    set +a
fi

# Restore the Receipts PostgreSQL database from a backup.
# Usage: ./scripts/restore.sh <backup_file.sql.gz>

if [ $# -lt 1 ]; then
    echo "Usage: $0 <backup_file.sql.gz>"
    echo ""
    echo "Available backups:"
    ls -lh backups/receipts_*.sql.gz 2>/dev/null || echo "  No backups found in ./backups/"
    exit 1
fi

BACKUP_FILE="$1"

if [ ! -f "${BACKUP_FILE}" ]; then
    echo "Error: Backup file not found: ${BACKUP_FILE}"
    exit 1
fi

echo "WARNING: This will replace the current database with the backup."
echo "Backup: ${BACKUP_FILE}"
read -rp "Continue? [y/N] " confirm
if [[ ! "${confirm}" =~ ^[Yy]$ ]]; then
    echo "Aborted."
    exit 0
fi

echo "Stopping app..."
docker compose stop app

trap 'echo "Starting app..."; docker compose start app' EXIT

echo "Restoring database from ${BACKUP_FILE}..."
gunzip -c "${BACKUP_FILE}" | docker compose exec -T db psql \
    -U "${POSTGRES_USER:-receipts}" \
    -d "${POSTGRES_DB:-receipts}" \
    --quiet

echo "Restore complete."
