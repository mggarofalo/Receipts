using System.Net;
using API.Configuration;
using API.Hubs;
using API.Middleware;
using API.Services;
using Application.Interfaces;
using Application.Services;
using Infrastructure.Services;
using Microsoft.AspNetCore.HttpOverrides;

// Create builder
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
builder.AddApplicationConfiguration();

// Register services
builder.Services
	.AddOpenApiServices()
	.AddVersioningServices()
	.AddApplicationServices()
	.AddCorsServices()
	.AddAuthServices(builder.Configuration)
	.RegisterProgramServices()
	.RegisterApplicationServices(builder.Configuration)
	.RegisterInfrastructureServices(builder.Configuration);

// Build application
WebApplication app = builder.Build();

// Configure middleware
var forwardedHeadersOptions = new ForwardedHeadersOptions
{
	ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
};
forwardedHeadersOptions.KnownIPNetworks.Add(new System.Net.IPNetwork(IPAddress.Parse("172.16.0.0"), 12));
app.UseForwardedHeaders(forwardedHeadersOptions);
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
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

// Serve SPA static files in production (Vite dev server handles this in development)
if (!app.Environment.IsDevelopment())
{
	app.UseDefaultFiles();
	app.UseStaticFiles();
}

// Map Aspire health check endpoints (/health, /alive)
app.MapDefaultEndpoints();

// Map controllers
app.MapControllers();

// Map SignalR hubs
app.MapHub<EntityHub>("/hubs/entities");

// SPA fallback: serve index.html for client-side routes in production
if (!app.Environment.IsDevelopment())
{
	app.MapFallbackToFile("index.html");
}

// Run application
await app.RunAsync();
