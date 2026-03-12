# Releases

Releases are automated via [release-please](https://github.com/googleapis/release-please). The repository uses the `simple` release type, treating the entire repo (backend + frontend) as a single releasable unit.

## How It Works

1. **Conventional commits drive versioning.** Every commit merged to `main` is parsed by release-please:
   - `feat:` bumps the minor version (or patch while on `0.x`)
   - `fix:` bumps the patch version
   - `feat!:` or `BREAKING CHANGE:` bumps the major version (or minor while on `0.x`)
2. **Release PR is opened automatically.** When conventional commits land on `main`, release-please opens (or updates) a single "Release PR" that contains:
   - Updated `CHANGELOG.md` with grouped, human-readable entries
   - Version bumps in `version.txt`, `Directory.Build.props`, and `src/client/package.json`
3. **Merging the Release PR creates a GitHub Release.** The merge triggers release-please to tag the commit and publish a GitHub Release with the changelog as the body.

## Key Files

| File | Purpose |
|------|---------|
| `version.txt` | Canonical version for the `simple` release type |
| `release-please-config.json` | Release-please configuration (release type, extra files, pre-major bump rules) |
| `.release-please-manifest.json` | Tracks the current released version |
| `Directory.Build.props` | Stamps .NET assembly version (updated via XML updater) |
| `src/client/package.json` | Stamps frontend version (updated via JSON updater) |
| `.github/workflows/release-please.yml` | GitHub Action that runs release-please |

## Triggering a Release

No manual steps are required. Simply merge PRs with conventional commit messages to `main`. Release-please handles the rest:
- If no Release PR exists, one is created
- If a Release PR already exists, it is updated with the new commits
- Merging the Release PR publishes the release

## Token Configuration

The workflow uses a `RELEASE_PLEASE_TOKEN` secret (fine-grained PAT) instead of the default `GITHUB_TOKEN`. This ensures that the Release PR itself triggers CI checks, which would not happen with `GITHUB_TOKEN` due to GitHub's recursive workflow prevention.
