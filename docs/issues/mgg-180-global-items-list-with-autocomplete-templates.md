---
identifier: MGG-180
title: Global items list with autocomplete templates
id: 796e0473-5c10-4b6b-8892-2faa0807d40a
status: Done
priority:
  value: 3
  name: Medium
assignee: Michael Garofalo
createdBy: Michael Garofalo
project: Receipts
team: mggarofalo
labels:
  - frontend
  - backend
  - Feature
milestone: "Phase 4 [Frontend]"
url: "https://linear.app/mggarofalo/issue/MGG-180/global-items-list-with-autocomplete-templates"
gitBranchName: mggarofalo/mgg-180-global-items-list-with-autocomplete-templates
createdAt: "2026-02-25T10:45:26.440Z"
updatedAt: "2026-02-26T13:30:40.953Z"
completedAt: "2026-02-26T13:25:49.179Z"
attachments:
  - title: "feat: add item template management with autocomplete (MGG-180)"
    url: "https://github.com/mggarofalo/Receipts/pull/39"
---

# Global items list with autocomplete templates

Add a global items list that serves as reusable templates for receipt line items. When adding items to a receipt, autocomplete should suggest from this list and populate defaults that can be overridden.

## Requirements

* **Management page**: CRUD for global item templates (name, default category, default unit price, default unit, etc.)
* **Autocomplete on receipt item entry**: As the user types an item name, suggest matching global templates
* Selecting a template populates the line item form with template defaults
* All template-populated values can be overridden per receipt item
* Template items are distinct from receipt items — a template is a starting point, not a foreign-key link
