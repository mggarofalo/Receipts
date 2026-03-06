#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
if [ -f "${SCRIPT_DIR}/../.env" ]; then
    set -a
    source "${SCRIPT_DIR}/../.env"
    set +a
fi

# Update Receipts to a new image version with automatic rollback on failure.
# Usage: ./scripts/update.sh [image_tag]

NEW_TAG="${1:-latest}"
HEALTH_URL="http://localhost:${APP_PORT:-8080}/api/health"
MAX_WAIT=60

# Capture current image ID for rollback (tags are mutable, IDs are not)
CURRENT_IMAGE_ID=$(docker compose images app --format json 2>/dev/null | head -1 | grep -o '"ID":"[^"]*"' | cut -d'"' -f4 || echo "")
CURRENT_IMAGE_TAG=$(docker compose images app --format json 2>/dev/null | head -1 | grep -o '"Tag":"[^"]*"' | cut -d'"' -f4 || echo "")
if [ -z "${CURRENT_IMAGE_ID}" ] && [ -z "${CURRENT_IMAGE_TAG}" ]; then
    echo "Warning: Could not determine current image for rollback"
fi

echo "Updating to tag: ${NEW_TAG}"

# Pre-update backup
echo "Creating pre-update backup..."
bash "${SCRIPT_DIR}/backup.sh"

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
if [ -n "${CURRENT_IMAGE_ID}" ]; then
    # Re-tag the previous image by ID so compose resolves it correctly
    COMPOSE_IMAGE=$(docker compose config --images 2>/dev/null | head -1 || echo "")
    IMAGE_NAME="${COMPOSE_IMAGE%:*}"
    if [ -n "${IMAGE_NAME}" ] && [ -n "${CURRENT_IMAGE_TAG}" ]; then
        docker tag "${CURRENT_IMAGE_ID}" "${IMAGE_NAME}:${CURRENT_IMAGE_TAG}"
    fi
    IMAGE_TAG="${CURRENT_IMAGE_TAG:-latest}" docker compose up -d app
    echo "Rolled back to image ID ${CURRENT_IMAGE_ID}"
elif [ -n "${CURRENT_IMAGE_TAG}" ]; then
    IMAGE_TAG="${CURRENT_IMAGE_TAG}" docker compose up -d app
    echo "Rolled back to tag ${CURRENT_IMAGE_TAG} (warning: tag may have been overwritten)"
else
    echo "ERROR: No previous image available for rollback. Manual intervention required."
fi
exit 1
