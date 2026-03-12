---
identifier: MGG-69
title: "Frontend: Audit History & Change Tracking UI"
id: 162c1a17-c889-4f3a-a8a0-1692ca13c433
status: Done
priority:
  value: 3
  name: Medium
createdBy: Michael Garofalo
project: Receipts
team: mggarofalo
labels:
  - security
  - frontend
milestone: "Phase 4 [Frontend]"
url: "https://linear.app/mggarofalo/issue/MGG-69/frontend-audit-history-and-change-tracking-ui"
gitBranchName: mggarofalo/mgg-69-frontend-audit-history-change-tracking-ui
createdAt: "2026-02-11T05:30:11.561Z"
updatedAt: "2026-02-22T02:40:58.357Z"
completedAt: "2026-02-22T02:40:58.343Z"
---

# Frontend: Audit History & Change Tracking UI

## Objective

Build comprehensive UI for viewing audit logs, change history, and deleted items.

## Tasks

### Entity History Panel

- [ ] Create `ChangeHistory` component for individual entities
- [ ] Add "History" tab to detail views (Receipt, Transaction, etc.)
- [ ] Display timeline of changes:
  - Who changed it (user or API key name)
  - When it was changed
  - What fields changed
  - Before/after values
- [ ] Show create, update, delete, restore events
- [ ] Add filtering by date range
- [ ] Add field-level change highlighting
- [ ] Implement side-by-side diff view for text fields
- [ ] Show JSON diff for complex fields

### Global Audit Log Viewer

- [ ] Create `/audit` page (admin/settings section)
- [ ] Build audit log table with columns:
  - Timestamp
  - Entity Type
  - Entity (link to entity)
  - Action (Create/Update/Delete/Restore)
  - Changed By
  - 

    # of fields changed
- [ ] Add filters:
  - Date range picker
  - Entity type dropdown
  - Action type (Create/Update/Delete)
  - User filter
- [ ] Add search by entity ID
- [ ] Implement pagination or infinite scroll
- [ ] Add export to CSV functionality
- [ ] Show expandable row with field changes

### Deleted Items Viewer (Recycle Bin)

- [ ] Create `/trash` or `/deleted` page
- [ ] List all soft-deleted items by entity type
- [ ] Show tabs for each entity type (Receipts, Transactions, etc.)
- [ ] Display:
  - Deleted item details
  - Who deleted it
  - When it was deleted
- [ ] Add "Restore" button for each item
- [ ] Add "Permanently Delete" option (with confirmation)
- [ ] Add bulk restore functionality
- [ ] Add search and filter for deleted items
- [ ] Show preview of deleted item data

### Authentication Audit Log Viewer

- [ ] Create `/security` or `/auth-log` page
- [ ] Display user's authentication events:
  - Login history (success/failed)
  - API key usage
  - Recent sessions
- [ ] Show suspicious activity warnings:
  - Failed login attempts
  - Unusual IP addresses
  - Multiple failed attempts
- [ ] Add filtering by event type
- [ ] Display IP address and location (optional)
- [ ] Show user agent/device info

### Change Diff Component

- [ ] Create reusable `FieldDiff` component
- [ ] Show before/after side-by-side for text
- [ ] Highlight changed characters/words
- [ ] Format numbers, dates, currencies appropriately
- [ ] Handle null/empty values
- [ ] Support JSON diffs with syntax highlighting

### UX Enhancements

- [ ] Add keyboard shortcuts for audit views
- [ ] Make all audit views accessible
- [ ] Add loading states and skeletons
- [ ] Implement real-time updates (SignalR) for new audit events
- [ ] Add toast notifications for restore actions
- [ ] Create timeline visualization for entity history

## Example History Timeline UI

```tsx
<Timeline>
  <TimelineItem icon={<Edit />}>
    <TimelineHeader>
      <strong>John Doe</strong> updated this receipt
      <TimeStamp>2 hours ago</TimeStamp>
    </TimelineHeader>
    <FieldChange 
      field="Amount" 
      oldValue="$50.00" 
      newValue="$75.00" 
    />
  </TimelineItem>
</Timeline>
```

## Acceptance Criteria

* History tab shows all changes to entity
* Global audit log filterable and searchable
* Deleted items viewable and restorable
* Auth log shows login history
* Diff view clearly shows what changed
* All views performant with large datasets
* Keyboard navigation works
* Accessible to screen readers
* Real-time updates for new changes
