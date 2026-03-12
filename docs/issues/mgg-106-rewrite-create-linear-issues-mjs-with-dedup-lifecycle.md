---
identifier: MGG-106
title: "Rewrite `create-linear-issues.mjs` with dedup + lifecycle"
id: 8b6608bd-50c0-4ff1-a0e5-f90206f734ef
status: Done
priority:
  value: 1
  name: Urgent
assignee: Michael Garofalo
createdBy: Michael Garofalo
project: Receipts
team: mggarofalo
labels:
  - security
  - infra
milestone: "Phase 8: Security Automation"
url: "https://linear.app/mggarofalo/issue/MGG-106/rewrite-create-linear-issuesmjs-with-dedup-lifecycle"
gitBranchName: mggarofalo/mgg-106-rewrite-create-linear-issuesmjs-with-dedup-lifecycle
createdAt: "2026-02-15T17:52:25.879Z"
updatedAt: "2026-02-15T18:04:04.334Z"
completedAt: "2026-02-15T18:04:04.315Z"
attachments:
  - title: Rewrite Snyk-to-Linear integration (MGG-104)
    url: "https://github.com/mggarofalo/Receipts/pull/14"
---

# Rewrite `create-linear-issues.mjs` with dedup + lifecycle

Complete rewrite of `scripts/create-linear-issues.mjs`:\\n- Use `@linear/sdk` instead of hand-rolled `https` calls\\n- Parse SARIF format with correct severity mapping (error/warning/note)\\n- Fingerprint-based dedup using `<!-- snyk:fingerprint:<value> -->` in issue descriptions\\n- Two-scan resolution lifecycle (first miss → add `resolved-by-scan` label, second miss → auto-close)\\n- Reappearance detection (remove `resolved-by-scan` label + comment)\\n- Summary output: N new | N tracked | N resolving | N closed | N recurred\\n- Always exit 0
