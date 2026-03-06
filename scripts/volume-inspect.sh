#!/usr/bin/env bash
set -euo pipefail

# Inspect Docker volume usage for the Receipts database.
# Usage: ./scripts/volume-inspect.sh

COMPOSE_PROJECT=$(basename "$(pwd)" | tr '[:upper:]' '[:lower:]')
VOLUME_NAME="${COMPOSE_PROJECT}_db-data"

echo "Volume: ${VOLUME_NAME}"
echo ""

if docker volume inspect "${VOLUME_NAME}" > /dev/null 2>&1; then
    docker volume inspect "${VOLUME_NAME}"
    echo ""
    echo "Disk usage:"
    docker system df -v 2>/dev/null | grep -E "(VOLUME|${VOLUME_NAME})" || \
        echo "  Run 'docker system df -v' for detailed usage"
else
    echo "Volume not found. Has docker compose been run?"
fi
