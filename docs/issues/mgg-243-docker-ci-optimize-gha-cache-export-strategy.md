---
identifier: MGG-243
title: "Docker CI: optimize GHA cache export strategy"
id: 83e9cc3f-b772-4193-ac3d-a9f5a6de930e
status: Done
priority:
  value: 3
  name: Medium
assignee: Michael Garofalo
createdBy: Michael Garofalo
project: Receipts
team: mggarofalo
labels:
  - Improvement
url: "https://linear.app/mggarofalo/issue/MGG-243/docker-ci-optimize-gha-cache-export-strategy"
gitBranchName: mggarofalo/mgg-243-docker-ci-optimize-gha-cache-export-strategy
createdAt: "2026-03-05T15:01:41.090Z"
updatedAt: "2026-03-05T16:26:06.431Z"
completedAt: "2026-03-05T16:26:06.413Z"
attachments:
  - title: "perf(infra): switch GHA cache to mode=min"
    url: "https://github.com/mggarofalo/Receipts/pull/91"
---

# Docker CI: optimize GHA cache export strategy

## Problem

The Docker build uses `cache-to: type=gha,mode=max` which exports ALL intermediate layers to GitHub Actions cache. This step alone took **637 seconds (\~10.6 min)** in the latest run — nearly a third of total build time.

## Proposed Solutions (pick one)

1. **Use** `mode=min` instead of `mode=max` — only caches the final image layers. Saves cache export time at the cost of less granular cache hits on rebuilds. Good tradeoff if the Dockerfile layers don't change often.
2. **Switch to registry-based caching** (`type=registry`) — pushes cache to GHCR alongside the image. Faster writes and no GHA cache size limits (10GB cap).
3. **Scope cache keys per-architecture** — if using the matrix strategy from the parallel builds issue, each arch job caches independently, avoiding cross-arch cache pollution.

## Expected Impact

5-10 minutes saved on cache export.

## References

* [BuildKit cache backends](<https://docs.docker.com/build/cache/backends/>)
* Current config: `.github/workflows/docker-publish.yml` lines 61-62
