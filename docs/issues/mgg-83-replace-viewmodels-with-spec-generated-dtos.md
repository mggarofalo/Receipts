---
identifier: MGG-83
title: Replace ViewModels with spec-generated DTOs
id: c4944599-6e27-48f3-aea6-880746dcf8db
status: Done
priority:
  value: 3
  name: Medium
createdBy: Michael Garofalo
project: Receipts
team: mggarofalo
labels:
  - epic
  - backend
milestone: "Phase 2: Backend DTO Generation"
url: "https://linear.app/mggarofalo/issue/MGG-83/replace-viewmodels-with-spec-generated-dtos"
gitBranchName: mggarofalo/mgg-83-replace-viewmodels-with-spec-generated-dtos
createdAt: "2026-02-11T11:22:30.031Z"
updatedAt: "2026-02-15T11:41:03.003Z"
completedAt: "2026-02-15T11:41:02.983Z"
---

# Replace ViewModels with spec-generated DTOs

## Overview

Replace all ViewModel (VM) classes with Request/Response DTOs that are **generated from the OpenAPI spec** (MGG-21). This is not a simple rename — the VMs are being replaced by generated types that are guaranteed to match the TypeScript frontend types.

## Context

With the spec-first architecture (MGG-21), the flow is:

```
OpenAPI spec (authoritative)
    ├── generates .NET Request/Response DTOs (this epic)
    └── generates TypeScript types (MGG-36)
```

The old `*VM` classes in `Shared/ViewModels/` are hand-written DTOs. They'll be replaced by DTOs generated from the spec, then the old classes and the Shared project's ViewModel layer can be removed.

## Scope

This epic covers:

1. **MGG-88**: Generate .NET Request/Response DTOs from the OpenAPI spec, update controllers/mappers/tests, remove old VMs
2. **MGG-87**: Update documentation to reflect the new architecture

## Dependencies

* **Blocked by**: MGG-21 (OpenAPI spec must be established first)
* The blocking relationship with MGG-32 (React rewrite) is removed — the spec work can proceed independently since it's a backend concern

## Sub-Issues

* **MGG-88** — Generate .NET DTOs from spec, replace VMs in API layer
* **MGG-87** — Update documentation
