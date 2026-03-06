# Deployment Guide

Deploy the Receipts app on a Raspberry Pi (or any arm64/amd64 host) using Docker.

## Prerequisites

- Raspberry Pi 4/5 (4GB+ RAM) with Raspberry Pi OS (64-bit)
- Docker Engine 24+ and Docker Compose v2
- Nginx Proxy Manager (or similar reverse proxy) for TLS termination
- DNS pointing to your Pi's public IP

### Install Docker on Raspberry Pi

```bash
curl -fsSL https://get.docker.com | sh
sudo usermod -aG docker $USER
# Log out and back in
```

## First Deploy

### 1. Clone and configure

```bash
git clone https://github.com/mggarofalo/receipts.git
cd receipts
cp .env.example .env
```

### 2. Generate secrets

```bash
bash scripts/generate-secrets.sh
```

Copy the output values into `.env`:

```bash
# Edit .env with the generated JWT_KEY and POSTGRES_PASSWORD
# Also set ADMIN_EMAIL and ADMIN_PASSWORD for the initial admin account
nano .env
```

### 3. Start services

```bash
docker compose up -d
```

Verify both services are healthy:

```bash
docker compose ps
curl http://localhost:8080/api/health
```

### 4. Configure reverse proxy

In Nginx Proxy Manager, create a proxy host:

| Field | Value |
|-------|-------|
| Domain | `receipts.yourdomain.com` |
| Scheme | `http` |
| Forward Hostname | `localhost` (or Pi's LAN IP) |
| Forward Port | `8080` |
| SSL | Request new Let's Encrypt certificate |
| Force SSL | Enable |
| WebSocket Support | **Enable** (required for `/hubs/*` real-time updates) |

### 5. First login

1. Navigate to `https://receipts.yourdomain.com`
2. Log in with the `ADMIN_EMAIL` / `ADMIN_PASSWORD` from `.env`
3. You'll be prompted to change the password on first login

## Maintenance

### Backup

Manual backup:

```bash
bash scripts/backup.sh
```

Automated daily backup via cron:

```bash
crontab -e
# Add:
0 2 * * * cd /path/to/receipts && bash scripts/backup.sh >> /var/log/receipts-backup.log 2>&1
```

### Restore from backup

```bash
bash scripts/restore.sh backups/receipts_20260101_020000.sql.gz
```

### Update to new version

```bash
# Update to latest
bash scripts/update.sh

# Update to specific version
bash scripts/update.sh v1.2.3
```

The update script automatically creates a backup and rolls back if the health check fails.

### Manual rollback

```bash
bash scripts/rollback.sh v1.1.0
```

### View logs

```bash
docker compose logs -f app     # API logs
docker compose logs -f db      # Database logs
```

### Check volume usage

```bash
bash scripts/volume-inspect.sh
```

## Troubleshooting

### App won't start

```bash
# Check logs for errors
docker compose logs app

# Verify environment variables
docker compose config

# Check database connectivity
docker compose exec db pg_isready -U receipts
```

### Database connection refused

Ensure the `db` service is healthy before the `app` starts:

```bash
docker compose ps
# db should show "healthy"
```

### Out of disk space

```bash
# Check disk usage
df -h

# Prune unused Docker resources
docker system prune -f

# Check backup directory size
du -sh backups/
```

### WebSocket connections failing

Ensure Nginx Proxy Manager has **WebSocket Support** enabled for the proxy host. The SignalR hub at `/hubs/entities` requires WebSocket connections.

## Architecture

```
Internet → Nginx Proxy Manager (TLS) → receipts-app:8080 → receipts-db:5432
                                              ↓
                                        wwwroot/ (React SPA)
                                        /api/* (REST endpoints)
                                        /hubs/* (SignalR WebSocket)
```

The app container serves both the React SPA (static files) and the .NET API. PostgreSQL runs in a separate container on an internal Docker network (not exposed to the host).
