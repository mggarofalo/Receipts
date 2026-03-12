---
identifier: MGG-78
title: Integrate Aspire MCP Server for AI Agent Access
id: e11092ac-0b77-49a5-a2a8-5d5eb74f1a2a
status: Done
priority:
  value: 1
  name: Urgent
createdBy: Michael Garofalo
project: Receipts
team: mggarofalo
labels:
  - dx
  - infra
milestone: "Phase 3: Aspire Developer Experience"
url: "https://linear.app/mggarofalo/issue/MGG-78/integrate-aspire-mcp-server-for-ai-agent-access"
gitBranchName: mggarofalo/mgg-78-integrate-aspire-mcp-server-for-ai-agent-access
createdAt: "2026-02-11T05:43:58.474Z"
updatedAt: "2026-02-18T10:33:54.942Z"
completedAt: "2026-02-18T10:33:54.927Z"
---

# Integrate Aspire MCP Server for AI Agent Access

## Objective

Set up Aspire MCP (Model Context Protocol) server to expose Aspire dashboard data to AI agents like Claude.

## Tasks

- [ ] Install Aspire MCP package:

  ```bash
  npm install -g @microsoft/aspire-mcp
  ```
- [ ] Configure MCP server in Claude Desktop config:

  ```json
  {
    "mcpServers": {
      "aspire": {
        "command": "npx",
        "args": ["-y", "@microsoft/aspire-mcp"],
        "env": {
          "ASPIRE_DASHBOARD_URL": "http://localhost:15888"
        }
      }
    }
  }
  ```
- [ ] Or add to AppHost as a resource (if available)
- [ ] Test MCP server connects to Aspire Dashboard
- [ ] Verify Claude can query dashboard via MCP:
  - "Show me recent API traces"
  - "What's the health status of services?"
  - "Show error logs from the last hour"
  - "What are the current metrics?"
- [ ] Document MCP setup for team
- [ ] Create example prompts for AI agent testing
- [ ] Add MCP server to F5 launch (optional)

## AI Agent Capabilities via MCP

* Query application metrics in real-time
* View traces and spans
* Access structured logs
* Check service health
* Monitor resource usage
* Analyze error patterns

## Example AI Agent Usage

```
You: "Are there any errors in the API?"
Claude: [queries Aspire via MCP] 
        "Yes, I see 3 errors in the last 10 minutes:
         - NullReferenceException in ReceiptsController
         - Database connection timeout
         - ..."
```

## Acceptance Criteria

* MCP server running and connected to Aspire
* Claude can query dashboard data
* Metrics, traces, and logs accessible via AI
* Example prompts documented
* Team can use AI for debugging/monitoring
