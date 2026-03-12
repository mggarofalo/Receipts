---
identifier: MGG-236
title: Add timeout warning state to loading buttons and respect API abort signals
id: 251e42f2-ce7d-4a2e-a3bb-41a8ed078ac0
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
  - Feature
url: "https://linear.app/mggarofalo/issue/MGG-236/add-timeout-warning-state-to-loading-buttons-and-respect-api-abort"
gitBranchName: mggarofalo/mgg-236-add-timeout-warning-state-to-loading-buttons-and-respect-api
createdAt: "2026-03-05T11:37:42.869Z"
updatedAt: "2026-03-05T16:22:35.369Z"
completedAt: "2026-03-05T16:22:35.347Z"
attachments:
  - title: "feat(client): add timeout warning to loading buttons (MGG-236)"
    url: "https://github.com/mggarofalo/Receipts/pull/88"
---

# Add timeout warning state to loading buttons and respect API abort signals

## Summary

All buttons that have a loading/spinner state should show a warning indicator after 5 seconds of waiting, signaling to the user that something may be wrong. Additionally, all loading states must receive and respect API timeout abort signals so they stop spinning when the request is actually dead.

## Requirements

- [ ] After 5 seconds of loading, show a warning icon (e.g., ⚠️ or an alert triangle icon) on/near the button
- [ ] Optionally update the button text (e.g., "Still working..." or "This is taking longer than expected")
- [ ] All loading states must listen for `AbortSignal` timeout/abort events and transition out of the loading state when the signal fires
- [ ] On abort/timeout, show a clear error message to the user with a retry option
- [ ] Applies to: Login, ChangePassword, and any other forms with submit loading states

## Design Notes

* Warning icon should be visually distinct but not alarming — a yellow/amber caution icon is appropriate
* The 5s threshold is a UX heuristic; the actual API timeout (30s) is the hard backstop
* Consider a shared hook or wrapper component to avoid duplicating this logic across every button
