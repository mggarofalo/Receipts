# AGENTS.md

This file provides guidance to AI agents when working with code in this repository.

## Prerequisites

- **.NET 10 SDK** — build, test, and run the API
- **Node.js 18+** and **npm** — OpenAPI spec linting (`@stoplight/spectral-cli`) and semantic drift detection (`js-yaml`)
- **PostgreSQL** — runtime database (connection via environment variables)

After cloning, run:

```bash
dotnet restore Receipts.slnx
npm install
```

## Development Workflow

When working on tasks that are expected to result in code changes, follow this standard process:

1. **Linear Issue Management**
   - Check if a Linear issue exists for the work
   - If no issue exists, create one with:
     - Clear title describing the work
     - Description with acceptance criteria
     - At least one layer label (`backend`, `frontend`, `infra`, `docs`) + type labels as needed
     - Team assignment to "Mggarofalo"
   - Link the issue ID to your work
   - **See [LINEAR.md](LINEAR.md)** for full workspace structure, milestone phases, priority semantics, and how to determine "what's next"

   **Linear MCP Access:**
   - Linear is available via MCP server - you can directly create/update issues
   - Team is "Mggarofalo" (team ID: `a4aff05d-41e6-45dc-b670-cdb485fef765`)
   - **Do not check for teams** - the team information is stable and documented here
   - Use the team name "Mggarofalo" directly when creating issues
   - All issues should be assigned to project "Receipts" and an appropriate milestone (Phase 0–5)

2. **Branch Strategy (Two-Tier)**

   This project uses a hierarchical branching model: **milestone branches** for CI/PR gating, optional **parent branches** for epics, and **issue branches** for individual work items.

   **Milestone branches** (one per phase):
   - Created when work on a milestone begins, named `milestone/phase-N` (e.g., `milestone/phase-0`)
   - All issue work within that phase merges locally into the milestone branch
   - When the milestone is complete, open a **PR from the milestone branch to `master`**
   - The PR triggers CI — this is the safety net that catches issues the agent may have missed
   - After PR merge, delete the milestone branch

   **Parent branches** (for epics with multiple children):
   - When an epic has multiple child issues, create a parent branch using the epic's `gitBranchName`
   - Parent branch is created off `master` (or the milestone branch if one exists)
   - Child issue branches are created off the parent branch and squash-merge back into it
   - When all children are complete, the parent branch gets a PR to `master`
   - This keeps related changes grouped and avoids polluting `master` with intermediate work

   **Issue branches** (one per Linear issue):
   - Branch off the parent branch (if epic) or milestone branch, NOT `master`
   - Use the `gitBranchName` from the Linear issue
   - Merge locally into the parent/milestone branch via squash merge (no PR needed)
   - Delete the issue branch after merge

   ```
   master
     ├── milestone/phase-0                              (PR → master)
     │     ├── mggarofalo/mgg-90-remove-blazor          (squash-merge into milestone)
     │     └── mggarofalo/mgg-82-update-ci              (squash-merge into milestone)
     │
     └── mggarofalo/mgg-83-replace-viewmodels-...       (epic parent, PR → master)
           ├── mggarofalo/mgg-88-generate-dtos          (squash-merge into parent)
           └── mggarofalo/mgg-87-update-docs            (squash-merge into parent)
   ```

   **Worktrees (mandatory for all branch work):**
   - **ALWAYS** use worktrees for issue and milestone branches — do NOT checkout branches in the main repo
   - The main repo at `Source/Receipts` must **always stay on `master`** and never be switched to another branch
   - Use `/worktree <issue-id>` to create an isolated working directory in `.worktrees/`
   - **ALWAYS** create worktrees in `.worktrees/` at the repo root — NEVER as sibling directories
   - This gives agents full filesystem control in their worktree without affecting the main repo

3. **Merging Issue Work into Parent/Milestone Branch**
   - All merges happen inside worktrees — never checkout branches in the main repo
   - Remove the issue worktree, then merge from the parent (or milestone) worktree:
     ```bash
     git worktree remove .worktrees/mggarofalo-mgg-88-generate-dtos
     cd .worktrees/mggarofalo-mgg-83-replace-viewmodels
     git merge --squash mggarofalo/mgg-88-generate-dtos
     git commit -m "feat(api): generate DTOs from OpenAPI spec (MGG-88)"
     git branch -D mggarofalo/mgg-88-generate-dtos
     ```
   - If no parent/milestone worktree exists yet, create one:
     ```bash
     git branch mggarofalo/mgg-83-replace-viewmodels master
     git worktree add .worktrees/mggarofalo-mgg-83-replace-viewmodels mggarofalo/mgg-83-replace-viewmodels
     ```

