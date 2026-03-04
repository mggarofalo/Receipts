#!/usr/bin/env bash
set -euo pipefail

# Generate cryptographically secure secrets for Receipts deployment.
# Usage: ./scripts/generate-secrets.sh

JWT_KEY=$(openssl rand -base64 64 | tr -d '\n')
POSTGRES_PASSWORD=$(openssl rand -base64 32 | tr -d '\n')

echo "Generated secrets (copy into your .env file):"
echo ""
echo "JWT_KEY=${JWT_KEY}"
echo "POSTGRES_PASSWORD=${POSTGRES_PASSWORD}"
