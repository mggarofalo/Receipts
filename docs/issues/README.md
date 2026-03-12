# Issue Archive

Archived Linear issue specifications, organized by milestone/phase.

**175 issues** across 10 milestones.

## Phase 0: Housekeeping

| Issue | Title | Labels |
|-------|-------|--------|
| [MGG-82](./mgg-82-update-github-actions-workflow-to-net-10-and-add-comprehensive-ci-steps.md) | Update GitHub Actions workflow to .NET 10 and add comprehensive CI steps | `infra` |
| [MGG-90](./mgg-90-remove-blazor-wasm-frontend-project.md) | Remove Blazor WASM Frontend Project | `cleanup`, `backend` |
| [MGG-91](./mgg-91-migrate-solution-file-from-sln-to-slnx-format.md) | Migrate solution file from .sln to .slnx format | `cleanup`, `infra` |
| [MGG-92](./mgg-92-add-pre-commit-hooks-with-husky-net.md) | Add pre-commit hooks with Husky.NET | `dx`, `infra` |

## Phase 1: OpenAPI Spec-First

| Issue | Title | Labels |
|-------|-------|--------|
| [MGG-14](./mgg-14-verify-swagger-openapi-baseline-output.md) | Verify Swagger/OpenAPI baseline output | `testing`, `backend` |
| [MGG-21](./mgg-21-establish-openapi-spec-as-authoritative-api-contract.md) | Establish OpenAPI spec as authoritative API contract | `codegen`, `backend` |
| [MGG-89](./mgg-89-establish-openapi-spec-first-api-contract.md) | Establish OpenAPI Spec-First API Contract | `epic`, `backend` |
| [MGG-93](./mgg-93-add-openapi-endpoint-metadata-attributes-to-all-controllers.md) | Add OpenAPI endpoint metadata attributes to all controllers | `dx`, `backend` |

## Phase 2: Backend DTO Generation

| Issue | Title | Labels |
|-------|-------|--------|
| [MGG-83](./mgg-83-replace-viewmodels-with-spec-generated-dtos.md) | Replace ViewModels with spec-generated DTOs | `epic`, `backend` |
| [MGG-87](./mgg-87-update-documentation-to-reflect-viewmodel-dto-rename.md) | Update documentation to reflect ViewModel → DTO rename | `docs` |
| [MGG-88](./mgg-88-generate-net-request-response-dtos-from-openapi-spec.md) | Generate .NET Request/Response DTOs from OpenAPI spec | `codegen`, `backend` |
| [MGG-95](./mgg-95-add-mapping-unit-tests-for-generated-dtos-to-detect-controller-service-entity-dr.md) | Add mapping unit tests for generated DTOs to detect controller/service/entity drift | `testing`, `backend` |
| [MGG-96](./mgg-96-upgrade-drift-detection-to-semantic-property-level-openapi-comparison.md) | Upgrade drift detection to semantic property-level OpenAPI comparison | `dx`, `backend` |
| [MGG-97](./mgg-97-add-breaking-change-detection-to-ci-pipeline.md) | Add breaking change detection to CI pipeline | `dx`, `infra` |
| [MGG-100](./mgg-100-evaluate-kiota-as-nswag-replacement-for-dto-generation.md) | Evaluate Kiota as NSwag replacement for DTO generation | `codegen`, `backend` |

## Phase 3: Aspire Developer Experience

