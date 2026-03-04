#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
if [ -f "${SCRIPT_DIR}/../.env" ]; then
    set -a
    source "${SCRIPT_DIR}/../.env"
    set +a
fi

# Roll back Receipts to a specific image tag.
# Usage: ./scripts/rollback.sh <image_tag>

if [ $# -lt 1 ]; then
    echo "Usage: $0 <image_tag>"
    echo ""
    echo "Example: $0 v1.2.3"
    echo "Example: $0 sha-abc1234"
    exit 1
fi

TAG="$1"
HEALTH_URL="http://localhost:${APP_PORT:-8080}/api/health"

echo "Rolling back to tag: ${TAG}"

IMAGE_TAG="${TAG}" docker compose pull app
IMAGE_TAG="${TAG}" docker compose up -d app

echo "Waiting for health check..."
sleep 5
if wget -qO- "${HEALTH_URL}" 2>/dev/null | grep -q '"status"'; then
    echo "Rollback to ${TAG} complete. Health check passed."
else
    echo "WARNING: Health check not passing yet. Monitor with: docker compose logs -f app"
fi
