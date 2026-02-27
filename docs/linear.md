# LINEAR.md

Guidance for AI agents working with the Linear workspace for this project.

## Workspace Structure

- **Team:** Mggarofalo (ID: `a4aff05d-41e6-45dc-b670-cdb485fef765`)
- **Project:** Receipts (ID: `06199e4e-d3a8-4f98-9edf-e1e02efb6cee`)
- **Execution Roadmap:** A Linear document attached to the Receipts project with full phase-by-phase ordering, label reference, and decision rules. Query it with `get_document` using slug `0c391490ba60`.

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

## Milestones (Execution Phases)

All active issues are assigned to a milestone. Milestones are ordered and represent sequential phases:

| Milestone | Description | Can Start When |
|-----------|-------------|----------------|
| **Phase 0: Housekeeping** | Standalone cleanup (remove Blazor, fix CI) | **COMPLETE** |
| **Phase 1: OpenAPI Spec-First** | Establish API contract as source of truth | **COMPLETE** |
| **Phase 2: Backend DTO Generation** | Replace ViewModels with spec-generated DTOs | **COMPLETE** |
| **Phase 3: Aspire Developer Experience** | Local dev orchestration with .NET Aspire | Immediately |
| **Phase 4: React Frontend** | Build React/Vite SPA | Phase 0 done (setup), Phase 1 done (codegen) |
| **Phase 5: Docker Deployment** | Containerize and deploy to Raspberry Pi | Phase 4 MVP done |
| **Phase 6: Correctness Hardening** | Business invariants, receipt coherence validation | Phase 2 done (stable API model) |
| **Phase 7: Security Automation** | Snyk-to-Linear integration | Phase 2 done |
| **Phase 8: Test Coverage** | Code coverage collection, CI reporting, enforcement gate, agent test-writing loop | Phase 3 MVP done |

## Priority Semantics

Priority reflects **execution readiness**, not importance:

| Priority | Meaning | Agent Action |
|----------|---------|--------------|
| **Urgent (1)** | Ready to start now, critical path | Pick these first |
| **High (2)** | Ready or blocked by one step | Pick when blockers clear |
| **Medium (3)** | Blocked by 2+ steps | Do not attempt yet |
| **Low (4)** | Far future | Ignore until predecessors are done |

## How to Determine "What's Next"

