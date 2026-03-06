# Fail2ban Configuration

Protect the Receipts API from brute-force attacks by banning IPs that trigger rate limiting.

## Prerequisites

Install fail2ban on the Pi host (not inside Docker):

```bash
sudo apt install fail2ban
```

## Filter

Create `/etc/fail2ban/filter.d/receipts.conf`:

```ini
[Definition]
failregex = Rate limit exceeded.*IP: <HOST>
ignoreregex =
```

## Jail

Create `/etc/fail2ban/jail.d/receipts.conf`:

```ini
[receipts]
enabled = true
filter = receipts
logpath = /var/lib/docker/containers/*/*-json.log
maxretry = 10
findtime = 600
bantime = 3600
action = iptables-multiport[name=receipts, port="http,https"]
```

## Verify

```bash
sudo systemctl restart fail2ban
sudo fail2ban-client status receipts
```

## Notes

- The API logs rate limit rejections with the client IP, which fail2ban reads from Docker's JSON log files
- `maxretry=10` allows some headroom above the per-IP rate limit before banning
- `bantime=3600` bans offending IPs for 1 hour
- Adjust thresholds based on your traffic patterns
