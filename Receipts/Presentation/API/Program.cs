using Application;
using Application.Interfaces;
using Infrastructure;
using System.Reflection;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register all AutoMapper profiles in the assembly
builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());

// Register application services
builder.Services.AddApplicationServices(builder.Configuration);

// Resolve IInfrastructureService and call AddInfrastructureServices
builder.Services.AddScoped<IInfrastructureService>(provider =>
{
	InfrastructureService infrastructureService = new();
	infrastructureService.AddInfrastructureServices(builder.Services, builder.Configuration);
	return infrastructureService;
});

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();
await app.RunAsync();