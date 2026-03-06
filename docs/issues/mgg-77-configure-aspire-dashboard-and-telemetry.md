---
identifier: MGG-77
title: Configure Aspire Dashboard and Telemetry
id: d3102513-047f-4cd7-b021-988bb5d67ea9
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
url: "https://linear.app/mggarofalo/issue/MGG-77/configure-aspire-dashboard-and-telemetry"
gitBranchName: mggarofalo/mgg-77-configure-aspire-dashboard-and-telemetry
createdAt: "2026-02-11T05:43:47.467Z"
updatedAt: "2026-02-18T01:11:22.467Z"
completedAt: "2026-02-18T01:11:22.454Z"
---

# Configure Aspire Dashboard and Telemetry

## Objective

Set up Aspire Dashboard for comprehensive observability with traces, metrics, logs, and health monitoring.

## Tasks

- [ ] Ensure Aspire Dashboard auto-starts with AppHost
- [ ] Configure dashboard port (default: 15888)
- [ ] Enable OpenTelemetry collection from all services
- [ ] Configure structured logging to flow to dashboard:

  ```csharp
  // In API Program.cs
  builder.Logging.AddOpenTelemetry(logging => {
      logging.IncludeFormattedMessage = true;
      logging.IncludeScopes = true;
  });
  ```
- [ ] Enable distributed tracing:
  - API calls traced
  - Database queries traced
  - HTTP client calls traced
- [ ] Configure metrics collection:
  - Request rates
  - Response times
  - Error rates
  - Custom business metrics
- [ ] Add custom metrics (optional):

  ```csharp
  var receiptCreatedCounter = meter.CreateCounter<int>("receipts.created");
  receiptCreatedCounter.Add(1, new KeyValuePair<string, object>("user", userId));
  ```
- [ ] Configure health checks to appear in dashboard
- [ ] Test dashboard shows all services
- [ ] Test traces flow from API to database
- [ ] Document dashboard features and usage

## Dashboard Features to Verify

* **Resources**: All services visible with status
* **Traces**: Request flows across services
* **Metrics**: Performance graphs and counters
* **Logs**: Structured logs from all services
* **Console**: Service output and errors

## Acceptance Criteria

* Dashboard accessible at [http://localhost:15888](<http://localhost:15888>)
* All services visible in Resources view
* Traces show complete request path
* Logs from all services aggregated
* Metrics graphs displaying correctly
* Health checks visible and green
* Can filter/search logs effectively
