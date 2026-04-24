using Application.Interfaces.Services;
using Common;
using Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using VlmEval;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

VlmEvalOptions evalOptions = new();
builder.Configuration.GetSection("VlmEval").Bind(evalOptions);

VlmOcrOptions vlmOptions = new();
builder.Configuration.GetSection(ConfigurationVariables.OcrVlmSection).Bind(vlmOptions);

if (string.IsNullOrWhiteSpace(vlmOptions.OllamaUrl))
{
	vlmOptions.OllamaUrl = builder.Configuration[ConfigurationVariables.OllamaBaseUrl]
		?? "http://localhost:11434";
}

if (evalOptions.OllamaTimeoutSeconds > 0)
{
	vlmOptions.TimeoutSeconds = evalOptions.OllamaTimeoutSeconds;
}

string fixturesPath = Path.GetFullPath(evalOptions.FixturesPath);

builder.Services.AddSingleton(evalOptions);
builder.Services.AddSingleton(vlmOptions);

builder.Services.AddHttpClient<IReceiptExtractionService, OllamaReceiptExtractionService>(client =>
{
	client.BaseAddress = new Uri(vlmOptions.OllamaUrl!.TrimEnd('/') + "/");
	// Per-call timeout is enforced inside OllamaReceiptExtractionService via its own token source.
	// Leave HttpClient.Timeout unbounded so resilience handlers don't cancel the long VLM call.
	client.Timeout = Timeout.InfiniteTimeSpan;
});

builder.Services.AddHttpClient("ollama-probe");

builder.Services.AddSingleton<FixtureLoader>();
builder.Services.AddSingleton<FixtureEvaluator>();
builder.Services.AddSingleton<Reporter>();
builder.Services.AddSingleton<EvalRunner>();

IHost host = builder.Build();

try
{
	await host.StartAsync();

	EvalRunner runner = host.Services.GetRequiredService<EvalRunner>();
	int exitCode = await runner.RunAsync(fixturesPath, CancellationToken.None);

	await host.StopAsync();
	return exitCode;
}
catch (Exception ex)
{
	ILogger logger = host.Services.GetRequiredService<ILoggerFactory>().CreateLogger("VlmEval");
	logger.LogCritical(ex, "VlmEval terminated with an unhandled exception.");
	return 1;
}
