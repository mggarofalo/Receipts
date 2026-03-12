---
identifier: MGG-73
title: Configure API Service in AppHost
id: c58cd182-1440-43bf-a400-2037e95e4237
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
url: "https://linear.app/mggarofalo/issue/MGG-73/configure-api-service-in-apphost"
gitBranchName: mggarofalo/mgg-73-configure-api-service-in-apphost
createdAt: "2026-02-11T05:43:07.515Z"
updatedAt: "2026-02-18T00:59:04.820Z"
completedAt: "2026-02-18T00:59:04.799Z"
---

# Configure API Service in AppHost

## Objective

Add the .NET API project to Aspire AppHost with proper configuration and service discovery.

## Tasks

- [ ] Add API project reference to AppHost
- [ ] Configure API service in Program.cs:

  ```csharp
  var api = builder.AddProject<Projects.API>("api")
      .WithHttpEndpoint(port: 5000, name: "http")
      .WithHttpsEndpoint(port: 5001, name: "https");
  ```
- [ ] Configure environment variables for API:
  - ASPNETCORE_ENVIRONMENT=Development
  - ConnectionStrings (from database resource)
  - JWT settings
- [ ] Enable hot reload for API
- [ ] Configure health checks endpoint
- [ ] Add service reference for frontend to consume
- [ ] Configure OpenTelemetry in API to send to Aspire dashboard
- [ ] Test API starts via AppHost
- [ ] Test API accessible at [http://localhost:5000](<http://localhost:5000>)

## OpenTelemetry Configuration (API)

```csharp
// Program.cs in API
builder.AddServiceDefaults(); // Aspire defaults

// Or manually:
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddEntityFrameworkCoreInstrumentation())
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation());
```

## Acceptance Criteria

* API starts when AppHost runs
* API accessible on defined ports
* Hot reload works (change code, see updates)
* Environment variables properly set
* Telemetry flowing to Aspire dashboard
* Health checks visible in dashboard
