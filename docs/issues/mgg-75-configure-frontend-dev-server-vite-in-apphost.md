---
identifier: MGG-75
title: Configure Frontend Dev Server (Vite) in AppHost
id: c10d847e-4878-4caa-a977-d4b3cda7c0f5
status: Done
priority:
  value: 3
  name: Medium
createdBy: Michael Garofalo
project: Receipts
team: mggarofalo
labels:
  - dx
  - infra
milestone: "Phase 4 [Frontend]"
url: "https://linear.app/mggarofalo/issue/MGG-75/configure-frontend-dev-server-vite-in-apphost"
gitBranchName: mggarofalo/mgg-75-configure-frontend-dev-server-vite-in-apphost
createdAt: "2026-02-11T05:43:26.786Z"
updatedAt: "2026-02-21T15:03:33.498Z"
completedAt: "2026-02-21T15:03:33.485Z"
---

# Configure Frontend Dev Server (Vite) in AppHost

## Objective

Add Vite development server to AppHost with hot module replacement and API proxy configuration.

## Tasks

- [ ] Add Node.js app resource for Vite:

  ```csharp
  var frontend = builder.AddNpmApp("frontend", "../frontend")
      .WithHttpEndpoint(port: 5173, env: "PORT")
      .WithExternalHttpEndpoints()
      .PublishAsDockerFile();
  ```
- [ ] Configure frontend to reference API:

  ```csharp
  var frontend = builder.AddNpmApp("frontend", "../frontend")
      .WithReference(api)
      .WithEnvironment("VITE_API_URL", api.GetEndpoint("http"));
  ```
- [ ] Configure npm scripts in package.json:

  ```json
  {
    "scripts": {
      "dev": "vite",
      "dev:aspire": "vite --host 0.0.0.0"
    }
  }
  ```
- [ ] Configure Vite proxy to API (vite.config.ts):

  ```ts
  export default defineConfig({
    server: {
      proxy: {
        '/api': {
          target: process.env.VITE_API_URL || 'http://localhost:5000',
          changeOrigin: true
        }
      }
    }
  })
  ```
- [ ] Test frontend starts with HMR enabled
- [ ] Test API calls work through proxy
- [ ] Verify hot reload works (change React code, see updates)
- [ ] Configure environment variables for frontend

## Alternative: Standalone Vite

If Node.js integration is problematic, document manual start:

```csharp
// In AppHost, just start API + DB
// Manually run: npm run dev (in separate terminal)
// Or use VS Code tasks
```

## Acceptance Criteria

* Frontend dev server starts when AppHost runs
* Accessible at [http://localhost:5173](<http://localhost:5173>)
* HMR (hot module replacement) works
* API proxy configured correctly
* Environment variables passed to Vite
* Can make API calls from frontend
