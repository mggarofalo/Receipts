# Testing

## .NET Backend

### Running Tests

```bash
# All tests
dotnet test Receipts.slnx

# Single project
dotnet test tests/Application.Tests/Application.Tests.csproj

# Single test by name
dotnet test --filter "FullyQualifiedName~TestMethodName"
```

### Code Coverage

Coverage is collected via [coverlet](https://github.com/coverlet-coverage/coverlet) using the `XPlat Code Coverage` data collector and output in Cobertura XML format.

```bash
# Run tests with coverage
dotnet test Receipts.slnx --collect:"XPlat Code Coverage" --settings scripts/tests/coverlet.runsettings --results-directory TestResults

# Merge reports and generate HTML (requires dotnet-reportgenerator-globaltool)
reportgenerator -reports:TestResults/**/coverage.cobertura.xml -targetdir:coveragereport -reporttypes:Html
```

### Coverage Exclusions

Configured in `scripts/tests/coverlet.runsettings`:

| Exclusion | Reason |
|-----------|--------|
| `[*]*.Migrations.*` | EF Core migration classes are auto-generated |
| `**/Infrastructure/Migrations/*.cs` | Migration source files |
| `**/API/Generated/*.g.cs` | NSwag-generated DTOs from OpenAPI spec |
| `GeneratedCodeAttribute` | Any code marked with `[GeneratedCode]` (Mapperly, source generators) |
| `CompilerGeneratedAttribute` | Compiler-generated code (async state machines, lambdas, etc.) |
| Auto-properties | Skipped via `<SkipAutoProps>true</SkipAutoProps>` |
| Test assemblies | Excluded via `<IncludeTestAssembly>false</IncludeTestAssembly>` |

### Test Conventions

- **Framework:** xUnit
- **Mocking:** Moq
- **Assertions:** FluentAssertions
- **Naming:** `MethodName_Condition_ExpectedResult`
- **Structure:** Arrange / Act / Assert
- **Mappers:** Use concrete instances, never mock Mapperly mappers

## React Frontend

### Running Tests

```bash
# Run all tests (from src/client/)
npx vitest run

# Watch mode
npx vitest

# Run with coverage
npx vitest run --coverage
```

### Code Coverage

Coverage is collected via Vitest's built-in `@vitest/coverage-v8` provider, output in Cobertura XML, HTML, and text formats.

```bash
# Generate coverage report
npm run coverage

# Output: src/client/coverage/cobertura-coverage.xml (for CI)
# Output: src/client/coverage/ (HTML report, open index.html)
```

### Coverage Exclusions

Configured in `src/client/vite.config.ts` under `test.coverage.exclude`:

| Exclusion | Reason |
|-----------|--------|
| `src/generated/**` | OpenAPI-generated TypeScript types |
| `*.d.ts` | Type declaration files |
| `src/test/**` | Test setup and utilities |
| `src/main.tsx` | Application entry point (side-effect only) |

### Test Conventions

- **Framework:** Vitest (Jest-compatible API)
- **Component testing:** React Testing Library
- **User interactions:** `@testing-library/user-event`
- **DOM matchers:** `@testing-library/jest-dom`
- **Environment:** jsdom
- **No snapshot tests** (brittle, low coverage signal)
- Test files: `*.test.ts` / `*.test.tsx` colocated with source

## CI Coverage Reporting

Both stacks report coverage on every PR via GitHub Actions (`.github/workflows/github-ci.yml`):

- **Backend:** `irongut/CodeCoverageSummary` parses merged Cobertura XML, posts as a sticky PR comment (`coverage-backend` header)
- **Frontend:** Same action parses `coverage/cobertura-coverage.xml`, posts as a separate sticky PR comment (`coverage-frontend` header)

## Coverage Thresholds

Both stacks enforce minimum coverage as CI required status checks on `main`. PRs that drop coverage below these thresholds will fail CI.

| Stack | Line % | Branch % | Configured In |
|-------|--------|----------|---------------|
| Backend (.NET) | 70% | 65% | `irongut/CodeCoverageSummary` in `build` job |
| Frontend (React) | 80% | 80% | `irongut/CodeCoverageSummary` in `frontend-test` job |

Thresholds are set slightly below measured coverage to allow minor fluctuations. Raise them incrementally as test coverage improves.
