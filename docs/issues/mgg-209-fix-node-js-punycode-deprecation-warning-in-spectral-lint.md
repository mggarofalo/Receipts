---
identifier: MGG-209
title: Fix Node.js punycode deprecation warning in Spectral lint
id: c3420650-401b-4640-8e1a-111371f8c366
status: Done
priority:
  value: 4
  name: Low
assignee: Michael Garofalo
createdBy: Michael Garofalo
project: Receipts
team: mggarofalo
labels:
  - dx
  - cleanup
url: "https://linear.app/mggarofalo/issue/MGG-209/fix-nodejs-punycode-deprecation-warning-in-spectral-lint"
gitBranchName: mggarofalo/mgg-209-fix-nodejs-punycode-deprecation-warning-in-spectral-lint
createdAt: "2026-03-02T00:54:42.970Z"
updatedAt: "2026-03-03T12:07:44.684Z"
completedAt: "2026-03-03T12:07:44.666Z"
attachments:
  - title: "fix(tooling): resolve punycode deprecation warning in Spectral lint (MGG-209)"
    url: "https://github.com/mggarofalo/Receipts/pull/56"
---

# Fix Node.js punycode deprecation warning in Spectral lint

## Problem

Running `npx spectral lint openapi/spec.yaml` produces a Node.js deprecation warning:

```
(node:49864) [DEP0040] DeprecationWarning: The `punycode` module is deprecated. Please use a userland alternative instead.
```

The lint itself passes (no errors), but the deprecation warning is noisy and may eventually become a breaking change in future Node.js versions.

## Acceptance Criteria

- [ ] Running `npx spectral lint openapi/spec.yaml` produces no deprecation warnings
- [ ] Spectral lint continues to function correctly (no regressions)

## Investigation Notes

* DEP0040 refers to the built-in `punycode` module deprecated since Node.js 21
* Likely caused by a transitive dependency in Spectral or its plugins
* Potential fixes: update Spectral to a version that addresses this, or pin a userland `punycode` package
