---
identifier: MGG-35
title: "Frontend: Authentication UI & Protected Routes"
id: 11259c07-c1c4-452e-a830-b0ee5e9fdfe1
status: Done
priority:
  value: 1
  name: Urgent
createdBy: Michael Garofalo
project: Receipts
team: mggarofalo
labels:
  - security
  - frontend
milestone: "Phase 4 [Frontend]"
url: "https://linear.app/mggarofalo/issue/MGG-35/frontend-authentication-ui-and-protected-routes"
gitBranchName: mggarofalo/mgg-35-frontend-authentication-ui-protected-routes
createdAt: "2026-02-11T05:06:31.353Z"
updatedAt: "2026-02-21T16:52:38.672Z"
completedAt: "2026-02-21T16:52:38.657Z"
---

# Frontend: Authentication UI & Protected Routes

## Objective

Build login UI and implement authentication flow in React supporting both user login and API key management.

## Tasks

### User Authentication (JWT)

- [ ] Create auth context/store (Zustand or React Context)
- [ ] Implement auth API client functions (login, logout, refresh)
- [ ] Build Login page with shadcn/ui form components
- [ ] Add form validation (React Hook Form + Zod)
- [ ] Implement token storage (localStorage with XSS protection)
- [ ] Create axios/fetch interceptor for adding JWT to requests
- [ ] Implement automatic token refresh logic
- [ ] Create ProtectedRoute component (redirect to login if not authenticated)
- [ ] Add logout functionality with token cleanup
- [ ] Create auth loading state for initial app load
- [ ] Build simple user profile display (username, logout button)

### API Key Management UI

- [ ] Create API Keys management page (Settings or Profile section)
- [ ] Build "Generate API Key" dialog:
  - Name input (e.g., "Paperless Integration")
  - Optional expiration date
  - Generate button
- [ ] Display generated API key ONCE in modal:
  - Copy to clipboard button
  - Warning: "Save this key now, it won't be shown again"
  - Auto-select text for easy copying
- [ ] Build API Keys list view:
  - Table/list of existing API keys
  - Columns: Name, Created, Last Used, Expires, Status
  - Revoke button for each key
  - Confirmation dialog before revoke
- [ ] Add API key status indicators (active, expired, revoked)
- [ ] Show usage statistics (last used timestamp)
- [ ] Add search/filter for API keys
- [ ] Create API key revocation confirmation dialog

### Security & UX

- [ ] Add clipboard copy with toast notification
- [ ] Mask partial API key in list (show first 8 chars: "sk_1234...")
- [ ] Add expiration warnings (expires soon)
- [ ] Implement confirmation before closing "API Key Generated" modal
- [ ] Add keyboard shortcuts for API key management
- [ ] Ensure API key UI is accessible

## Example API Key Display

```tsx
<Dialog>
  <DialogContent>
    <h2>API Key Generated</h2>
    <Alert variant="warning">
      Save this key now. You won't be able to see it again.
    </Alert>
    <Input 
      readOnly 
      value={apiKey}
      onClick={(e) => e.target.select()}
    />
    <Button onClick={copyToClipboard}>
      Copy to Clipboard
    </Button>
  </DialogContent>
</Dialog>
```

## Acceptance Criteria

### User Auth

* User can login with username/password
* Invalid credentials show error message
* JWT automatically attached to API requests
* Token auto-refreshes before expiration
* Logout clears tokens and redirects to login
* Protected routes redirect unauthenticated users
* Auth state persists across page refresh

### API Key Management

* User can generate new API keys with custom names
* Generated API key shown once with copy button
* API keys list shows all user's keys
* User can revoke API keys
* Revoked keys immediately invalidated
* API key expiration displayed
* Copy to clipboard works reliably
* UI warns before closing generated key modal
