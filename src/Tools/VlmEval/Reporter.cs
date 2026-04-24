using System.Globalization;
using Microsoft.Extensions.Logging;

namespace VlmEval;

public sealed class Reporter(ILogger<Reporter> logger)
{
	public void PrintHeader(string ollamaUrl, string fixturesPath)
	{
		logger.LogInformation("VLM accuracy eval — {Timestamp}", DateTimeOffset.UtcNow.ToString("u", CultureInfo.InvariantCulture));
		logger.LogInformation("Ollama base URL: {Url}", ollamaUrl);
		logger.LogInformation("Fixtures directory: {Path}", fixturesPath);
	}

	public void PrintOllamaUnreachable(string ollamaUrl)
	{
		logger.LogError("Ollama is not reachable at {Url}. Verify the vlm-ocr container is running.", ollamaUrl);
	}

	public void PrintEmptyFixturesDirectory(string path)
	{
		logger.LogWarning(
			"No fixtures found. Drop a receipt file (.jpg/.jpeg/.png/.pdf) and a <name>.<ext>.expected.json sidecar at: {Path}",
			path);
	}

	public void PrintNoValidFixtures()
	{
		logger.LogWarning("No valid fixtures (all candidate files were malformed or missing sidecars).");
	}

	public void PrintOrphan(string filePath)
	{
		logger.LogWarning(
			"Fixture {FilePath} has no companion {Sidecar}; skipping.",
			Path.GetFileName(filePath),
			Path.GetFileName(filePath) + ".expected.json");
	}

	public void PrintFixtureResult(FixtureResult result)
	{
		string status = result.Passed ? "PASS" : "FAIL";
		string elapsed = FormatElapsed(result.Elapsed);

		if (result.Error is not null)
		{
			logger.LogError("[{Status}] {Name}  {Elapsed}  ERROR: {Error}", status, result.FixtureName, elapsed, result.Error);
			return;
		}

		string summary = FormatSummary(result.FieldDiffs);
		if (result.Passed)
		{
			logger.LogInformation("[{Status}] {Name}  {Elapsed}  {Summary}", status, result.FixtureName, elapsed, summary);
		}
		else
		{
			logger.LogError("[{Status}] {Name}  {Elapsed}  {Summary}", status, result.FixtureName, elapsed, summary);
			foreach (FieldDiff diff in result.FieldDiffs.Where(d => d.Status == DiffStatus.Fail))
			{
				logger.LogError(
					"    {Field}: expected={Expected} actual={Actual}{Detail}",
					diff.Field,
					diff.Expected ?? "(none)",
					diff.Actual ?? "(none)",
					diff.Detail is null ? string.Empty : "  " + diff.Detail);
			}
		}
	}

	public void PrintSummary(IReadOnlyList<FixtureResult> results, TimeSpan totalElapsed)
	{
		int passed = results.Count(r => r.Passed);
		int failed = results.Count - passed;
		double rate = results.Count == 0 ? 0.0 : 100.0 * passed / results.Count;

		logger.LogInformation(
			"Summary: {Passed}/{Total} fixtures passed ({Rate:F0}%)  elapsed={Elapsed}",
			passed,
			results.Count,
			rate,
			FormatElapsed(totalElapsed));

		if (failed > 0)
		{
			logger.LogError(
				"Failed: {Names}",
				string.Join(", ", results.Where(r => !r.Passed).Select(r => r.FixtureName)));
		}
	}

	private static string FormatElapsed(TimeSpan elapsed)
	{
		return elapsed.TotalSeconds < 60
			? $"{elapsed.TotalSeconds:F1}s"
			: $"{(int)elapsed.TotalMinutes}m{elapsed.Seconds:D2}s";
	}

	private static string FormatSummary(IReadOnlyList<FieldDiff> diffs)
	{
		List<string> parts = [];
		foreach (FieldDiff d in diffs)
		{
			string tag = d.Status switch
			{
				DiffStatus.Pass => "ok",
				DiffStatus.Fail => "FAIL",
				_ => null!,
			};
			if (tag is null)
			{
				continue;
			}

			parts.Add($"{d.Field}:{tag}");
		}
		return string.Join(" ", parts);
	}
}
