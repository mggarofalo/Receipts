---
identifier: MGG-45
title: "Keyboard Navigation & Shortcuts System"
id: 9e943014-9c6d-426d-b591-e562420ecfdb
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
  - Feature
milestone: "Phase 4 [Frontend]"
url: "https://linear.app/mggarofalo/issue/MGG-45/keyboard-navigation-and-shortcuts-system"
gitBranchName: mggarofalo/mgg-45-keyboard-navigation-shortcuts-system
createdAt: "2026-02-11T05:07:17.796Z"
updatedAt: "2026-02-25T10:48:36.102Z"
completedAt: "2026-02-25T10:48:36.084Z"
attachments:
  - title: "feat(client): keyboard navigation & shortcuts system (MGG-45)"
    url: "https://github.com/mggarofalo/Receipts/pull/26"
---

# Keyboard Navigation & Shortcuts System

## Objective

Implement comprehensive keyboard navigation and shortcuts throughout the app.

## Tasks

- [ ] Create keyboard shortcut manager (react-hotkeys-hook or similar)
- [ ] Implement global shortcuts:
  - Cmd/Ctrl+K: Global search
  - Cmd/Ctrl+Shift+N: New item (context-aware)
  - ?: Show keyboard shortcuts help
  - Esc: Close modals/cancel actions
- [ ] Add context-specific shortcuts:
  - Lists: j/k + uparrow/downarrow (up/down), Enter (open), Del (delete), Cmd/Ctrl+A (select all)
  - Forms: Cmd/Ctrl+Enter (submit), Esc (cancel)
  - Modals: Tab (focus trap), Esc (close)
- [ ] Create ShortcutsHelp modal (triggered by ?)
- [ ] Implement focus management (auto-focus inputs, return focus on modal close)
- [ ] Add visual focus indicators (keyboard users)
- [ ] Create breadcrumb navigation with keyboard support

## Acceptance Criteria

* All major actions accessible via keyboard
* Focus management works correctly
* Visual indicators for keyboard navigation
* Help modal shows all available shortcuts
* No keyboard traps
* All listeners are handled safely and no listeners are added duplicatively (memory leak risk)
