# Releases

Releases are **tag-driven**. There is no release PR, no `develop` branch, and no
release-please. **Pushing a `vX.Y.Z` git tag on `main` IS the release.**

The tag is the single source of truth for the version:

- The .NET assembly version is derived from the tag by [MinVer](https://github.com/adamralph/minver).
- The Docker image version is the tag (minus the leading `v`), passed into the
  build as the `VERSION` build arg (the build context has no `.git`, so MinVer
  cannot run inside the image).
- `src/client/package.json` `version` is pinned to `0.0.0` and is **not** the
  real version — the client is never published to npm. The runtime app version
  for the frontend comes from the `VITE_APP_VERSION` build arg, which CI sets to
  the release tag.

## What a tag triggers

Pushing a tag matching `v*.*.*` to GitHub starts two independent workflows:

| Workflow | File | Result |
|----------|------|--------|
| Docker Publish | `.github/workflows/docker-publish.yml` | Multi-arch images pushed to GHCR |
| GitHub Release | `.github/workflows/github-release.yml` | GitHub Release with auto-generated notes |

The GitHub Release job is idempotent — if a release for the tag already exists it
skips creation.

## Cutting a release

The `/release` command automates every step below. To do it by hand:

### 1. Make sure `main` is current

```bash
git checkout main
git fetch origin --tags
git pull --ff-only
```

### 2. Find the latest release tag

```bash
git tag -l 'v*' --sort=-v:refname | head -n1
```

### 3. List the commits since that tag

```bash
git log <lasttag>..origin/main --oneline
```

### 4. Compute the next semver

The bump is derived from the Conventional Commit types in that range. The project
is pre-1.0 (`0.x`), so the pre-major rules apply:

| Highest-significance commit | Pre-1.0 bump (`0.x`) | Post-1.0 bump |
|-----------------------------|----------------------|---------------|
| `feat!:` / `BREAKING CHANGE` | minor (`0.Y+1.0`)    | major         |
| `feat:`                     | patch (`0.Y.Z+1`)    | minor         |
| `fix:` / anything else      | patch (`0.Y.Z+1`)    | patch         |

### 5. Create and push the annotated tag

```bash
git tag -a vX.Y.Z -m "vX.Y.Z"
git push origin vX.Y.Z
```

That is the entire release. The tag push triggers the Docker publish and the
GitHub Release. Monitor them:

```bash
gh run list --workflow docker-publish.yml --limit 1
gh run list --workflow github-release.yml --limit 1
gh release view vX.Y.Z
```

## Notes

- Tags must be pushed against a commit on `main`.
- Use **annotated** tags (`git tag -a`) so the release has a message and MinVer
  treats it as a release version rather than a height-based pre-release.
- Untagged builds (local dev, CI on PRs) produce a MinVer pre-release version such
  as `0.5.1-alpha.0.3` — this is expected.
