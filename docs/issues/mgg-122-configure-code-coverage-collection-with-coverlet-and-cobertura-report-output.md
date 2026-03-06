---
identifier: MGG-122
title: Configure code coverage collection with coverlet and Cobertura report output
id: b805d197-7bbb-43ca-8a4a-4f808b576268
status: Done
priority:
  value: 2
  name: High
createdBy: Michael Garofalo
project: Receipts
team: mggarofalo
labels:
  - testing
  - infra
  - backend
milestone: "Phase 5: Test Coverage"
url: "https://linear.app/mggarofalo/issue/MGG-122/configure-code-coverage-collection-with-coverlet-and-cobertura-report"
gitBranchName: mggarofalo/mgg-122-configure-code-coverage-collection-with-coverlet-and
createdAt: "2026-02-18T02:14:56.180Z"
updatedAt: "2026-02-27T13:45:12.648Z"
completedAt: "2026-02-27T13:45:12.633Z"
---

# Configure code coverage collection with coverlet and Cobertura report output

Set up code coverage collection across all test projects in the solution.\\n\\n## Acceptance Criteria\\n\\n- Add `coverlet.collector` to all `*.Tests` projects\\n- Add a `Directory.Build.props` (or project-level) entry to enable coverage collection: `<CollectCoverage>true</CollectCoverage>`\\n- Output format: **Cobertura** XML (`coverage.cobertura.xml`) — required by GitHub's native coverage report action and most CI tooling\\n- Run locally with: `dotnet test --collect:\"XPlat Code Coverage\"` and confirm `coverage.cobertura.xml` is produced under `TestResults/`\\n- Exclude generated code and migration files from coverage (e.g., `[*.Generated.*]*`, `[*.Migrations.*]*`)\\n- Document the local invocation command in the repo README or a `docs/testing.md` file\\n\\n## Notes\\n\\n- Use `coverlet.collector` (MSBuild integration), not the standalone `coverlet.console` CLI\\n- Cobertura is preferred over OpenCover because GitHub Actions natively renders it in PR summaries\\n- This issue is a prerequisite for CI publication (MGG-122) and enforcement (MGG-123)
