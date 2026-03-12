---
identifier: MGG-68
title: "Backend: Authentication Audit Log with 180-Day Retention"
id: 1846ab5e-c422-4250-aeb8-3ad3993b3256
status: Done
priority:
  value: 2
  name: High
createdBy: Michael Garofalo
project: Receipts
team: mggarofalo
labels:
  - security
  - backend
milestone: "Phase 4 [Backend]"
url: "https://linear.app/mggarofalo/issue/MGG-68/backend-authentication-audit-log-with-180-day-retention"
gitBranchName: mggarofalo/mgg-68-backend-authentication-audit-log-with-180-day-retention
createdAt: "2026-02-11T05:29:52.057Z"
updatedAt: "2026-02-21T14:23:32.224Z"
completedAt: "2026-02-21T14:23:32.207Z"
---

# Backend: Authentication Audit Log with 180-Day Retention

## Objective

Implement comprehensive authentication audit logging with automatic cleanup of logs older than 180 days.

## Tasks

- [ ] Create `AuthAuditLog` entity:
  - `Id` (Guid)
  - `EventType` (enum: Login, LoginFailed, Logout, ApiKeyUsed, ApiKeyCreated, ApiKeyRevoked, PasswordChanged, UserRegistered)
  - `UserId` (Guid?, nullable for failed logins)
  - `ApiKeyId` (Guid?, nullable)
  - `Username` (string, for failed login attempts)
  - `Success` (bool)
  - `FailureReason` (string, nullable)
  - `IpAddress` (string)
  - `UserAgent` (string)
  - `Timestamp` (DateTime)
  - `Metadata` (JSON, for additional context)
- [ ] Implement auth event logging:
  - Log successful logins (JWT)
  - Log failed login attempts (wrong password, user not found)
  - Log logout events
  - Log API key usage (each API call)
  - Log API key creation/revocation
  - Log password changes
  - Log user registration
- [ ] Capture request context:
  - IP address from HttpContext
  - User agent from headers
  - Additional metadata (requested resource, etc.)
- [ ] Implement rate limiting/lockout based on failed attempts:
  - Track failed login attempts per IP/user
  - Temporary lockout after X failed attempts
  - Log lockout events
- [ ] Create retention cleanup job:
  - Background service or scheduled task
  - Delete AuthAuditLog entries older than 180 days
  - Run daily at off-peak hours (e.g., 3 AM)
  - Log cleanup statistics
- [ ] Add indexes:
  - `(UserId, Timestamp)` for user activity
  - `(EventType, Timestamp)` for event filtering
  - `Timestamp` for retention cleanup
  - `IpAddress` for security analysis
- [ ] Create API endpoints:
  - GET /api/auth/audit/me - Current user's auth history
  - GET /api/auth/audit/recent - Recent auth events (admin)
  - GET /api/auth/audit/failed - Failed login attempts (admin)
- [ ] Add configuration for retention period (default 180 days)
- [ ] Document auth audit log usage

## Example AuthAuditLog Entry

```json
{
  "id": "def-456",
  "eventType": "Login",
  "userId": "abc-123",
  "success": true,
  "ipAddress": "192.168.1.100",
  "userAgent": "Mozilla/5.0...",
  "timestamp": "2024-01-16T10:30:00Z",
  "metadata": {
    "loginMethod": "JWT",
    "sessionDuration": "15m"
  }
}
```

## Background Cleanup Job

```csharp
public class AuthAuditCleanupService : BackgroundService {
    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        while (!stoppingToken.IsCancellationRequested) {
            var cutoffDate = DateTime.UtcNow.AddDays(-180);
            var deleted = await _db.AuthAuditLogs
                .Where(x => x.Timestamp < cutoffDate)
                .ExecuteDeleteAsync();
            _logger.LogInformation("Deleted {Count} auth audit logs", deleted);
            await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
        }
    }
}
```

## Acceptance Criteria

* All authentication events logged
* Failed login attempts tracked
* IP address and user agent captured
* 180-day retention enforced automatically
* Cleanup job runs daily
* API endpoints provide auth history
* Indexes optimize query performance
* Rate limiting prevents brute force attacks
