using API.Hubs;
using API.Services;
using Application.Interfaces;
using Application.Services;
using Infrastructure.Services;
using Microsoft.OpenApi.Models;

const string CorsPolicyAllowAll = "AllowAll";

// Create builder
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Register configuration
builder.Configuration
	.SetBasePath(Directory.GetCurrentDirectory())
	.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
	.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
	.AddEnvironmentVariables()
	.AddUserSecrets<Program>(optional: true);

// Register logging service
builder.AddLoggingService();

// Register SwaggerGen
builder.Services.AddSwaggerGen(options =>
{
	OpenApiInfo openApiInfo = new()
	{
		Title = "Receipts API",
		Version = "v1"
	};

	options.SwaggerDoc("v1", openApiInfo);
});

// Register controllers
builder.Services
	.AddControllers()
	.AddJsonOptions(options =>
	{
		options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
	});

// Register services
builder.Services
	.RegisterProgramServices()
	.RegisterApplicationServices(builder.Configuration)
	.RegisterInfrastructureServices(builder.Configuration)
	.AddCors(options =>
	{
		options.AddPolicy(CorsPolicyAllowAll, builder =>
		{
			builder.AllowAnyOrigin()
				   .AllowAnyMethod()
				   .AllowAnyHeader();
		});
	});

// Build application
WebApplication app = builder.Build();

// Register Swagger
if (app.Environment.IsDevelopment())
{
	Console.WriteLine("Development environment detected. Enabling Swagger.");
	app.UseSwagger();
	app.UseSwaggerUI(options =>
	{
		options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
		options.RoutePrefix = "swagger";
	});
}
else
{
	Console.WriteLine("Production environment detected. Disabling Swagger.");
	app.UseExceptionHandler("/Error");
	app.UseHsts();
}

// Run database migrations
using (IServiceScope scope = app.Services.CreateScope())
{
	await scope.ServiceProvider.GetRequiredService<IDatabaseMigratorService>().MigrateAsync();
}

// Register middleware
app.UseHttpsRedirection();
app.UseRouting();
app.UseCors(CorsPolicyAllowAll);

// Map controllers
app.MapControllers();

// Map SignalR hubs
app.MapHub<ReceiptsHub>("/receipts");

// Run application
await app.RunAsync();