| Issue | Title | Labels |
|-------|-------|--------|
| [MGG-72](./mgg-72-create-net-aspire-apphost-project.md) | Create .NET Aspire AppHost Project | `dx`, `infra` |
| [MGG-73](./mgg-73-configure-api-service-in-apphost.md) | Configure API Service in AppHost | `dx`, `infra` |
| [MGG-74](./mgg-74-configure-database-resource-in-apphost.md) | Configure Database Resource in AppHost | `dx`, `infra` |
| [MGG-76](./mgg-76-setup-vs-code-launch-json-and-tasks-json-for-f5-debugging.md) | Setup VS Code launch.json and tasks.json for F5 Debugging | `dx` |
| [MGG-77](./mgg-77-configure-aspire-dashboard-and-telemetry.md) | Configure Aspire Dashboard and Telemetry | `dx`, `infra` |
| [MGG-78](./mgg-78-integrate-aspire-mcp-server-for-ai-agent-access.md) | Integrate Aspire MCP Server for AI Agent Access | `dx`, `infra` |
| [MGG-79](./mgg-79-setup-agent-browser-for-ai-powered-testing.md) | Setup Agent Browser for AI-Powered Testing | `testing`, `dx` |
| [MGG-80](./mgg-80-documentation-f5-debugging-workflow-developer-guide.md) | Documentation: F5 Debugging Workflow & Developer Guide | `dx`, `docs` |
| [MGG-109](./mgg-109-core-aspire-orchestration.md) | Core Aspire Orchestration | `epic`, `dx`, `infra` |
| [MGG-110](./mgg-110-ai-powered-dev-tooling.md) | AI-Powered Dev Tooling | `epic`, `dx` |
| [MGG-196](./mgg-196-restructure-agents-md-and-consolidate-documentation-into-docs.md) | Restructure AGENTS.md and consolidate documentation into docs/ | `dx`, `docs` |

## Phase 4 [Backend]

| Issue | Title | Labels |
|-------|-------|--------|
| [MGG-34](./mgg-34-backend-asp-net-identity-jwt-authentication.md) | Backend: ASP.NET Identity + JWT Authentication | `security`, `backend` |
| [MGG-66](./mgg-66-backend-soft-delete-implementation.md) | Backend: Soft Delete Implementation | `backend`, `Feature` |
| [MGG-67](./mgg-67-backend-field-level-change-auditing-system.md) | Backend: Field-Level Change Auditing System | `security`, `backend` |
| [MGG-68](./mgg-68-backend-authentication-audit-log-with-180-day-retention.md) | Backend: Authentication Audit Log with 180-Day Retention | `security`, `backend` |
| [MGG-129](./mgg-129-backend-protect-existing-api-endpoints-with-authorize.md) | Backend: Protect existing API endpoints with [Authorize] | `security`, `backend` |
| [MGG-130](./mgg-130-backend-role-based-authorization-policies.md) | Backend: Role-based authorization policies | `security`, `backend`, `Feature` |
| [MGG-166](./mgg-166-fix-applicationdbcontext-di-adddbcontextfactory-scoped-icurrentuseraccessor-cras.md) | Fix ApplicationDbContext DI: AddDbContextFactory + scoped ICurrentUserAccessor crash at startup | `cleanup`, `backend` |
| [MGG-167](./mgg-167-add-ef-migration-for-auditlogs-and-authauditlogs-tables.md) | Add EF migration for AuditLogs and AuthAuditLogs tables | `cleanup`, `backend` |
| [MGG-169](./mgg-169-require-seeded-admin-to-reset-password-on-first-login.md) | Require seeded admin to reset password on first login | `security`, `frontend`, `backend` |
| [MGG-195](./mgg-195-server-side-enforcement-of-mustresetpassword-via-jwt-claim-middleware.md) | Server-side enforcement of MustResetPassword via JWT claim + middleware | `security`, `backend` |

## Phase 4 [Frontend]

