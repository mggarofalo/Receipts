---
identifier: MGG-33
title: "Project Setup: React + Vite + TypeScript + shadcn/ui"
id: 379219b3-40b2-417a-abcd-bf8b7455a33d
status: Done
priority:
  value: 2
  name: High
createdBy: Michael Garofalo
project: Receipts
team: mggarofalo
labels:
  - infra
  - frontend
milestone: "Phase 4 [Frontend]"
url: "https://linear.app/mggarofalo/issue/MGG-33/project-setup-react-vite-typescript-shadcnui"
gitBranchName: mggarofalo/mgg-33-project-setup-react-vite-typescript-shadcnui
createdAt: "2026-02-11T05:06:18.194Z"
updatedAt: "2026-02-26T13:06:25.985Z"
completedAt: "2026-02-21T15:03:32.095Z"
---

# Project Setup: React + Vite + TypeScript + shadcn/ui

## Objective

Bootstrap the new React frontend project with all necessary tooling and dependencies.

## Tasks

- [ ] Initialize Vite project with React + TypeScript template
- [ ] Configure Tailwind CSS
- [ ] Install and configure shadcn/ui (init, configure path aliases)
- [ ] Setup ESLint + Prettier (with TypeScript rules)
- [ ] Configure Vite build settings (output to `/wwwroot` or similar for .NET hosting)
- [ ] Setup basic folder structure (`/components`, `/pages`, `/lib`, `/hooks`, `/types`, `/api`)
- [ ] Install React Router v7
- [ ] Install TanStack Query (React Query) for server state
- [ ] Install @microsoft/signalr for real-time updates
- [ ] Add initial shadcn/ui components (Button, Input, Card, Dialog, Toast)
- [ ] Create README with setup instructions

## Acceptance Criteria

* Dev server runs without errors
* TypeScript strict mode enabled
* Tailwind working with shadcn/ui components
* Hot reload functional
* Build process outputs production bundle
