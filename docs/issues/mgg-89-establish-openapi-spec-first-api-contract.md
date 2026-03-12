---
identifier: MGG-89
title: Establish OpenAPI Spec-First API Contract
id: ed86cc2e-2e55-47c0-bc91-762acc530299
status: Done
priority:
  value: 1
  name: Urgent
createdBy: Michael Garofalo
project: Receipts
team: mggarofalo
labels:
  - epic
  - backend
milestone: "Phase 1: OpenAPI Spec-First"
url: "https://linear.app/mggarofalo/issue/MGG-89/establish-openapi-spec-first-api-contract"
gitBranchName: mggarofalo/mgg-89-establish-openapi-spec-first-api-contract
createdAt: "2026-02-12T11:38:03.372Z"
updatedAt: "2026-02-14T15:31:07.391Z"
completedAt: "2026-02-14T15:31:07.381Z"
---

# Establish OpenAPI Spec-First API Contract

## Overview

Migrate the API from code-first Swashbuckle to a spec-first OpenAPI 3.1 workflow using `Microsoft.AspNetCore.OpenApi`. The canonical spec becomes the single source of truth from which both .NET DTOs and TypeScript types are generated.

## Phases

1. **Establish OpenAPI infrastructure** (MGG-21) — Replace Swashbuckle with `Microsoft.AspNetCore.OpenApi`, add Scalar docs UI, configure build-time spec export, author canonical `spec.yaml`, set up Spectral linting and drift detection
2. **Verify spec output** (MGG-14) — Validate all endpoints appear correctly, run gap analysis, confirm Scalar UI works, verify build-time export

## Why Spec-First

* Spec is the contract, not an artifact of implementation
* Both .NET and TypeScript types are generated — neither can drift
* API reviews happen at the spec level
* Enables future clients (mobile, CLI) with zero additional work

## Blocks

This epic blocks:

* MGG-83 (Replace ViewModels with spec-generated DTOs)
* MGG-36 (Generate TypeScript types from spec)
* MGG-88 (Generate .NET DTOs from spec)
