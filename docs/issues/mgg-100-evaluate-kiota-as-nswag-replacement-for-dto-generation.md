---
identifier: MGG-100
title: Evaluate Kiota as NSwag replacement for DTO generation
id: 1da4553d-c6f2-4210-ae91-edaf574b6d0a
status: Done
priority:
  value: 4
  name: Low
createdBy: Michael Garofalo
project: Receipts
team: mggarofalo
labels:
  - codegen
  - backend
milestone: "Phase 2: Backend DTO Generation"
url: "https://linear.app/mggarofalo/issue/MGG-100/evaluate-kiota-as-nswag-replacement-for-dto-generation"
gitBranchName: mggarofalo/mgg-100-evaluate-kiota-as-nswag-replacement-for-dto-generation
createdAt: "2026-02-14T16:00:08.999Z"
updatedAt: "2026-02-14T21:17:25.212Z"
completedAt: "2026-02-14T21:17:25.185Z"
---

# Evaluate Kiota as NSwag replacement for DTO generation

## Context

NSwag is currently the planned tool for generating C# DTOs from the OpenAPI spec ([MGG-88](https://linear.app/mggarofalo/issue/MGG-88/generate-net-requestresponse-dtos-from-openapi-spec)). However, NSwag has a single-maintainer risk and already lags behind .NET versions (using `Net90` target for .NET 10).

**Key insight:** DTOs are transport models — identical on client and server side. A tool that generates "client" model classes produces the same POCOs that the server needs to receive and map. There's no fundamental difference between a "client DTO" and a "server DTO" for the same schema.

## Evaluation Result: NSwag

**Kiota is disqualified.** Three dealbreakers:

1. **No model-only generation** — Kiota cannot generate just model classes without the HTTP client and request builder infrastructure. GitHub issue #3912 asked for this; it was closed as "not planned."
2. **Models are not POCOs** — Kiota models implement `IParsable`, `IAdditionalDataHolder`, and require `Microsoft.Kiota.Abstractions` as a dependency. This violates Clean Architecture and is incompatible with the Mapperly workflow.
3. **No MSBuild NuGet package** — No equivalent to `NSwag.MSBuild`; requires manual CLI wiring.

**NSwag's previous weakness is resolved** — v14.6.3 ships with .NET 10 SDK support, eliminating the version lag concern.

### Final Evaluation Matrix

| Criterion | NSwag | Kiota |
| -- | -- | -- |
| Model-only generation | Yes | No (dealbreaker) |
| POCO/record output | Yes (configurable) | No (IParsable infra) |
| MSBuild integration | Yes (NSwag.MSBuild) | No official package |
| .NET 10 support | Yes (v14.6.3) | Lagging (needs .NET 9 runtime) |
| Mapperly compatibility | Yes (plain POCOs) | Poor |
| OpenAPI 3.1 | Partial (works for 3.0-compat specs) | Full |

**Decision: Proceed with NSwag for MGG-88.**

The single-maintainer risk is manageable — NSwag has been maintained since 2015, and the generated POCO output is simple enough that migrating away would be straightforward if needed.

**Note on OpenAPI 3.1:** The spec uses `openapi: 3.1.0` but only 3.0-compatible constructs. If NSwag has issues with the version string, downgrade to `3.0.3` — the content is fully compatible.
