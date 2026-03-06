---
identifier: MGG-231
title: "Improve API test coverage to near 100%"
id: 985afcfc-f77a-42b2-adc9-ee9de93f659b
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
url: "https://linear.app/mggarofalo/issue/MGG-231/improve-api-test-coverage-to-near-100percent"
gitBranchName: mggarofalo/mgg-231-improve-api-test-coverage-to-near-100
createdAt: "2026-03-04T19:20:09.046Z"
updatedAt: "2026-03-04T21:13:39.665Z"
completedAt: "2026-03-04T21:13:39.646Z"
attachments:
  - title: "test(api): add comprehensive controller test coverage (MGG-231)"
    url: "https://github.com/mggarofalo/Receipts/pull/79"
---

# Improve API test coverage to near 100%

## Problem

Backend API test coverage has significant gaps, particularly in the controller/endpoint layer. While the Domain, Application, and Infrastructure layers have strong coverage, many controllers and auth endpoints are untested.

## Current State

**Tested controllers (5/9 core):**

* AccountsController
* AdjustmentsController
* ReceiptItemsController
* ReceiptsController
* TransactionsController

**Missing controller tests:**

* CategoriesController
* ItemTemplatesController
* SubcategoriesController
* TrashController

**Missing endpoint tests:**

* AuthController
* UserRolesController
* UsersController
* ApiKeyController
* HealthController

**Other gaps:**

* SignalR hub tests (ReceiptsHub)
* Integration tests for authentication flows

## Acceptance Criteria

- [ ] All controllers have unit tests covering success and error paths
- [ ] Auth endpoints (login, refresh, password change, admin reset) have tests
- [ ] User management endpoints have tests
- [ ] API key endpoints have tests
- [ ] Health check endpoint has test
- [ ] Trash/recycle bin endpoints have tests
- [ ] SignalR hub has basic tests
- [ ] All new test files follow existing xUnit + Arrange/Act/Assert conventions
- [ ] Target: at or near 100% coverage on non-generated API code
