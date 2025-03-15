using API.Configuration;
using API.Hubs;
using API.Services;
using Application.Interfaces;
using Application.Services;
using Infrastructure.Services;

// Create builder
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.AddApplicationConfiguration();

// Register services
builder.Services
	.AddSwaggerServices()
	.AddApplicationServices()
	.AddCorsServices()
	.RegisterProgramServices()
	.RegisterApplicationServices(builder.Configuration)
	.RegisterInfrastructureServices(builder.Configuration);

// Build application
WebApplication app = builder.Build();

// Configure middleware
app.UseSwaggerServices()
   .UseApplicationServices()
   .UseCorsServices();

// Run database migrations
using (IServiceScope scope = app.Services.CreateScope())
{
	await scope.ServiceProvider.GetRequiredService<IDatabaseMigratorService>().MigrateAsync();
}

// Map controllers
app.MapControllers();

// Map SignalR hubs
app.MapHub<ReceiptsHub>("/receipts");

// Run application
await app.RunAsync();
