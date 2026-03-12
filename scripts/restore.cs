#!/usr/bin/env dotnet

using System.Diagnostics;

string repoRoot = GetRepoRoot();
LoadEnvFile(Path.Combine(repoRoot, ".env"));

if (args.Length < 1)
{
    Console.Error.WriteLine("Usage: dotnet run scripts/restore.cs -- <backup_file.sql.gz>");
    Console.Error.WriteLine();
    Console.Error.WriteLine("Available backups:");

    string backupsDir = Path.Combine(repoRoot, "backups");
    if (Directory.Exists(backupsDir))
    {
        foreach (string file in Directory.GetFiles(backupsDir, "receipts_*.sql.gz").OrderDescending())
        {
            FileInfo fi = new(file);
            Console.Error.WriteLine($"  {fi.Name}  ({fi.Length / 1024.0:F1}K)");
        }
    }
    else
    {
        Console.Error.WriteLine("  No backups found in ./backups/");
    }

    return 1;
}

string backupFile = args[0];
if (!File.Exists(backupFile))
{
    Console.Error.WriteLine($"Error: Backup file not found: {backupFile}");
    return 1;
}

Console.WriteLine("WARNING: This will replace the current database with the backup.");
Console.WriteLine($"Backup: {backupFile}");
Console.Write("Continue? [y/N] ");
string? confirm = Console.ReadLine();
if (!string.Equals(confirm, "y", StringComparison.OrdinalIgnoreCase))
{
    Console.WriteLine("Aborted.");
    return 0;
}

string pgUser = Environment.GetEnvironmentVariable("POSTGRES_USER") ?? "receipts";
string pgDb = Environment.GetEnvironmentVariable("POSTGRES_DB") ?? "receipts";

Console.WriteLine("Stopping app...");
await RunAsync("docker", ["compose", "stop", "app"], repoRoot);

try
{
    Console.WriteLine($"Restoring database from {backupFile}...");

    // gunzip -c file | psql
    ProcessStartInfo gunzipPsi = new("gunzip", ["-c", backupFile])
    {
        RedirectStandardOutput = true,
        UseShellExecute = false,
    };

    using Process? gunzipProc = Process.Start(gunzipPsi);
    if (gunzipProc is null)
    {
        Console.Error.WriteLine("Error: Failed to start gunzip.");
        return 1;
    }

    ProcessStartInfo psqlPsi = new("docker", ["compose", "exec", "-T", "db", "psql", "-U", pgUser, "-d", pgDb, "--quiet", "--single-transaction"])
    {
        WorkingDirectory = repoRoot,
        RedirectStandardInput = true,
        UseShellExecute = false,
    };

    using Process? psqlProc = Process.Start(psqlPsi);
    if (psqlProc is null)
    {
        Console.Error.WriteLine("Error: Failed to start psql.");
        return 1;
    }

    await gunzipProc.StandardOutput.BaseStream.CopyToAsync(psqlProc.StandardInput.BaseStream);
    psqlProc.StandardInput.Close();

    await gunzipProc.WaitForExitAsync();
    await psqlProc.WaitForExitAsync();

    if (psqlProc.ExitCode != 0)
    {
        Console.Error.WriteLine("Error: Restore failed.");
        return 1;
    }

    Console.WriteLine("Restore complete.");
    return 0;
}
finally
{
    Console.WriteLine("Starting app...");
    await RunAsync("docker", ["compose", "start", "app"], repoRoot);
}

static async Task<int> RunAsync(string command, string[] arguments, string workingDirectory)
{
    ProcessStartInfo psi = new(command, arguments)
    {
        WorkingDirectory = workingDirectory,
        UseShellExecute = false,
    };

    using Process? process = Process.Start(psi);
    if (process is null)
    {
        return 1;
    }

    await process.WaitForExitAsync();
    return process.ExitCode;
}

static string GetRepoRoot()
{
    ProcessStartInfo psi = new("git", ["rev-parse", "--show-toplevel"])
    {
        RedirectStandardOutput = true,
        UseShellExecute = false,
    };

    using Process? proc = Process.Start(psi);
    string output = proc!.StandardOutput.ReadToEnd().Trim();
    proc.WaitForExit();
    return output;
}

static void LoadEnvFile(string path)
{
    if (!File.Exists(path))
    {
        return;
    }

    foreach (string line in File.ReadLines(path))
    {
        string trimmed = line.Trim();
        if (trimmed.Length == 0 || trimmed.StartsWith('#'))
        {
            continue;
        }

        int eq = trimmed.IndexOf('=');
        if (eq <= 0)
        {
            continue;
        }

        string key = trimmed[..eq];
        string value = trimmed[(eq + 1)..].Trim('"').Trim('\'');
        Environment.SetEnvironmentVariable(key, value);
    }
}
