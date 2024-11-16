using API.Hubs;
using API.Services;
using Application.Interfaces;
using Application.Services;
using Infrastructure.Services;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Configuration
	.SetBasePath(Directory.GetCurrentDirectory())
	.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
	.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
	.AddEnvironmentVariables()
	.AddUserSecrets<Program>(optional: true);

builder.AddLoggingService();

builder.Services
	.RegisterProgramServices()
	.RegisterApplicationServices(builder.Configuration)
	.RegisterInfrastructureServices(builder.Configuration);

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI(options =>
	{
		options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
		options.RoutePrefix = string.Empty;
	});
}

using (IServiceScope scope = app.Services.CreateScope())
{
	await scope.ServiceProvider.GetRequiredService<IDatabaseMigratorService>().MigrateAsync();
}

app.UseStaticFiles();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapHub<ReceiptsHub>("/receipts");

await app.RunAsync();
