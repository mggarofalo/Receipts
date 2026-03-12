#!/usr/bin/env dotnet

using System.Diagnostics;

string composeProject = Path.GetFileName(GetRepoRoot()).ToLowerInvariant();
string volumeName = $"{composeProject}_db-data";
string repoRoot = GetRepoRoot();

Console.WriteLine($"Volume: {volumeName}");
Console.WriteLine();

// Check if volume exists
int inspectResult = await RunAsync("docker", ["volume", "inspect", volumeName], repoRoot);

if (inspectResult == 0)
{
    Console.WriteLine();
    Console.WriteLine("Disk usage:");
    int dfResult = await RunAsync("docker", ["system", "df", "-v"], repoRoot, grepPattern: volumeName, grepHeader: "VOLUME");

    if (dfResult != 0)
    {
        Console.WriteLine("  Run 'docker system df -v' for detailed usage");
    }
}
else
{
    Console.WriteLine("Volume not found. Has docker compose been run?");
}

return 0;

static async Task<int> RunAsync(string command, string[] arguments, string workingDirectory, string? grepPattern = null, string? grepHeader = null)
{
    ProcessStartInfo psi = new(command, arguments)
    {
        WorkingDirectory = workingDirectory,
        UseShellExecute = false,
        RedirectStandardOutput = grepPattern is not null,
    };

    using Process? process = Process.Start(psi);
    if (process is null)
    {
        return 1;
    }

    if (grepPattern is not null)
    {
        string output = await process.StandardOutput.ReadToEndAsync();
        bool found = false;
        foreach (string line in output.Split('\n'))
        {
            if ((grepHeader is not null && line.Contains(grepHeader)) || line.Contains(grepPattern))
            {
                Console.WriteLine(line);
                found = true;
            }
        }

        await process.WaitForExitAsync();
        return found ? 0 : 1;
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
