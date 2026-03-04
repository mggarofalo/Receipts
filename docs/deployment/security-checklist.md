# Deployment Security Checklist

## Container Security

- [x] Non-root user (ASP.NET noble image runs as `app` user by default)
- [x] Read-only filesystem (`read_only: true` in docker-compose)
- [x] No privilege escalation (`no-new-privileges` security option)
- [x] Resource limits (1GB memory, 2 CPU)
- [x] Internal database network (PostgreSQL not exposed to host)
- [x] Trivy vulnerability scanning in CI

## Application Security

- [x] HTTPS enforced via Nginx Proxy Manager (TLS termination at reverse proxy)
- [x] Forwarded headers configured for accurate client IP logging
- [x] Rate limiting on all endpoints (100 req/min global, 5 req/min auth)
- [x] JWT authentication with configurable key
- [x] HSTS headers via reverse proxy
- [x] No secrets in Docker image (all via environment variables)

## Infrastructure Security

- [x] Secrets generated via `openssl rand` (scripts/generate-secrets.sh)
- [x] `.env` file gitignored
- [x] Database credentials never exposed outside Docker network
- [x] Log rotation configured (50MB max, 3 files)
- [x] Fail2ban integration for brute-force protection

## Maintenance

- [x] Automated backups with 7-day retention
- [x] Update script with automatic rollback on health check failure
- [x] Health check includes database connectivity verification
