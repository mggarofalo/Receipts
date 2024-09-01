using API.Services;
using Application.Interfaces;
using Application.Services;
using Infrastructure.Services;
using System.Reflection;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.AddLoggingService();

builder.Services.RegisterProgramServices(Assembly.GetExecutingAssembly())
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
	await scope.ServiceProvider.GetRequiredService<IDatabaseMigrator>().MigrateAsync();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

await app.RunAsync();