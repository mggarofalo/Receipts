using Application.Interfaces.Services;
using Infrastructure;
using Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
builder.AddServiceDefaults();

if (!InfrastructureService.IsDatabaseConfigured(builder.Configuration))
{
	Console.Error.WriteLine("Database is not configured. Set POSTGRES_* env vars or an Aspire connection string.");
	return 1;
}

builder.Services.RegisterInfrastructureServices(builder.Configuration);

IHost host = builder.Build();
try
{
	await host.StartAsync();

	ILogger logger = host.Services.GetRequiredService<ILoggerFactory>().CreateLogger("DbExporter");

	string? outputPath = args.Length > 0 ? args[0] : null;

	using IServiceScope scope = host.Services.CreateScope();
	IBackupService backupService = scope.ServiceProvider.GetRequiredService<IBackupService>();

	logger.LogInformation("Exporting database to SQLite...");
	string exportedPath = await backupService.ExportToSqliteAsync();

	if (outputPath is not null)
	{
		File.Move(exportedPath, outputPath, overwrite: true);
		logger.LogInformation("Backup saved to {Path}", outputPath);
	}
	else
	{
		logger.LogInformation("Backup saved to {Path}", exportedPath);
	}

	await host.StopAsync();
	return 0;
}
catch (Exception ex)
{
	Console.Error.WriteLine($"Export failed: {ex}");
	return 1;
}
