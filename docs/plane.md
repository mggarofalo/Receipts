# Plane

Guidance for AI agents working with the Plane workspace for this project.

## Workspace

- **Project:** Receipts (identifier: `RECEIPTS`)
- **CLI:** `plane` — use `plane --help` and `plane <command> --help` to discover commands. If you encounter bugs or missing features in the CLI, create an issue at https://github.com/mggarofalo/plane-cli
- **Output:** `-o json` (default) for programmatic parsing, `-o table` for human-readable

The CLI supports name resolution — use state names, label names, and member names instead of UUIDs.

## Labels

Every issue needs at least one **layer** label. Add **type** labels as appropriate.

| Layer | Type |
|-------|------|
| `backend`, `frontend`, `infra`, `docs` | `Feature`, `Improvement`, `Bug`, `cleanup`, `security`, `codegen`, `dx`, `testing`, `epic` |

Issues labeled `epic` are parent containers — skip and work their children.

## Modules

Phases are tracked as Plane **modules**. Use `plane module list --all -p RECEIPTS` to discover them. Active: Phase 15 (Logical Accounts). Planned: Phase 16 (UX Polish). Completed: Phases 0–12, 14, and Ad-hoc & Legacy (historical catch-all for ad-hoc work that sat outside any phase). Cancelled: Phase 13 (Paperless Integration).

## Priority

Priority reflects **execution readiness**, not importance: Urgent = ready now, High = one step away, Medium = blocked by 2+, Low = far future.

## What's Next

1. List issues in Backlog/Todo state, exclude Done/Cancelled/Duplicate
2. Skip `epic`-labeled issues — work their children
3. Skip issues with unresolved blockers
4. Pick the highest-priority unblocked issue

## Issue Workflow

**Start:** `plane issue update <id> -p RECEIPTS --state "In Progress"` — branch as `<type>/receipts-<id>-<desc>`

**Finish:** `plane issue update <id> -p RECEIPTS --state Done` — check if this unblocks downstream issues

**Create:** Always use `-p RECEIPTS`, assign a module, set priority by readiness, add at least one layer label
