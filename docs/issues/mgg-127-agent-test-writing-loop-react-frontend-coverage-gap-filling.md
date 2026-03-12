---
identifier: MGG-127
title: "Agent test-writing loop: React frontend coverage gap filling"
id: 073033c0-e899-4229-aed0-a151611dd575
status: Done
priority:
  value: 3
  name: Medium
createdBy: Michael Garofalo
project: Receipts
team: mggarofalo
labels:
  - testing
  - dx
  - frontend
milestone: "Phase 5: Test Coverage"
url: "https://linear.app/mggarofalo/issue/MGG-127/agent-test-writing-loop-react-frontend-coverage-gap-filling"
gitBranchName: mggarofalo/mgg-127-agent-test-writing-loop-react-frontend-coverage-gap-filling
createdAt: "2026-02-18T02:17:30.967Z"
updatedAt: "2026-02-27T13:53:38.190Z"
completedAt: "2026-02-27T13:53:38.175Z"
---

# Agent test-writing loop: React frontend coverage gap filling

Use an AI agent loop to write Vitest + React Testing Library tests by reading `coverage/cobertura-coverage.xml`, identifying uncovered components and hooks, writing tests, and re-running coverage to verify improvement.\\n\\n## Loop Design\\n\\n1. **Read** `coverage/cobertura-coverage.xml` — parse uncovered lines/branches per file\\n2. **Prioritize** — rank by: (a) coverage % ascending, (b) type (custom hooks > business-logic utils > components > pages)\\n3. **Read source** — open the target file and understand its props, state, and interactions\\n4. **Write tests** using Vitest + `@testing-library/react`:\\n   \* Custom hooks: `renderHook` from `@testing-library/react`\\n   \* Components: render + user-event interactions\\n   \* Utils: pure function unit tests\\n5. **Run** `npm run coverage` in the React app directory\\n6. **Re-read** the updated report — confirm coverage improved\\n7. **Repeat** until threshold (from MGG-124) is met or no further uncovered paths remain\\n\\n## Priority Targets\\n\\nThe following areas have **zero test coverage** and should be prioritized:\\n\\n### Permission system (from MGG-128)\\n- `usePermission` hook — test `hasRole()`, `isAdmin()`, default empty roles when user is null\\n- `AdminOnly` component — renders children for admin, renders nothing for non-admin\\n- `AdminRoute` component — renders children for admin, redirects for non-admin\\n- `useRoles` hooks (`useUserRoles`, `useAssignRole`, `useRemoveRole`) — mock API calls, verify query invalidation and toast feedback\\n- `AdminUsers` page — userId lookup flow, role display, assign/remove interactions\\n- **Token refresh listener** (`addTokenRefreshListener`) — verify multiple listeners, cleanup via unsubscribe, `notifyTokenRefresh` calls all listeners\\n\\n### Backend: UsersController (add to .NET test suite, not Vitest)\\n- `GET /api/users` — admin can list, non-admin gets 403, pagination works, roles included in response\\n- Batch role query returns correct roles per user\\n\\n## Acceptance Criteria\\n\\n\* Core custom hooks and utility functions have meaningful test coverage\\n\* Component tests cover primary render paths and key user interactions (not every branch)\\n\* Tests do not test implementation details — assert on rendered output and behavior\\n\* Generated API client code (`src/generated/`) is excluded and never tested\\n\* Agent can be re-run and will only write tests for paths still uncovered\\n\\n## Notes\\n\\n\* Requires MGG-126 (Vitest setup) so the coverage report exists\\n\* Mock API calls with `vi.mock()` — tests must not make real HTTP requests\\n\* Use `msw` (Mock Service Worker) if the component fetches data and needs realistic mock responses\\n\* Do not write snapshot tests — they are brittle and add no coverage signal
