---
identifier: MGG-25
title: Add FluentAssertions NuGet package to test projects
id: 98930e57-c3b4-4156-bc4b-5da02948c434
status: Done
priority:
  value: 2
  name: High
createdBy: Michael Garofalo
project: Receipts
team: mggarofalo
url: "https://linear.app/mggarofalo/issue/MGG-25/add-fluentassertions-nuget-package-to-test-projects"
gitBranchName: mggarofalo/mgg-25-add-fluentassertions-nuget-package-to-test-projects
createdAt: "2026-02-11T04:59:18.169Z"
updatedAt: "2026-02-11T10:39:06.194Z"
completedAt: "2026-02-11T10:39:06.179Z"
---

# Add FluentAssertions NuGet package to test projects

## Setup

1. Add `FluentAssertions` to `Directory.Packages.props` (central package management)
2. Add `<PackageReference Include="FluentAssertions" />` to all 6 test `.csproj` files:
   * `tests/Domain.Tests/Domain.Tests.csproj`
   * `tests/Infrastructure.Tests/Infrastructure.Tests.csproj`
   * `tests/Presentation.Shared.Tests/Presentation.Shared.Tests.csproj`
   * `tests/Presentation.API.Tests/Presentation.API.Tests.csproj`
   * `tests/Application.Tests/Application.Tests.csproj`
   * `tests/Presentation.Client.Tests/Presentation.Client.Tests.csproj`
3. Run `dotnet build` to verify compilation

Commit: `chore: add FluentAssertions package to test projects`
