#!/usr/bin/env bash
set -euo pipefail

# Update Receipts to a new image version with automatic rollback on failure.
# Usage: ./scripts/update.sh [image_tag]

NEW_TAG="${1:-latest}"
HEALTH_URL="http://localhost:${APP_PORT:-8080}/api/health"
MAX_WAIT=60

# Capture current image for rollback
CURRENT_IMAGE=$(docker compose images app --format json 2>/dev/null | head -1 | grep -o '"Tag":"[^"]*"' | cut -d'"' -f4 || echo "")
if [ -z "${CURRENT_IMAGE}" ]; then
    echo "Warning: Could not determine current image tag for rollback"
fi

echo "Updating to tag: ${NEW_TAG}"

# Pre-update backup
echo "Creating pre-update backup..."
bash scripts/backup.sh

# Pull new image
echo "Pulling new image..."
IMAGE_TAG="${NEW_TAG}" docker compose pull app

# Restart with new image
echo "Restarting app..."
IMAGE_TAG="${NEW_TAG}" docker compose up -d app

# Wait for health check
echo "Waiting for health check (max ${MAX_WAIT}s)..."
ELAPSED=0
while [ $ELAPSED -lt $MAX_WAIT ]; do
    if wget -qO- "${HEALTH_URL}" 2>/dev/null | grep -q '"status"'; then
        echo "Health check passed after ${ELAPSED}s"
        echo "Update to ${NEW_TAG} complete."
        exit 0
    fi
    sleep 2
    ELAPSED=$((ELAPSED + 2))
done

# Health check failed — rollback
echo "Health check failed after ${MAX_WAIT}s. Rolling back..."
if [ -n "${CURRENT_IMAGE}" ]; then
    IMAGE_TAG="${CURRENT_IMAGE}" docker compose up -d app
    echo "Rolled back to ${CURRENT_IMAGE}"
else
    echo "ERROR: No previous image tag available for rollback. Manual intervention required."
fi
exit 1
