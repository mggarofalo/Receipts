# Deployment Guide

Deploy Receipts on a Raspberry Pi (or any Linux host) using Docker Compose with Nginx Proxy Manager (NPM) for HTTPS termination.

## Prerequisites

- **Raspberry Pi 4/5** (4GB+ RAM) or any ARM64/AMD64 Linux host
- **Docker** and **Docker Compose** (v2+)
- **Nginx Proxy Manager** running on the same host or network
- A **domain name** pointed to your public IP (A record)

### Install Docker (Raspberry Pi OS)

```bash
curl -fsSL https://get.docker.com | sh
sudo usermod -aG docker $USER
# Log out and back in for group change to take effect
```

## Quick Start

```bash
# 1. Clone the repository
git clone https://github.com/mggarofalo/Receipts.git
cd Receipts

# 2. Generate secrets and create .env
bash scripts/generate-secrets.sh > .env
# Edit .env to set your admin email, name, etc.

# 3. Start the application
docker compose up -d

# 4. Verify
docker compose ps
curl http://localhost:8080/api/health
```

The app will be available at `http://<your-pi-ip>:8080`.

## Configuration

All configuration is via environment variables in `.env`. See `.env.example` for the full list.

| Variable | Required | Default | Description |
|----------|----------|---------|-------------|
| `POSTGRES_USER` | Yes | `receipts` | Database username |
| `POSTGRES_PASSWORD` | Yes | — | Database password |
| `POSTGRES_DB` | No | `receipts` | Database name |
| `JWT_KEY` | Yes | — | JWT signing key (min 32 chars) |
| `JWT_ISSUER` | No | `receipts-api` | JWT issuer claim |
| `JWT_AUDIENCE` | No | `receipts-app` | JWT audience claim |
| `ADMIN_EMAIL` | Yes | — | Initial admin account email |
| `ADMIN_PASSWORD` | Yes | — | Initial admin password |
| `ADMIN_FIRST_NAME` | No | `Admin` | Admin first name |
| `ADMIN_LAST_NAME` | No | `User` | Admin last name |
| `APP_PORT` | No | `8080` | Host port for the app |
| `IMAGE_TAG` | No | `latest` | Docker image tag |

## Nginx Proxy Manager Setup

NPM provides HTTPS termination with automatic Let's Encrypt certificates.

### Create a Proxy Host

1. Open NPM admin panel (typically `http://<pi-ip>:81`)
2. **Proxy Hosts** → **Add Proxy Host**
3. Configure:
   - **Domain Names**: `receipts.yourdomain.com`
   - **Scheme**: `http`
   - **Forward Hostname/IP**: `receipts-app` (or `localhost` if on same Docker network)
   - **Forward Port**: `8080`
   - **Websockets Support**: **ON** (required for SignalR real-time updates)
4. **SSL** tab:
   - **SSL Certificate**: Request a new certificate
   - **Force SSL**: ON
   - **HTTP/2 Support**: ON
   - **HSTS Enabled**: ON

### Custom Nginx Configuration (Advanced tab)

```nginx
# Security headers
add_header X-Content-Type-Options "nosniff" always;
add_header X-Frame-Options "SAMEORIGIN" always;
add_header Referrer-Policy "strict-origin-when-cross-origin" always;

# Increase body size for receipt uploads
client_max_body_size 10M;
```

### Network Configuration

If NPM runs on the same host, add the NPM container to the `receipts-net` network, or use the host IP. The app container is named `receipts-app` and listens on port 8080.

## Operations

### Update to a new version

```bash
bash scripts/update.sh              # Update to latest
bash scripts/update.sh v1.2.3       # Update to specific version
```

The update script automatically backs up the database before updating and rolls back if the health check fails.

### Roll back

```bash
bash scripts/rollback.sh v1.1.0     # Roll back to a specific tag
bash scripts/rollback.sh sha-abc123 # Roll back to a specific build
```

### Backup and restore

```bash
bash scripts/backup.sh              # Backup to ./backups/
bash scripts/backup.sh /mnt/usb     # Backup to external drive
bash scripts/restore.sh backups/receipts_20260308_120000.sql.gz
```

Backups older than 7 days are automatically pruned. For automated daily backups, add a cron job:

```bash
crontab -e
# Add:
0 3 * * * cd /home/pi/Receipts && bash scripts/backup.sh >> /var/log/receipts-backup.log 2>&1
```