4. **PR: Parent/Milestone → Master**
   - When all issues are complete, push the branch and open a PR:
     ```bash
     cd .worktrees/mggarofalo-mgg-83-replace-viewmodels
     git push -u origin mggarofalo/mgg-83-replace-viewmodels
     gh pr create --title "Replace ViewModels with spec-generated DTOs" --body "..."
     ```
   - The PR triggers CI (build + test) — this is the checkpoint that surfaces issues
   - After CI passes and the PR is approved, merge into `master`
   - Clean up worktree and branches:
     ```bash
     cd <repo-root>
     git worktree remove .worktrees/mggarofalo-mgg-83-replace-viewmodels
     git branch -d mggarofalo/mgg-83-replace-viewmodels
     git push origin --delete mggarofalo/mgg-83-replace-viewmodels
     git pull   # update master with the merged PR
     ```

5. **Direct Commits to Master**
   - Only use for non-Linear work like:
     - Trivial typo fixes
     - Documentation updates
     - Tooling/build configuration
   - **NEVER** commit Linear-based work directly to master
   - When in doubt, create a branch

## Build and Test Commands

```bash
# Build entire solution
dotnet build Receipts.slnx

# Run all tests
dotnet test Receipts.slnx

# Run tests for a specific project
dotnet test tests/Application.Tests/Application.Tests.csproj

# Run a single test by name
dotnet test --filter "FullyQualifiedName~TestMethodName"

# Run the API
dotnet run --project src/Presentation/API/API.csproj

# Add EF Core migration (run from Infrastructure project)
dotnet ef migrations add MigrationName --project src/Infrastructure/Infrastructure.csproj --startup-project src/Presentation/API/API.csproj
```

## Pre-commit Hooks (Native Git Hooks)

This repo uses native Git hooks via `core.hooksPath`. Hooks install automatically on `dotnet restore` or `dotnet build` (via an MSBuild target in `Directory.Build.targets`).

**Pre-commit pipeline (runs on every `git commit`):**
1. `npx spectral lint openapi/spec.yaml` — OpenAPI spec linting
2. `dotnet format --verify-no-changes` — code formatting check
3. `dotnet build -p:TreatWarningsAsErrors=true` — build with warnings-as-errors (also generates `openapi/generated/API.json`)
4. `git diff --exit-code -- src/Presentation/API/Generated/` — DTO staleness check
5. `node scripts/check-drift.mjs` — semantic drift detection (compares schema properties, types, formats, $refs, required fields, and operation structures between spec and generated output)
6. `dotnet test --no-build` — run all tests

**Skipping hooks** (use sparingly):
```bash
git commit --no-verify -m "message"
```

