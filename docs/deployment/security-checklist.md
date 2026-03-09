# Deployment Security Checklist

## Container Security

- [x] Capability dropping (`cap_drop: ALL` on all containers with minimal `cap_add`)
- [x] No privilege escalation (`no-new-privileges` security option)
- [x] Clean privilege de-escalation via `gosu` (root entrypoint → app user)
- [x] Resource limits (1GB memory, 2 CPU on app container)
- [x] Internal database network (PostgreSQL not exposed to host)
- [x] Trivy vulnerability scanning in CI
- [x] Log rotation configured (50MB max, 3 files)

## Secrets Management

- [x] Secrets auto-generated on first run (no manual setup required)
- [x] Secrets stored as files in a Docker volume (not in `.env` or image layers)
- [x] Entrypoint bridges file secrets to environment variables at runtime
- [x] Database uses `POSTGRES_PASSWORD_FILE` (native file-based secret support)
- [x] Init container runs with `restart: "no"` and no network access

## Application Security

- [x] HTTPS enforced via Nginx Proxy Manager (TLS termination at reverse proxy)
- [x] Forwarded headers configured for accurate client IP logging
- [x] Rate limiting on all endpoints (100 req/min global, 5 req/min auth)
- [x] JWT authentication with auto-generated signing key
- [x] HSTS headers via reverse proxy
- [x] No secrets baked into Docker image

## Infrastructure Security

- [x] Database credentials never exposed outside Docker network
- [x] Fail2ban integration for brute-force protection
- [x] Secrets volume mounted read-only in app and db containers

## Maintenance

- [x] Automated backups with 7-day retention
- [x] Update script with automatic rollback on health check failure
- [x] Health check includes database connectivity verification
- [x] Secret rotation via volume removal and restart
