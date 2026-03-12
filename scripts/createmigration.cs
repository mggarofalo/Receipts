#!/usr/bin/env dotnet

using System.Diagnostics;

if (args.Length == 0)
{
    Console.Error.WriteLine("Usage: dotnet run scripts/createmigration.cs -- <MigrationName>");
    return 1;
}

string migrationName = args[0];

if (string.IsNullOrWhiteSpace(migrationName))
{
    Console.Error.WriteLine("Error: Migration name cannot be empty.");
    return 1;
}

ProcessStartInfo psi = new("dotnet", ["ef", "migrations", "add", migrationName, "--output-dir", "Migrations", "--project", "src/Infrastructure/Infrastructure.csproj"])
{
    RedirectStandardOutput = false,
    RedirectStandardError = false,
    UseShellExecute = false,
};

using Process? process = Process.Start(psi);
if (process is null)
{
    Console.Error.WriteLine("Error: Failed to start dotnet ef.");
    return 1;
}

await process.WaitForExitAsync();

if (process.ExitCode == 0)
{
    Console.WriteLine($"Migration '{migrationName}' created successfully.");
}

return process.ExitCode;
