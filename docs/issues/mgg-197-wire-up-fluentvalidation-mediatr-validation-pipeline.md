---
identifier: MGG-197
title: Wire up FluentValidation + MediatR validation pipeline
id: db10599e-f45a-4370-ba75-b543b8933578
status: Done
priority:
  value: 1
  name: Urgent
assignee: Michael Garofalo
createdBy: Michael Garofalo
project: Receipts
team: mggarofalo
labels:
  - backend
  - Improvement
milestone: "Phase 7: Correctness Hardening"
url: "https://linear.app/mggarofalo/issue/MGG-197/wire-up-fluentvalidation-mediatr-validation-pipeline"
gitBranchName: mggarofalo/mgg-197-wire-up-fluentvalidation-mediatr-validation-pipeline
createdAt: "2026-02-27T14:54:49.930Z"
updatedAt: "2026-03-02T01:24:11.016Z"
completedAt: "2026-03-02T01:24:11.004Z"
attachments:
  - title: "feat(api): wire up FluentValidation + MediatR validation pipeline (MGG-197)"
    url: "https://github.com/mggarofalo/Receipts/pull/45"
---

# Wire up FluentValidation + MediatR validation pipeline

## Scope\\n\\nConnect the 5 existing FluentValidation validators (currently orphaned — never registered in DI, never invoked) to the MediatR pipeline so that invalid requests return structured 400 ProblemDetails instead of falling through to domain constructors that throw `ArgumentException` (500s).\\n\\n## Tasks\\n\\n- \[ \] Add `FluentValidation.DependencyInjectionExtensions` to `API.csproj`\\n- \[ \] Register validators via `AddValidatorsFromAssemblyContaining<>()` in `src/Presentation/API/Configuration/ApplicationConfiguration.cs`\\n- \[ \] Create `ValidationBehavior<TRequest, TResponse> : IPipelineBehavior` in `src/Application/Behaviors/ValidationBehavior.cs`\\n- \[ \] Register behavior in `src/Application/Services/ApplicationService.cs` via `cfg.AddOpenBehavior()`\\n- \[ \] Add exception handler that catches `ValidationException` → 400 ProblemDetails response\\n- \[ \] Verify existing 5 validators now fire (CreateReceipt, CreateTransaction, CreateUser, UpdateUser, AdminResetPassword)\\n\\n## Critical Files\\n\\n- `src/Application/Services/ApplicationService.cs` (MediatR registration)\\n- `src/Presentation/API/Configuration/ApplicationConfiguration.cs` (DI setup)\\n- `src/Presentation/API/Validators/*.cs` (existing validators)\\n\\n## Dependencies\\n\\n- **Blocks:** C3 (Hard invariants), C4 (Soft invariants)\\n- **Blocked by:** Nothing — can start immediately
