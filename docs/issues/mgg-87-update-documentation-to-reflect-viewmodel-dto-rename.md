---
identifier: MGG-87
title: Update documentation to reflect ViewModel → DTO rename
id: 790b161b-8aad-4b88-8bb4-763f621fb64f
status: Done
priority:
  value: 3
  name: Medium
createdBy: Michael Garofalo
project: Receipts
team: mggarofalo
labels:
  - docs
milestone: "Phase 2: Backend DTO Generation"
url: "https://linear.app/mggarofalo/issue/MGG-87/update-documentation-to-reflect-viewmodel-dto-rename"
gitBranchName: mggarofalo/mgg-87-update-documentation-to-reflect-viewmodel-dto-rename
createdAt: "2026-02-11T11:22:57.509Z"
updatedAt: "2026-02-15T02:05:40.124Z"
completedAt: "2026-02-15T02:05:40.099Z"
---

# Update documentation to reflect ViewModel → DTO rename

## Scope

Update all documentation, comments, and guidance to use DTO terminology instead of ViewModel:

## Tasks

- [ ] Update `AGENTS.md`:
  - Architecture section mentions ViewModels
  - Mapperly patterns section references VMs
  - Update folder structure descriptions
- [ ] Update `README.md` if it mentions ViewModels
- [ ] Search codebase for "ViewModel" or "VM" in comments
- [ ] Update any XML documentation comments that reference ViewModels
- [ ] Update any commit message templates or contribution guidelines
- [ ] Consider updating [MEMORY.md](<http://MEMORY.md>) if it has ViewModel references

## Search Commands

```bash
# Find documentation references
grep -r "ViewModel" *.md
grep -r "\\bVM\\b" *.md

# Find code comments
grep -r "ViewModel" --include="*.cs"
```

## Dependencies

* Should be done after all code changes are complete

## Notes

* This is the final cleanup step
* Ensures future developers use correct terminology
* Double-check [AGENTS.md](<http://AGENTS.md>) since it's the primary agent guidance
