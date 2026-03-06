---
identifier: MGG-104
title: Snyk-to-Linear integration rewrite
id: 94bb9103-0a5e-4caf-901d-781b4f252d9e
status: Done
priority:
  value: 2
  name: High
assignee: Michael Garofalo
createdBy: Michael Garofalo
project: Receipts
team: mggarofalo
labels:
  - epic
  - security
  - infra
milestone: "Phase 8: Security Automation"
url: "https://linear.app/mggarofalo/issue/MGG-104/snyk-to-linear-integration-rewrite"
gitBranchName: mggarofalo/mgg-104-snyk-to-linear-integration-rewrite
createdAt: "2026-02-15T17:52:14.326Z"
updatedAt: "2026-02-15T18:14:20.269Z"
completedAt: "2026-02-15T18:14:20.251Z"
---

# Snyk-to-Linear integration rewrite

Rewrite the Snyk-to-Linear integration to be idempotent and lifecycle-aware.\\n\\n- Replace hand-rolled `https` calls with `@linear/sdk`\\n- Fingerprint-based deduplication using `snyk/asset/finding/v1`\\n- Two-scan confirmation lifecycle for auto-closing resolved vulnerabilities\\n- Fix SARIF severity mapping (error/warning/note, not critical/high/medium)\\n- Clean up CI workflow to use official Snyk action
