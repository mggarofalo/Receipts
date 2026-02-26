---
name: clone
description: Create a local clone for isolated development on a Linear issue branch.
argument-hint: [issue-identifier e.g. MGG-90]
allowed-tools: Bash(*), Read, Glob, Grep
---

# Clone Skill

Create a local clone (`git clone --local`) for isolated, parallel development on a Linear issue branch.

## Rules

- **ALWAYS** create clones inside `.clones/` at the repository root
- Clone path format: `.clones/<branch-name>`
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

4. **Create the local clone**:
   ```bash
   git clone --local . .clones/<branch-name>
   ```

5. **Create and checkout the issue branch** in the clone:
   ```bash
   cd .clones/<branch-name>
   git checkout -b <branch-name> milestone/phase-N
   ```
   - If the branch already exists locally or remotely:
     ```bash
     cd .clones/<branch-name>
     git checkout <branch-name>
     ```

6. **Restore dependencies** in the clone:
   ```bash
   dotnet restore .clones/<branch-name>/Receipts.slnx
   ```
   If the project has a `src/client/` with a `package.json`:
   ```bash
   npm install --prefix .clones/<branch-name>/src/client
   ```

7. **Update issue status**: Move the Linear issue to "In Progress" using `save_issue`.

8. **Report**: Tell the user:
   - The clone path (`.clones/<branch-name>`)
   - The branch name and its base (`milestone/phase-N`)
   - How to work in it: `cd .clones/<branch-name>`
   - Remind: when done, squash-merge into the milestone branch (not master)

## Cleanup (when user asks to remove a clone)

```bash
rm -rf .clones/<branch-name>
git branch -d <branch-name>  # Only if merged into milestone branch
```
