---
name: qa-suite
description: >
  Full QA test suite orchestrator. Discovers Aspire (or connects to production),
  authenticates, spawns parallel browser-use subagents for 10 test groups with
  isolated sessions, runs API-based DB integrity checks, and produces a
  consolidated pass/fail report.
allowed-tools: Bash, Read, Grep, Glob, Agent, mcp__aspire__list_resources, mcp__aspire__list_console_logs
user_invocable: true
argument: "[--target local|prod] [--groups 1,2,3...] [--cleanup] [--headed]"
---

# QA Suite Orchestrator

Run a comprehensive, repeatable QA test suite against the Receipts application.

**Duration:** ~5-10 min (local), ~10-15 min (production)

## References

Before starting, read:
- `references/qa-suite-common.md` -- shared patterns, browser-use commands, credential matrix, result format
- `references/qa-suite-db-checks.md` -- API-based DB integrity checks

Group-specific references are read by subagents, not the orchestrator.

## Argument Parsing

| Argument | Default | Description |
|----------|---------|-------------|
| `--target local` | `local` | Target environment: `local` (Aspire) or `prod` (receipts.wallingford.me) |
| `--groups 1,2,3` | all (1-10) | Comma-separated group numbers to run |
| `--cleanup` | off | Delete QA-prefixed test data after run |
| `--headed` | off | Pass `--headed` to browser-use for visual debugging |

## Phase 0: Prerequisites

1. Verify `browser-use` is installed:
   ```bash
   browser-use --version
   ```
   If not found, tell the user: "`browser-use` CLI is required but not installed. Install it before running the QA suite." Then abort.

> **Note:** This suite uses `browser-use` CLI (session-based, index-addressed elements) rather than `agent-browser` (snapshot-based, `@eN`-addressed). The older `qa-crud`, `qa-wizard`, and `smoke-test` skills still use `agent-browser`. Both tools coexist — use whichever the skill specifies.

## Phase 1: Environment Bootstrap

### Local Target

1. Call `mcp__aspire__list_resources` to check if Aspire is running.
2. If not running, tell the user: "Aspire is not running. Start it with F5 in VS Code or `dotnet run --project src/Receipts.AppHost`." Then abort.
3. If running, call `mcp__aspire__list_console_logs` for the `frontend` resource. Parse logs for `Local:   http://localhost:XXXX` to get `BASE_URL`.
4. Call `mcp__aspire__list_console_logs` for the `api` resource (or `apiservice`). Parse for the API URL. If not found, derive from frontend (same origin, different port, or check resources).
5. Set credentials: `EMAIL=admin@receipts.local`, `PASSWORD=Admin123!@#`, `FALLBACK_PASSWORD=QaTest2024!@#`

### Production Target

1. Set `BASE_URL=https://receipts.wallingford.me`
2. Set `API_URL=https://receipts.wallingford.me` (API is same origin)
3. Set credentials: `EMAIL=claude@code.com`, `PASSWORD=Password123!@#`
4. Health check: `curl -s ${API_URL}/api/health` -- abort if not 200.

## Phase 2: Preflight Auth

Handle MustResetPassword and acquire JWT token. This runs synchronously BEFORE any parallel agents.

1. Open browser-use session `qa-preflight`:
   ```
   browser-use --session qa-preflight open ${BASE_URL}/login
   ```
2. Run `state`, find email/password/submit indices
3. Fill email and password, click Sign In
4. Check result:
   - If redirected to `/change-password`: complete the password change flow (current → new: `QaTest2024!@#`)
   - If login fails: try `FALLBACK_PASSWORD`
   - If dashboard loads: success
5. Record the known-good password as `ACTIVE_PASSWORD`
6. Close preflight session: `browser-use --session qa-preflight close`
7. Acquire JWT token via API:
   ```bash
   curl -s -X POST ${API_URL}/api/auth/login \
     -H "Content-Type: application/json" \
     -d '{"email":"${EMAIL}","password":"${ACTIVE_PASSWORD}"}'
   ```
   Parse `accessToken` from response. Store as `ACCESS_TOKEN`.

## Phase 3: Wave-Based Parallel Dispatch

### Wave Planning

Groups are organized into waves based on data dependencies:

