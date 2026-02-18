using Serilog;
using Serilog.Sinks.OpenTelemetry;

namespace API.Services;

public static class LoggingService
{
	public static WebApplicationBuilder AddLoggingService(this WebApplicationBuilder builder)
	{
		LoggerConfiguration loggerConfiguration = new LoggerConfiguration()
			.Enrich.FromLogContext();

		if (builder.Environment.IsDevelopment())
		{
			loggerConfiguration
				.WriteTo.Console()
				.MinimumLevel.Debug();
		}
		else
		{
			loggerConfiguration
				.WriteTo.Console()
				.MinimumLevel.Information();
		}

		// Forward logs to Aspire Dashboard via OTLP when running under Aspire
		string? otlpEndpoint = builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];
		if (!string.IsNullOrEmpty(otlpEndpoint))
		{
			loggerConfiguration.WriteTo.OpenTelemetry(options =>
			{
				options.Endpoint = otlpEndpoint;
				options.Protocol = OtlpProtocol.Grpc;
				options.ResourceAttributes = new Dictionary<string, object>
				{
					["service.name"] = builder.Environment.ApplicationName
				};
			});
		}

		Log.Logger = loggerConfiguration.CreateLogger();

		builder.Host.UseSerilog(Log.Logger);

		builder.Services.AddLogging(loggingBuilder =>
		{
			loggingBuilder.ClearProviders();
			loggingBuilder.AddSerilog(Log.Logger);
		});

		Log.Information("Starting Receipts API");

		return builder;
	}
}
