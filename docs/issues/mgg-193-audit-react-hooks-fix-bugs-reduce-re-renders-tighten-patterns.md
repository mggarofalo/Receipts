---
identifier: MGG-193
title: "Audit React hooks: fix bugs, reduce re-renders, tighten patterns"
id: b19a1dc6-335c-4d51-8fe3-b87f1559745a
status: Done
priority:
  value: 4
  name: Low
assignee: Michael Garofalo
createdBy: Michael Garofalo
project: Receipts
team: mggarofalo
labels:
  - cleanup
  - frontend
url: "https://linear.app/mggarofalo/issue/MGG-193/audit-react-hooks-fix-bugs-reduce-re-renders-tighten-patterns"
gitBranchName: mggarofalo/mgg-193-audit-react-hooks-fix-bugs-reduce-re-renders-tighten
createdAt: "2026-02-26T03:19:06.979Z"
updatedAt: "2026-03-03T12:10:45.788Z"
completedAt: "2026-03-03T12:10:45.766Z"
attachments:
  - title: "fix(client): fix hook bugs and reduce unnecessary re-renders (MGG-193)"
    url: "https://github.com/mggarofalo/Receipts/pull/57"
---

# Audit React hooks: fix bugs, reduce re-renders, tighten patterns

## Summary

Full audit of custom hooks, `useEffect`, `useMemo`, and `useCallback` usage across `src/client/src`. No memory leaks found, but there's one bug and several optimization opportunities.

---

## Bug (High Priority)

- [ ] `ApiKeys.tsx` \~line 149-155 — `useEffect` missing dependency array
  The `useEffect` that adds a `"shortcut:new-item"` window listener has **no dependency array**, so the listener is added and removed on every render. All four other pages (`Accounts`, `Receipts`, `ReceiptItems`, `Transactions`) correctly pass `[]`. Add `[]` here.

## Anti-Pattern (Medium Priority)

- [ ] `useListKeyboardNav.ts` \~lines 32-35 — state update during render
  `setPrevItemsLength(items.length)` runs during the render body (not inside `useEffect`). This is a React anti-pattern that can cause extra re-render cycles. Move into a `useEffect` or use a ref.

## Re-render Optimizations (Low Priority)

- [ ] **Inline** `onSaveFilter` callbacks — In `Transactions.tsx`, `Receipts.tsx`, `ReceiptItems.tsx`, and `Accounts.tsx`, the `onSaveFilter` prop passed to `<FilterPanel>` creates a new object + arrow function every render. Wrapping in `useCallback` would stabilize the reference and prevent `FilterPanel` re-renders.
- [ ] `useGlobalShortcuts.ts` \~line 11-14 — The `useCallback` wrapping the help-toggle may be unnecessary since `useKeyboardShortcut` doesn't hold a stable reference. Verify and simplify if so. Also add a null guard on `ctx` before accessing `ctx.helpOpen`/`ctx.setHelpOpen`.
- [ ] **Trivial** `useMemo` in `Accounts.tsx` \~line 100-103 — Memoizes a simple type cast / fallback (`results as AccountResponse[] ?? []`). This isn't expensive and could be inlined.

## Overall Assessment

| Category | Count | Verdict |
| -- | -- | -- |
| Custom hooks | 23 | Well-structured, all appropriately use hook APIs |
| `useEffect` | 11 | 1 bug (missing deps), rest correct with proper cleanup |
| `useMemo` | 21 | All correct, 1 trivially unnecessary |
| `useCallback` | 10 | All correct, 1 possibly unnecessary |
| Memory leak risks | 0 | All subscriptions/timers properly cleaned up |
| React Query usage | All data hooks | Correct — no manual fetch/abort needed |

