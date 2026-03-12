---
identifier: MGG-230
title: "Fix OpenAPI spec drift: remove 500 status codes not present in generated output"
id: a3716983-618b-4ada-b521-53c706b18a91
status: Done
priority:
  value: 2
  name: High
assignee: Michael Garofalo
createdBy: Michael Garofalo
project: Receipts
team: mggarofalo
labels:
  - backend
  - Bug
url: "https://linear.app/mggarofalo/issue/MGG-230/fix-openapi-spec-drift-remove-500-status-codes-not-present-in"
gitBranchName: mggarofalo/mgg-230-fix-openapi-spec-drift-remove-500-status-codes-not-present
createdAt: "2026-03-04T18:56:21.465Z"
updatedAt: "2026-03-04T19:29:15.178Z"
completedAt: "2026-03-04T19:29:15.162Z"
attachments:
  - title: "fix(openapi): remove 500 status codes causing spec drift (MGG-230)"
    url: "https://github.com/mggarofalo/Receipts/pull/75"
---

# Fix OpenAPI spec drift: remove 500 status codes not present in generated output

## Context

The pre-commit drift check (`scripts/check-drift.mjs`) reports 93 differences: every endpoint in `openapi/spec.yaml` declares a `500` response status that does not appear in the generated `openapi/generated/API.json`.

This means the spec manually added `500` responses that the API framework doesn't produce via `[ProducesResponseType]` or equivalent metadata. The drift check blocks commits.

## Goal

Remove the `500` response declarations from all endpoints in `openapi/spec.yaml` so the spec matches the generated output, or add the corresponding `[ProducesResponseType(StatusCodes.Status500InternalServerError)]` attributes to controllers if 500 responses should be documented.

## Affected Endpoints

All 93 endpoints across every controller — accounts, categories, subcategories, item-templates, receipts, receipt-items, transactions, adjustments, trash, aggregates, audit, auth, apikeys, users, and user-roles.

## Acceptance Criteria

* `npm run check:drift` passes with zero differences
* Pre-commit hook completes successfully
* No behavioral changes to the API