| Issue | Title | Labels |
|-------|-------|--------|
| [MGG-33](./mgg-33-project-setup-react-vite-typescript-shadcn-ui.md) | Project Setup: React + Vite + TypeScript + shadcn/ui | `infra`, `frontend` |
| [MGG-35](./mgg-35-frontend-authentication-ui-protected-routes.md) | Frontend: Authentication UI & Protected Routes | `security`, `frontend` |
| [MGG-36](./mgg-36-generate-typescript-types-and-api-client-from-openapi-spec.md) | Generate TypeScript types and API client from OpenAPI spec | `codegen`, `frontend` |
| [MGG-37](./mgg-37-signalr-integration-for-real-time-updates.md) | SignalR Integration for Real-time Updates | `frontend`, `backend` |
| [MGG-38](./mgg-38-receipts-module-full-crud-with-search-filters.md) | Receipts Module: Full CRUD with Search & Filters | `frontend`, `Feature` |
| [MGG-39](./mgg-39-receipt-items-module.md) | Receipt Items Module | `frontend`, `Feature` |
| [MGG-40](./mgg-40-accounts-module.md) | Accounts Module | `frontend`, `Feature` |
| [MGG-41](./mgg-41-transactions-module.md) | Transactions Module | `frontend`, `Feature` |
| [MGG-42](./mgg-42-trips-module.md) | Trips Module | `frontend`, `Feature` |
| [MGG-43](./mgg-43-aggregate-views-receiptwithitems-transactionaccount.md) | Aggregate Views: ReceiptWithItems & TransactionAccount | `frontend`, `Feature` |
| [MGG-44](./mgg-44-fuzzy-search-advanced-filtering-system.md) | Fuzzy Search & Advanced Filtering System | `frontend`, `Feature` |
| [MGG-45](./mgg-45-keyboard-navigation-shortcuts-system.md) | Keyboard Navigation & Shortcuts System | `frontend`, `Feature` |
| [MGG-46](./mgg-46-accessibility-audit-wcag-2-1-aa-compliance.md) | Accessibility Audit & WCAG 2.1 AA Compliance | `frontend`, `Improvement` |
| [MGG-47](./mgg-47-animations-transitions-loading-states.md) | Animations, Transitions & Loading States | `frontend`, `Improvement` |
| [MGG-48](./mgg-48-error-handling-user-feedback-system.md) | Error Handling & User Feedback System | `frontend`, `Improvement` |
| [MGG-49](./mgg-49-testing-suite-unit-integration-e2e-tests.md) | Testing Suite: Unit, Integration & E2E Tests | `testing`, `frontend` |
| [MGG-50](./mgg-50-documentation-developer-guide.md) | Documentation & Developer Guide | `docs`, `frontend` |
| [MGG-69](./mgg-69-frontend-audit-history-change-tracking-ui.md) | Frontend: Audit History & Change Tracking UI | `security`, `frontend` |
| [MGG-75](./mgg-75-configure-frontend-dev-server-vite-in-apphost.md) | Configure Frontend Dev Server (Vite) in AppHost | `dx`, `infra` |
| [MGG-111](./mgg-111-frontend-bootstrap-codegen.md) | Frontend Bootstrap & Codegen | `epic`, `codegen`, `infra`, `frontend` |
| [MGG-112](./mgg-112-authentication-system.md) | Authentication System | `epic`, `security`, `frontend`, `backend` |
| [MGG-113](./mgg-113-core-crud-modules.md) | Core CRUD Modules | `epic`, `frontend`, `Feature` |
| [MGG-114](./mgg-114-data-safety-audit-trail.md) | Data Safety & Audit Trail | `epic`, `security`, `frontend`, `backend` |
| [MGG-115](./mgg-115-ux-polish-enhancements.md) | UX Polish & Enhancements | `epic`, `frontend`, `Improvement` |
| [MGG-116](./mgg-116-frontend-quality-documentation.md) | Frontend Quality & Documentation | `epic`, `testing`, `docs`, `frontend` |
| [MGG-128](./mgg-128-permission-system.md) | Permission System | `epic`, `security`, `frontend`, `backend` |
| [MGG-131](./mgg-131-frontend-permission-aware-ui.md) | Frontend: Permission-aware UI | `security`, `frontend`, `Feature` |
| [MGG-132](./mgg-132-frontend-admin-permissions-manager.md) | Frontend: Admin Permissions Manager | `security`, `frontend`, `Feature` |
| [MGG-159](./mgg-159-backend-get-api-users-endpoint-for-admin-user-listing.md) | Backend: GET /api/users endpoint for admin user listing | `backend`, `Feature` |
| [MGG-160](./mgg-160-fix-n-1-query-in-userscontroller-listusers.md) | Fix N+1 query in UsersController.ListUsers | `backend`, `Bug` |
| [MGG-161](./mgg-161-fix-clean-architecture-violation-userscontroller-bypasses-service-layer.md) | Fix Clean Architecture violation: UsersController bypasses service layer | `backend`, `Bug` |
| [MGG-162](./mgg-162-backend-list-deleted-items-api-endpoints-for-proper-recycle-bin.md) | Backend: List deleted items API endpoints for proper recycle bin | `backend`, `Feature` |
| [MGG-163](./mgg-163-frontend-audit-ui-enhancements-date-range-pagination-real-time-character-level-d.md) | Frontend: Audit UI enhancements (date range, pagination, real-time, character-level diff) | `frontend`, `Feature` |
| [MGG-168](./mgg-168-dark-mode-light-dark-system-switcher.md) | Dark Mode (Light/Dark/System Switcher) | `frontend`, `Feature` |
| [MGG-170](./mgg-170-remove-registration-and-add-admin-only-user-management.md) | Remove registration and add admin-only user management | `security`, `frontend`, `backend` |
| [MGG-176](./mgg-176-eliminate-all-uuid-entry-fields.md) | Eliminate all UUID entry fields | `epic`, `frontend`, `Feature` |
| [MGG-177](./mgg-177-add-empty-trash-button-for-hard-deleting-soft-deleted-items.md) | Add Empty Trash button for hard-deleting soft-deleted items | `frontend`, `backend`, `Feature` |
| [MGG-178](./mgg-178-click-to-select-row-in-data-tables.md) | Click-to-select row in data tables | `frontend`, `Feature` |
| [MGG-179](./mgg-179-reorganize-top-nav-bar-with-grouped-hover-menus.md) | Reorganize top nav bar with grouped hover menus | `frontend`, `Feature` |
| [MGG-180](./mgg-180-global-items-list-with-autocomplete-templates.md) | Global items list with autocomplete templates | `frontend`, `backend`, `Feature` |
| [MGG-181](./mgg-181-support-dual-item-quantification-qty-unit-price-vs-flat-price.md) | Support dual item quantification: qty × unit price vs flat price | `frontend`, `backend`, `Feature` |
| [MGG-182](./mgg-182-currency-input-component-replace-numeric-spinner.md) | Currency input component (replace numeric spinner) | `frontend`, `Feature` |
| [MGG-183](./mgg-183-category-and-subcategory-management-pages.md) | Category and subcategory management pages | `frontend`, `Feature` |
| [MGG-184](./mgg-184-replace-uuid-inputs-in-transaction-creation-with-comboboxes.md) | Replace UUID inputs in Transaction creation with comboboxes | `frontend`, `Feature` |
| [MGG-185](./mgg-185-replace-uuid-input-in-trip-search-with-contextual-picker.md) | Replace UUID input in Trip search with contextual picker | `frontend`, `Feature` |
| [MGG-186](./mgg-186-overhaul-user-management-to-show-user-list-instead-of-uuid-lookup.md) | Overhaul User Management to show user list instead of UUID lookup | `frontend`, `Feature` |
| [MGG-187](./mgg-187-accessibility-follow-up-public-route-landmarks-page-titles-heading-semantics.md) | Accessibility follow-up: public route landmarks, page titles, heading semantics | `frontend`, `Improvement` |
| [MGG-192](./mgg-192-fix-focus-outline-on-table-row-click.md) | Fix focus outline on table row click | `cleanup`, `frontend` |
| [MGG-261](./mgg-261-buffer-signalr-broadcasts-aggregate-client-toasts.md) | Buffer SignalR broadcasts & aggregate client toasts | `frontend`, `backend`, `Improvement` |

