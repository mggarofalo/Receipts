using Infrastructure;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
builder.AddServiceDefaults();

builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
{
	options.UseNpgsql(InfrastructureService.GetConnectionString(builder.Configuration), b =>
	{
		string? assemblyName = typeof(ApplicationDbContext).Assembly.FullName;
		b.MigrationsAssembly(assemblyName);
	});
});

try
{
	await using ServiceProvider services = builder.Services.BuildServiceProvider();
	IDbContextFactory<ApplicationDbContext> factory = services.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();
	await using ApplicationDbContext context = await factory.CreateDbContextAsync();

	ILogger logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("DbMigrator");
	logger.LogInformation("Applying EF Core migrations...");

	await context.Database.MigrateAsync();

	logger.LogInformation("Migrations applied successfully.");
	return 0;
}
catch (Exception ex)
{
	Console.Error.WriteLine($"Migration failed: {ex}");
	return 1;
}
