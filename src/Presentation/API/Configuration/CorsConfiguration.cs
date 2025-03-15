namespace API.Configuration;

public static class CorsConfiguration
{
	public const string CorsPolicyAllowAll = "AllowAll";

	public static IServiceCollection AddCorsServices(this IServiceCollection services)
	{
		services.AddCors(options =>
		{
			options.AddPolicy(CorsPolicyAllowAll, builder =>
			{
				builder.AllowAnyOrigin()
					   .AllowAnyMethod()
					   .AllowAnyHeader();
			});
		});

		return services;
	}

	public static IApplicationBuilder UseCorsServices(this IApplicationBuilder app)
	{
		app.UseCors(CorsPolicyAllowAll);
		return app;
	}
}