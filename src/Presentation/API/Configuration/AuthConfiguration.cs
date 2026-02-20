using API.Authentication;
using Common;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace API.Configuration;

public static class AuthConfiguration
{
	public static IServiceCollection AddAuthServices(this IServiceCollection services, IConfiguration configuration)
	{
		string jwtKey = configuration[ConfigurationVariables.JwtKey] ?? "build-time-placeholder-key-32-chars!!";
		string jwtIssuer = configuration[ConfigurationVariables.JwtIssuer] ?? "receipts-api";
		string jwtAudience = configuration[ConfigurationVariables.JwtAudience] ?? "receipts-app";

		services
			.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
			.AddJwtBearer(options =>
			{
				options.TokenValidationParameters = new TokenValidationParameters
				{
					ValidateIssuerSigningKey = true,
					IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
					ValidateIssuer = true,
					ValidIssuer = jwtIssuer,
					ValidateAudience = true,
					ValidAudience = jwtAudience,
					ValidateLifetime = true,
					ClockSkew = TimeSpan.Zero,
				};
			})
			.AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(
				ApiKeyAuthenticationDefaults.AuthenticationScheme,
				_ => { });

		services.AddAuthorization(options =>
		{
			options.AddPolicy("ApiOrJwt", policy =>
			{
				policy.AddAuthenticationSchemes(
					JwtBearerDefaults.AuthenticationScheme,
					ApiKeyAuthenticationDefaults.AuthenticationScheme);
				policy.RequireAuthenticatedUser();
			});

			options.DefaultPolicy = options.GetPolicy("ApiOrJwt")!;
		});

		return services;
	}

	public static IApplicationBuilder UseAuthServices(this IApplicationBuilder app)
	{
		app.UseAuthentication();
		app.UseAuthorization();
		return app;
	}
}
