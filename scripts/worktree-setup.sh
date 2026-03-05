#!/usr/bin/env bash
# Bootstrap a git worktree with all dependencies and generated files.
# Usage: bash scripts/worktree-setup.sh

set -euo pipefail

ROOT="$(git rev-parse --show-toplevel)"
cd "$ROOT"

echo "==> Restoring NuGet packages and configuring git hooks..."
dotnet restore Receipts.slnx

echo "==> Installing root npm dependencies..."
npm install

echo "==> Installing client npm dependencies..."
(cd src/client && npm install)

echo "==> Building solution (generates openapi/generated/API.json)..."
dotnet build Receipts.slnx

echo "==> Generating TypeScript types from OpenAPI spec..."
(cd src/client && npm run generate:types)

echo "==> Worktree setup complete."
