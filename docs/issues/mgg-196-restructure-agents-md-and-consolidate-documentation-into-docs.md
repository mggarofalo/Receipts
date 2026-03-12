---
identifier: MGG-196
title: Restructure AGENTS.md and consolidate documentation into docs/
id: 95ac1cd9-de77-406d-aab8-9e6e7c0415dc
status: Done
priority:
  value: 4
  name: Low
assignee: Michael Garofalo
createdBy: Michael Garofalo
project: Receipts
team: mggarofalo
labels:
  - dx
  - docs
milestone: "Phase 3: Aspire Developer Experience"
url: "https://linear.app/mggarofalo/issue/MGG-196/restructure-agentsmd-and-consolidate-documentation-into-docs"
gitBranchName: mggarofalo/mgg-196-restructure-agentsmd-and-consolidate-documentation-into-docs
createdAt: "2026-02-27T11:16:24.093Z"
updatedAt: "2026-02-27T12:27:44.673Z"
completedAt: "2026-02-27T12:27:44.658Z"
attachments:
  - title: Restructure AGENTS.md and consolidate docs into docs/
    url: "https://github.com/mggarofalo/Receipts/pull/41"
---

# Restructure AGENTS.md and consolidate documentation into docs/

## Summary

[AGENTS.md](<http://AGENTS.md>) is 428 lines and mixes critical workflow rules with reference material, tutorials, stale archaeology, and generic advice. Every agent session loads the full file, but most content is only relevant to specific task types. Split it into a concise rules file (\~100-120 lines) with linked reference docs, and consolidate all non-AGENTS documentation into a `docs/` folder.

## Goals

* Reduce [AGENTS.md](<http://AGENTS.md>) to essential per-session rules (\~100-120 lines)
* Extract reference material into linked `docs/` files
* Consolidate scattered root-level and `documentation/` docs into `docs/`
* Remove stale Blazor/WASM/ViewModel references
* Remove generic advice sections (Library Migration, README Maintenance)
* Update [README.md](<http://README.md>) to reflect current state (API + React SPA)

---

## Checklist

### 1\. Create `docs/` directory structure and move existing docs

- [ ] Create `docs/` and `docs/design/` directories
- [ ] Move `LINEAR.md` → `docs/linear.md`
- [ ] Move `DEVELOPMENT.md` → `docs/development.md`
- [ ] Move `documentation/DbContextFactory.md` → `docs/design/dbcontext-factory.md`
- [ ] Move `documentation/PaperlessIntegration.md` → `docs/design/paperless-integration.md`
- [ ] Delete `documentation/Design.md` (deprecated Blazor/WASM doc — fully stale)
- [ ] Remove empty `documentation/` directory

### 2\. Create `docs/architecture.md` (extracted from [AGENTS.md](<http://AGENTS.md>))

- [ ] Extract **Architecture** section (layer structure, key patterns, database config)
- [ ] Extract **Mapperly Patterns** section (full code examples for mappers)
- [ ] Extract **Test Project Structure** note (test mirroring)
- [ ] Remove Blazor Client line (strikethrough `~~Removed~~`)
- [ ] Remove Shared layer ViewModel archaeology
- [ ] Update Presentation layer to reflect current state (API + React/Vite SPA)

### 3\. Create `docs/branching.md` (extracted from [AGENTS.md](<http://AGENTS.md>))

- [ ] Extract **Branch Strategy** section (full description with ASCII diagram)
- [ ] Extract **Merging Issue Work** section (squash-merge examples)
- [ ] Extract **PR: Parent/Milestone → Master** section (PR workflow with bash examples)
- [ ] Extract **Direct Commits to Master** section
- [ ] Extract **Directory isolation** section (clone-based isolation)

### 4\. Rewrite [AGENTS.md](<http://AGENTS.md>) (\~100-120 lines)

New structure:

- [ ] **Prerequisites** — keep as-is (6 lines)
- [ ] **Workflow Rules** — Linear: one-liner → `docs/linear.md`; Branching: one-liner + link → `docs/branching.md`; Commit convention: keep types/scopes table; OpenAPI spec-first: trim to 5-line summary + key files list
- [ ] **Build and Test Commands** — keep as-is
- [ ] **Pre-commit Hooks** — keep as-is
- [ ] **Architecture** — 3-line summary + link → `docs/architecture.md`
- [ ] **C# Coding Standards** — keep as-is (5 lines)
- [ ] **Mapperly Rules** — trim to 5-10 lines of essential rules only (no code examples): use Mapperly not AutoMapper; don't mock mappers; don't use `[UseMapper]`; ignore AdditionalProperties on generated DTOs; manual mapping for aggregates
- [ ] **Validation** — keep LSP checks section, trim slightly
- [ ] Remove **Library Migration Best Practices** section entirely
- [ ] Remove **README Maintenance** section entirely
- [ ] Remove all Blazor/WASM/ViewModel archaeology from [AGENTS.md](<http://AGENTS.md>)

### 5\. Rewrite `README.md` to reflect current state

- [ ] Update opening description to reflect full stack (API + React SPA)
- [ ] Add frontend to tech stack table (React, Vite, TypeScript, TanStack Query/Router, Tailwind, shadcn/ui)
- [ ] Update architecture tree: add `src/client/` (React SPA) and `src/AppHost/` (Aspire), remove Presentation/Shared
- [ ] Update `DEVELOPMENT.md` reference → `docs/development.md`
- [ ] Fix Swagger reference → Scalar (`/scalar` not `/swagger`)
- [ ] Replace Branching Strategy section with link to `docs/branching.md`
- [ ] Verify API endpoint tables against `openapi/spec.yaml` (update if stale)
- [ ] Remove all Blazor, WASM, ViewModel, and old design doc references

### 6\. Update cross-references in all files

- [ ] [AGENTS.md](<http://AGENTS.md>): `LINEAR.md` → `docs/linear.md`
- [ ] [README.md](<http://README.md>): `DEVELOPMENT.md` → `docs/development.md`
- [ ] `docs/development.md`: verify internal references ([AGENTS.md](<http://AGENTS.md>) stays at root — should still work)
- [ ] Scan all markdown files for broken cross-links (`git grep` for `.md` references)

### 7\. Verify [CLAUDE.md](<http://CLAUDE.md>)

- [ ] Confirm `CLAUDE.md` redirect to `AGENTS.md` still works (no changes needed)

---

## What NOT to change

* `README.md` stays at root (standard convention)
* `AGENTS.md` stays at root (Claude Code convention)
* `CLAUDE.md` stays at root (Claude Code convention)
* Private memory files (`react-client.md`, etc.) — untouched
* No frontend section added to [AGENTS.md](<http://AGENTS.md>) (stays in memory only)

## Verification

1. `AGENTS.md` is \~100-120 lines
2. All links in all markdown files resolve correctly
3. `docs/` contains: `linear.md`, `development.md`, `architecture.md`, `branching.md`
4. `docs/design/` contains: `dbcontext-factory.md`, `paperless-integration.md`
5. `documentation/` directory is removed
6. No Blazor/WASM/ViewModel references remain in any doc
7. No content is lost — everything is in a `docs/` file, trimmed with intent, or deliberately removed
8. `git grep` for broken markdown links returns nothing
