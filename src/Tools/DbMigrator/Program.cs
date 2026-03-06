using Infrastructure;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
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

builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
{
	options.UseNpgsql(InfrastructureService.GetConnectionString(builder.Configuration), b =>
	{
		string? assemblyName = typeof(ApplicationDbContext).Assembly.FullName;
		b.MigrationsAssembly(assemblyName);
	});
	options.ConfigureWarnings(w => w.Log(
		(RelationalEventId.PendingModelChangesWarning, LogLevel.Warning)));
});

IHost host = builder.Build();
try
{
	await host.StartAsync();

	IDbContextFactory<ApplicationDbContext> factory = host.Services.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();
	await using ApplicationDbContext context = await factory.CreateDbContextAsync();

	ILogger logger = host.Services.GetRequiredService<ILoggerFactory>().CreateLogger("DbMigrator");
	logger.LogInformation("Applying EF Core migrations...");

	await context.Database.MigrateAsync();

	logger.LogInformation("Migrations applied successfully.");

	await host.StopAsync();
	return 0;
}
catch (Exception ex)
{
	Console.Error.WriteLine($"Migration failed: {ex}");
	return 1;
}
