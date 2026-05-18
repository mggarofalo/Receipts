## YOU MUST EXECUTE ALL STEPS BELOW

This is a **checklist you must execute**, not documentation. You are responsible for every step. After completing each step, move to the next one. If you stop early, the task is incomplete.

After any context compaction, re-read this file (`.claude/commands/release.md`) to get the full checklist back in context.

This project uses a **single-trunk, tag-driven** release model. There is no `develop` branch, no release PR, and no release-please. **Pushing an annotated `vX.Y.Z` tag on `main` IS the release.** See `docs/releases.md` for the model.

## Execution Modes

Detect which mode you are in **before starting Step 1**.

**Interactive mode** (default): You are running in a direct conversation with the user. Confirm the proposed version with the user before tagging.

**Autonomous mode**: You are running as a subagent, or the invoking prompt contains "autonomous", "plan approved", or "headless". In this mode:
- Do NOT use `EnterPlanMode` or `AskUserQuestion` — these block and you cannot receive interactive responses.
- Stop after Step 4 (version computed). Do NOT create or push the tag — report the proposed tag and the commit list so the parent/user can decide.

---

## Step 1: Fetch latest state

```bash
git fetch origin --tags --prune
```

Confirm `main` is current and you intend to release the tip of `origin/main`.

## Step 2: Find the latest release tag

```bash
git tag -l 'v*' --sort=-v:refname
```

The first entry is the latest release tag (e.g. `v0.5.0`). If there are **no** `v*` tags, treat the previous version as `v0.0.0` and the first release as `v0.1.0`.

## Step 3: List the commits since that tag

```bash
git log <lasttag>..origin/main --oneline
```

If there are **no commits**, stop and report: "Nothing to release — `main` is even with the latest tag." Otherwise, group the commits by Conventional Commit type (feat, fix, docs, refactor, test, chore) for the release summary.

## Step 4: Compute the suggested semver bump

The project is pre-1.0 (`0.x`), so the pre-major rules apply. Inspect the commit range from Step 3 and pick the highest-significance type present:

| Highest-significance commit | Pre-1.0 bump (`0.x`) | Post-1.0 bump |
|-----------------------------|----------------------|---------------|
| `feat!:` / `BREAKING CHANGE` footer | minor (`0.Y+1.0`) | major |
| `feat:`                     | patch (`0.Y.Z+1`)    | minor         |
| `fix:` / anything else      | patch (`0.Y.Z+1`)    | patch         |

Compute the next tag `vX.Y.Z` from the latest tag in Step 2.

- **Interactive:** Present the proposed version and the grouped commit summary, and ask the user to confirm (or override) the version before continuing.
- **Autonomous:** **Stop here.** Report the proposed tag, the latest tag, and the grouped commit list. Do not create or push the tag.

## Step 5: Create and push the annotated tag

Only after the version is confirmed (interactive mode).

```bash
git tag -a vX.Y.Z -m "vX.Y.Z"
git push origin vX.Y.Z
```

The tag must be **annotated** (`-a`) and must point at a commit on `origin/main`.

## Step 6: Report what the tag triggers

The tag push starts two independent workflows:

| Workflow | File | Result |
|----------|------|--------|
| Docker Publish | `.github/workflows/docker-publish.yml` | Multi-arch images pushed to GHCR |
| GitHub Release | `.github/workflows/github-release.yml` | GitHub Release with auto-generated notes |

The .NET assembly version is derived from the tag by MinVer; the Docker image version is passed as the `VERSION` build arg.

## Step 7: Monitor the workflows

```bash
gh run list --workflow docker-publish.yml --limit 1 --json status,conclusion,url
gh run list --workflow github-release.yml --limit 1 --json status,conclusion,url
```

Poll briefly (up to ~5 minutes for the release job, longer for Docker) until both complete. Then confirm the release exists:

```bash
gh release view vX.Y.Z
```

## Step 8: Report final status

```
## Release Summary

| Item | Value |
|------|-------|
| Previous tag | <lasttag> |
| New tag | vX.Y.Z |
| Bump | <major/minor/patch + reason> |
| Docker Publish | <running/completed + URL> |
| GitHub Release | <running/completed + URL> |

### Commits released
<grouped commit list from Step 3>
```

**You are done.**

---

## Rules

- **Never force-push** and never delete or move an existing tag.
- **Tag the tip of `origin/main`** — never an arbitrary commit.
- **Annotated tags only** (`git tag -a`) — lightweight tags break MinVer's release detection.
- **No compound Bash commands** — no `&&`, `||`, `;` in a single Bash call (hook constraint). Use separate sequential calls.
- **Autonomous mode stops at Step 4** — propose the version, do not tag.

## When to Use

- User says "release", "cut a release", "tag a release", "ship `main`".

## When NOT to Use

- User is asking about the release process without wanting to execute it.
- There is nothing new on `main` since the last tag.

## Edge Cases Handled

- **Nothing to release** — Step 3 detects no delta and short-circuits.
- **No existing tags** — Step 2 falls back to `v0.0.0` → `v0.1.0`.
- **Autonomous mode** — Step 4 stops with a proposal instead of tagging.
