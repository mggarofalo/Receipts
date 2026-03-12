---
identifier: MGG-34
title: "Backend: ASP.NET Identity + JWT Authentication"
id: bdbc811a-738c-4375-b735-5d7a2b3384a4
status: Done
priority:
  value: 1
  name: Urgent
createdBy: Michael Garofalo
project: Receipts
team: mggarofalo
labels:
  - security
  - backend
milestone: "Phase 4 [Backend]"
url: "https://linear.app/mggarofalo/issue/MGG-34/backend-aspnet-identity-jwt-authentication"
gitBranchName: mggarofalo/mgg-34-backend-aspnet-identity-jwt-authentication
createdAt: "2026-02-11T05:06:23.932Z"
updatedAt: "2026-02-19T01:57:50.787Z"
completedAt: "2026-02-19T01:39:07.406Z"
---

# Backend: ASP.NET Identity + JWT Authentication

## Objective

Implement secure authentication on the .NET API supporting both user login (JWT) and service authentication (API keys) with proper audit attribution using foreign keys.

## Tasks

### User Authentication (JWT)

- [ ] Install Microsoft.AspNetCore.Identity.EntityFrameworkCore
- [ ] Install Microsoft.AspNetCore.Authentication.JwtBearer
- [ ] Configure [ASP.NET](<http://ASP.NET>) Identity with Entity Framework
- [ ] Create User model (inherit IdentityUser)
- [ ] Create AuthController with endpoints:
  - POST /api/auth/register (for initial user setup)
  - POST /api/auth/login (returns JWT + refresh token)
  - POST /api/auth/refresh (refresh access token)
  - POST /api/auth/logout (invalidate refresh token)
- [ ] Configure JWT token generation (access token: 15min, refresh token: 7 days)
- [ ] Implement secure password hashing (bcrypt via Identity)

### API Key Authentication (Services)

- [ ] Create ApiKey entity:
  - Id, Name, KeyHash (hashed API key), UserId (owner), CreatedAt, LastUsedAt, ExpiresAt, IsRevoked
- [ ] Create API key generation service (cryptographically secure random keys)
- [ ] Hash API keys before storage (similar to password hashing)
- [ ] Create ApiKeyController with endpoints:
  - POST /api/apikeys (generate new API key, return key ONCE)
  - GET /api/apikeys (list all API keys for user)
  - DELETE /api/apikeys/{id} (revoke API key)
- [ ] Create API key authentication middleware/handler
- [ ] Support API key in header: `X-API-Key: <key>` or `Authorization: ApiKey <key>`
- [ ] Update LastUsedAt timestamp on each API key use

### Unified Authentication & Authorization

- [ ] Create custom authentication scheme supporting both JWT and API keys
- [ ] Add \[Authorize\] attributes to existing controllers
- [ ] Create IPrincipal that works for both Users and ApiKeys
- [ ] Implement audit attribution with foreign keys:
  - CreatedByUserId, CreatedByApiKeyId (one populated, one null)
  - UpdatedByUserId, UpdatedByApiKeyId
  - DeletedByUserId, DeletedByApiKeyId
  - Proper foreign key constraints to User and ApiKey tables
- [ ] Create database migrations for Identity tables + ApiKey table
- [ ] Add CORS policy for frontend origin
- [ ] Implement refresh token rotation for security

### Audit System

- [ ] Create IAuditable interface with foreign key audit fields
- [ ] Implement automatic audit field population in SaveChanges
- [ ] Populate correct UserId or ApiKeyId based on authentication method
- [ ] Add navigation properties for User and ApiKey entities
- [ ] Create audit log table (optional, for detailed tracking)

## Example ApiKey Model

```csharp
public class ApiKey {
    public Guid Id { get; set; }
    public string Name { get; set; }  // e.g., "Paperless Integration"
    public string KeyHash { get; set; }  // Hashed key
    public Guid UserId { get; set; }  // Owner (FK)
    public User User { get; set; }  // Navigation property
    public DateTime CreatedAt { get; set; }
    public DateTime? LastUsedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool IsRevoked { get; set; }
}
```

## Example Audit Fields

```csharp
public interface IAuditable {
    DateTime CreatedAt { get; set; }
    Guid? CreatedByUserId { get; set; }
    User CreatedByUser { get; set; }
    Guid? CreatedByApiKeyId { get; set; }
    ApiKey CreatedByApiKey { get; set; }
    
    DateTime? UpdatedAt { get; set; }
    Guid? UpdatedByUserId { get; set; }
    User UpdatedByUser { get; set; }
    Guid? UpdatedByApiKeyId { get; set; }
    ApiKey UpdatedByApiKey { get; set; }
}
```

## Usage in SaveChanges

```csharp
protected override async Task<int> SaveChangesAsync(CancellationToken cancellationToken) {
    foreach (var entry in ChangeTracker.Entries<IAuditable>()) {
        if (entry.State == EntityState.Added) {
            entry.Entity.CreatedAt = DateTime.UtcNow;
            if (_currentUser.IsApiKey) {
                entry.Entity.CreatedByApiKeyId = _currentUser.ApiKeyId;
            } else {
                entry.Entity.CreatedByUserId = _currentUser.UserId;
            }
        }
        if (entry.State == EntityState.Modified) {
            entry.Entity.UpdatedAt = DateTime.UtcNow;
            if (_currentUser.IsApiKey) {
                entry.Entity.UpdatedByApiKeyId = _currentUser.ApiKeyId;
            } else {
                entry.Entity.UpdatedByUserId = _currentUser.UserId;
            }
        }
    }
    return await base.SaveChangesAsync(cancellationToken);
}
```

## Acceptance Criteria

* Single user can register/login with JWT
* JWT tokens issued on successful login
* API keys can be generated and revoked
* Both JWT and API key authentication work
* Protected API endpoints accept both auth methods
* Refresh token flow working
* Passwords and API keys securely hashed
* Audit fields use foreign keys (referential integrity enforced)
* Can eager load User/ApiKey details in queries
* Navigation properties work correctly
* API keys shown only once on creation
* Expired/revoked API keys rejected
