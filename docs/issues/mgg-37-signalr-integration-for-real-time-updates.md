---
identifier: MGG-37
title: SignalR Integration for Real-time Updates
id: e1cfd129-879b-4234-9eaf-390e0bef0632
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
milestone: "Phase 4 [Frontend]"
url: "https://linear.app/mggarofalo/issue/MGG-37/signalr-integration-for-real-time-updates"
gitBranchName: mggarofalo/mgg-37-signalr-integration-for-real-time-updates
createdAt: "2026-02-11T05:06:42.981Z"
updatedAt: "2026-02-23T03:03:43.146Z"
completedAt: "2026-02-23T03:03:43.130Z"
attachments:
  - title: "feat: SignalR real-time updates integration (MGG-37)"
    url: "https://github.com/mggarofalo/Receipts/pull/23"
---

# SignalR Integration for Real-time Updates

## Objective

Integrate SignalR for real-time data updates across the application.

## Tasks

- [X] Create SignalR connection manager hook (useSignalR)
- [X] Configure connection to `/receipts` hub
- [X] Implement automatic reconnection logic
- [X] Add connection state indicators (connected, disconnected, reconnecting)
- [X] Listen for server events (receipt created/updated/deleted)
- [X] Invalidate TanStack Query caches on SignalR events
- [X] Add toast notifications for real-time changes
- [X] Handle connection errors gracefully
- [X] Implement connection on auth success, disconnect on logout
- [X] Add debug logging for SignalR events (dev mode only)

## Acceptance Criteria

* SignalR connects after successful login
* Real-time updates reflect in UI without refresh
* Connection status visible to user
* Auto-reconnect works after network interruption
* TanStack Query caches updated on SignalR events
