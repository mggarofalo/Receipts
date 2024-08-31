using API.Services;
using Application.Interfaces;
using Application.Services;
using Infrastructure.Services;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.AddLoggingService();
builder.Services.RegisterProgramServices(builder.Configuration);
builder.Services.RegisterApplicationServices(builder.Configuration);
builder.Services.RegisterInfrastructureServices(builder.Configuration);

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