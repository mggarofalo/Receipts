---
identifier: MGG-36
title: Generate TypeScript types and API client from OpenAPI spec
id: 87b43322-f3f9-410d-9159-e0e9bf905a5c
status: Done
priority:
  value: 3
  name: Medium
createdBy: Michael Garofalo
project: Receipts
team: mggarofalo
labels:
  - codegen
  - frontend
milestone: "Phase 4 [Frontend]"
url: "https://linear.app/mggarofalo/issue/MGG-36/generate-typescript-types-and-api-client-from-openapi-spec"
gitBranchName: mggarofalo/mgg-36-generate-typescript-types-and-api-client-from-openapi-spec
createdAt: "2026-02-11T05:06:37.834Z"
updatedAt: "2026-02-21T15:03:32.791Z"
completedAt: "2026-02-21T15:03:32.774Z"
---

# Generate TypeScript types and API client from OpenAPI spec

## Objective

Generate type-safe TypeScript types and an API client from the canonical OpenAPI spec (MGG-21). Both the .NET DTOs (MGG-88) and these TypeScript types are generated from the **same spec**, making it impossible for the frontend and backend contracts to drift.

## Technology Stack

| Purpose | Package | Version | Install |
| -- | -- | -- | -- |
| TypeScript type generation | `openapi-typescript` | 7.12.0+ | `npm install -D openapi-typescript` |
| Type-safe fetch client | `openapi-fetch` | 0.13.0+ | `npm install openapi-fetch` |
| TanStack Query integration | `@tanstack/react-query` | 5.x | `npm install @tanstack/react-query` |

## Type Generation Setup

### Install

```bash
cd src/client
npm install -D openapi-typescript
npm install openapi-fetch
```

### Add generation script to `package.json`

```json
{
  "scripts": {
    "generate:types": "openapi-typescript ../../openapi/spec.yaml -o src/generated/api.d.ts",
    "prebuild": "npm run generate:types",
    "predev": "npm run generate:types"
  }
}
```

### CLI command (manual run)

```bash
npx openapi-typescript ../../openapi/spec.yaml -o src/generated/api.d.ts
```

### Generated output (`src/generated/api.d.ts`)

This generates a `paths` type that describes every endpoint, plus `components` for schemas:

```typescript
export interface paths {
  "/api/accounts": {
    get: {
      responses: { 200: { content: { "application/json": components["schemas"]["AccountResponse"][] } } };
    };
    post: {
      requestBody: { content: { "application/json": components["schemas"]["CreateAccountRequest"] } };
      responses: { 201: { content: { "application/json": components["schemas"]["AccountResponse"] } } };
    };
  };
  // ...
}

export interface components {
  schemas: {
    AccountResponse: { id: string; name: string; /* ... */ };
    CreateAccountRequest: { name: string; /* ... */ };
    UpdateAccountRequest: { id: string; name: string; /* ... */ };
    // ...
  };
}
```

### openapi-typescript v7 flags

```bash
npx openapi-typescript spec.yaml -o api.d.ts \
  --enum                    # generate real TS enums (not string unions) \
  --export-type             # use 'export type' for type-only exports \
  --default-non-nullable    # enabled by default in v7
```

## API Client Setup

### Create typed client (`src/lib/api-client.ts`)

```typescript
import createClient from "openapi-fetch";
import type { paths } from "@/generated/api";

export const api = createClient<paths>({
  baseUrl: import.meta.env.VITE_API_URL ?? "http://localhost:5000",
});
```

### TanStack Query hooks (`src/hooks/useAccounts.ts`)

```typescript
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { api } from "@/lib/api-client";

export function useAccounts() {
  return useQuery({
    queryKey: ["accounts"],
    queryFn: async () => {
      const { data, error } = await api.GET("/api/accounts");
      if (error) throw error;
      return data;
    },
  });
}

export function useCreateAccount() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (body: { name: string }) => {
      const { data, error } = await api.POST("/api/accounts", { body });
      if (error) throw error;
      return data;
    },
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["accounts"] }),
  });
}
```

**Type safety is automatic** — `api.GET("/api/accounts")` infers the return type from the `paths` interface. Passing the wrong body to `api.POST` is a compile error. No hand-written types needed.

## Pre-commit Hook Integration

The pre-commit hook (defined in [MGG-21](./mgg-21-establish-openapi-spec-as-authoritative-api-contract.md)) handles TypeScript type validation:

```bash
# Step 3 of .husky/pre-commit:
# Regenerate TS types and check for staleness
npx openapi-typescript openapi/spec.yaml -o src/client/src/generated/api.d.ts
git diff --exit-code -- "src/client/src/generated/" \
  || (echo "ERROR: Generated TypeScript types are stale. Run 'npm run generate:types' and stage the changes." && exit 1)
```

**What this catches:**

* Developer edits `spec.yaml` but forgets to regenerate TS types → stale check fails
* Developer hand-edits `api.d.ts` → regeneration overwrites, stale check fails
* New endpoint added to spec → types auto-update, hooks need implementation (caught in code review)

## Generated Files Policy

**Recommendation: commit generated files.** Rationale:

* CI doesn't need to run generation (faster builds)
* Diffs on generated files in PRs act as a visual "contract change review"
* Pre-commit hook ensures they're never stale
* Add header comment to generated file: `// THIS FILE IS AUTO-GENERATED — DO NOT EDIT`

Alternative: `.gitignore` generated files and regenerate in CI. Tradeoff: no contract review in PRs, but zero noise in diffs.

## Tasks

### Type Generation

- [ ] Install `openapi-typescript` 7.12.0+ as dev dependency
- [ ] Add `generate:types` script to `package.json`
- [ ] Add `prebuild` and `predev` hooks to auto-regenerate
- [ ] Generate types into `src/generated/api.d.ts`
- [ ] Add `--enum` flag if API uses enums (e.g., Currency)
- [ ] Verify generated types match the spec schemas

### API Client

- [ ] Install `openapi-fetch` 0.13.0+
- [ ] Create `src/lib/api-client.ts` with `createClient<paths>()`
- [ ] Configure base URL from `VITE_API_URL` environment variable
- [ ] Add auth token interceptor (when [MGG-35](./mgg-35-frontend-authentication-ui-protected-routes.md) is implemented)

### TanStack Query Hooks

- [ ] Create custom hooks per entity module:
  - `useAccounts`, `useAccount`, `useCreateAccount`, `useUpdateAccount`, `useDeleteAccount`
  - `useReceipts`, `useReceipt`, `useCreateReceipt`, `useUpdateReceipt`, `useDeleteReceipt`
  - `useReceiptItems`, `useCreateReceiptItem`, etc.
  - `useTransactions`, `useCreateTransaction`, etc.
  - `useTrips`, `useCreateTrip`, etc.
  - `useReceiptWithItems`, `useTransactionAccounts` (aggregates)
- [ ] Add query invalidation on mutations
- [ ] Implement optimistic updates for better UX
- [ ] Add retry logic for failed requests
- [ ] Create loading and error states for all queries

### Error Handling

- [ ] Create typed error handler for API responses
- [ ] Handle 401 → redirect to login
- [ ] Handle 403 → show forbidden message
- [ ] Handle 500 → show generic error with retry
- [ ] Handle network errors → show offline state

### Validation

- [ ] Verify generated types match .NET DTOs (both from same spec)
- [ ] Pre-commit hook validates staleness (via [MGG-21](./mgg-21-establish-openapi-spec-as-authoritative-api-contract.md) hook pipeline)

## Dependencies

* **Blocked by**: [MGG-21](./mgg-21-establish-openapi-spec-as-authoritative-api-contract.md) (OpenAPI spec + pre-commit hooks must exist first)
* **Blocked by** (implicit): [MGG-33](./mgg-33-project-setup-react-vite-typescript-shadcn-ui.md) (React project must be scaffolded to have a `package.json`)
* **Blocks**: [MGG-38](./mgg-38-receipts-module-full-crud-with-search-filters.md) through [MGG-43](./mgg-43-aggregate-views-receiptwithitems-transactionaccount.md) (all frontend UI modules need the API client)

## Acceptance Criteria

- [ ] TypeScript types auto-generated from `openapi/spec.yaml` (not hand-written)
- [ ] `openapi-fetch` client provides full type safety (autocomplete, compile errors on mismatch)
- [ ] `npm run generate:types` regenerates types from spec
- [ ] `prebuild`/`predev` hooks ensure types are always fresh
- [ ] Pre-commit stale check passes (generated files match spec)
- [ ] TanStack Query hooks work with proper caching
- [ ] Mutations trigger query invalidation
- [ ] Network errors handled gracefully
