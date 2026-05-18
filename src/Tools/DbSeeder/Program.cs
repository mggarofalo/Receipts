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

	ILogger logger = host.Services.GetRequiredService<ILoggerFactory>().CreateLogger("DbSeeder");
	logger.LogInformation("Seeding database with roles and admin user...");

	await DatabaseSeederService.SeedRolesAndAdminAsync(host.Services);

	// Optional: a large, realistic sample dataset for local dev / demo environments.
	// Gated behind SampleData:Enabled so production seeding stays limited to roles + admin.
	if (string.Equals(builder.Configuration["SampleData:Enabled"], "true", StringComparison.OrdinalIgnoreCase))
	{
		await SampleDataSeederService.SeedAsync(host.Services);
	}

	logger.LogInformation("Database seeding completed successfully.");

	await host.StopAsync();
	return 0;
}
catch (Exception ex)
{
	Console.Error.WriteLine($"Seeding failed: {ex}");
	return 1;
}
