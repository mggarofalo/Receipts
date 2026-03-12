---
identifier: MGG-96
title: Upgrade drift detection to semantic property-level OpenAPI comparison
id: 62f46233-b666-461e-8557-b78f8881031f
status: Done
priority:
  value: 3
  name: Medium
createdBy: Michael Garofalo
project: Receipts
team: mggarofalo
labels:
  - dx
  - backend
milestone: "Phase 2: Backend DTO Generation"
url: "https://linear.app/mggarofalo/issue/MGG-96/upgrade-drift-detection-to-semantic-property-level-openapi-comparison"
gitBranchName: mggarofalo/mgg-96-upgrade-drift-detection-to-semantic-property-level-openapi
createdAt: "2026-02-14T15:59:13.975Z"
updatedAt: "2026-02-15T11:48:54.627Z"
completedAt: "2026-02-15T11:48:54.620Z"
---

# Upgrade drift detection to semantic property-level OpenAPI comparison

## Context

The current drift detection (`scripts/check-drift.mjs`) only compares **operation paths** and **schema names** between the hand-authored spec and build-time generated API.json. It does NOT compare schema properties, types, or required fields. This means property-level drift passes silently.

## Goal

Replace the name-level drift check with a semantic OpenAPI diff tool that compares the full schema structure — properties, types, nullability, required fields, and constraints.

## Tool Options

| Tool | Pros | Cons |
| -- | -- | -- |
| `@useoptic/optic` | Semantic diff, breaking change detection, CI integration | Heavier dependency |
| `openapi-diff` | Lightweight, focused on diff | Less maintained |
| `oasdiff` (Go binary) | Fast, breaking change detection built-in, CLI-friendly | Requires Go binary or Docker |

**Recommendation:** `oasdiff` — it does both drift detection AND breaking change detection (covers [MGG-97](./mgg-97-add-breaking-change-detection-to-ci-pipeline.md) too), is fast, and has a single binary with no runtime dependencies. Available via npm (`oasdiff`), Docker, or direct download.

## Implementation

1. Remove `scripts/check-drift.mjs` and its `js-yaml` dependency
2. Add the chosen tool to the project (npm or binary)
3. Update `.husky/task-runner.json` drift-check step to use the new tool
4. The tool should compare `openapi/spec.yaml` (canonical) vs `openapi/generated/API.json` (build export)
5. Fail on ANY structural difference (not just names)

## Acceptance Criteria

- [ ] Property-level differences between spec and generated API are detected
- [ ] Type mismatches are detected (e.g., string vs int)
- [ ] Missing/extra properties are detected
- [ ] Required field differences are detected
- [ ] Pre-commit hook uses the new tool
- [ ] `scripts/check-drift.mjs` and `js-yaml` dependency removed

## Dependencies

* **Blocked by**: [MGG-88](./mgg-88-generate-net-request-response-dtos-from-openapi-spec.md) (spec must be refactored to Request/Response first)
