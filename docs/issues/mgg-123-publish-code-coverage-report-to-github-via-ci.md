---
identifier: MGG-123
title: Publish code coverage report to GitHub via CI
id: 899b77cc-3343-4b0f-ac26-51c154baf1e8
status: Done
priority:
  value: 3
  name: Medium
createdBy: Michael Garofalo
project: Receipts
team: mggarofalo
labels:
  - testing
  - dx
  - infra
  - frontend
milestone: "Phase 5: Test Coverage"
url: "https://linear.app/mggarofalo/issue/MGG-123/publish-code-coverage-report-to-github-via-ci"
gitBranchName: mggarofalo/mgg-123-publish-code-coverage-report-to-github-via-ci
createdAt: "2026-02-18T02:15:03.717Z"
updatedAt: "2026-02-27T13:48:19.875Z"
completedAt: "2026-02-27T13:48:19.867Z"
---

# Publish code coverage report to GitHub via CI

Add CI steps that publish Cobertura coverage reports from **both the .NET backend and the React frontend** as GitHub PR check summaries and artifacts after every test run.

## Acceptance Criteria

**Backend (.NET)**

* After `dotnet test --collect:"XPlat Code Coverage"`, upload `coverage.cobertura.xml` as a workflow artifact (retained 30 days)

**Frontend (React/Vitest)**

* After `npm run coverage`, upload `coverage/cobertura-coverage.xml` as a workflow artifact (retained 30 days)

**Shared**

* A PR comment or check summary is posted with per-package/per-file and overall line/branch coverage for each stack separately
* Recommended approach: `irongut/CodeCoverageSummary` (or equivalent) run once per Cobertura file, posting two summary blocks
* Report step runs on every push to a PR branch and on every merge to `master`
* Does not block the build — report is informational at this stage (enforcement is [MGG-124](./mgg-124-enforce-minimum-test-coverage-threshold-as-a-ci-branch-protection-gate.md))

## Notes

* Requires [MGG-122](./mgg-122-configure-code-coverage-collection-with-coverlet-and-cobertura-report-output.md) (.NET collection) and [MGG-126](./mgg-126-configure-vitest-with-coverage-v8-cobertura-output-in-the-react-app.md) (Vitest setup) to be in place
* Keep the report step in its own job or clearly separated steps so it can be toggled independently of enforcement
* Store workflow changes in `.github/workflows/`
