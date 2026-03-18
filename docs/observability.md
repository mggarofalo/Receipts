# Observability

The Receipts application uses a hybrid observability strategy:

- **Backend**: OpenTelemetry (traces, metrics, logs) routed through an OTel Collector to a self-hosted Grafana LGTM stack
- **Frontend**: Sentry for error reporting, user feedback, and session replay

## Architecture

```
┌──────────┐  OTLP/gRPC   ┌────────────────┐
│ API      │──────────────►│ OTel Collector  │
│ (.NET)   │               └───────┬─┬─┬────┘
└──────────┘                       │ │ │
                          traces   │ │ │  logs     metrics
                                   ▼ │ ▼          ▼
                            ┌──────┐ │ ┌─────┐ ┌────────────┐
                            │ Tempo│ │ │ Loki│ │ Prometheus  │
                            └──┬───┘ │ └──┬──┘ └─────┬──────┘
                               │     │    │          │
                               └─────┼────┼──────────┘
                                     ▼
                              ┌──────────┐
                              │ Grafana  │ :3000
                              └──────────┘

┌──────────┐
│ React    │───► Sentry (cloud)
│ Frontend │     - Error reporting
└──────────┘     - Session replay
                 - User feedback
```

## Backend Observability (Grafana LGTM)

The .NET API emits OpenTelemetry signals via `Receipts.ServiceDefaults`:

| Signal  | Instrumentation                                              | Backend   |
|---------|--------------------------------------------------------------|-----------|
| Traces  | ASP.NET Core, HttpClient, Entity Framework Core              | Tempo     |
| Metrics | ASP.NET Core, HttpClient, .NET Runtime                       | Prometheus|
| Logs    | Serilog with OTLP sink, structured with CorrelationId        | Loki      |

### Docker Compose Services

All services are defined in `docker-compose.yml` under the `# --- Observability Stack ---` section:

| Service         | Image                                      | Port  | Purpose                     |
|-----------------|--------------------------------------------|-------|-----------------------------|
| otel-collector  | otel/opentelemetry-collector-contrib:0.121.0 | 4317/4318 | Receives and routes OTLP |
| tempo           | grafana/tempo:2.7.2                         | -     | Distributed trace storage   |
| loki            | grafana/loki:3.4.3                          | -     | Log aggregation             |
| prometheus      | prom/prometheus:v3.3.0                      | -     | Metrics storage             |
| grafana         | grafana/grafana:11.6.0                      | 3000  | Dashboards and alerting     |

### Accessing Grafana

- URL: `http://<host>:3000`
- Admin user: `admin`, password auto-generated in `/secrets/grafana_password` (same pattern as the PostgreSQL and JWT secrets)
- Anonymous read access is enabled by default

### Pre-configured Datasources

Datasources are auto-provisioned on startup (`observability/grafana/provisioning/datasources/`):
- **Prometheus** (default) - with trace exemplar links to Tempo
- **Tempo** - with trace-to-log correlation via Loki and service map via Prometheus
- **Loki** - with derived fields extracting traceId for Tempo linking

### Dashboards

Provisioned dashboards (`observability/grafana/dashboards/`):
- **API Overview** - request rate, error rate, latency percentiles (p50/p95/p99), status codes, GC, memory, thread pool, recent traces

### Alerting

Provisioned alert rules (`observability/grafana/provisioning/alerting/`):
- **Elevated 5xx Error Rate** - fires when 5xx rate exceeds 0.1 req/s for 5 minutes
- **High Request Latency** - fires when p95 latency exceeds 2 seconds for 5 minutes
- **API Service Down** - fires when no traffic is detected for 5 minutes

### Verifying the Stack

1. Start everything: `docker compose up -d`
2. Generate some API traffic (login, browse receipts)
3. Open Grafana at `http://localhost:3000`
4. Navigate to **Explore > Tempo** and search for traces by `service.name = receipts-api`
5. Navigate to **Explore > Loki** and query `{service_name="receipts-api"}`
6. Check the **API Overview** dashboard for metrics

## Frontend Observability (Sentry)

The React frontend uses `@sentry/react` configured in `src/client/src/lib/sentry.ts`.

### Features

| Feature          | Integration                        | Sampling                        |
|------------------|------------------------------------|---------------------------------|
| Error capture    | `Sentry.ErrorBoundary` + global    | 100%                            |
| Browser tracing  | React Router v7 integration        | 20% prod, 100% dev             |
| Session replay   | `replayIntegration`                | 1% sessions, 100% on error     |
| User feedback    | `feedbackIntegration` (auto-inject)| On demand                       |

### Configuration

All Sentry config is via environment variables (no hardcoded DSN):

| Variable                  | Where Set                  | Purpose                    |
|---------------------------|----------------------------|----------------------------|
| `VITE_SENTRY_DSN`         | `.env` / Docker build arg  | Sentry project DSN         |
| `VITE_SENTRY_ENVIRONMENT` | `.env` / Docker build arg  | Environment tag            |
| `SENTRY_AUTH_TOKEN`        | GitHub Actions secret      | Source map uploads          |
| `SENTRY_ORG`              | GitHub Actions secret      | Sentry organization slug   |
| `SENTRY_PROJECT`           | GitHub Actions secret      | Sentry project slug        |

### Source Maps

Source maps are uploaded to Sentry during Docker image builds via `@sentry/vite-plugin`. They are deleted from the production bundle after upload, so they are not publicly accessible.

### Required Secrets (Production)

Add these to your GitHub repository secrets:

1. `SENTRY_DSN` - your Sentry project DSN (e.g., `https://examplePublicKey@o0.ingest.sentry.io/0`)
2. `SENTRY_AUTH_TOKEN` - Sentry auth token with `project:releases` and `org:read` scopes
3. `SENTRY_ORG` - your Sentry organization slug
4. `SENTRY_PROJECT` - your Sentry project slug

The Grafana admin password is auto-generated by the init container into `/secrets/grafana_password`, just like the other secrets. To retrieve it: `docker compose exec grafana cat /secrets/grafana_password`.
