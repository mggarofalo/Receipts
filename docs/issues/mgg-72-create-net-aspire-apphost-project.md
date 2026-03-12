---
identifier: MGG-72
title: Create .NET Aspire AppHost Project
id: 74165a27-35f5-4a39-aded-68d637cc914d
status: Done
priority:
  value: 2
  name: High
createdBy: Michael Garofalo
project: Receipts
team: mggarofalo
labels:
  - dx
  - infra
milestone: "Phase 3: Aspire Developer Experience"
url: "https://linear.app/mggarofalo/issue/MGG-72/create-net-aspire-apphost-project"
gitBranchName: mggarofalo/mgg-72-create-net-aspire-apphost-project
createdAt: "2026-02-11T05:42:58.891Z"
updatedAt: "2026-02-18T00:53:14.318Z"
completedAt: "2026-02-18T00:53:14.299Z"
---

# Create .NET Aspire AppHost Project

## Objective

Bootstrap the .NET Aspire AppHost project that will orchestrate all local development services.

## Tasks

- [ ] Install .NET Aspire workload:

  ```bash
  dotnet workload install aspire
  ```
- [ ] Create AppHost project:

  ```bash
  dotnet new aspire-apphost -n Receipts.AppHost
  ```
- [ ] Add AppHost to solution
- [ ] Update project structure:

  ```
  /src/Receipts.AppHost/
    ├── Program.cs
    ├── appsettings.json
    └── Receipts.AppHost.csproj
  ```
- [ ] Configure basic AppHost with builder:

  ```csharp
  var builder = DistributedApplication.CreateBuilder(args);
  var app = builder.Build();
  await app.RunAsync();
  ```
- [ ] Install required NuGet packages:
  - Aspire.Hosting
  - Aspire.Hosting.PostgreSQL (or SQLite)
  - Aspire.Hosting.NodeJs (for Vite)
- [ ] Set up environment-specific configuration
- [ ] Configure default ports and settings
- [ ] Test AppHost runs successfully

## Acceptance Criteria

* AppHost project created and builds successfully
* Can run AppHost (even with no services yet)
* Project structure follows Aspire conventions
* Dependencies installed correctly
