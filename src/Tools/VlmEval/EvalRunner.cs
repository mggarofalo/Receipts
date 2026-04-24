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
	public async Task<int> RunAsync(string fixturesDirectory, CancellationToken cancellationToken)
	{
		string ollamaUrl = vlmOptions.OllamaUrl ?? "(unset)";
		reporter.PrintHeader(ollamaUrl, fixturesDirectory);

		if (!Directory.Exists(fixturesDirectory))
		{
			try
			{
				Directory.CreateDirectory(fixturesDirectory);
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Failed to create fixtures directory at {Path}", fixturesDirectory);
				return 1;
			}
		}

		if (!await IsOllamaReachableAsync(cancellationToken))
		{
			reporter.PrintOllamaUnreachable(ollamaUrl);
			return 1;
		}

		LoadedFixtures loaded = fixtureLoader.LoadFrom(fixturesDirectory);

		if (loaded.Fixtures.Count == 0 && loaded.OrphanFiles.Count == 0)
		{
			reporter.PrintEmptyFixturesDirectory(fixturesDirectory);
			return 0;
		}

		foreach (string orphan in loaded.OrphanFiles)
		{
			reporter.PrintOrphan(orphan);
		}

		if (loaded.Fixtures.Count == 0)
		{
			reporter.PrintNoValidFixtures();
			return 0;
		}

		Stopwatch total = Stopwatch.StartNew();
		List<FixtureResult> results = [];
		foreach (Fixture fixture in loaded.Fixtures)
		{
			if (cancellationToken.IsCancellationRequested)
			{
				break;
			}

			FixtureResult result = await fixtureEvaluator.EvaluateAsync(fixture, cancellationToken);
			reporter.PrintFixtureResult(result);
			results.Add(result);
		}
		total.Stop();

		reporter.PrintSummary(results, total.Elapsed);

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