## Phase 5: Test Coverage

| Issue | Title | Labels |
|-------|-------|--------|
| [MGG-121](./mgg-121-test-coverage-pipeline.md) | Test Coverage Pipeline | `epic`, `testing`, `infra`, `frontend`, `backend` |
| [MGG-122](./mgg-122-configure-code-coverage-collection-with-coverlet-and-cobertura-report-output.md) | Configure code coverage collection with coverlet and Cobertura report output | `testing`, `infra`, `backend` |
| [MGG-123](./mgg-123-publish-code-coverage-report-to-github-via-ci.md) | Publish code coverage report to GitHub via CI | `testing`, `dx`, `infra`, `frontend` |
| [MGG-124](./mgg-124-enforce-minimum-test-coverage-threshold-as-a-ci-branch-protection-gate.md) | Enforce minimum test coverage threshold as a CI branch protection gate | `testing`, `infra`, `frontend` |
| [MGG-125](./mgg-125-agent-test-writing-loop-read-coverage-report-and-fill-gaps-iteratively.md) | Agent test-writing loop: read coverage report and fill gaps iteratively | `testing`, `dx`, `backend` |
| [MGG-126](./mgg-126-configure-vitest-with-coverage-v8-cobertura-output-in-the-react-app.md) | Configure Vitest with coverage (v8 + Cobertura output) in the React app | `testing`, `dx`, `frontend` |
| [MGG-127](./mgg-127-agent-test-writing-loop-react-frontend-coverage-gap-filling.md) | Agent test-writing loop: React frontend coverage gap filling | `testing`, `dx`, `frontend` |

