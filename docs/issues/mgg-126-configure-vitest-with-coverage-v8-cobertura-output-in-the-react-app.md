---
identifier: MGG-126
title: Configure Vitest with coverage (v8 + Cobertura output) in the React app
id: c30f0ff3-b51d-4cb3-a0f0-934d0a094de3
status: Done
priority:
  value: 3
  name: Medium
createdBy: Michael Garofalo
project: Receipts
team: mggarofalo
labels:
  - testing
  - dx
  - frontend
milestone: "Phase 5: Test Coverage"
url: "https://linear.app/mggarofalo/issue/MGG-126/configure-vitest-with-coverage-v8-cobertura-output-in-the-react-app"
gitBranchName: mggarofalo/mgg-126-configure-vitest-with-coverage-v8-cobertura-output-in-the
createdAt: "2026-02-18T02:17:07.278Z"
updatedAt: "2026-02-27T13:46:26.793Z"
completedAt: "2026-02-27T13:46:26.770Z"
---

# Configure Vitest with coverage (v8 + Cobertura output) in the React app

Set up Vitest as the test runner and coverage collector for the React/Vite frontend. Vitest is the correct choice for Vite projects — it shares the Vite config, has a Jest-compatible API, and natively outputs Cobertura XML for CI consumption.

**Note:** Test data factories from [MGG-49](./mgg-49-testing-suite-unit-integration-e2e-tests.md) (`src/test/factories.ts`) are available for reuse. Playwright E2E infrastructure is also in place.

## Why Vitest over Jest

* Shares `vite.config.ts` — no separate babel/transform config needed
* Jest-compatible API (`describe`, `it`, `expect`, `vi.fn()` = `jest.fn()`)
* `@vitest/coverage-v8` uses Node's built-in V8 coverage — fast, no extra instrumentation
* Outputs Cobertura XML out of the box, compatible with [MGG-123](./mgg-123-publish-code-coverage-report-to-github-via-ci.md)'s CI workflow

## Acceptance Criteria

* Install deps: `vitest`, `@vitest/coverage-v8`, `@testing-library/react`, `@testing-library/user-event`, `jsdom`
* Add `test` and `coverage` scripts to `package.json`:
  * `"test": "vitest run"`
  * `"test:watch": "vitest"`
  * `"coverage": "vitest run --coverage"`
* Configure in `vite.config.ts` (or `vitest.config.ts`):

  ```ts
  test: {
    environment: 'jsdom',
    coverage: {
      provider: 'v8',
      reporter: ['text', 'cobertura'],
      reportsDirectory: 'coverage',
      exclude: ['src/generated/**', 'src/**/*.d.ts']
    }
  }
  ```
* Running `npm run coverage` produces `coverage/cobertura-coverage.xml`
* Exclude generated API client code from coverage (the NSwag/OpenAPI-generated TypeScript client)
* Add a smoke test (e.g., renders the root `App` component) to confirm the setup works end-to-end

## Notes

* Requires the React app to exist (MGG-33)
* Output path `coverage/cobertura-coverage.xml` must match what [MGG-123](./mgg-123-publish-code-coverage-report-to-github-via-ci.md)'s CI step expects for the frontend artifact
* Reuse test data factories from [MGG-49](./mgg-49-testing-suite-unit-integration-e2e-tests.md) (`src/test/factories.ts`)
