#!/usr/bin/env bash
set -euo pipefail

# Generate cryptographically secure secrets for Receipts deployment.
# Outputs a complete .env file to stdout.
#
# Usage:
#   bash scripts/generate-secrets.sh > .env
#   docker compose up -d

JWT_KEY=$(openssl rand -base64 64 | tr -d '\n')
POSTGRES_PASSWORD=$(openssl rand -base64 32 | tr -d '\n')
# Admin password: 24 chars mixing upper, lower, digits, and special chars.
ADMIN_PASSWORD=$(openssl rand -base64 24 | tr -d '\n')

cat <<EOF
# Receipts — Generated Environment Configuration
# Generated on: $(date -u +"%Y-%m-%dT%H:%M:%SZ")
# NEVER commit this file to source control.

# ─── Database ───────────────────────────────────────────────────────────────────
POSTGRES_USER=receipts
POSTGRES_PASSWORD=${POSTGRES_PASSWORD}
POSTGRES_DB=receipts

# ─── Authentication ─────────────────────────────────────────────────────────────
JWT_KEY=${JWT_KEY}
JWT_ISSUER=receipts-api
JWT_AUDIENCE=receipts-app

# ─── Admin Seed ─────────────────────────────────────────────────────────────────
ADMIN_EMAIL=admin@example.com
ADMIN_PASSWORD=${ADMIN_PASSWORD}
ADMIN_FIRST_NAME=Admin
ADMIN_LAST_NAME=User

# ─── Application ────────────────────────────────────────────────────────────────
APP_PORT=8080
IMAGE_TAG=latest
EOF

echo "" >&2
echo "Secrets generated. Review .env then run: docker compose up -d" >&2
