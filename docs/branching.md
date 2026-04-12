# Branching Strategy

This project uses a hierarchical branching model with `develop` as the integration branch. Feature work merges into `develop`, and `develop` is promoted to `main` via release cycles.

## Merge Flow

```
feature/issue branches → develop → main
                                    ↑
                          hotfix/* ──┘  (emergency only)
```

**CI enforces** that only `develop`, `release-please--*`, and `hotfix/*` branches can open PRs to `main`. All other branches must target `develop`.

## Branch Types

### `develop` — Integration Branch

- All feature/module/parent branches merge into `develop` via PR
- `develop` is promoted to `main` when a release is cut (via release-please or manual PR)
- After a release merges to `main`, an automated workflow creates a sync PR (`main → develop`) to keep `develop` up to date

### Module Branches

One per phase, named `module/phase-N` (e.g., `module/phase-0`):
- Created when work on a module begins
- All issue work within that phase merges locally into the module branch
- When the module is complete, open a **PR from the module branch to `develop`**
- The PR triggers CI — this is the safety net that catches issues the agent may have missed
- After PR merge, delete the module branch

### Parent Branches (for epics)

When an epic has multiple child issues:
- Create a parent branch using the epic's identifier (e.g., `feat/receipts-83-description`)
- Parent branch is created off `develop` (or the module branch if one exists)
- Child issue branches are created off the parent branch and squash-merge back into it
- When all children are complete, the parent branch gets a PR to `develop`
- This keeps related changes grouped and avoids polluting `develop` with intermediate work

### Issue Branches

One per tracked issue:
- Branch off the parent branch (if epic) or module branch, NOT `develop` or `main`
- Use the issue identifier to form a branch name (e.g., `feat/receipts-123-short-description`)
- Merge locally into the parent/module branch via squash merge (no PR needed)
- Delete the issue branch after merge

### Hotfix Branches

For emergency fixes that must bypass `develop`:
- Branch off `main` as `hotfix/<description>` (e.g., `hotfix/fix-auth-crash`)
- Open a PR directly to `main`
- After merge, the sync workflow creates a PR to bring the fix back into `develop`

### Diagram

```
main
  │
  ├── (release-please PR from develop)
  │
develop
  ├── module/phase-0                                  (PR → develop)
  │     ├── chore/receipts-90-remove-blazor          (squash-merge into module)
  │     └── fix/receipts-82-update-ci                (squash-merge into module)
  │
  └── feat/receipts-83-replace-viewmodels            (epic parent, PR → develop)
        ├── feat/receipts-88-generate-dtos           (squash-merge into parent)
        └── docs/receipts-87-update-docs             (squash-merge into parent)
```

## Merging Issue Work into Parent/Module Branch

From the main repo (or the parent/module clone), squash-merge the issue branch:

```bash
git checkout module/phase-0
git merge --squash feat/receipts-88-generate-dtos
git commit -m "feat(api): generate DTOs from OpenAPI spec (RECEIPTS-88)"
git branch -D feat/receipts-88-generate-dtos
```

If using a clone for the issue, delete it after merge:

```bash
rm -rf .clones/<branch-name>
```

## PR: Parent/Module to develop

When all issues are complete, push the branch and open a PR **to `develop`**:

```bash
git push -u origin feat/receipts-83-replace-viewmodels
gh pr create --base develop --title "Replace ViewModels with spec-generated DTOs" --body "..."
```

The PR triggers CI (build + test) — this is the checkpoint that surfaces issues.

After CI passes and the PR is approved, merge into `develop`:

```bash
git branch -d feat/receipts-83-replace-viewmodels
git push origin --delete feat/receipts-83-replace-viewmodels
git pull   # update develop with the merged PR
```

## Promoting develop to main

When ready for a release:
1. Release-please (or a manual PR) opens a PR from `develop` to `main`
2. CI runs; after merge, the `sync-develop` workflow auto-creates a PR to sync any release commits back to `develop`

## Direct Commits to Main

**Do not commit directly to main.** All changes flow through `develop` (or `hotfix/*` for emergencies).

Direct commits to `develop` are acceptable only for non-tracked work like:
- Trivial typo fixes
- Documentation updates
- Tooling/build configuration

**NEVER** commit tracked issue work directly to `develop` or `main`. When in doubt, create a branch.

## Directory Isolation

Two mechanisms are available for working on issue branches without affecting the main repo:

### Git Worktrees (preferred for AI agents)

`git worktree add` creates a linked working tree sharing the same `.git` directory:

```bash
git worktree add .claude/worktrees/<branch-name> -b <branch-name>
```

- Shares git history and refs with the main worktree (no duplication)
- Claude Code's `isolation: "worktree"` parameter automates this for subagents
- Worktrees live in `.claude/worktrees/` (gitignored)

#### Detecting a worktree

- `test -f .git` — if `.git` is a **file** (not a directory), you're in a worktree
- `git rev-parse --show-toplevel` — confirms your working directory root

#### What's shared vs not shared

| Shared (via common `.git` dir) | NOT shared (need fresh setup) |
|-------------------------------|-------------------------------|
| Git history, refs, branches | `node_modules/` (root and `src/client/`) |
| Hooks config (`core.hooksPath`) | `bin/` / `obj/` (.NET build output) |
| `src/client/src/generated/api.d.ts` (checked in) | `openapi/generated/` |
| | `src/Presentation/API/Generated/*.g.cs` |

#### Bootstrap commands

Run these in order (or use `dotnet run scripts/worktree-setup.cs` to run them all):

```bash
dotnet restore Receipts.slnx          # NuGet packages + configures git hooks
npm install                            # Root tooling (Spectral, js-yaml, cross-env)
cd src/client && npm install && cd -   # React client dependencies
dotnet run scripts/download-onnx-model.cs  # Download ONNX embedding model (~90MB)
dotnet build Receipts.slnx             # Compiles + generates DTOs and openapi/generated/API.json
cd src/client && npm run generate:types:write && cd -  # TypeScript types from OpenAPI spec
```

#### Branch naming in worktrees

Use the same convention as issue branches (e.g., `feat/receipts-123-short-description`). Worktrees are just an isolation mechanism — the branch name should reflect the work, not the worktree.

#### Permission settings

The file `.claude/settings.local.json` pre-approves read operations, MCP tools, git commands, and build tools so agents don't face excessive approval prompts. This file is gitignored (`.local.json` suffix) so it's per-user. Copy it from the main worktree if it's missing.

### Local Clones (alternative)

`git clone --local` creates a lightweight local clone:

```bash
git clone --local . .clones/<branch-name>
```

- Hardlinks objects (fast, no network), fully independent git repo
- `cd`, `git commit`, etc. all work normally
- Clones live in `.clones/` at the repo root (gitignored)

### When to Use Which

- **Worktrees** — preferred for parallel AI agent work (faster setup, shared git state)
- **Local clones** — preferred for human developers who want full independence
- **Neither** — for simple/small changes, it's fine to work directly on the module branch
