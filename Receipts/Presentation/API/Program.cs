using Application;
using Application.Interfaces;
using Infrastructure;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Register IInfrastructureService
builder.Services.AddScoped<IInfrastructureService, InfrastructureService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAutoMapper(typeof(API.Mapping.MappingProfile));

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