---
identifier: MGG-125
title: "Agent test-writing loop: read coverage report and fill gaps iteratively"
id: e8be496f-e51a-49c4-a71b-add28f27cdae
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
  - backend
milestone: "Phase 5: Test Coverage"
url: "https://linear.app/mggarofalo/issue/MGG-125/agent-test-writing-loop-read-coverage-report-and-fill-gaps-iteratively"
gitBranchName: mggarofalo/mgg-125-agent-test-writing-loop-read-coverage-report-and-fill-gaps
createdAt: "2026-02-18T02:15:24.493Z"
updatedAt: "2026-02-27T13:57:17.888Z"
completedAt: "2026-02-27T13:57:17.854Z"
---

# Agent test-writing loop: read coverage report and fill gaps iteratively

Use an AI agent loop to systematically write unit tests by reading the Cobertura coverage report, identifying uncovered code paths, writing tests to cover them, and re-running coverage to verify improvement.\\n\\n## Loop Design\\n\\n1. **Read** `coverage.cobertura.xml` — parse uncovered lines/branches per class\\n2. **Prioritize** — rank by: (a) coverage % ascending, (b) layer (Domain > Application > Infrastructure > API)\\n3. **Read source** — open the target class and understand its logic\\n4. **Write tests** — add xUnit tests in the corresponding `*.Tests` project; follow existing test conventions (Moq for mocking, FluentAssertions for assertions)\\n5. **Run** `dotnet test --collect:\"XPlat Code Coverage\"` in the relevant test project\\n6. **Re-read** the updated report — confirm coverage improved\\n7. **Repeat** until threshold (from [MGG-123](./mgg-123-publish-code-coverage-report-to-github-via-ci.md)) is met or no further uncovered paths remain\\n\\n## Acceptance Criteria\\n\\n- All four test projects (`Domain.Tests`, `Application.Tests`, `Infrastructure.Tests`, `API.Tests`) have meaningful test coverage for their core logic\\n- The agent can be re-run at any time and will only add tests for paths still uncovered\\n- Tests follow the same naming and structure conventions as any existing tests in the repo\\n- Coverage report is readable from the file system (output to a known path, e.g., `TestResults/coverage.cobertura.xml`)\\n\\n## Notes\\n\\n- Requires [MGG-122](./mgg-122-configure-code-coverage-collection-with-coverlet-and-cobertura-report-output.md) (collection) so the coverage report exists\\n- Agent should read the report from the local file system, not from CI — this loop runs locally or as a Claude Code session task\\n- Prefer unit tests (no DB/network) using InMemory provider or mocked repositories\\n- Do not write tests that merely exercise pass-through code with no logic (mappers, trivial getters)
