---
identifier: MGG-107
title: "Clean up security job in `github-ci.yml`"
id: 3743089a-1140-4b0f-b151-1cc65c0baec9
status: Done
priority:
  value: 1
  name: Urgent
assignee: Michael Garofalo
createdBy: Michael Garofalo
project: Receipts
team: mggarofalo
labels:
  - infra
milestone: "Phase 8: Security Automation"
url: "https://linear.app/mggarofalo/issue/MGG-107/clean-up-security-job-in-github-ciyml"
gitBranchName: mggarofalo/mgg-107-clean-up-security-job-in-github-ciyml
createdAt: "2026-02-15T17:52:30.537Z"
updatedAt: "2026-02-15T18:04:04.335Z"
completedAt: "2026-02-15T18:04:04.316Z"
---

# Clean up security job in `github-ci.yml`

Clean up the security job in `.github/workflows/github-ci.yml`:\\n- Replace `npm install -g snyk` with official `snyk/actions/node@master` action\\n- Simplify Linear sync step (remove LINEAR_TEAM_ID and LINEAR_PROJECT_ID env vars — now hardcoded)\\n- Add Node.js setup step with npm cache\\n- Add `npm ci` step for `@linear/sdk` dependency\\n- Keep NuGet vulnerability check and SARIF upload unchanged
