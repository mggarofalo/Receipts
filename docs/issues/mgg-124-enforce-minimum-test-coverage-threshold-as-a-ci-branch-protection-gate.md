---
identifier: MGG-124
title: Enforce minimum test coverage threshold as a CI branch protection gate
id: fdae9bf0-96a4-4729-b662-537e7c91f451
status: Done
priority:
  value: 3
  name: Medium
createdBy: Michael Garofalo
project: Receipts
team: mggarofalo
labels:
  - testing
  - infra
  - frontend
milestone: "Phase 5: Test Coverage"
url: "https://linear.app/mggarofalo/issue/MGG-124/enforce-minimum-test-coverage-threshold-as-a-ci-branch-protection-gate"
gitBranchName: mggarofalo/mgg-124-enforce-minimum-test-coverage-threshold-as-a-ci-branch
createdAt: "2026-02-18T02:15:11.909Z"
updatedAt: "2026-02-27T14:04:03.521Z"
completedAt: "2026-02-27T14:04:03.500Z"
---

# Enforce minimum test coverage threshold as a CI branch protection gate

Fail the CI check (and block PR merge) if overall line coverage drops below defined thresholds for either the .NET backend or the React frontend.

## Acceptance Criteria

**Backend (.NET)**

* Minimum line coverage threshold (start at 60%, raise as tests are added via MGG-125)
* Parses `coverage.cobertura.xml` and fails CI with a clear message if below threshold

**Frontend (React/Vitest)**

* Separate minimum line coverage threshold (start at 60%, raise as tests are added via MGG-127)
* Parses `coverage/cobertura-coverage.xml` and fails CI with a clear message if below threshold

**Shared**

* Both checks are listed as required status checks in GitHub branch protection for `master`
* Each threshold is configurable via a single value in the workflow file (not hardcoded in multiple places)
* When both pass, PRs can merge normally

## Notes

* Can reuse the same GitHub Action as the reporting step (MGG-123) if it supports fail-on-threshold; otherwise add a lightweight parse step
* Initial thresholds should be set at or slightly below **current** coverage so existing PRs are not immediately blocked; raise incrementally
* Requires MGG-122 (.NET collection), MGG-126 (Vitest setup), and MGG-123 (reporting) to be in place
