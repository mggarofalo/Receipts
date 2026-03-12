---
identifier: MGG-165
title: Manual WCAG 2.1 AA verification (post MGG-46)
id: 27418d53-e136-47cc-911d-38794c1dbafb
status: Done
priority:
  value: 4
  name: Low
createdBy: Michael Garofalo
project: Receipts
team: mggarofalo
labels:
  - testing
  - frontend
url: "https://linear.app/mggarofalo/issue/MGG-165/manual-wcag-21-aa-verification-post-mgg-46"
gitBranchName: mggarofalo/mgg-165-manual-wcag-21-aa-verification-post-mgg-46
createdAt: "2026-02-24T02:10:24.270Z"
updatedAt: "2026-02-25T13:08:13.714Z"
completedAt: "2026-02-25T13:08:13.701Z"
---

# Manual WCAG 2.1 AA verification (post [MGG-46](./mgg-46-accessibility-audit-wcag-2-1-aa-compliance.md))

## Follow-up from [MGG-46](./mgg-46-accessibility-audit-wcag-2-1-aa-compliance.md)

[MGG-46](./mgg-46-accessibility-audit-wcag-2-1-aa-compliance.md) implemented all automatable WCAG 2.1 AA compliance items. The following require manual human testing with a browser:

- [ ] Run axe DevTools audit on every route — install axe DevTools browser extension
- [ ] Verify color contrast ratios — key concern: `--muted-foreground` (oklch 0.556) on white is \~4.5:1, borderline. If axe flags it, darken to `oklch(0.50 0 0)`
- [ ] Test with screen reader (NVDA/VoiceOver) — navigate full app
- [ ] Test browser zoom at 200% and 400% — check for content overflow/overlap
- [ ] Keyboard-only navigation — overlaps with [MGG-45](./mgg-45-keyboard-navigation-shortcuts-system.md) scope, but verify tab order is logical

If any issues are found, fix them in this issue or create additional issues as needed.
