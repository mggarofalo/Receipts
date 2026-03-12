---
identifier: MGG-190
title: Migrate to createBrowserRouter (data router) for useNavigation support
id: a2fae571-e5fe-4784-ae85-8e5db3293fdd
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
url: "https://linear.app/mggarofalo/issue/MGG-190/migrate-to-createbrowserrouter-data-router-for-usenavigation-support"
gitBranchName: mggarofalo/mgg-190-migrate-to-createbrowserrouter-data-router-for-usenavigation
createdAt: "2026-02-25T13:58:27.621Z"
updatedAt: "2026-02-25T14:02:22.209Z"
completedAt: "2026-02-25T14:02:22.195Z"
---

# Migrate to createBrowserRouter (data router) for useNavigation support

`useNavigation` in Layout.tsx requires a data router (`createBrowserRouter` + `RouterProvider`) but the app uses `<BrowserRouter>`. This causes: "useNavigation must be used within a data router."

Migrate from `<BrowserRouter>` / `<Routes>` / `<Route>` to `createBrowserRouter` + `<RouterProvider>` in main.tsx and App.tsx.
