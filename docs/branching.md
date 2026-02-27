# Branching Strategy

This project uses a hierarchical branching model: **milestone branches** for CI/PR gating, optional **parent branches** for epics, and **issue branches** for individual work items.

## Branch Types

### Milestone Branches

One per phase, named `milestone/phase-N` (e.g., `milestone/phase-0`):
- Created when work on a milestone begins
- All issue work within that phase merges locally into the milestone branch
- When the milestone is complete, open a **PR from the milestone branch to `master`**
- The PR triggers CI — this is the safety net that catches issues the agent may have missed
- After PR merge, delete the milestone branch

### Parent Branches (for epics)

When an epic has multiple child issues:
- Create a parent branch using the epic's `gitBranchName`
- Parent branch is created off `master` (or the milestone branch if one exists)
- Child issue branches are created off the parent branch and squash-merge back into it
- When all children are complete, the parent branch gets a PR to `master`
- This keeps related changes grouped and avoids polluting `master` with intermediate work

### Issue Branches

One per Linear issue:
- Branch off the parent branch (if epic) or milestone branch, NOT `master`
- Use the `gitBranchName` from the Linear issue
- Merge locally into the parent/milestone branch via squash merge (no PR needed)
- Delete the issue branch after merge

### Diagram

```
master
  ├── milestone/phase-0                              (PR → master)
  │     ├── mggarofalo/mgg-90-remove-blazor          (squash-merge into milestone)
  │     └── mggarofalo/mgg-82-update-ci              (squash-merge into milestone)
  │
  └── mggarofalo/mgg-83-replace-viewmodels-...       (epic parent, PR → master)
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

## PR: Parent/Milestone to Master

When all issues are complete, push the branch and open a PR:

```bash
git push -u origin mggarofalo/mgg-83-replace-viewmodels
gh pr create --title "Replace ViewModels with spec-generated DTOs" --body "..."
```

The PR triggers CI (build + test) — this is the checkpoint that surfaces issues.

After CI passes and the PR is approved, merge into `master`:

```bash
git branch -d mggarofalo/mgg-83-replace-viewmodels
git push origin --delete mggarofalo/mgg-83-replace-viewmodels
git pull   # update master with the merged PR
```

## Direct Commits to Master

Only use for non-Linear work like:
- Trivial typo fixes
- Documentation updates
- Tooling/build configuration

**NEVER** commit Linear-based work directly to master. When in doubt, create a branch.

## Directory Isolation

When you need to work on an issue branch without affecting the main repo, use `git clone --local` to create a lightweight local clone:

```bash
git clone --local . .clones/<branch-name>
```

- The clone hardlinks objects (fast, no network), and is a fully independent git repo
- `cd`, `git commit`, etc. all work normally
- Clones live in `.clones/` at the repo root (gitignored)
- Use `/clone <issue-id>` to create an isolated clone for an issue
- For simple/small changes, it's fine to work directly on the milestone branch without isolation
