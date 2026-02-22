namespace API.Configuration;

public static class CorsConfiguration
{
	public const string CorsPolicyAllowAll = "AllowAll";

	public static IServiceCollection AddCorsServices(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
	{
		string[] allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

		services.AddCors(options =>
		{
			options.AddPolicy(CorsPolicyAllowAll, builder =>
			{
				if (allowedOrigins.Length > 0)
				{
					builder.WithOrigins(allowedOrigins);
				}
				else if (environment.IsDevelopment() && configuration.GetValue<bool>("Cors:AllowAnyOriginInDev"))
				{
					// No origins configured — allow any origin per-request (dev fallback).
					// AllowAnyOrigin() is intentionally avoided: it sets Access-Control-Allow-Origin: *
					// which is incompatible with AllowCredentials(), required by SignalR.
					// Requires Cors:AllowAnyOriginInDev=true to prevent accidental activation
					// if ASPNETCORE_ENVIRONMENT is mistakenly set to Development in a deployed env.
					builder.SetIsOriginAllowed(_ => true);
				}
				// Non-development with no configured origins: no origins are added to the policy,
				// so all cross-origin requests are denied by default (safe fallback).
				// A critical warning is logged at startup via UseCorsServices.

				builder.AllowAnyMethod()
					   .AllowAnyHeader()
					   .AllowCredentials();
			});
		});

		return services;
	}

	public static IApplicationBuilder UseCorsServices(this IApplicationBuilder app)
	{
		// Guard: emit a startup error when AllowedOrigins is unconfigured in non-development.
		// This prevents silent fallback to allow-any-origin-with-credentials in production.
		if (app is WebApplication webApp)
		{
			string[] allowedOrigins = webApp.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
			if (!webApp.Environment.IsDevelopment() && allowedOrigins.Length == 0)
			{
				webApp.Logger.LogCritical(
					"Cors:AllowedOrigins is not configured in a non-development environment. " +
					"All cross-origin requests will be denied. Set Cors:AllowedOrigins to allow specific origins.");
			}
		}

		app.UseCors(CorsPolicyAllowAll);
		return app;
	}
}