**Wave 1** (parallel, all independent):
- Group 1: Auth & Session
- Group 2: Account CRUD (creates QA accounts)
- Group 3: Category & Subcategory CRUD (creates QA categories)
- Group 7: Dashboard & Analytics (read-only)
- Group 8: Admin Features (creates/deletes QA template)
- Group 9: Cross-Cutting (read-only)
- Group 10: Validation & Error Handling (read-only)

**Wave 2** (after Wave 1 -- needs accounts + categories):
- Group 4: Receipt Wizard E2E (creates QA receipts)

**Wave 3** (after Wave 2 -- needs receipts):
- Group 5: Receipt List/Detail/Edit
- Group 6: Transactions & Receipt Items CRUD

Only include waves that contain requested groups (from `--groups` argument).

**Dependency warning:** Before dispatching, check if any requested group has a dependency on a group NOT in the run set. If so, emit a warning: "Warning: Group N depends on Group M which is not included in this run. Ensure prerequisite data exists." Proceed anyway — the dependency may be satisfied by pre-existing data.

### Subagent Dispatch

For each group in the current wave, spawn a background Agent:

```
Agent(
  description: "QA Group N: [Name]",
  prompt: <see template below>,
  run_in_background: true
)
```

**Subagent prompt template:**

```
You are a QA tester for the Receipts web app. Use browser-use CLI for all browser interaction.

FIRST: Read these reference files (paths relative to skill directory .claude/skills/qa-suite/):
- references/qa-suite-common.md
- references/qa-suite-group-[name].md

VARIABLES:
- SESSION: qa-group-N
- BASE_URL: ${BASE_URL}
- EMAIL: ${EMAIL}
- PASSWORD: ${ACTIVE_PASSWORD}
- API_URL: ${API_URL}
- ACCESS_TOKEN: ${ACCESS_TOKEN}
[If --headed: - Add --headed flag to all browser-use commands]

CRITICAL RULES:
- Each browser-use command = SEPARATE Bash call (never chain with &&)
- Screenshots to $TEMP ONLY -- NEVER read screenshot files back into context
- Always re-run 'state' after clicks/navigation to get fresh indices
- Use 'state' for inspection, NOT screenshots or 'get html'

WORKFLOW:
1. Login per qa-suite-common.md
2. Execute all tests from the group reference file
3. Close session: browser-use --session qa-group-N close
4. Output the structured JSON result block per qa-suite-common.md
```

### Wave Execution

1. Dispatch all agents for the current wave
2. Wait for ALL agents in the wave to complete
3. Collect results from each agent's output
4. If any group returned BLOCKED, note it but continue to next wave
5. Advance to next wave

## Phase 4: DB Integrity Checks

After all waves complete, run the checks defined in `references/qa-suite-db-checks.md`.

Use `curl` with the `ACCESS_TOKEN` to query API endpoints. For each check:
1. Make the API call
2. Parse the JSON response
3. Verify the check criteria
4. Record PASS/FAIL

## Phase 5: Cleanup (if `--cleanup` flag)

Delete QA-prefixed test data via API DELETE calls in reverse dependency order:

1. Receipt items with description containing "QA"
2. Transactions from QA receipts
3. Receipts with location containing "QA"
4. Item templates with name containing "QA"
5. Subcategories with name containing "QA"
6. Categories with name containing "QA"
7. Accounts with code containing "QA"

Use `curl -X DELETE` with Bearer token for each item.

If `--cleanup` is not set, skip this phase. QA data remains for manual inspection.

## Phase 6: Report

Generate and display a consolidated markdown report:

```markdown
# QA Suite Report

**Date:** YYYY-MM-DD | **Target:** local/prod (URL) | **Groups:** N run

## Results

| # | Group | Status | Pass | Fail | Skip | Details |
|---|-------|--------|------|------|------|---------|
| 1 | Auth & Session | PASS | 5 | 0 | 0 | |
| 2 | Account CRUD | PASS | 5 | 0 | 0 | |
| ... | ... | ... | ... | ... | ... | ... |

## DB Integrity Checks

| Check | Status | Details |
|-------|--------|---------|
| Account Integrity | PASS | 12 accounts, all valid |
| Category Tree | PASS | No orphaned subcategories |
| ... | ... | ... |

## Failures (if any)

### Group N: [Name]
| Test | Status | Error |
|------|--------|-------|
| Test N.X | FAIL | [error description] |

Screenshots: $TEMP/qa-suite-groupN-*.png

## Summary
- **Groups:** X run, Y passed, Z failed
- **DB Checks:** X/Y passed
- **Overall: PASS/FAIL**
```
