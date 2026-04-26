using System.Diagnostics;
using Infrastructure.Services;
using Microsoft.Extensions.Logging;

namespace VlmEval;

public sealed class EvalRunner(
	IHttpClientFactory httpClientFactory,
	VlmOcrOptions vlmOptions,
	FixtureLoader fixtureLoader,
	FixtureEvaluator fixtureEvaluator,
	Reporter reporter,
	VlmEvalOptions options,
	ILogger<EvalRunner> logger)
{
	/// <summary>POSIX exit code for SIGINT (cancellation).</summary>
	private const int ExitCodeCancelled = 130;

	public async Task<int> RunAsync(string fixturesDirectory, CancellationToken cancellationToken)
	{
		string ollamaUrl = vlmOptions.OllamaUrl ?? "(unset)";
		DateTimeOffset startedAt = DateTimeOffset.UtcNow;
		reporter.PrintHeader(ollamaUrl, fixturesDirectory);

		// Missing fixtures dir is a configuration error: a typo'd FixturesPath looks identical
		// to "no fixtures yet" if we silently mkdir. Treat it as failure when the strict flag
		// is set; otherwise warn and exit 0 to preserve the "always green when flag off" contract.
		if (!Directory.Exists(fixturesDirectory))
		{
			reporter.PrintMissingFixturesDirectory(fixturesDirectory);
			reporter.WriteReport(
				new RunInfo(startedAt, ollamaUrl, fixturesDirectory),
				results: [],
				totalElapsed: TimeSpan.Zero,
				cancelled: false);
			return options.FailOnAnyFixtureFailure ? 1 : 0;
		}

		if (!await IsOllamaReachableAsync(cancellationToken))
		{
			reporter.PrintOllamaUnreachable(ollamaUrl);
			reporter.WriteReport(
				new RunInfo(startedAt, ollamaUrl, fixturesDirectory),
				results: [],
				totalElapsed: TimeSpan.Zero,
				cancelled: false);
			return 1;
		}

		LoadedFixtures loaded = fixtureLoader.LoadFrom(fixturesDirectory);

		if (loaded.Fixtures.Count == 0 && loaded.OrphanFiles.Count == 0)
		{
			reporter.PrintEmptyFixturesDirectory(fixturesDirectory);
			reporter.WriteReport(
				new RunInfo(startedAt, ollamaUrl, fixturesDirectory),
				results: [],
				totalElapsed: TimeSpan.Zero,
				cancelled: false);
			return options.FailOnAnyFixtureFailure ? 1 : 0;
		}

		foreach (string orphan in loaded.OrphanFiles)
		{
			reporter.PrintOrphan(orphan);
		}

		if (loaded.Fixtures.Count == 0)
		{
			reporter.PrintNoValidFixtures();
			reporter.WriteReport(
				new RunInfo(startedAt, ollamaUrl, fixturesDirectory),
				results: [],
				totalElapsed: TimeSpan.Zero,
				cancelled: false);
			return options.FailOnAnyFixtureFailure ? 1 : 0;
		}

		Stopwatch total = Stopwatch.StartNew();
		List<FixtureResult> results = [];
		bool cancelled = false;
		foreach (Fixture fixture in loaded.Fixtures)
		{
			if (cancellationToken.IsCancellationRequested)
			{
				cancelled = true;
				break;
			}

			FixtureResult result = await fixtureEvaluator.EvaluateAsync(fixture, cancellationToken);
			reporter.PrintFixtureResult(result);
			results.Add(result);
		}
		total.Stop();

		if (cancelled)
		{
			reporter.PrintCancelled(results.Count, loaded.Fixtures.Count);
			reporter.WriteReport(
				new RunInfo(startedAt, ollamaUrl, fixturesDirectory),
				results,
				total.Elapsed,
				cancelled: true);
			return ExitCodeCancelled;
		}

		reporter.PrintSummary(results, total.Elapsed);
		reporter.WriteReport(
			new RunInfo(startedAt, ollamaUrl, fixturesDirectory),
			results,
			total.Elapsed,
			cancelled: false);

		if (!options.FailOnAnyFixtureFailure)
		{
			return 0;
		}

		return results.Any(r => !r.Passed) ? 1 : 0;
	}

	private async Task<bool> IsOllamaReachableAsync(CancellationToken cancellationToken)
	{
		if (string.IsNullOrWhiteSpace(vlmOptions.OllamaUrl))
		{
			return false;
		}

		using HttpClient probe = httpClientFactory.CreateClient("ollama-probe");
		probe.BaseAddress = new Uri(vlmOptions.OllamaUrl.TrimEnd('/') + "/");
		probe.Timeout = TimeSpan.FromSeconds(5);

		using CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
		cts.CancelAfter(TimeSpan.FromSeconds(5));

		try
		{
			using HttpResponseMessage response = await probe.GetAsync("api/tags", cts.Token);
			return response.IsSuccessStatusCode;
		}
		catch (Exception ex)
		{
			logger.LogDebug(ex, "Ollama availability probe failed");
			return false;
		}
	}
}
