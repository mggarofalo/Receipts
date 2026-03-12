# MCP Server Setup

This project uses [Model Context Protocol (MCP)](https://modelcontextprotocol.io/) servers to give AI agents access to project management (Plane) and local dev orchestration (Aspire). The `.mcp.json` config file is **gitignored** because it contains API keys.

## Template `.mcp.json`

Create a `.mcp.json` file at the repository root with the following structure:

```json
{
  "mcpServers": {
    "aspire": {
      "command": "aspire",
      "args": ["mcp", "start"]
    },
    "plane": {
      "command": "uvx",
      "args": ["plane-mcp-server", "stdio"],
      "env": {
        "PLANE_API_KEY": "<your-plane-api-key>",
        "PLANE_WORKSPACE_SLUG": "dev",
        "PLANE_BASE_URL": "https://plane.wallingford.me"
      }
    }
  }
}
```

## Server Details

### Aspire

- **Purpose:** Interact with the .NET Aspire dev stack (list resources, read logs, manage services)
- **Prerequisite:** [Aspire CLI](https://aspire.dev/get-started/install-cli/) installed globally (`dotnet tool install --global Aspire.Cli`)
- **No API key required** — runs locally against the Aspire AppHost

### Plane

- **Purpose:** Project management — create/update/query work items, labels, milestones, cycles
- **Prerequisite:** Python `uvx` (from [uv](https://docs.astral.sh/uv/)) to run `plane-mcp-server`
- **Environment variables:**
  - `PLANE_API_KEY` — Generate from Plane: **Profile → API Tokens → Create Token**
  - `PLANE_WORKSPACE_SLUG` — `dev` (the workspace slug in the Plane URL)
  - `PLANE_BASE_URL` — `https://plane.wallingford.me` (the self-hosted instance URL)

## Claude Code Permissions

The file `.claude/settings.local.json` pre-approves MCP tool calls so agents don't face excessive approval prompts. Ensure it includes:

```json
{
  "permissions": {
    "allow": [
      "mcp__aspire__*",
      "mcp__plane__*"
    ]
  },
  "enableAllProjectMcpServers": true,
  "enabledMcpjsonServers": ["qmd", "aspire", "plane"]
}
```

This file uses the `.local.json` suffix and is gitignored — each developer maintains their own copy.

## Verification

After creating `.mcp.json`, restart Claude Code. You should see Plane MCP tools (e.g., `mcp__plane__list_projects`, `mcp__plane__list_work_items`) available in the tool list.
