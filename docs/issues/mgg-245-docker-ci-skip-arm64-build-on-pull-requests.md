---
identifier: MGG-245
title: "Docker CI: skip arm64 build on pull requests"
id: 41ce5ea0-5760-4f61-97dc-0bb1178474e8
status: Done
priority:
  value: 2
  name: High
assignee: Michael Garofalo
createdBy: Michael Garofalo
project: Receipts
team: mggarofalo
labels:
  - Improvement
url: "https://linear.app/mggarofalo/issue/MGG-245/docker-ci-skip-arm64-build-on-pull-requests"
gitBranchName: mggarofalo/mgg-245-docker-ci-skip-arm64-build-on-pull-requests
createdAt: "2026-03-05T15:01:51.816Z"
updatedAt: "2026-03-05T16:21:03.565Z"
completedAt: "2026-03-05T16:21:03.543Z"
attachments:
  - title: "ci: skip arm64 build on pull requests"
    url: "https://github.com/mggarofalo/Receipts/pull/90"
---

# Docker CI: skip arm64 build on pull requests

## Problem

PR builds run both `linux/amd64` and `linux/arm64` even though PRs never push images. The arm64 build under QEMU adds \~18 minutes of wall time purely for validation that the Dockerfile works cross-platform.

## Proposed Solution

Conditionally set the platforms list:

```yaml
platforms: ${{ github.event_name == 'pull_request' && 'linux/amd64' || 'linux/amd64,linux/arm64' }}
```

PRs validate with amd64 only. Merges to main and tag pushes build both architectures.

## Expected Impact

PR Docker checks drop from \~30 min to \~5 min. Cross-platform correctness is still validated on every merge to main.

## References

* Workflow: `.github/workflows/docker-publish.yml`