## Phase 7: Correctness Hardening

| Issue | Title | Labels |
|-------|-------|--------|
| [MGG-94](./mgg-94-correctness-hardening.md) | Correctness Hardening | `epic`, `frontend`, `backend` |
| [MGG-98](./mgg-98-implement-header-based-api-versioning.md) | Implement header-based API versioning | `backend`, `Feature` |
| [MGG-99](./mgg-99-add-runtime-openapi-contract-validation-middleware-dev-mode.md) | Add runtime OpenAPI contract validation middleware (dev mode) | `testing`, `backend` |
| [MGG-164](./mgg-164-audit-and-fix-clean-architecture-layer-violations.md) | Audit and fix Clean Architecture layer violations | `cleanup`, `backend` |
| [MGG-197](./mgg-197-wire-up-fluentvalidation-mediatr-validation-pipeline.md) | Wire up FluentValidation + MediatR validation pipeline | `backend`, `Improvement` |
| [MGG-198](./mgg-198-add-adjustment-entity-adjustmenttype-enum-and-evolve-domain-aggregates.md) | Add Adjustment entity, AdjustmentType enum, and evolve domain aggregates | `codegen`, `backend`, `Feature` |
| [MGG-199](./mgg-199-enforce-tier-1-hard-invariants.md) | Enforce Tier 1 hard invariants | `backend`, `Feature` |
| [MGG-200](./mgg-200-add-tier-2-soft-invariant-warnings.md) | Add Tier 2 soft invariant warnings | `codegen`, `backend`, `Improvement` |
| [MGG-201](./mgg-201-frontend-adjustment-management-and-validation-display.md) | Frontend — Adjustment management and validation display | `frontend`, `Feature` |
| [MGG-202](./mgg-202-comprehensive-test-suite-for-correctness-hardening.md) | Comprehensive test suite for correctness hardening | `testing`, `backend` |
| [MGG-203](./mgg-203-update-documentation-and-linear-workspace-for-phase-7.md) | Update documentation and Linear workspace for Phase 7 | `docs` |

## Phase 8: Security Automation

| Issue | Title | Labels |
|-------|-------|--------|
| [MGG-101](./mgg-101-improve-logging-security-practices.md) | Improve Logging & Security Practices | `triage` |
| [MGG-104](./mgg-104-snyk-to-linear-integration-rewrite.md) | Snyk-to-Linear integration rewrite | `epic`, `security`, `infra` |
| [MGG-105](./mgg-105-create-snyk-and-resolved-by-scan-linear-labels.md) | Create `snyk` and `resolved-by-scan` Linear labels | `infra` |
| [MGG-106](./mgg-106-rewrite-create-linear-issues-mjs-with-dedup-lifecycle.md) | Rewrite `create-linear-issues.mjs` with dedup + lifecycle | `security`, `infra` |
| [MGG-107](./mgg-107-clean-up-security-job-in-github-ci-yml.md) | Clean up security job in `github-ci.yml` | `infra` |

## Uncategorized

