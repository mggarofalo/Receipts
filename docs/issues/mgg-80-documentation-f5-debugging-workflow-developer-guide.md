---
identifier: MGG-80
title: "Documentation: F5 Debugging Workflow & Developer Guide"
id: 0f543482-29cc-4b67-9e3a-4e2123a7c6c3
status: Done
priority:
  value: 2
  name: High
createdBy: Michael Garofalo
project: Receipts
team: mggarofalo
labels:
  - dx
  - docs
milestone: "Phase 3: Aspire Developer Experience"
url: "https://linear.app/mggarofalo/issue/MGG-80/documentation-f5-debugging-workflow-and-developer-guide"
gitBranchName: mggarofalo/mgg-80-documentation-f5-debugging-workflow-developer-guide
createdAt: "2026-02-11T05:44:18.075Z"
updatedAt: "2026-02-18T01:13:08.373Z"
completedAt: "2026-02-18T01:13:08.348Z"
---

# Documentation: F5 Debugging Workflow & Developer Guide

## Objective

Create comprehensive documentation for the Aspire-based local development and debugging workflow.

## Tasks

- [ ] Create `DEVELOPMENT.md` in repository root
- [ ] Document prerequisites:
  - .NET 10 SDK
  - Node.js 20+
  - Docker (for PostgreSQL, optional)
  - VS Code with C# extension
- [ ] Document initial setup:
  - Clone repository
  - Install .NET Aspire workload
  - Restore packages
  - npm install for frontend
- [ ] Document F5 debugging workflow:
  - Open VS Code
  - Press F5
  - What happens behind the scenes
  - Where to find each service
  - How to view logs/traces
- [ ] Document Aspire Dashboard usage:
  - Resources view
  - Traces view
  - Metrics view
  - Logs view
  - Console view
- [ ] Document hot reload:
  - API hot reload (change .cs files)
  - Frontend HMR (change .tsx files)
  - Database migration workflow
- [ ] Document troubleshooting:
  - Common errors and solutions
  - Port conflicts
  - Database connection issues
  - Frontend build failures
- [ ] Document AI agent integration:
  - Aspire MCP setup
  - Agent-browser usage
  - Example prompts for debugging
- [ ] Create quick reference guide
- [ ] Add architecture diagrams
- [ ] Include screenshots of dashboard

## Documentation Sections

1. **Getting Started**
2. **F5 Debugging Guide**
3. **Aspire Dashboard Overview**
4. **Hot Reload & Live Updates**
5. **AI Agent Testing**
6. **Troubleshooting**
7. **Advanced Topics**
8. **FAQ**

## Acceptance Criteria

* Complete documentation covers all workflows
* New developer can set up environment from docs
* F5 debugging clearly explained
* Aspire features documented with screenshots
* AI agent integration covered
* Troubleshooting guide helpful
* README links to [DEVELOPMENT.md](<http://DEVELOPMENT.md>)
