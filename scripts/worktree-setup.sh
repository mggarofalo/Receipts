#!/usr/bin/env bash
# Bootstrap a git worktree with all dependencies and generated files.
# Usage: bash scripts/worktree-setup.sh          # full setup
#        bash scripts/worktree-setup.sh --check   # verify prerequisites only

set -euo pipefail

ROOT="$(git rev-parse --show-toplevel)"
cd "$ROOT"

if [ "${1:-}" = "--check" ]; then
  missing=()

  [ ! -d "node_modules" ] && missing+=("node_modules/ — run: npm install")
  [ ! -d "src/client/node_modules" ] && missing+=("src/client/node_modules/ — run: cd src/client && npm install")

  if ! find src/ -maxdepth 3 -type d -name obj -print -quit 2>/dev/null | grep -q .; then
    missing+=("src/*/obj/ — run: dotnet restore Receipts.slnx")
  fi

  [ ! -f "openapi/generated/API.json" ] && missing+=("openapi/generated/API.json — run: dotnet build Receipts.slnx")

  if ! find src/Presentation/API/Generated/ -name '*.g.cs' -print -quit 2>/dev/null | grep -q .; then
    missing+=("src/Presentation/API/Generated/*.g.cs — run: dotnet build Receipts.slnx")
  fi

  [ ! -f "src/client/src/generated/api.d.ts" ] && missing+=("src/client/src/generated/api.d.ts — run: cd src/client && npm run generate:types")

  if [ ${#missing[@]} -gt 0 ]; then
    echo "Pre-commit prerequisites missing:"
    for item in "${missing[@]}"; do
      echo "  - $item"
    done
    echo ""
    echo "Run 'bash scripts/worktree-setup.sh' to fix all at once."
    exit 1
  fi

  echo "All prerequisites present."
  exit 0
fi

echo "==> Restoring NuGet packages and configuring git hooks..."
dotnet restore Receipts.slnx

echo "==> Installing root npm dependencies..."
npm install

echo "==> Installing client npm dependencies..."
(cd src/client && npm install)

echo "==> Building solution (generates DTOs and openapi/generated/API.json)..."
dotnet build Receipts.slnx

echo "==> Generating TypeScript types from OpenAPI spec..."
(cd src/client && npm run generate:types)

echo "==> Worktree setup complete."
