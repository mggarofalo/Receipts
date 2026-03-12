---
identifier: MGG-242
title: "Docker CI: build arm64 natively instead of QEMU emulation"
id: e1f297fc-6632-43a9-8a76-b668c48d3bd0
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
url: "https://linear.app/mggarofalo/issue/MGG-242/docker-ci-build-arm64-natively-instead-of-qemu-emulation"
gitBranchName: mggarofalo/mgg-242-docker-ci-build-arm64-natively-instead-of-qemu-emulation
createdAt: "2026-03-05T15:01:33.398Z"
updatedAt: "2026-03-05T17:15:02.873Z"
completedAt: "2026-03-05T17:15:02.770Z"
attachments:
  - title: "perf(infra): native arm64 Docker CI builds (MGG-242)"
    url: "https://github.com/mggarofalo/Receipts/pull/92"
---

# Docker CI: build arm64 natively instead of QEMU emulation

## Problem

The Docker Publish workflow builds both `linux/amd64` and `linux/arm64` in a single `docker/build-push-action` invocation using QEMU emulation for arm64. This makes arm64 steps 4-8x slower than native:

* `dotnet restore`: 456s (arm64) vs 54s (amd64)
* `dotnet publish`: 646s (arm64) vs 116s (amd64)
* `npm run build`: 404s (arm64) vs 56s (amd64)
* `npm ci`: 209s (arm64) vs 25s (amd64)

## Proposed Solution

Split into a **matrix strategy** with parallel jobs:

* `amd64` on `ubuntu-latest` (native)
* `arm64` on `ubuntu-latest-arm64` (native, GitHub now offers ARM runners)

Then use `docker/build-push-action` with `docker buildx imagetools create` to merge into a multi-arch manifest.

This eliminates QEMU entirely and lets both architectures build in parallel (\~2 min for restore+publish instead of \~18 min).

## Expected Impact

\~18 minutes saved (arm64 emulation overhead). Total build time should drop from \~30 min to \~5-8 min.

## References

* [GitHub ARM runners](<https://github.blog/changelog/2025-01-16-linux-arm64-hosted-runners-now-available-for-free-in-public-repositories-public-preview/>)
* Workflow: `.github/workflows/docker-publish.yml`
* Log: [https://github.com/mggarofalo/Receipts/actions/runs/22721536033/job/65884747830](<https://github.com/mggarofalo/Receipts/actions/runs/22721536033/job/65884747830>)
