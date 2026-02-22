using Microsoft.AspNetCore.Authentication.JwtBearer;
using Serilog;

namespace API.Configuration;

public static class AuthConfiguration
{
	public static IServiceCollection AddAuthServices(this IServiceCollection services, IConfiguration configuration)
	{
		services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
			.AddJwtBearer(options =>
			{
				// Bind JWT settings from the "Authentication:Schemes:Bearer" configuration section.
				// Required keys: Authority (or MetadataAddress) and Audience.
				configuration.GetSection("Authentication:Schemes:Bearer").Bind(options);

				options.Events = new JwtBearerEvents
				{
					OnMessageReceived = context =>
					{
						// WebSocket upgrade requests cannot carry Authorization headers, so SignalR
						// clients must pass the JWT as ?access_token=<token>. This is the
						// Microsoft-documented workaround for SignalR authentication.
						// The path must match the registered hub endpoint exactly (/hubs/receipts).
						if (context.HttpContext.Request.Path.StartsWithSegments("/hubs/receipts"))
						{
							string? accessToken = context.Request.Query["access_token"];
							if (!string.IsNullOrEmpty(accessToken))
							{
								context.Token = accessToken;
							}
						}

						return Task.CompletedTask;
					}
				};
			});

		return services;
	}

	public static WebApplication UseAuthServices(this WebApplication app)
	{
		// Use Serilog structured request logging with access_token redaction.
		// The ?access_token=<jwt> query parameter is the required workaround for WebSocket
		// connections that cannot carry Authorization headers. Tokens in URLs are persisted
		// in access logs, so the value is redacted here before it reaches any log sink.
		app.UseSerilogRequestLogging(opts =>
		{
			opts.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
			{
				string queryString = httpContext.Request.QueryString.Value ?? string.Empty;
				if (!string.IsNullOrEmpty(queryString))
				{
					if (queryString.Contains("access_token=", StringComparison.OrdinalIgnoreCase))
					{
						queryString = System.Text.RegularExpressions.Regex.Replace(
							queryString,
							@"(?i)access_token=[^&]*",
							"access_token=[REDACTED]");
					}

					diagnosticContext.Set("QueryString", queryString);
				}
			};
		});

		return app;
	}
}
