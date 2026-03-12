---
identifier: MGG-237
title: Update AGENTS.md with worktree setup instructions
id: 34c8bd02-49d9-4f1b-929c-ba69126f8c07
status: Done
priority:
  value: 3
  name: Medium
assignee: Michael Garofalo
createdBy: Michael Garofalo
project: Receipts
team: mggarofalo
labels:
  - dx
  - docs
url: "https://linear.app/mggarofalo/issue/MGG-237/update-agentsmd-with-worktree-setup-instructions"
gitBranchName: mggarofalo/mgg-237-update-agentsmd-with-worktree-setup-instructions
createdAt: "2026-03-05T11:39:20.618Z"
updatedAt: "2026-03-05T16:21:21.784Z"
completedAt: "2026-03-05T16:21:21.761Z"
attachments:
  - title: "docs: add worktree setup instructions and bootstrap script (MGG-237)"
    url: "https://github.com/mggarofalo/Receipts/pull/89"
---

# Update AGENTS.md with worktree setup instructions

## Problem

Agents working in git worktrees don't have clear guidance on what setup steps are needed. Worktrees share the `.git` directory but NOT `node_modules`, `bin/obj` folders, or other build artifacts. Agents frequently hit issues like missing `node_modules` and don't know the full set of commands needed to bootstrap a worktree.

Additionally, there's no documented convention for worktree branch naming, no guidance on detecting whether the agent is already in a worktree, and no recommended permission settings to avoid excessive approval prompts for routine operations.

## Requirements

- [ ] Add a "Worktree Setup" section to [AGENTS.md](<http://AGENTS.md>) documenting all required steps after creating/entering a worktree
- [ ] Audit the full repo to identify every directory that needs bootstrapping (e.g., `npm install` in `src/client/`, `dotnet restore` at solution level, any other tool installs)
- [ ] Document the commands in the correct order
- [ ] Note any caveats (e.g., files that ARE shared via symlinks vs files that are NOT)
- [ ] Consider whether a single bootstrap script (`scripts/worktree-setup.sh` or similar) would be more reliable than listing individual commands
- [ ] Define a branch naming convention for worktree branches (e.g., how they relate to issue branches, whether they should include the worktree name, etc.)
- [ ] Add instructions for agents to detect if they're already in a worktree before attempting file reads (e.g., `git rev-parse --show-toplevel` or checking for `.git` file vs directory)
- [ ] Document recommended permission settings (e.g., `settings.local.json` or `.claude/settings.json`) that auto-allow read operations, MCP tool calls (e.g., Linear), and other routine non-destructive actions so agents aren't blocked by unnecessary approval prompts
