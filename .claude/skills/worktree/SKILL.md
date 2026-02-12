---
name: worktree
description: Create a git worktree for a Linear issue. MUST be used for all branch-based work — the main repo must always stay on master.
argument-hint: [issue-identifier e.g. MGG-90]
allowed-tools: Bash(*), Read, Glob, Grep
---

# Worktree Skill

Create a git worktree for isolated, parallel development on a Linear issue branch.

## Rules

- **ALWAYS** create worktrees inside `.worktrees/` at the repository root — NEVER as sibling directories
- Worktree path format: `.worktrees/<branch-name>`
- Issue branches MUST branch off the **milestone branch**, not `master`
- If no argument is provided, ask the user which issue they want to work on

## Steps

1. **Resolve the issue**: Use the Linear MCP `get_issue` tool to fetch the issue by `$ARGUMENTS` (e.g., `MGG-90`). Extract the `gitBranchName` field and the milestone (phase).

2. **Validate prerequisites**:
   - Confirm the issue is not blocked (`blockedBy` should be empty or all Done)
   - Confirm the issue is not labeled `epic` (epics should not be worked directly)

3. **Ensure the milestone branch exists**:
   - The milestone branch is named `milestone/phase-N` (e.g., `milestone/phase-0`)
   - If it doesn't exist yet, create it from `master`:
     ```bash
     git branch milestone/phase-0 master
     ```

4. **Create the worktree** branching off the milestone branch:
   ```bash
   git worktree add .worktrees/<branch-name> -b <branch-name> milestone/phase-N
   ```
   - If the branch already exists remotely or locally, use:
     ```bash
     git worktree add .worktrees/<branch-name> <branch-name>
     ```

5. **Restore dependencies** in the new worktree:
   ```bash
   dotnet restore .worktrees/<branch-name>/Receipts.slnx
   ```

6. **Update issue status**: Move the Linear issue to "In Progress" using `update_issue`.

7. **Report**: Tell the user:
   - The worktree path (`.worktrees/<branch-name>`)
   - The branch name and its base (`milestone/phase-N`)
   - How to switch to it: `cd .worktrees/<branch-name>`
   - Remind: when done, squash-merge into the milestone branch (not master)

## Cleanup (when user asks to remove a worktree)

```bash
git worktree remove .worktrees/<branch-name>
git branch -d <branch-name>  # Only if merged into milestone branch
```

Use the `clean_gone` skill or `git worktree prune` to clean up stale entries.
