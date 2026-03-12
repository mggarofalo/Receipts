---
identifier: MGG-76
title: Setup VS Code launch.json and tasks.json for F5 Debugging
id: 5defd225-9938-46b2-9adc-bcaaca5468d7
status: Done
priority:
  value: 2
  name: High
createdBy: Michael Garofalo
project: Receipts
team: mggarofalo
labels:
  - dx
milestone: "Phase 3: Aspire Developer Experience"
url: "https://linear.app/mggarofalo/issue/MGG-76/setup-vs-code-launchjson-and-tasksjson-for-f5-debugging"
gitBranchName: mggarofalo/mgg-76-setup-vs-code-launchjson-and-tasksjson-for-f5-debugging
createdAt: "2026-02-11T05:43:38.547Z"
updatedAt: "2026-02-18T01:07:13.283Z"
completedAt: "2026-02-18T01:07:13.269Z"
---

# Setup VS Code launch.json and tasks.json for F5 Debugging

## Objective

Configure VS Code to enable F5 debugging of the entire Aspire stack with proper breakpoints and debugging experience.

## Tasks

- [ ] Create/update `.vscode/launch.json`:

  ```json
  {
    "version": "0.2.0",
    "configurations": [
      {
        "name": "Launch Aspire AppHost",
        "type": "coreclr",
        "request": "launch",
        "preLaunchTask": "build-apphost",
        "program": "${workspaceFolder}/src/Receipts.AppHost/bin/Debug/net10.0/Receipts.AppHost.dll",
        "args": [],
        "cwd": "${workspaceFolder}/src/Receipts.AppHost",
        "stopAtEntry": false,
        "env": {
          "ASPNETCORE_ENVIRONMENT": "Development"
        },
        "serverReadyAction": {
          "action": "openExternally",
          "pattern": "Now listening on: (https?://\\S+)",
          "uriFormat": "http://localhost:15888"
        }
      }
    ]
  }
  ```
- [ ] Create/update `.vscode/tasks.json`:

  ```json
  {
    "version": "2.0.0",
    "tasks": [
      {
        "label": "build-apphost",
        "command": "dotnet",
        "type": "process",
        "args": [
          "build",
          "${workspaceFolder}/src/Receipts.AppHost/Receipts.AppHost.csproj"
        ],
        "problemMatcher": "$msCompile"
      }
    ]
  }
  ```
- [ ] Add compound launch configuration (optional):

  ```json
  {
    "name": "Debug Everything",
    "configurations": [
      "Launch Aspire AppHost",
      "Attach to API"
    ],
    "preLaunchTask": "build-all"
  }
  ```
- [ ] Configure breakpoints to work in API code
- [ ] Test F5 launches entire stack
- [ ] Test breakpoints hit in API
- [ ] Configure automatic browser opening to dashboard
- [ ] Document debugging workflow in README

## Debugging Tips

* Use Aspire Dashboard to see all services
* Set breakpoints in API code
* Frontend runs in dev mode (HMR enabled)
* Database visible in Aspire Dashboard
* Logs aggregated in dashboard

## Acceptance Criteria

* F5 in VS Code starts AppHost
* All services start (API, DB, frontend)
* Aspire Dashboard opens automatically
* Can set breakpoints in API code
* Breakpoints hit when API called
* Hot reload works for both API and frontend
* Stopping debugger stops all services cleanly
