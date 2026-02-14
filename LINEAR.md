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
| **Phase 0: Housekeeping** | Standalone cleanup (remove Blazor, fix CI) | Immediately |
| **Phase 1: OpenAPI Spec-First** | Establish API contract as source of truth | Immediately |
| **Phase 2: Backend DTO Generation** | Replace ViewModels with spec-generated DTOs | Phase 1 done |
| **Phase 3: Aspire Developer Experience** | Local dev orchestration with .NET Aspire | Mostly immediate (MGG-75 needs Phase 4 started) |
| **Phase 4: React Frontend** | Build React/Vite SPA | Phase 0 done (for setup), Phase 1 done (for codegen) |
| **Phase 5: Docker Deployment** | Containerize and deploy to Raspberry Pi | Phase 4 done |
| **Phase 6: Correctness Hardening** | Business invariants, receipt coherence validation, reconciliation | Phase 2 done (stable API model) |

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

Epics are parent issues that group related work. Key epics:

| Epic | Milestone | Children |
|------|-----------|----------|
| **MGG-89** OpenAPI Spec-First API Contract | Phase 1 | MGG-21, MGG-14 |
| **MGG-83** Replace ViewModels with DTOs | Phase 2 | MGG-88, MGG-87 |
| **MGG-71** .NET Aspire AppHost | Phase 3 | MGG-72–80 |
| **MGG-32** React/Vite Frontend | Phase 4 | MGG-33–50, MGG-66–69 |
| **MGG-51** Docker Deployment | Phase 5 | MGG-52–65, MGG-70 |

Standalone issues (no parent): **MGG-90** (Remove Blazor), **MGG-82** (CI/.NET 10), **MGG-94** (Receipt coherence validation — Phase 6)

## Key Cross-Epic Dependencies

These are the dependencies that span across epics and phases:

```
MGG-90 (Remove Blazor) ──blocks──> MGG-32 (React Frontend epic)
                                    └──> MGG-33 (React project setup)

MGG-21 (Establish OpenAPI spec) ──blocks──> MGG-14 (Verify spec output)
                                ──blocks──> MGG-83 (DTO replacement epic)
                                ──blocks──> MGG-36 (TypeScript codegen)
                                ──blocks──> MGG-88 (Generate .NET DTOs)

MGG-33 (React project setup) ──blocks──> MGG-36 (TypeScript codegen)
                              ──blocks──> MGG-75 (Vite in Aspire)

MGG-32 (React Frontend epic) ──blocks──> MGG-51 (Docker epic)
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
