using API.Configuration;
using API.Hubs;
using API.Services;
using Application.Interfaces;
using Application.Services;
using Infrastructure.Services;

// Create builder
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
builder.AddApplicationConfiguration();

// Register services
builder.Services
	.AddOpenApiServices()
	.AddApplicationServices()
	.AddCorsServices()
	.AddAuthServices(builder.Configuration)
	.RegisterProgramServices()
	.RegisterApplicationServices(builder.Configuration)
	.RegisterInfrastructureServices(builder.Configuration);

// Build application
WebApplication app = builder.Build();

// Configure middleware
app.UseOpenApiServices()
   .UseApplicationServices()
   .UseCorsServices()
   .UseAuthServices();

// Run database migrations (skipped when DB is not configured, e.g. build-time OpenAPI generation)
if (Infrastructure.Services.InfrastructureService.IsDatabaseConfigured(builder.Configuration))
{
	using IServiceScope scope = app.Services.CreateScope();
	await scope.ServiceProvider.GetRequiredService<IDatabaseMigratorService>().MigrateAsync();
	await app.Services.SeedRolesAndAdminAsync();
}

// Map Aspire health check endpoints (/health, /alive)
app.MapDefaultEndpoints();

// Map controllers
app.MapControllers();

// Map SignalR hubs
app.MapHub<ReceiptsHub>("/receipts");

// Run application
await app.RunAsync();
