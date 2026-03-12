#!/usr/bin/env dotnet

using System.Diagnostics;
using System.Text.Json;

string repoRoot = GetRepoRoot();
LoadEnvFile(Path.Combine(repoRoot, ".env"));

string newTag = args.Length > 0 ? args[0] : "latest";
string appPort = Environment.GetEnvironmentVariable("APP_PORT") ?? "8080";
string healthUrl = $"http://localhost:{appPort}/api/health";
int maxWait = 60;

// Capture current image for rollback
(string currentImageId, string currentImageTag) = GetCurrentImage(repoRoot);
if (currentImageId.Length == 0 && currentImageTag.Length == 0)
{
    Console.WriteLine("Warning: Could not determine current image for rollback");
}

Console.WriteLine($"Updating to tag: {newTag}");

// Pre-update backup
Console.WriteLine("Creating pre-update backup...");
int backupExitCode = await RunAsync("dotnet", ["run", Path.Combine(repoRoot, "scripts", "backup.cs")], repoRoot);
if (backupExitCode != 0)
{
    Console.Error.WriteLine("Warning: Backup failed, continuing anyway...");
}

// Pull new image
Console.WriteLine("Pulling new image...");
Dictionary<string, string> tagEnv = new() { ["IMAGE_TAG"] = newTag };
await RunAsync("docker", ["compose", "pull", "app"], repoRoot, tagEnv);

// Restart with new image
Console.WriteLine("Restarting app...");
await RunAsync("docker", ["compose", "up", "-d", "app"], repoRoot, tagEnv);

// Wait for health check
Console.WriteLine($"Waiting for health check (max {maxWait}s)...");
using HttpClient http = new();
int elapsed = 0;
while (elapsed < maxWait)
{
    try
    {
        string body = await http.GetStringAsync(healthUrl);
        if (body.Contains("\"status\""))
        {
            Console.WriteLine($"Health check passed after {elapsed}s");
            Console.WriteLine($"Update to {newTag} complete.");
            return 0;
        }
    }
    catch
    {
        // Not ready yet
    }

    await Task.Delay(2000);
    elapsed += 2;
}

// Health check failed — rollback
Console.Error.WriteLine($"Health check failed after {maxWait}s. Rolling back...");

if (currentImageId.Length > 0)
{
    // Re-tag the previous image by ID so compose resolves it correctly
    string composeName = GetComposeImageName(repoRoot);
    if (composeName.Length > 0 && currentImageTag.Length > 0)
    {
        await RunAsync("docker", ["tag", currentImageId, $"{composeName}:{currentImageTag}"], repoRoot);
    }

    Dictionary<string, string> rollbackEnv = new() { ["IMAGE_TAG"] = currentImageTag.Length > 0 ? currentImageTag : "latest" };
    await RunAsync("docker", ["compose", "up", "-d", "app"], repoRoot, rollbackEnv);
    Console.Error.WriteLine($"Rolled back to image ID {currentImageId}");
}
else if (currentImageTag.Length > 0)
{
    Dictionary<string, string> rollbackEnv = new() { ["IMAGE_TAG"] = currentImageTag };
    await RunAsync("docker", ["compose", "up", "-d", "app"], repoRoot, rollbackEnv);
    Console.Error.WriteLine($"Rolled back to tag {currentImageTag} (warning: tag may have been overwritten)");
}
else
{
    Console.Error.WriteLine("ERROR: No previous image available for rollback. Manual intervention required.");
}

return 1;

static (string id, string tag) GetCurrentImage(string workingDirectory)
{
    ProcessStartInfo psi = new("docker", ["compose", "images", "app", "--format", "json"])
    {
        WorkingDirectory = workingDirectory,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false,
    };

    using Process? proc = Process.Start(psi);
    if (proc is null)
    {
        return ("", "");
    }

    string output = proc.StandardOutput.ReadToEnd().Trim();
    proc.WaitForExit();

    if (output.Length == 0)
    {
        return ("", "");
    }

    // Parse first line of JSON
    string firstLine = output.Split('\n')[0];
    try
    {
        using JsonDocument doc = JsonDocument.Parse(firstLine);
        string id = doc.RootElement.TryGetProperty("ID", out JsonElement idEl) ? idEl.GetString() ?? "" : "";
        string tag = doc.RootElement.TryGetProperty("Tag", out JsonElement tagEl) ? tagEl.GetString() ?? "" : "";
        return (id, tag);
    }
    catch
    {
        return ("", "");
    }
}

static string GetComposeImageName(string workingDirectory)
{
    ProcessStartInfo psi = new("docker", ["compose", "config", "--images"])
    {
        WorkingDirectory = workingDirectory,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false,
    };

    using Process? proc = Process.Start(psi);
    if (proc is null)
    {
        return "";
    }

    string output = proc.StandardOutput.ReadToEnd().Trim();
    proc.WaitForExit();

    if (output.Length == 0)
    {
        return "";
    }

    string firstLine = output.Split('\n')[0];
    int colonIdx = firstLine.LastIndexOf(':');
    return colonIdx > 0 ? firstLine[..colonIdx] : firstLine;
}

static async Task<int> RunAsync(string command, string[] arguments, string workingDirectory, Dictionary<string, string>? envVars = null)
{
    ProcessStartInfo psi = new(command, arguments)
    {
        WorkingDirectory = workingDirectory,
        UseShellExecute = false,
    };

    if (envVars is not null)
    {
        foreach (KeyValuePair<string, string> kvp in envVars)
        {
            psi.Environment[kvp.Key] = kvp.Value;
        }
    }

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
