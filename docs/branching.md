# Branching Strategy

This project uses a hierarchical branching model: **milestone branches** for CI/PR gating, optional **parent branches** for epics, and **issue branches** for individual work items.

## Branch Types

### Milestone Branches

One per phase, named `milestone/phase-N` (e.g., `milestone/phase-0`):
- Created when work on a milestone begins
- All issue work within that phase merges locally into the milestone branch
- When the milestone is complete, open a **PR from the milestone branch to `main`**
- The PR triggers CI — this is the safety net that catches issues the agent may have missed
- After PR merge, delete the milestone branch

### Parent Branches (for epics)

When an epic has multiple child issues:
- Create a parent branch using the epic's identifier (e.g., `mggarofalo/mgg-83-description`)
- Parent branch is created off `main` (or the milestone branch if one exists)
- Child issue branches are created off the parent branch and squash-merge back into it
- When all children are complete, the parent branch gets a PR to `main`
- This keeps related changes grouped and avoids polluting `main` with intermediate work

### Issue Branches

One per tracked issue:
- Branch off the parent branch (if epic) or milestone branch, NOT `main`
- Use the issue identifier to form a branch name (e.g., `mggarofalo/mgg-123-short-description`)
- Merge locally into the parent/milestone branch via squash merge (no PR needed)
- Delete the issue branch after merge

### Diagram

```
main
  ├── milestone/phase-0                              (PR → main)
  │     ├── mggarofalo/mgg-90-remove-blazor          (squash-merge into milestone)
  │     └── mggarofalo/mgg-82-update-ci              (squash-merge into milestone)
  │
  └── mggarofalo/mgg-83-replace-viewmodels-...       (epic parent, PR → main)
        ├── mggarofalo/mgg-88-generate-dtos          (squash-merge into parent)
        └── mggarofalo/mgg-87-update-docs            (squash-merge into parent)
```

## Merging Issue Work into Parent/Milestone Branch

From the main repo (or the parent/milestone clone), squash-merge the issue branch:

```bash
git checkout milestone/phase-0
git merge --squash mggarofalo/mgg-88-generate-dtos
git commit -m "feat(api): generate DTOs from OpenAPI spec (MGG-88)"
git branch -D mggarofalo/mgg-88-generate-dtos
```

If using a clone for the issue, delete it after merge:

```bash
rm -rf .clones/<branch-name>
```

## PR: Parent/Milestone to Main

When all issues are complete, push the branch and open a PR:

```bash
git push -u origin mggarofalo/mgg-83-replace-viewmodels
gh pr create --title "Replace ViewModels with spec-generated DTOs" --body "..."
```

The PR triggers CI (build + test) — this is the checkpoint that surfaces issues.

After CI passes and the PR is approved, merge into `main`:

```bash
git branch -d mggarofalo/mgg-83-replace-viewmodels
git push origin --delete mggarofalo/mgg-83-replace-viewmodels
git pull   # update main with the merged PR
```

## Direct Commits to Main

Only use for non-tracked work like:
- Trivial typo fixes
- Documentation updates
- Tooling/build configuration

**NEVER** commit tracked-issue work directly to main. When in doubt, create a branch.

## Directory Isolation

Two mechanisms are available for working on issue branches without affecting the main repo:

### Git Worktrees (preferred for AI agents)

`git worktree add` creates a linked working tree sharing the same `.git` directory:

```bash
git worktree add .claude/worktrees/<branch-name> -b <branch-name>
```

- Shares git history and refs with the main worktree (no duplication)
- Requires bootstrapping: `dotnet restore`, `npm install`, `npm install` in `src/client/`
- Claude Code's `isolation: "worktree"` parameter automates this for subagents
- Worktrees live in `.claude/worktrees/` (gitignored)
- See [AGENTS.md](../AGENTS.md#worktree-setup) for bootstrap commands

### Local Clones (alternative)

`git clone --local` creates a lightweight local clone:

```bash
git clone --local . .clones/<branch-name>
```

- Hardlinks objects (fast, no network), fully independent git repo
- `cd`, `git commit`, etc. all work normally
- Clones live in `.clones/` at the repo root (gitignored)

### When to Use Which

- **Worktrees** — preferred for parallel AI agent work (faster setup, shared git state)
- **Local clones** — preferred for human developers who want full independence
- **Neither** — for simple/small changes, it's fine to work directly on the milestone branch
