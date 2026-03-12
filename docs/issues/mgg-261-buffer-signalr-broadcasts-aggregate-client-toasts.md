---
identifier: MGG-261
title: "Buffer SignalR broadcasts & aggregate client toasts"
id: e9eac945-4e6e-478a-b8ea-74d81d8458a6
status: Done
priority:
  value: 2
  name: High
assignee: Michael Garofalo
createdBy: Michael Garofalo
project: Receipts
team: mggarofalo
labels:
  - frontend
  - backend
  - Improvement
milestone: "Phase 4 [Frontend]"
url: "https://linear.app/mggarofalo/issue/MGG-261/buffer-signalr-broadcasts-and-aggregate-client-toasts"
gitBranchName: mggarofalo/mgg-261-buffer-signalr-broadcasts-aggregate-client-toasts
createdAt: "2026-03-06T12:43:51.982Z"
updatedAt: "2026-03-06T12:50:49.632Z"
completedAt: "2026-03-06T12:50:49.612Z"
attachments:
  - title: "feat(api,client): buffer SignalR broadcasts and aggregate client toasts (MGG-261)"
    url: "https://github.com/mggarofalo/Receipts/pull/98"
---

# Buffer SignalR broadcasts & aggregate client toasts

Bulk operations (e.g., deleting 20 transactions) trigger `NotifyBulkChanged` which loops through each ID and fires a separate `EntityChanged` SignalR message per entity. On the client, each message spawns an individual `toast.info()` call, flooding the screen with identical toasts.\\n\\n## Solution\\n\\nTwo-layer fix:\\n\\n### Server: Buffered EntityChangeNotifier\\n- Accumulate notifications in a `ConcurrentDictionary<(entityType, changeType), NotificationBucket>`\\n- 1-second periodic `Timer` flushes all pending buckets\\n- Each flush sends **one** `EntityChanged` per unique `(entityType, changeType)` pair with aggregated `Count` field\\n- `EntityChangeNotification` record now includes `Count` property (default 1)\\n- DI registration changed from `Scoped` → `Singleton`\\n\\n### Client: Toast Aggregation\\n- New `signalr-toast-buffer.ts` module accumulates by `(entityType, changeType)` key\\n- 5-second flush window shows aggregated toast: "3 receipts were deleted by another user"\\n- Query invalidation remains immediate (no delay)\\n- Handles pluralization (category → categories)\\n\\n## Checklist\\n- \[x\] Add `Count` property to `EntityChangeNotification` record\\n- \[x\] Rewrite `EntityChangeNotifier` as buffered singleton with `ConcurrentDictionary` + `Timer`\\n- \[x\] Change DI registration from `Scoped` → `Singleton` in `ProgramService.cs`\\n- \[x\] Create `signalr-toast-buffer.ts` client module\\n- \[x\] Update `useSignalR.ts` to use toast buffer instead of direct `toast.info()`\\n- \[x\] Server tests: `EntityChangeNotifierTests.cs` rewritten for buffered behavior\\n- \[x\] Client tests: `signalr-toast-buffer.test.ts` (new) + `useSignalR.test.ts` (updated)\\n- \[x\] All tests pass (955 backend, 826 client)
