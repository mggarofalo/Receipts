# Plane

Guidance for AI agents working with the Plane workspace for this project.

## Workspace Structure

- **Workspace:** `dev` (hosted at `https://plane.wallingford.me`)
- **Project:** Receipts
- **MCP Server:** `plane-mcp-server` — all issue operations go through MCP tools (see [docs/mcp-setup.md](mcp-setup.md))

Workspace and project IDs are managed in Plane and discoverable via MCP tools (`list_projects`, `get_me`, etc.). No hardcoded IDs are needed.

## Labels

Every issue has at least one **layer** label and zero or more **type** labels. Labels tell agents what skills/tools are needed and what kind of work the issue represents.

### Layer Labels (where the work happens)

| Label | Meaning |
|-------|---------|
| `backend` | .NET layers: Domain, Application, Infrastructure, API |
| `frontend` | React/TypeScript SPA |
| `infra` | Docker, CI/CD, Aspire, deployment, build config |
| `docs` | Documentation only — no code changes |

### Type Labels (what kind of work)

| Label | Meaning |
|-------|---------|
| `Feature` | New user-facing functionality |
| `Improvement` | Enhancement to existing functionality |
| `Bug` | Defect fix |
| `cleanup` | Removal, housekeeping, dead code |
| `security` | Auth, hardening, rate limiting, audit |
| `codegen` | Code generation from OpenAPI spec |
| `dx` | Developer experience, tooling, local dev |
| `testing` | Test infrastructure, test suites |
| `epic` | **Parent issue — do NOT work directly, work its children** |

### Label Rules for New Issues

- Always assign at least one layer label (`backend`, `frontend`, `infra`, or `docs`)
- Add type labels as appropriate (can have multiple)
- Mark parent issues with `epic` — agents will skip these and work children
- Use `codegen` for any work involving OpenAPI spec generation
- Use `security` for anything touching auth, audit, or hardening

## Milestones / Cycles

All active issues are assigned to a milestone or cycle. Use `list_milestones` or `list_cycles` to discover current phases.

| Phase | Description | Status |
|-------|-------------|--------|
| **Phase 0: Housekeeping** | Standalone cleanup (remove Blazor, fix CI) | **COMPLETE** |
| **Phase 1: OpenAPI Spec-First** | Establish API contract as source of truth | **COMPLETE** |
| **Phase 2: Backend DTO Generation** | Replace ViewModels with spec-generated DTOs | **COMPLETE** |
| **Phase 3: Aspire Developer Experience** | Local dev orchestration with .NET Aspire | **COMPLETE** |
| **Phase 4: React Frontend** | Build React/Vite SPA with auth, CRUD, audit, UX polish | **COMPLETE** |
| **Phase 5: Test Coverage** | Code coverage collection, CI reporting, enforcement gate | **COMPLETE** |
| **Phase 6: Docker Deployment** | Containerize and deploy to Raspberry Pi | **COMPLETE** |
| **Phase 7: Correctness Hardening** | Business invariants, receipt coherence validation | **COMPLETE** |
| **Phase 8: Security Automation** | Snyk integration | **COMPLETE** |
| **Phase 9: Test Quality & Integration Testing** | Frontend integration tests, backend Testcontainers | Active |
| **Phase 10: Dashboard** | Graphs, charts, and spending statistics | **COMPLETE** |
| **Phase 11: Receipt Entry Workflow** | Multi-step receipt entry wizard | **COMPLETE** |

## Priority Semantics

Priority reflects **execution readiness**, not importance:

| Priority | Meaning | Agent Action |
|----------|---------|--------------|
| **Urgent** | Ready to start now, critical path | Pick these first |
| **High** | Ready or blocked by one step | Pick when blockers clear |
| **Medium** | Blocked by 2+ steps | Do not attempt yet |
| **Low** | Far future | Ignore until predecessors are done |

## How to Determine "What's Next"

1. **Query:** `list_work_items` for the Receipts project, filtering by state (backlog/todo)
2. **Filter:** Exclude Done, Canceled, and Duplicate statuses
3. **Skip epics:** If the issue has label `epic`, skip it and work its children instead
4. **Check blockers:** For each issue, use `retrieve_work_item` or `list_work_item_relations` to inspect blocked-by relations. If ANY blocker is not Done, the issue cannot start.
5. **Sort:** Among unblocked issues, sort by priority (Urgent > High > Medium > Low)
6. **Pick:** The first unblocked issue at the highest priority is "what's next"
7. **Label check:** Use layer labels to verify you have the right tools/context (e.g., `frontend` = Node.js/React, `backend` = .NET/C#, `infra` = Docker/CI)
8. **Parallel work:** Multiple unblocked issues at the same priority can be worked in parallel

## Working with Plane Issues

### Before starting work

1. Find the issue using the decision rules above
2. Read the full issue description with `retrieve_work_item`
3. Check blocked-by relations — do not start blocked work
4. Move the issue status to "In Progress" with `update_work_item`
5. Use the issue identifier to form a branch name (e.g., `mggarofalo/mgg-123-short-description`)

### After completing work

1. Move the issue status to "Done" with `update_work_item`
2. Check if completing this issue unblocks downstream work
3. Update any downstream issues if their blockers are now all resolved

### Creating new issues

- Always assign to the Receipts project
- Set milestone/cycle to the appropriate phase
- Set priority based on readiness (see Priority Semantics above)
- Add blocked-by relations if the issue depends on other work via `create_work_item_relation`
- Add at least one layer label
