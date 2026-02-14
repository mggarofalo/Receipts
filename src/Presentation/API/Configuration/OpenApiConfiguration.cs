using Scalar.AspNetCore;

namespace API.Configuration;

public static class OpenApiConfiguration
{
	public static IServiceCollection AddOpenApiServices(this IServiceCollection services)
	{
		services.AddOpenApi();
		return services;
	}

	public static WebApplication UseOpenApiServices(this WebApplication app)
	{
		if (app.Environment.IsDevelopment())
		{
			app.MapOpenApi();
			app.MapScalarApiReference();
		}
		else
		{
			app.UseExceptionHandler("/Error");
			app.UseHsts();
		}

		return app;
	}
}
