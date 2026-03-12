---
identifier: MGG-74
title: Configure Database Resource in AppHost
id: 7d7c436e-b9d5-4804-8ac5-28eaf3debf09
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
url: "https://linear.app/mggarofalo/issue/MGG-74/configure-database-resource-in-apphost"
gitBranchName: mggarofalo/mgg-74-configure-database-resource-in-apphost
createdAt: "2026-02-11T05:43:17.094Z"
updatedAt: "2026-02-18T01:02:07.118Z"
completedAt: "2026-02-18T01:02:07.095Z"
---

# Configure Database Resource in AppHost

## Objective

Add PostgreSQL (or SQLite) database resource to AppHost with automatic provisioning and connection string management.

## Tasks

- [ ] Choose database approach:
  - PostgreSQL container (recommended for prod-like dev)
  - SQLite file (simplest, no container)
- [ ] Add PostgreSQL resource to AppHost:

  ```csharp
  var postgres = builder.AddPostgres("postgres")
      .WithDataVolume()
      .WithPgAdmin();
  
  var db = postgres.AddDatabase("receiptsdb");
  ```
- [ ] OR add SQLite resource:

  ```csharp
  var db = builder.AddSqlite("receiptsdb")
      .WithDataBindMount("./data");
  ```
- [ ] Pass database connection to API:

  ```csharp
  var api = builder.AddProject<Projects.API>("api")
      .WithReference(db);
  ```
- [ ] Configure database initialization (migrations)
- [ ] Add PgAdmin if using PostgreSQL (optional)
- [ ] Test database starts with AppHost
- [ ] Test connection string auto-wired to API
- [ ] Verify migrations run on startup

## Example AppHost Configuration

```csharp
var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithDataVolume()  // Persist data
    .WithPgAdmin();    // Optional admin UI

var db = postgres.AddDatabase("receiptsdb");

var api = builder.AddProject<Projects.API>("api")
    .WithReference(db)  // Auto-wires connection string
    .WithHttpEndpoint(port: 5000);

var app = builder.Build();
await app.RunAsync();
```

## Acceptance Criteria

* Database starts when AppHost runs
* Connection string automatically available to API
* Data persists across restarts (if using volume)
* Migrations run successfully on startup
* Can connect to database (via PgAdmin or connection string)
* No manual connection string configuration needed
