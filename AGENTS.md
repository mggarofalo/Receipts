# AGENTS.md

This file provides guidance to AI agents when working with code in this repository.

## Prerequisites

- **.NET 10 SDK** — build, test, and run the API
- **Node.js 18+** and **npm** — OpenAPI spec linting (`@stoplight/spectral-cli`) and semantic drift detection (`js-yaml`)
- **PostgreSQL** — runtime database (connection via environment variables)
- **ONNX model** — `all-MiniLM-L6-v2` for local embeddings (downloaded via `bash scripts/download-onnx-model.sh`)

After cloning, run `dotnet restore Receipts.slnx` then `npm install`. Run `bash scripts/download-onnx-model.sh` to download the ONNX embedding model (~90MB, required at runtime but not for building or CI).

## Worktree Setup

When working in a git worktree (e.g., created by Claude Code's `/worktree` or `git worktree add`), the workspace needs bootstrapping before you can build or test.

### Detecting a worktree

- `test -f .git` — if `.git` is a **file** (not a directory), you're in a worktree
- `git rev-parse --show-toplevel` — confirms your working directory root

### What's shared vs not shared

| Shared (via common `.git` dir) | NOT shared (need fresh setup) |
|-------------------------------|-------------------------------|
| Git history, refs, branches | `node_modules/` (root and `src/client/`) |
| Hooks config (`core.hooksPath`) | `bin/` / `obj/` (.NET build output) |
| | `openapi/generated/` |
| | `src/Presentation/API/Generated/*.g.cs` |
| | `src/client/src/generated/` |

### Bootstrap commands

Run these in order (or use `scripts/worktree-setup.sh` to run them all):

```bash
dotnet restore Receipts.slnx          # NuGet packages + configures git hooks
npm install                            # Root tooling (Spectral, js-yaml, cross-env)
cd src/client && npm install && cd -   # React client dependencies
bash scripts/download-onnx-model.sh    # Download ONNX embedding model (~90MB)
dotnet build Receipts.slnx             # Compiles + generates DTOs and openapi/generated/API.json
cd src/client && npm run generate:types && cd -  # TypeScript types from OpenAPI spec
```

### Branch naming in worktrees

Use the same convention as issue branches (Linear's `gitBranchName`). Worktrees are just an isolation mechanism — the branch name should reflect the work, not the worktree.

### Permission settings

The file `.claude/settings.local.json` pre-approves read operations, MCP tools (Linear), git commands, and build tools so agents don't face excessive approval prompts. This file is gitignored (`.local.json` suffix) so it's per-user. Copy it from the main worktree if it's missing.

## Workflow Rules

### Linear

All issue work is tracked in Linear. See **[docs/linear.md](docs/linear.md)** for workspace structure, milestone phases, priority semantics, label conventions, and the "what's next" decision process.

- Team: "Mggarofalo" (ID: `a4aff05d-41e6-45dc-b670-cdb485fef765`), Project: "Receipts"
- All issues assigned to a milestone (Phase 0-8) with at least one layer label (`backend`, `frontend`, `infra`, `docs`)
- Issues labeled `epic` are parent containers — skip and work their children
- **Issue lookup fallback:** If you cannot find an issue via the Linear MCP (e.g., API error, deleted, or archived), check `docs/issues/` — Linear issues are archived there as Markdown files named by issue ID (e.g., `mgg-123-short-title.md`)

### Branching

Two-tier hierarchical model: milestone branches for CI/PR gating, issue branches for individual work. See **[docs/branching.md](docs/branching.md)** for full strategy, merge procedures, and directory isolation.

### Commit Convention

[Conventional Commits](https://www.conventionalcommits.org/) format: `<type>(<scope>): <description>`

| Types | `feat`, `fix`, `docs`, `refactor`, `test`, `chore` |
|-------|-----------------------------------------------------|
| Scopes | `api`, `client`, `domain`, `application`, `infrastructure`, `infra`, `common`, `shared`, `ci`, `hooks` |

Enforced by:
- **Local:** `commit-msg` hook runs `commitlint` on every commit (see `.githooks/commit-msg`)
- **CI:** PR title validation via `amannn/action-semantic-pull-request` (squash-merge means the PR title becomes the commit on `main`)
- **Config:** `commitlint.config.mjs` at the repo root defines allowed types, scopes, and header length (100 chars max)

Multiple scopes are allowed with a comma separator (e.g., `feat(api,client): add pagination`).

### OpenAPI & API Guidelines

Spec-first workflow, endpoint return types, and authentication standards. See **[docs/api-guidelines.md](docs/api-guidelines.md)** for full details.

## Build and Test Commands

```bash
dotnet build Receipts.slnx                                    # Build entire solution
dotnet test Receipts.slnx                                     # Run all tests (including integration)
dotnet test Receipts.slnx --filter "Category!=Integration"    # Unit tests only (used by CI and pre-commit)
dotnet test --filter "Category=Integration"                    # Integration tests only (requires ONNX model)
dotnet test tests/Application.Tests/Application.Tests.csproj  # Single project
dotnet test --filter "FullyQualifiedName~TestMethodName"       # Single test
dotnet run --project src/Tools/DbMigrator/DbMigrator.csproj   # Apply EF Core migrations
dotnet run --project src/Tools/DbSeeder/DbSeeder.csproj       # Seed roles and admin user
dotnet run --project src/Presentation/API/API.csproj           # Run the API
dotnet ef migrations add MigrationName --project src/Infrastructure/Infrastructure.csproj --startup-project src/Tools/DbMigrator/DbMigrator.csproj
```

**Important:** The API does not self-migrate or self-seed. When running without Aspire, you must run DbMigrator and DbSeeder before starting the API. Aspire and Docker handle this automatically via orchestration.

## Git Hooks

Native Git hooks via `core.hooksPath`. Install automatically on `dotnet restore` (or `bash .githooks/setup.sh`).

### `commit-msg` hook

Validates the commit message against the Conventional Commits convention using `commitlint`. Runs after you write the commit message but before the commit is finalized.

### `pre-commit` hook

**Pipeline (runs on every `git commit`):**
0. `bash scripts/worktree-setup.sh --check` — prerequisite verification
1. `npx spectral lint openapi/spec.yaml` — OpenAPI spec linting
2. `dotnet format --verify-no-changes` — code formatting check
3. `dotnet build -p:TreatWarningsAsErrors=true` — build (also regenerates DTOs and `openapi/generated/API.json`)
4. `node scripts/check-drift.mjs` — semantic drift detection
5. `dotnet test --no-build --filter "Category!=Integration"` — run unit tests
6. `npx tsc -b --noEmit` — TypeScript type checking
7. `npx eslint src/client/src` — React client linting

**Quick mode** runs only steps 0, 2, 6, 7 (prerequisites, format, tsc, eslint):
```bash
PRECOMMIT_QUICK=1 git commit -m "message"
```

`git commit --no-verify` skips all hooks — use only as a last resort.

## Architecture

.NET 10 Clean Architecture: Domain → Application → Infrastructure → Presentation. Uses CQRS with MediatR, Repository pattern, Mapperly for compile-time mapping, soft-delete with audit logging.

See **[docs/architecture.md](docs/architecture.md)** for full layer structure, key patterns, Mapperly code examples, and test project layout.

## Coding Standards

C# conventions, Mapperly rules, EF Core query guidelines, and LSP validation workflow. See **[docs/coding-standards.md](docs/coding-standards.md)** for full details.

### React Custom Hook Conventions

All functions, objects, and arrays returned from custom hooks (`use*`) **must** be referentially stable:

- Wrap returned functions in `useCallback`
- Wrap returned objects/arrays in `useMemo`
- Ensure reducers return the same state reference when values haven't changed (bail out with `return state`)

**Why:** Consumers may place hook return values in `useEffect`/`useMemo`/`useCallback` dependency arrays. Unstable references cause infinite render loops that are invisible in static review and pass individual test files but hang the full test suite.

## Agent Workflow Rules

### Tests and Code Review

**All new functionality must include tests.** When implementing a new feature, endpoint, command, query, or fixing a bug, include corresponding unit tests in the same PR. Do not merge code without test coverage for the changes introduced. Follow existing test conventions (xUnit, Arrange/Act/Assert, FluentAssertions, Moq).

**Integration tests** use `[Trait("Category", "Integration")]` and are excluded from CI and pre-commit hooks via `--filter "Category!=Integration"`. Use this trait for tests that require external resources (e.g., ONNX model files, PostgreSQL). Unit tests do not need a trait — they run everywhere by default.

**Never write tests or perform code review in the main conversation context.** Always spawn subagents for these tasks:
- Use the `test-runner` or equivalent subagent for running and writing tests
- Use `pr-review-toolkit:code-reviewer` or similar review agents for code review
- This keeps the main context focused on implementation and prevents context window bloat
