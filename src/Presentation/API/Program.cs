using API.Hubs;
using API.Services;
using Application.Interfaces;
using Application.Services;
using Infrastructure.Services;

const string CorsPolicyAllowAll = "AllowAll";

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Configuration
	.SetBasePath(Directory.GetCurrentDirectory())
	.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
	.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
	.AddEnvironmentVariables()
	.AddUserSecrets<Program>(optional: true);

builder.AddLoggingService();

builder.Services
	.AddControllers()
	.AddJsonOptions(options =>
	{
		options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
	});

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

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
	Console.WriteLine("Development environment detected. Enabling Swagger.");
	app.UseSwagger();
	app.UseSwaggerUI(options =>
	{
		options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
		options.RoutePrefix = string.Empty;
	});
}
else
{
	Console.WriteLine("Production environment detected. Disabling Swagger.");
	app.UseExceptionHandler("/Error");
	app.UseHsts();
}

using (IServiceScope scope = app.Services.CreateScope())
{
	await scope.ServiceProvider.GetRequiredService<IDatabaseMigratorService>().MigrateAsync();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors(CorsPolicyAllowAll);
app.MapControllers();
app.MapHub<ReceiptsHub>("/receipts");

await app.RunAsync();
