#!/usr/bin/env dotnet

using System.Diagnostics;
using System.Runtime.InteropServices;

string repoRoot = GetRepoRoot();

// Clean up old test results and coverage reports
string coverageReportDir = Path.Combine(repoRoot, "coveragereport");
if (Directory.Exists(coverageReportDir))
{
    Directory.Delete(coverageReportDir, recursive: true);
}

DeleteMatchingFiles(repoRoot, "coverage.cobertura.xml");
DeleteMatchingDirs(repoRoot, "TestResults");

Console.WriteLine("Running tests with code coverage...");
string runSettingsPath = Path.Combine(repoRoot, "scripts", "tests", "coverlet.runsettings");
int testExitCode = await RunAsync("dotnet", ["test", "--collect:XPlat Code Coverage", $"--settings:{runSettingsPath}"], repoRoot);

if (testExitCode != 0)
{
    Console.Error.WriteLine("Tests failed.");
    return testExitCode;
}

// Find all coverage files
List<string> coverageFiles = Directory.GetFiles(repoRoot, "coverage.cobertura.xml", SearchOption.AllDirectories).ToList();

if (coverageFiles.Count == 0)
{
    Console.Error.WriteLine("Error: No coverage files found");
    return 1;
}

Console.WriteLine("Coverage files found:");
foreach (string file in coverageFiles)
{
    Console.WriteLine($"  {file}");
}

Console.WriteLine("Generating coverage report...");
int reportExitCode = await RunAsync("reportgenerator",
    ["-reports:**/coverage.cobertura.xml", $"-targetdir:{coverageReportDir}", "-reporttypes:Html"], repoRoot);

if (reportExitCode != 0)
{
    Console.Error.WriteLine("Report generation failed.");
    return reportExitCode;
}

string reportPath = Path.Combine(coverageReportDir, "index.html");
if (File.Exists(reportPath))
{
    Console.WriteLine("Opening coverage report...");
    if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
    {
        Process.Start("open", [reportPath]);
    }
    else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
    {
        Process.Start("xdg-open", [reportPath]);
    }
    else
    {
        Console.Error.WriteLine("Unsupported OS for automatic browser opening.");
    }

    Console.WriteLine("Coverage report opened in browser.");
}
else
{
    Console.Error.WriteLine($"Error: Coverage report not found at {reportPath}");
    return 1;
}

Console.WriteLine("Code coverage test completed.");
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

static void DeleteMatchingFiles(string root, string fileName)
{
    foreach (string file in Directory.GetFiles(root, fileName, SearchOption.AllDirectories))
    {
        File.Delete(file);
    }
}

static void DeleteMatchingDirs(string root, string dirName)
{
    foreach (string dir in Directory.GetDirectories(root, dirName, SearchOption.AllDirectories))
    {
        Directory.Delete(dir, recursive: true);
    }
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