1. **Query:** `list_issues` with `team: "Mggarofalo"`, `state: "backlog"` (or "todo")
2. **Filter:** Exclude Done, Canceled, and Duplicate statuses
3. **Skip epics:** If the issue has label `epic`, skip it and work its children instead
4. **Check blockers:** For each issue, use `get_issue` with `includeRelations: true` and inspect `blockedBy`. If ANY blocker is not Done, the issue cannot start.
5. **Sort:** Among unblocked issues, sort by priority (Urgent > High > Medium > Low)
6. **Pick:** The first unblocked issue at the highest priority is "what's next"
7. **Label check:** Use layer labels to verify you have the right tools/context (e.g., `frontend` = Node.js/React, `backend` = .NET/C#, `infra` = Docker/CI)
8. **Parallel work:** Multiple unblocked issues at the same priority can be worked in parallel

## Epics (Parent Issues)

Epics are parent issues that group related work.

### Completed Phases

| Epic | Milestone | Status |
|------|-----------|--------|
| **MGG-89** OpenAPI Spec-First API Contract | Phase 1 | Done |
| **MGG-83** Replace ViewModels with DTOs | Phase 2 | Done |

### Phase 3: Aspire Developer Experience

| Epic | Children | MVP? |
|------|----------|------|
| **MGG-109** Core Aspire Orchestration | MGG-72, 73, 74, 76, 77, 80 | Yes |
| **MGG-110** AI-Powered Dev Tooling | MGG-78, 79 | No — follow-up |

### Phase 4: React Frontend

| Epic | Children | MVP? |
|------|----------|------|
| **MGG-111** Frontend Bootstrap & Codegen | MGG-33, 36, 75, 48 | Yes |
| **MGG-112** Authentication System | MGG-34, 35 | Yes |
| **MGG-113** Core CRUD Modules | MGG-38, 39, 40, 41, 42, 43 | Yes |
| **MGG-114** Data Safety & Audit Trail | MGG-66, 67, 68, 69 | No — follow-up |
| **MGG-115** UX Polish & Enhancements | MGG-37, 44, 45, 46, 47 | No — follow-up |
| **MGG-116** Frontend Quality & Documentation | MGG-49, 50 | No — follow-up |

### Phase 5: Docker Deployment

| Epic | Children | MVP? |
|------|----------|------|
| **MGG-117** Container Architecture & CI | MGG-52, 53, 54, 55, 57 | Yes |
| **MGG-118** Production Operations | MGG-58, 59, 61, 62, 63, 64 | Mixed (58, 59 MVP) |
| **MGG-119** Deployment Security & Documentation | MGG-56, 60, 65, 70 | No — follow-up |

### Phase 8: Test Coverage

| Epic | Children | MVP? |
|------|----------|------|
| **MGG-121** Test Coverage Pipeline | MGG-122, 123, 124, 125, 126, 127 | All MVP |

Dependency chain:
```
MGG-122 (coverlet/.NET) ──┐
                           ├──> MGG-123 (CI report, both stacks) ──> MGG-124 (enforcement gate)
MGG-126 (Vitest/React) ───┘
MGG-33  (React app) ──────> MGG-126

MGG-122 ──> MGG-125 (agent loop: .NET)
MGG-126 ──> MGG-127 (agent loop: React)
```

### Retired Epics

These monolithic epics were replaced by the focused epics above:

| Old Epic | Replaced By | Status |
|----------|-------------|--------|
| **MGG-71** .NET Aspire AppHost | MGG-109, MGG-110 | Canceled |
| **MGG-32** React/Vite Frontend | MGG-111–116 | Canceled |
| **MGG-51** Docker Deployment | MGG-117–119 | Canceled |

## Key Cross-Epic Dependencies

```
Phase 4 entry points (no blockers):
  MGG-33 (React setup) — MGG-90 is Done
  MGG-34 (Backend auth) — no blockers

Phase 4 critical path:
  MGG-33 ──> MGG-36 (TS codegen) ──> MGG-38–43 (CRUD modules)
  MGG-33 ──> MGG-75 (Vite in Aspire)
  MGG-34 ──> MGG-35 (Frontend auth UI)
  MGG-34 ──> MGG-66, 67, 68 (Data safety)

Phase 5 gate:
  MGG-111 + MGG-112 + MGG-113 (Phase 4 MVP) ──blocks──> MGG-117 (Containers)
  MGG-117 ──blocks──> MGG-118 (Operations)
  MGG-117 ──blocks──> MGG-119 (Security & Docs)
```

## Working with Linear Issues

### Before starting work
1. Find the issue using the decision rules above
2. Read the full issue description with `get_issue`
3. Check `blockedBy` relations — do not start blocked work
4. Move the issue status to "In Progress" with `update_issue`
5. Use the `gitBranchName` from the issue for your feature branch

### After completing work
1. Move the issue status to "Done" with `update_issue`
2. Check if completing this issue unblocks downstream work
3. Update any downstream issues if their blockers are now all resolved

### Creating new issues
- Always assign to team "Mggarofalo" and project "Receipts"
- Set milestone to the appropriate phase
- Set priority based on readiness (see Priority Semantics above)
- Add `blockedBy` relations if the issue depends on other work
- Add `blocks` relations if other issues depend on this one

## Linear View (UI)

A "Receipts Roadmap" view should exist in the Mggarofalo team configured as:
- **Filter:** Project = Receipts, exclude Done/Canceled/Duplicate
- **Grouping:** Milestone
- **Ordering:** Priority
- **Sub-issues:** Shown
- **Display properties:** Status, Priority, Labels, Milestone