### View logs

```bash
docker compose logs -f app          # Follow app logs
docker compose logs -f db           # Follow database logs
docker compose logs --tail=100 app  # Last 100 lines
```

### Restart services

```bash
docker compose restart app           # Restart app only
docker compose restart               # Restart all services
docker compose down && docker compose up -d  # Full restart
```

## Troubleshooting

### App won't start

```bash
docker compose logs app | head -50   # Check startup logs
docker compose exec db pg_isready -U receipts  # Check DB connectivity
```

### Database connection errors

Ensure `POSTGRES_USER` and `POSTGRES_PASSWORD` match between the `app` and `db` services in your `.env` file. The database must be healthy before the app starts (enforced by `depends_on`).

### Port already in use

Change `APP_PORT` in `.env` to a different port (e.g., `APP_PORT=8081`).

### Out of memory

The app is limited to 1GB RAM by default. Check usage:

```bash
docker stats --no-stream
```

If the Pi is running low, reduce PostgreSQL memory settings in `docker-compose.yml` (e.g., `shared_buffers=64MB`).

### SignalR WebSocket issues

Ensure **Websockets Support** is enabled in your NPM proxy host configuration. SignalR falls back to long-polling if WebSockets are unavailable, but real-time updates will be delayed.

## Security

- All containers run as **non-root** users
- App container uses a **read-only filesystem** with tmpfs for `/tmp`
- Database is only accessible on the **internal Docker network** (not exposed to host)
- `no-new-privileges` security option prevents privilege escalation
- Docker images are scanned for vulnerabilities with **Trivy** in CI
- Secrets are stored in `.env` (gitignored) — never committed to source control
- Generate strong secrets with `bash scripts/generate-secrets.sh`

### Firewall (recommended)

Only expose the ports NPM needs:

```bash
sudo ufw default deny incoming
sudo ufw allow ssh
sudo ufw allow 80/tcp    # HTTP (NPM redirect to HTTPS)
sudo ufw allow 443/tcp   # HTTPS (NPM)
sudo ufw enable
```

Do **not** expose port 8080 externally — NPM proxies to it internally.

### Application Rate Limiting

The API enforces rate limiting at the application level (defense-in-depth with CloudFlare and NPM):

| Policy | Endpoints | Limit | Window |
|--------|-----------|-------|--------|
| Global | All endpoints | 100 requests | 1 minute (sliding) |
| `auth` | Login | 5 requests | 1 minute |
| `auth-sensitive` | Refresh, change password | 10 requests | 1 minute |
| `api-key` | API key operations | 10 requests | 1 minute |

Rate limits are configurable in `appsettings.json` under the `RateLimiting` section — no code changes required.

Clients receive HTTP 429 with a `Retry-After` header when limits are exceeded. All rate limit violations are logged to the auth audit trail.

### Fail2ban (recommended)

Fail2ban automatically blocks IPs with repeated auth failures at the firewall level.

**Install:**

```bash
sudo apt install fail2ban
```

**Create jail** (`/etc/fail2ban/jail.d/receipts-app.conf`):

```ini
[receipts-app]
enabled = true
port = http,https
filter = receipts-app
logpath = /var/log/syslog
backend = systemd
maxretry = 5
findtime = 600
bantime = 3600
ignoreip = 127.0.0.1 192.168.0.0/16
```

**Create filter** (`/etc/fail2ban/filter.d/receipts-app.conf`):

```ini
[Definition]
failregex = ^.*LoginFailed.*IpAddress=<HOST>.*$
            ^.*RateLimitExceeded.*IpAddress=<HOST>.*$
ignoreregex =
```

**Enable and start:**

```bash
sudo systemctl enable fail2ban
sudo systemctl start fail2ban
```

**Monitor:**

```bash
sudo fail2ban-client status receipts-app     # Show jail status
sudo fail2ban-client get receipts-app banned  # List banned IPs
sudo fail2ban-client set receipts-app unbanip 1.2.3.4  # Unban an IP
```

## Resource Usage

Target for Raspberry Pi 4 (4GB RAM):

| Service | Memory Limit | Typical Usage |
|---------|-------------|---------------|
| App (API + SPA) | 1 GB | ~200-400 MB |
| PostgreSQL | uncapped | ~100-200 MB |
| **Total** | — | ~300-600 MB |

Disk: Docker images ~500 MB, database grows with usage.