| Issue | Title | Labels |
|-------|-------|--------|
| [MGG-1](./mgg-1-get-familiar-with-linear.md) | Get familiar with Linear |  |
| [MGG-2](./mgg-2-set-up-your-teams.md) | Set up your teams |  |
| [MGG-3](./mgg-3-connect-your-tools.md) | Connect your tools |  |
| [MGG-4](./mgg-4-import-your-data.md) | Import your data |  |
| [MGG-13](./mgg-13-refactor-totalamount-calculation-to-automapper.md) | Refactor TotalAmount calculation to AutoMapper |  |
| [MGG-18](./mgg-18-migrate-from-automapper-to-mapperly.md) | Migrate from AutoMapper to Mapperly |  |
| [MGG-23](./mgg-23-fix-build-warnings-and-test-failures.md) | Fix build warnings and test failures |  |
| [MGG-24](./mgg-24-refactor-model-classes-to-pocos-and-adopt-fluentassertions.md) | Refactor model classes to POCOs and adopt FluentAssertions | `Improvement` |
| [MGG-25](./mgg-25-add-fluentassertions-nuget-package-to-test-projects.md) | Add FluentAssertions NuGet package to test projects |  |
| [MGG-26](./mgg-26-strip-iequatable-boilerplate-from-all-19-model-classes.md) | Strip IEquatable boilerplate from all 19 model classes |  |
| [MGG-27](./mgg-27-update-domain-tests-remove-equality-tests-adopt-fluentassertions.md) | Update Domain.Tests — remove equality tests, adopt FluentAssertions |  |
| [MGG-28](./mgg-28-update-infrastructure-tests-remove-equality-tests-adopt-fluentassertions.md) | Update Infrastructure.Tests — remove equality tests, adopt FluentAssertions |  |
| [MGG-29](./mgg-29-update-presentation-shared-tests-remove-equality-tests-adopt-fluentassertions.md) | Update Presentation.Shared.Tests — remove equality tests, adopt FluentAssertions |  |
| [MGG-30](./mgg-30-update-presentation-api-tests-adopt-fluentassertions-for-mapping-tests.md) | Update Presentation.API.Tests — adopt FluentAssertions for mapping tests |  |
| [MGG-31](./mgg-31-update-application-tests-adopt-fluentassertions-for-query-handler-assertions.md) | Update Application.Tests — adopt FluentAssertions for query handler assertions |  |
| [MGG-81](./mgg-81-resolve-mapperly-nullable-mapping-warnings-for-id-properties.md) | Resolve Mapperly nullable mapping warnings for ID properties | `Bug` |
| [MGG-133](./mgg-133-split-nswag-generated-dtos-g-cs-into-one-file-per-class.md) | Split NSwag-generated Dtos.g.cs into one file per class | `dx`, `codegen`, `backend` |
| [MGG-134](./mgg-134-migrate-from-husky-to-native-git-hooks.md) | Migrate from Husky to native Git hooks | `dx`, `cleanup` |
| [MGG-165](./mgg-165-manual-wcag-2-1-aa-verification-post-mgg-46.md) | Manual WCAG 2.1 AA verification (post MGG-46) | `testing`, `frontend` |
| [MGG-171](./mgg-171-shortcut-doesn-t-trigger-help-modal.md) | ? shortcut doesn't trigger help modal | `frontend`, `Bug` |
| [MGG-172](./mgg-172-ctrl-shift-n-intercepted-by-chrome-opens-incognito.md) | Ctrl+Shift+N intercepted by Chrome (opens incognito) | `frontend`, `Bug` |
| [MGG-173](./mgg-173-enter-on-focused-row-edit-dialog-opens-then-immediately-closes.md) | Enter on focused row: edit dialog opens then immediately closes | `frontend`, `Bug` |
| [MGG-174](./mgg-174-ctrl-k-command-palette-opens-over-active-dialogs.md) | Ctrl+K command palette opens over active dialogs | `frontend`, `Bug` |
| [MGG-175](./mgg-175-signalr-hub-not-broadcasting-entity-changes-to-other-clients.md) | SignalR hub not broadcasting entity changes to other clients | `frontend`, `backend`, `Bug` |
| [MGG-188](./mgg-188-theme-toggle-context-aware-icons-sun-moon-sun-moon-for-system.md) | Theme toggle: context-aware icons (Sun, Moon, Sun+Moon for system) | `frontend`, `Improvement` |
| [MGG-189](./mgg-189-fix-flash-of-white-background-on-dark-mode-page-load.md) | Fix flash of white background on dark mode page load | `frontend`, `Improvement` |
| [MGG-190](./mgg-190-migrate-to-createbrowserrouter-data-router-for-usenavigation-support.md) | Migrate to createBrowserRouter (data router) for useNavigation support | `frontend`, `Improvement` |
| [MGG-191](./mgg-191-responsive-layout-mobile-hamburger-menu-for-narrow-viewports.md) | Responsive layout: mobile hamburger menu for narrow viewports | `frontend`, `Improvement` |
| [MGG-193](./mgg-193-audit-react-hooks-fix-bugs-reduce-re-renders-tighten-patterns.md) | Audit React hooks: fix bugs, reduce re-renders, tighten patterns | `cleanup`, `frontend` |
| [MGG-194](./mgg-194-remove-unused-frontend-code-dead-components-hooks-pages.md) | Remove unused frontend code: dead components, hooks, pages | `cleanup`, `frontend` |
| [MGG-209](./mgg-209-fix-node-js-punycode-deprecation-warning-in-spectral-lint.md) | Fix Node.js punycode deprecation warning in Spectral lint | `dx`, `cleanup` |
| [MGG-210](./mgg-210-add-pre-commit-hook-to-reorder-using-statements-to-system-first-standard.md) | Add pre-commit hook to reorder using statements to system-first standard | `dx`, `cleanup` |
| [MGG-211](./mgg-211-review-and-decompose-large-tsx-page-components.md) | Review and decompose large TSX page components | `cleanup`, `frontend` |
| [MGG-212](./mgg-212-comprehensive-frontend-test-coverage.md) | Comprehensive frontend test coverage | `testing`, `frontend` |
| [MGG-213](./mgg-213-add-typescript-and-eslint-checks-to-pre-commit-hooks-for-react-client.md) | Add TypeScript and ESLint checks to pre-commit hooks for React client | `dx`, `frontend` |
| [MGG-214](./mgg-214-bug-new-subcategory-created-during-item-edit-does-not-persist.md) | Bug: new subcategory created during item edit does not persist | `frontend`, `Bug` |
| [MGG-215](./mgg-215-clean-up-rest-api-surface-extract-non-collection-path-params-to-query-params.md) | Clean up REST API surface: extract non-collection path params to query params | `codegen`, `cleanup`, `frontend`, `backend` |
| [MGG-216](./mgg-216-reset-password-flow-should-require-a-different-password.md) | Reset password flow should require a different password | `security`, `frontend`, `backend`, `Improvement` |
| [MGG-217](./mgg-217-add-visibility-toggle-icon-to-password-fields.md) | Add visibility toggle icon to password fields | `frontend`, `Improvement` |
| [MGG-218](./mgg-218-add-sample-data-seeding-for-dev-environments.md) | Add sample data seeding for dev environments | `dx`, `backend` |
| [MGG-219](./mgg-219-implement-offset-limit-pagination-in-all-apis.md) | Implement offset/limit pagination in all APIs | `frontend`, `backend`, `Feature` |
| [MGG-220](./mgg-220-fix-frontend-typescript-errors-and-test-failures.md) | Fix frontend TypeScript errors and test failures | `testing`, `frontend`, `Bug` |
| [MGG-221](./mgg-221-replace-it-isany-guid-with-specific-values-in-unit-tests-and-add-testing-guidanc.md) | Replace It.IsAny<Guid>() with specific values in unit tests and add testing guidance | `cleanup`, `backend` |
| [MGG-222](./mgg-222-add-multi-group-batch-test-coverage-for-transaction-operations.md) | Add multi-group batch test coverage for transaction operations | `backend` |
| [MGG-223](./mgg-223-wrap-batch-transaction-operations-in-a-database-transaction-for-atomicity.md) | Wrap batch transaction operations in a database transaction for atomicity | `backend` |
| [MGG-224](./mgg-224-improve-client-unit-test-coverage-for-unhealthy-packages-src-src-pages.md) | Improve client unit test coverage for unhealthy packages (src, src.pages) | `frontend` |
| [MGG-225](./mgg-225-fix-csv-export-quoting-in-auditlog-exporttocsv.md) | Fix CSV export quoting in AuditLog exportToCsv |  |
| [MGG-226](./mgg-226-include-canonical-id-in-relevant-api-response-headers.md) | Include canonical ID in relevant API response headers | `backend`, `Feature` |
| [MGG-227](./mgg-227-migrate-controller-endpoints-to-typedresults-with-concrete-results-t1-t2-return-.md) | Migrate controller endpoints to TypedResults with concrete Results<T1, T2, ...> return types | `backend`, `Improvement` |
| [MGG-228](./mgg-228-enforce-narrow-ef-core-projections-minimum-required-fields-in-all-queries.md) | Enforce narrow EF Core projections — minimum required fields in all queries | `backend`, `Improvement` |
| [MGG-229](./mgg-229-implement-token-auth-per-rfc-6749-introspection-per-rfc-7662-and-revocation-per-.md) | Implement token auth per RFC 6749, introspection per RFC 7662, and revocation per RFC 7009 | `security`, `backend` |
| [MGG-230](./mgg-230-fix-openapi-spec-drift-remove-500-status-codes-not-present-in-generated-output.md) | Fix OpenAPI spec drift: remove 500 status codes not present in generated output | `backend`, `Bug` |
| [MGG-231](./mgg-231-improve-api-test-coverage-to-near-100.md) | Improve API test coverage to near 100% | `backend` |
| [MGG-233](./mgg-233-dev-data-seeder-crashes-api-on-restart-duplicate-key-constraint.md) | Dev data seeder crashes API on restart (duplicate key constraint) | `dx`, `backend`, `Bug` |
| [MGG-235](./mgg-235-migrate-dev-seed-data-to-ef-core-hasdata.md) | Migrate dev seed data to EF Core HasData | `dx`, `backend`, `Improvement` |
| [MGG-236](./mgg-236-add-timeout-warning-state-to-loading-buttons-and-respect-api-abort-signals.md) | Add timeout warning state to loading buttons and respect API abort signals | `frontend`, `Feature` |
| [MGG-237](./mgg-237-update-agents-md-with-worktree-setup-instructions.md) | Update AGENTS.md with worktree setup instructions | `dx`, `docs` |
| [MGG-238](./mgg-238-fix-ef-core-sentinel-value-warning-for-receiptitementity-pricingmode.md) | Fix EF Core sentinel value warning for ReceiptItemEntity.PricingMode | `backend`, `Bug` |
| [MGG-239](./mgg-239-extract-entity-configurations-from-applicationdbcontext-into-ientitytypeconfigur.md) | Extract entity configurations from ApplicationDbContext into IEntityTypeConfiguration classes | `cleanup`, `backend` |
| [MGG-240](./mgg-240-forced-password-change-can-be-bypassed-by-navigating-away.md) | Forced password change can be bypassed by navigating away | `security`, `frontend`, `backend` |
| [MGG-241](./mgg-241-navbar-dropdown-menus-don-t-align-to-their-trigger-element.md) | Navbar dropdown menus don't align to their trigger element | `frontend` |
| [MGG-242](./mgg-242-docker-ci-build-arm64-natively-instead-of-qemu-emulation.md) | Docker CI: build arm64 natively instead of QEMU emulation | `Improvement` |
| [MGG-243](./mgg-243-docker-ci-optimize-gha-cache-export-strategy.md) | Docker CI: optimize GHA cache export strategy | `Improvement` |
| [MGG-245](./mgg-245-docker-ci-skip-arm64-build-on-pull-requests.md) | Docker CI: skip arm64 build on pull requests | `Improvement` |
| [MGG-246](./mgg-246-fix-map-is-not-a-function-on-trips-and-transactionform.md) | Fix `.map is not a function` on /trips and TransactionForm | `frontend`, `Feature` |
| [MGG-257](./mgg-257-fix-nav-dropdown-transparency-in-viewport-false-mode.md) | Fix nav dropdown transparency in viewport=false mode | `frontend` |
| [MGG-258](./mgg-258-fix-api-key-creation-fails-with-500-non-utc-datetimeoffset-rejected-by-npgsql.md) | fix: API key creation fails with 500 — non-UTC DateTimeOffset rejected by Npgsql | `backend` |

*This index is auto-generated by `scripts/link-issue-docs.py`.*
