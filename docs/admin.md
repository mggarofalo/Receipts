# Admin Workflows

Admin capabilities are restricted to users with the **Admin** role. The first admin account is created during initial deployment via the database seeder.

## User management

Available at `/admin/users` in the UI and via `/api/users` endpoints.

### Create a user

```
POST /api/users
{
  "email": "user@example.com",
  "firstName": "Jane",
  "lastName": "Doe",
  "password": "SecurePassword123!",
  "role": "User"
}
```

Roles: `Admin`, `User`. New users must change their password on first login.

### Update a user

```
PUT /api/users/{userId}
{
  "email": "updated@example.com",
  "firstName": "Jane",
  "lastName": "Doe",
  "isDisabled": false,
  "role": "Admin"
}
```

### Disable a user

```
DELETE /api/users/{userId}
```

Soft-disables via account lockout. The user cannot log in but their data is preserved.

### Reset a password

```
POST /api/users/{userId}/reset-password
{ "newPassword": "NewSecurePassword123!" }
```

## Role management

```
GET    /api/users/{userId}/roles          # List roles
POST   /api/users/{userId}/roles/{role}   # Assign role
DELETE /api/users/{userId}/roles/{role}    # Remove role
```

## API keys

Available at `/admin/api-keys` in the UI and via `/api/apikeys` endpoints. API keys provide programmatic access without session tokens.

```
POST /api/apikeys
{
  "name": "Backup Script",
  "expiresAt": "2027-01-01T00:00:00Z",
  "bypassRateLimit": true
}
```

The `bypassRateLimit` flag requires the Admin role. The raw key is returned **only once** in the creation response.

### Key lifecycle

| Action | Endpoint |
|--------|----------|
| List keys | `GET /api/apikeys` |
| Create key | `POST /api/apikeys` |
| Revoke key | `DELETE /api/apikeys/{id}` |

Keys show `lastUsedAt` for monitoring. Revoked keys are immediately invalidated.

## Backup & Restore

See [backup-restore.md](backup-restore.md) for the full backup/restore guide. Both export and import require Admin.

## Audit trail

All admin and auth actions are logged to the audit trail:

- User registration and account changes
- Account disable/enable
- Password changes and resets
- API key creation and revocation
- Login attempts (success and failure)

Each entry records the acting user, IP address, user agent, and timestamp. The audit log is viewable at `/admin/audit-log` in the UI.
