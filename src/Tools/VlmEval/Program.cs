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

// CLI args take precedence over env/appsettings — typical for tools where you want to override
// just one knob (--report-path) without unsetting it from your shell. Supported:
//   --output console|json|markdown   (sets VlmEval:OutputFormat)
//   --report-path <path>             (sets VlmEval:ReportPath)
// Unknown flags are ignored to remain compatible with future hosts (e.g. Aspire) that may pass
// extra args.
ParseCliArgs(args, evalOptions);

string fixturesPath = Path.GetFullPath(evalOptions.FixturesPath);

builder.Services.AddSingleton(evalOptions);

// Share the production VLM client registration so eval results reflect production behavior
// (retry + circuit breaker + per-attempt timeout, with the standard ServiceDefaults handler
// removed). Without this, a flaky local Ollama would inflate the model's apparent failure
// rate during evaluation. See RECEIPTS-639.
builder.Services.AddVlmOcrClient(vlmOptions);

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
	IHostApplicationLifetime lifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();
	int exitCode = await runner.RunAsync(fixturesPath, lifetime.ApplicationStopping);

	await host.StopAsync();
	return exitCode;
}
catch (Exception ex)
{
	ILogger logger = host.Services.GetRequiredService<ILoggerFactory>().CreateLogger("VlmEval");
	logger.LogCritical(ex, "VlmEval terminated with an unhandled exception.");
	return 1;
}

static void ParseCliArgs(string[] args, VlmEvalOptions options)
{
	for (int i = 0; i < args.Length; i++)
	{
		string arg = args[i];
		if (string.Equals(arg, "--output", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
		{
			string value = args[++i];
			if (Enum.TryParse(value, ignoreCase: true, out ReportOutputFormat format))
			{
				options.OutputFormat = format;
			}
		}
		else if (string.Equals(arg, "--report-path", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
		{
			options.ReportPath = args[++i];
		}
	}
}
