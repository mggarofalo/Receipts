---
identifier: MGG-175
title: SignalR hub not broadcasting entity changes to other clients
id: 0c411e4e-fb8b-42e5-a25a-67828254160e
status: Done
priority:
  value: 3
  name: Medium
assignee: Michael Garofalo
createdBy: Michael Garofalo
project: Receipts
team: mggarofalo
labels:
  - frontend
  - backend
  - Bug
url: "https://linear.app/mggarofalo/issue/MGG-175/signalr-hub-not-broadcasting-entity-changes-to-other-clients"
gitBranchName: mggarofalo/mgg-175-signalr-hub-not-broadcasting-entity-changes-to-other-clients
createdAt: "2026-02-24T13:15:15.764Z"
updatedAt: "2026-03-04T18:58:06.717Z"
completedAt: "2026-03-04T18:58:06.697Z"
attachments:
  - title: "feat(api): generic SignalR entity change notifications (MGG-175)"
    url: "https://github.com/mggarofalo/Receipts/pull/73"
---

# SignalR hub not broadcasting entity changes to other clients

## Bug

When an entity is created/updated/deleted in one browser session, other connected browser sessions don't receive real-time updates. The SignalR "Live" indicator is green (connected), but the hub isn't broadcasting change events to all clients — only the originating client updates its UI.

## Reproduction

1. Open the app in two browser tabs/windows (both logged in)
2. Navigate to Accounts in both
3. In tab 1, create a new account
4. Expected: Tab 2 automatically shows the new account
5. Actual: Tab 2 doesn't update until manually refreshed

## Context

This is a single-tenant app serving multiple users. All connected clients should receive entity change notifications so their rendered components stay in sync. The SignalR hub is connected and the "Live" indicator works, but entity mutation events are not being broadcast to other clients.

## Notes

* The hub connection appears healthy (WebSocket connected, handshake complete)
* Console shows a transient race condition on mount ("connection was stopped during negotiation") which is likely React StrictMode double-mount — probably unrelated but worth investigating
* This may be a regression or an incomplete implementation
