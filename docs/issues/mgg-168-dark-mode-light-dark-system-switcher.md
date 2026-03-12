---
identifier: MGG-168
title: Dark Mode (Light/Dark/System Switcher)
id: d4fc8bc3-3d98-429e-b8c9-d0264c524cd5
status: Done
priority:
  value: 3
  name: Medium
createdBy: Michael Garofalo
project: Receipts
team: mggarofalo
labels:
  - frontend
  - Feature
milestone: "Phase 4 [Frontend]"
url: "https://linear.app/mggarofalo/issue/MGG-168/dark-mode-lightdarksystem-switcher"
gitBranchName: mggarofalo/mgg-168-dark-mode-lightdarksystem-switcher
createdAt: "2026-02-24T11:51:15.109Z"
updatedAt: "2026-02-25T13:58:33.363Z"
completedAt: "2026-02-25T13:25:59.488Z"
---

# Dark Mode (Light/Dark/System Switcher)

## Objective

Add a theme switcher supporting Light, Dark, and System (auto) modes across the entire application.

## Tasks

- [ ] Add theme provider (React context) that reads/writes preference to localStorage
- [ ] Implement Light / Dark / System toggle component in the header/nav area
- [ ] Configure Tailwind CSS dark mode (`class` strategy) for theme switching
- [ ] Ensure all shadcn/ui components respect dark mode (they use CSS variables by default)
- [ ] Audit custom components for hardcoded colors and replace with theme-aware tokens
- [ ] System mode: listen to `prefers-color-scheme` media query and auto-switch
- [ ] Persist user preference across sessions (localStorage)
- [ ] Ensure smooth transition when switching themes (no flash of wrong theme on load)

## Acceptance Criteria

* Three-way toggle: Light / Dark / System
* System mode tracks OS preference and updates in real-time
* Preference persists across page reloads and sessions
* No flash of unstyled/wrong-theme content on initial load
* All pages and components render correctly in both themes
