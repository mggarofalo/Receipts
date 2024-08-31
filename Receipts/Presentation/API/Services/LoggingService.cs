using Serilog;

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
