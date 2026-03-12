---
identifier: MGG-221
title: "Replace It.IsAny<Guid>() with specific values in unit tests and add testing guidance"
id: c23cd569-54f3-4ead-a9b5-67c15f62ac5b
status: Done
priority:
  value: 3
  name: Medium
assignee: Michael Garofalo
createdBy: Michael Garofalo
project: Receipts
team: mggarofalo
labels:
  - cleanup
  - backend
url: "https://linear.app/mggarofalo/issue/MGG-221/replace-itisanyguid-with-specific-values-in-unit-tests-and-add-testing"
gitBranchName: mggarofalo/mgg-221-replace-itisanyguid-with-specific-values-in-unit-tests-and
createdAt: "2026-03-03T21:20:58.993Z"
updatedAt: "2026-03-03T21:52:58.074Z"
completedAt: "2026-03-03T21:43:17.586Z"
attachments:
  - title: "Replace It.IsAny<T>() with specific values in unit tests (MGG-221)"
    url: "https://github.com/mggarofalo/Receipts/pull/67"
---

# Replace It.IsAny<Guid>() with specific values in unit tests and add testing guidance

## Summary

Audit all unit tests for overuse of `It.IsAny<T>()` in Moq setups where the value **does** matter for correctness. Replace with specific expected values to catch argument-ordering bugs.

This was identified during PR #65 review where `CreateTransactionCommandHandler` and `UpdateTransactionCommandHandler` passed `receiptId` and `accountId` in the wrong order to `ITransactionService.CreateAsync`/`UpdateAsync`. The bug was undetected because tests used `It.IsAny<Guid>()` for both parameters.

## Acceptance criteria

- [ ] Audit all `It.IsAny<Guid>()` usages across handler tests — replace with specific expected values where the argument identity matters
- [ ] Focus on `UpdateTransactionCommandHandlerTests` and `CreateTransactionCommandHandlerTests` first (these masked the swapped-arg bug)
- [ ] Add a section to [AGENTS.md](<http://AGENTS.md>) testing guidance: "`It.IsAny<T>()` should only be used when the value genuinely doesn't matter for the test's assertion (e.g., `CancellationToken`). For domain-meaningful parameters like IDs, use specific values to catch argument-ordering bugs."
- [ ] Consider adding a Roslyn analyzer or editorconfig rule to flag `It.IsAny<Guid>()` as a warning

## Context

* PR #65 had `CreateAsync([.. transactions], request.AccountId, request.ReceiptId, ct)` — args were swapped
* `ITransactionService.CreateAsync(models, receiptId, accountId, ct)` — positional Guid params
* Tests passed because both Guid params used `It.IsAny<Guid>()`
* Static analysis (grep/regex) cannot reliably detect same-type argument swaps; only semantic-level checks (Roslyn) or value-specific test assertions can catch these
