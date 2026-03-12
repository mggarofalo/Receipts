---
identifier: MGG-188
title: "Theme toggle: context-aware icons (Sun, Moon, Sun+Moon for system)"
id: 6e0f6ce9-cda8-4304-bd34-ca87a86f5f1b
status: Done
priority:
  value: 1
  name: Urgent
createdBy: Michael Garofalo
project: Receipts
team: mggarofalo
labels:
  - frontend
  - Improvement
url: "https://linear.app/mggarofalo/issue/MGG-188/theme-toggle-context-aware-icons-sun-moon-sunmoon-for-system"
gitBranchName: mggarofalo/mgg-188-theme-toggle-context-aware-icons-sun-moon-sunmoon-for-system
createdAt: "2026-02-25T13:58:18.657Z"
updatedAt: "2026-02-25T14:03:19.860Z"
completedAt: "2026-02-25T14:03:19.842Z"
---

# Theme toggle: context-aware icons (Sun, Moon, Sun+Moon for system)

Update the ThemeToggle button icon to reflect the current theme selection:

* **Light**: Sun icon
* **Dark**: Moon icon
* **System**: Both Sun and Moon overlapped, with the currently-inactive icon dimmed (e.g., if system resolves to dark, Sun is at 40% opacity)

Currently the toggle uses a CSS dark: variant trick that only distinguishes light vs dark, with no visual distinction for "system" mode.
