#!/usr/bin/env dotnet

using System.Diagnostics;

string scriptDir = Path.GetDirectoryName(AppContext.BaseDirectory) is { } d
    ? d
    : Directory.GetCurrentDirectory();
string repoRoot = GetRepoRoot();

LoadEnvFile(Path.Combine(repoRoot, ".env"));

string backupDir = args.Length > 0 ? args[0] : Path.Combine(repoRoot, "backups");
int retentionDays = 7;
string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
string backupFile = Path.Combine(backupDir, $"receipts_{timestamp}.sql.gz");

Directory.CreateDirectory(backupDir);

string pgUser = Environment.GetEnvironmentVariable("POSTGRES_USER") ?? "receipts";
string pgDb = Environment.GetEnvironmentVariable("POSTGRES_DB") ?? "receipts";

Console.WriteLine($"Backing up database to {backupFile}...");

// pg_dump | gzip > file
ProcessStartInfo dumpPsi = new("docker", ["compose", "exec", "-T", "db", "pg_dump", "-U", pgUser, "-d", pgDb, "--clean", "--if-exists"])
{
    WorkingDirectory = repoRoot,
    RedirectStandardOutput = true,
    UseShellExecute = false,
};

using Process? dumpProc = Process.Start(dumpPsi);
if (dumpProc is null)
{
    Console.Error.WriteLine("Error: Failed to start pg_dump.");
    return 1;
}

ProcessStartInfo gzipPsi = new("gzip")
{
    RedirectStandardInput = true,
    RedirectStandardOutput = true,
    UseShellExecute = false,
};

using Process? gzipProc = Process.Start(gzipPsi);
if (gzipProc is null)
{
    Console.Error.WriteLine("Error: Failed to start gzip.");
    return 1;
}

// Pipe pg_dump stdout → gzip stdin, and gzip stdout → file
using (FileStream fs = File.Create(backupFile))
{
    Task pipeIn = dumpProc.StandardOutput.BaseStream.CopyToAsync(gzipProc.StandardInput.BaseStream)
        .ContinueWith(_ => gzipProc.StandardInput.Close());
    Task pipeOut = gzipProc.StandardOutput.BaseStream.CopyToAsync(fs);
    await Task.WhenAll(pipeIn, pipeOut);
}

await dumpProc.WaitForExitAsync();
await gzipProc.WaitForExitAsync();

if (dumpProc.ExitCode != 0)
{
    Console.Error.WriteLine("Error: pg_dump failed.");
    return 1;
}

FileInfo fi = new(backupFile);
Console.WriteLine($"Backup complete: {backupFile} ({fi.Length / 1024.0:F1}K)");

// Prune old backups
int pruned = 0;
DateTimeOffset cutoff = DateTimeOffset.Now.AddDays(-retentionDays);
foreach (string file in Directory.GetFiles(backupDir, "receipts_*.sql.gz"))
{
    if (File.GetLastWriteTimeUtc(file) < cutoff.UtcDateTime)
    {
        File.Delete(file);
        pruned++;
    }
}

if (pruned > 0)
{
    Console.WriteLine($"Pruned {pruned} backup(s) older than {retentionDays} days");
}

return 0;

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
