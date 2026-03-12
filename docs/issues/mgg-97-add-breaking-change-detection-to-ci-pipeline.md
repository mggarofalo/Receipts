---
identifier: MGG-97
title: Add breaking change detection to CI pipeline
id: 1ca1f000-87a2-4587-83b3-85ff91b9547d
status: Done
priority:
  value: 3
  name: Medium
createdBy: Michael Garofalo
project: Receipts
team: mggarofalo
labels:
  - dx
  - infra
milestone: "Phase 2: Backend DTO Generation"
url: "https://linear.app/mggarofalo/issue/MGG-97/add-breaking-change-detection-to-ci-pipeline"
gitBranchName: mggarofalo/mgg-97-add-breaking-change-detection-to-ci-pipeline
createdAt: "2026-02-14T15:59:25.344Z"
updatedAt: "2026-02-15T11:54:50.853Z"
completedAt: "2026-02-15T11:54:50.827Z"
---

# Add breaking change detection to CI pipeline

## Context

Currently nothing detects whether a spec change is backwards-compatible. Removing a field, changing a type, or renaming a property are all breaking changes for existing clients. These should be caught in CI and fail the build (or require explicit acknowledgment).

## Goal

Add a CI step that compares `openapi/spec.yaml` on the PR branch vs `openapi/spec.yaml` on the base branch (`master`). Flag breaking changes as CI failures.

## Implementation

Use the same semantic diff tool selected in the drift detection upgrade issue (likely `oasdiff`). Most tools have a `breaking` subcommand:

```bash
oasdiff breaking origin/master:openapi/spec.yaml openapi/spec.yaml
```

This detects:

* Removed endpoints
* Removed required request fields
* Added required request fields (breaks existing clients)
* Changed response field types
* Removed response fields
* Changed path parameters

### CI Integration

Add a GitHub Actions step in the PR workflow:

```yaml
- name: Check for breaking API changes
  run: oasdiff breaking origin/${{ github.base_ref }}:openapi/spec.yaml openapi/spec.yaml --fail-on ERR
```

### Escape Hatch

For intentional breaking changes (during API version bumps), the tool should be configurable to:

1. Accept a `breaking-changes-allowed` label on the PR
2. Or use an `oasdiff` ignore file for acknowledged breaks

## Acceptance Criteria

- [ ] CI fails when a PR introduces breaking spec changes
- [ ] Non-breaking changes (adding optional fields, new endpoints) pass
- [ ] There's a documented escape hatch for intentional breaking changes
- [ ] Works with the API versioning strategy (when implemented)

## Dependencies

* Should use the same tool as the drift detection upgrade
* Related to API versioning issue (versioning provides the path for intentional breaking changes)
