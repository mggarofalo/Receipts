using Microsoft.OpenApi.Models;

namespace API.Configuration;

public static class SwaggerConfiguration
{
	public static IServiceCollection AddSwaggerServices(this IServiceCollection services)
	{
		services.AddSwaggerGen(options =>
		{
			OpenApiInfo openApiInfo = new()
			{
				Title = "Receipts API",
				Version = "v1"
			};

			options.SwaggerDoc("v1", openApiInfo);
		});

		return services;
	}

	public static WebApplication UseSwaggerServices(this WebApplication app)
	{
		if (app.Environment.IsDevelopment())
		{
			Console.WriteLine("Development environment detected. Enabling Swagger.");
			app.UseSwagger();
			app.UseSwaggerUI(options =>
			{
				options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
				options.RoutePrefix = "swagger";
				options.DefaultModelExpandDepth(0);
			});
		}
		else
		{
			Console.WriteLine("Production environment detected. Disabling Swagger.");
			app.UseExceptionHandler("/Error");
			app.UseHsts();
		}

		return app;
	}
}