**Manual setup** (if hooks aren't installed):
```bash
bash .githooks/setup.sh
```

## OpenAPI Spec-First Workflow

The canonical API contract lives in `openapi/spec.yaml` (OpenAPI 3.1.0). All API changes follow a spec-first workflow:

1. **Edit the spec** — modify `openapi/spec.yaml` to reflect the desired API change
2. **Lint** — `npm run lint:spec` (or `npx spectral lint openapi/spec.yaml --fail-severity warn`)
3. **Build** — `dotnet build` regenerates `openapi/generated/API.json` from the running API
4. **Check drift** — `npm run check:drift` (or `node scripts/check-drift.mjs`) performs semantic comparison of spec vs generated output (schema properties, types, formats, $refs, required fields, operation structures)

The pre-commit pipeline runs all of these automatically. If the spec and generated output diverge structurally, the drift check fails and the commit is blocked. Cosmetic differences (path casing, type array ordering, extra content types from ASP.NET) are intentionally ignored.

**Breaking change detection** runs in CI on pull requests. It compares the PR's `openapi/spec.yaml` against the base branch to catch backwards-incompatible changes (removed endpoints, removed properties, type changes, new required request fields, nullability narrowing). To allow intentional breaking changes, add the `breaking-changes-allowed` label to the PR.

**Key files:**
- `openapi/spec.yaml` — hand-authored canonical spec (checked in)
- `openapi/generated/API.json` — build-time export from the API (gitignored)
- `.spectral.yaml` — Spectral linting rules
- `scripts/check-drift.mjs` — semantic drift detection script
- `scripts/check-breaking.mjs` — breaking change detection script (CI only)

**npm scripts:**
- `npm run lint:spec` — lint the OpenAPI spec
- `npm run check:drift` — semantic drift check between spec and generated output
- `npm run check:breaking -- origin/master` — check for breaking API changes vs a base ref

## Validation and Code Quality

### LSP Server Checks

When validating code changes, **always check the LSP (Language Server Protocol) server** for warnings and hints when possible. The LSP provides real-time diagnostics that may not appear in build output but indicate potential issues.

**Common LSP Warnings That Require Action:**

- **Mapperly nullable mapping warnings**:
  - Example: "Mapping the nullable source property Id of Domain.Core.Account to the target property Id of Infrastructure.Entities.Core.AccountEntity which is not nullable"
  - **Action Required**: These warnings indicate a design mismatch that should trigger additional research and potential refactoring
  - Do not ignore these - they can lead to runtime null reference exceptions
  - Consider whether:
    - The source should be non-nullable (more common)
    - The target should be nullable (less common)
    - Additional null-handling logic is needed

- **Unused usings, variables, or parameters**: Often indicate incomplete refactoring or dead code

- **Potential null reference warnings**: Especially important with C# nullable reference types enabled

**Validation Workflow:**
1. Build the solution: `dotnet build`
2. Run tests: `dotnet test`
3. Check LSP diagnostics in your editor/IDE
4. Address any warnings or hints before considering work complete
5. If warnings indicate design issues, create a Linear issue for follow-up if the fix is non-trivial

## Architecture

This is a .NET 10 Clean Architecture solution for a receipt management application. It uses central package management via `Directory.Packages.props`.

### Layer Structure

- **Common** - Shared utilities, extension methods, and configuration variable constants
- **Domain** - Core domain models with no dependencies on other layers
  - `Core/` - Entity classes (Account, Receipt, ReceiptItem, Transaction)
  - `Aggregates/` - Composite domain objects (ReceiptWithItems, TransactionAccount, Trip)
- **Application** - Business logic using CQRS pattern with MediatR
  - `Commands/{Entity}/Create|Update|Delete/` - Command + Handler pairs for write operations
  - `Queries/Core/{Entity}/` - Query + Handler pairs for read operations
  - `Queries/Aggregates/` - Complex queries joining multiple entities
  - `Interfaces/Services/` - Service interfaces implemented by Infrastructure
- **Infrastructure** - Data access with PostgreSQL via EF Core
  - `Entities/` - Database entity classes (separate from Domain)
  - `Repositories/` - Repository pattern implementation
  - `Services/` - Service implementations
  - `Mapping/` - Mapperly mappers (Domain <-> Entity)
- **Presentation**
  - **API** - ASP.NET Core Web API with SignalR hub for real-time updates
    - `Controllers/Core/` and `Controllers/Aggregates/` - REST endpoints
    - `Mapping/` - Mapperly mappers (Domain <-> generated DTOs)
    - `Generated/` - NSwag-generated Request/Response DTOs from OpenAPI spec
    - `Validators/` - FluentValidation validators (business rules only; spec-expressible constraints use DataAnnotations)
    - `Configuration/` - Service registration extension methods
    - `Hubs/ReceiptsHub.cs` - SignalR hub
  - **Client** - ~~Removed~~ (was Blazor WebAssembly; removed in MGG-90; being replaced by React/Vite SPA in MGG-32)
  - **Shared** - HTTP clients shared between API and Client (ViewModels removed in MGG-88, replaced by generated DTOs in API layer)

### Key Patterns

- **CQRS**: Commands and Queries are separate with dedicated handlers
- **Mediator Pattern**: MediatR dispatches commands/queries to handlers
- **Repository Pattern**: Infrastructure repositories abstract EF Core
- **Mapping**: Mapperly handles Domain <-> Entity (Infrastructure) and Domain <-> generated DTOs (API)
- **Service Registration**: Each layer has a static extension method (`RegisterApplicationServices`, `RegisterInfrastructureServices`) for DI setup

### Database

PostgreSQL with EF Core. Connection configured via environment variables:
- `POSTGRES_HOST`, `POSTGRES_PORT`, `POSTGRES_USER`, `POSTGRES_PASSWORD`, `POSTGRES_DB`

Migrations run automatically on API startup via `IDatabaseMigratorService`.

## C# Coding Standards

- Use explicit type declarations instead of `var` (exception: when type is obvious and enhances readability)
- Use primary constructors where appropriate
- Use `new(..)` target-typed syntax: `DatabaseMigratorService service = new(...);`
- Unit tests: xUnit with Arrange/Act/Assert structure
- Use `expected` and `actual` as variable names in test assertions
- Avoid testing implementation details

## Test Project Structure

Tests mirror src structure. `SampleData` project provides shared test fixtures across test projects.

## Commit Message Convention

Use [Conventional Commits](https://www.conventionalcommits.org/) format:

```
<type>(<scope>): <description>

[optional body]
```

**Types:**
- `feat` - New feature
- `fix` - Bug fix
- `docs` - Documentation only
- `refactor` - Code change that neither fixes a bug nor adds a feature
- `test` - Adding or updating tests
- `chore` - Maintenance tasks, dependencies, build config

**Scopes** (optional): `api`, `client`, `domain`, `application`, `infrastructure`, `common`, `shared`

**Examples:**
- `feat(api): add receipt export endpoint`
- `fix(client): correct date picker timezone handling`
- `refactor(infrastructure): simplify repository base class`
- `docs: update agent guidance documentation`

## Object Mapping with Mapperly

This project uses [Mapperly](https://github.com/riok/mapperly) for compile-time object mapping. Mapperly was chosen over AutoMapper for:
- Zero licensing costs (Apache 2.0 vs AutoMapper's commercial license)
- 8.61x faster performance (no reflection)
- Compile-time safety (mapping errors caught during build)
- Debuggable generated code

### Mapperly Patterns

**Basic Mapper Structure (Domain <-> generated DTOs):**
```csharp
[Mapper]
public partial class AccountMapper
{
    [MapperIgnoreTarget(nameof(AccountResponse.AdditionalProperties))]
    public partial AccountResponse ToResponse(Account source);

    public Account ToDomain(CreateAccountRequest source)
    {
        return new Account(Guid.Empty, source.AccountCode, source.Name, source.IsActive);
    }

    public Account ToDomain(UpdateAccountRequest source)
    {
        return new Account(source.Id, source.AccountCode, source.Name, source.IsActive);
    }
}
```

**Value Object Decomposition (Money → decimal + Currency):**
```csharp
[Mapper]
public partial class ReceiptMapper
{
    // Flatten Money value object to separate fields
    [MapProperty(nameof(Receipt.TaxAmount.Amount), nameof(ReceiptEntity.TaxAmount))]
    [MapProperty(nameof(Receipt.TaxAmount.Currency), nameof(ReceiptEntity.TaxAmountCurrency))]
    public partial ReceiptEntity ToEntity(Receipt source);

    // Reconstruct Money value object from separate fields
    private Money MapTaxAmount(decimal amount, Currency currency) => new(amount, currency);

    public partial Receipt ToDomain(ReceiptEntity source);
}
```

**Ignoring Navigation Properties:**
```csharp
[MapperIgnoreTarget(nameof(ReceiptItemEntity.Receipt))]
[MapperIgnoreTarget(nameof(ReceiptItemEntity.ReceiptId))]
public partial ReceiptItemEntity ToEntity(ReceiptItem source);
```

**Aggregate Mappers with Nested Objects:**

When mapping aggregates that contain nested objects with value object decomposition, create manual mapping methods that delegate to the appropriate Core mappers:

```csharp
[Mapper]
public partial class ReceiptWithItemsMapper
{
    private readonly ReceiptMapper _receiptMapper = new();
    private readonly ReceiptItemMapper _receiptItemMapper = new();

    public ReceiptWithItemsResponse ToResponse(ReceiptWithItems source)
    {
        return new ReceiptWithItemsResponse
        {
            Receipt = _receiptMapper.ToResponse(source.Receipt),
            Items = source.Items.Select(_receiptItemMapper.ToResponse).ToList()
        };
    }
}
```

**Note:** Don't use `[UseMapper(typeof(...))]` - it doesn't work as expected. Instead, instantiate mapper dependencies as fields and call them explicitly.

### Testing with Mapperly

Use concrete mapper instances in tests instead of mocks:

```csharp
// GOOD: Use actual mapper
private readonly AccountMapper _mapper = new();
private readonly AccountService _service;

public AccountServiceTests()
{
    _service = new AccountService(_mockRepository.Object, _mapper);
}

// BAD: Don't mock mappers
Mock<IMapper> mapperMock = new();
```

Benefits:
- Tests use actual mapping logic (more realistic)
- No need to set up mock behaviors
- Catches mapping errors in tests
- Simpler test setup

## README Maintenance

The project README (`README.md`) is the primary onboarding document for developers. When making changes, consider whether the README needs updating:

- **New endpoints or controllers**: Update the API Endpoints tables
- **New dependencies or tooling**: Update the Tech Stack table and relevant sections
- **Architecture changes**: Update the Architecture section and directory tree
- **Test count changes**: Update the test count table after adding/removing test classes
- **Build/CI changes**: Update the CI/CD, Pre-commit Hooks, or Development sections
- **New environment variables**: Update the Getting Started configuration table

Keep the README factual and concise. It should reflect the current state of the project, not aspirational future state.

## Library Migration Best Practices

When migrating between libraries (e.g., AutoMapper → Mapperly):

1. **Layer-by-layer approach**: Migrate Infrastructure → API → Tests in sequence
2. **Test at each phase**: Run tests after completing each layer
3. **Update tests alongside production code**: Don't defer test updates to the end
4. **Delete obsolete code only after verification**: Keep old profiles until new mappers are working
5. **Use Task agents for repetitive updates**: Controllers and test files often follow similar patterns
6. **Verify clean build**: After removing old package, ensure no lingering references remain
7. **Document benefits in commit message**: Explain why the migration was necessary
