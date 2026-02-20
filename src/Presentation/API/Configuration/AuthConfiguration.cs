using API.Authentication;
using Common;
using Infrastructure.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
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

			options.AddPolicy("RequireAdmin", policy =>
			{
				policy.AddAuthenticationSchemes(
					JwtBearerDefaults.AuthenticationScheme,
					ApiKeyAuthenticationDefaults.AuthenticationScheme);
				policy.RequireAuthenticatedUser();
				policy.RequireRole(AppRoles.Admin);
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

	public static async Task SeedRolesAndAdminAsync(this IServiceProvider services)
	{
		using IServiceScope scope = services.CreateScope();
		RoleManager<IdentityRole> roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
		UserManager<ApplicationUser> userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
		IConfiguration configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

		foreach (string role in AppRoles.All)
		{
			if (!await roleManager.RoleExistsAsync(role))
			{
				await roleManager.CreateAsync(new IdentityRole(role));
			}
		}

		string? adminEmail = configuration[ConfigurationVariables.AdminSeedEmail];
		string? adminPassword = configuration[ConfigurationVariables.AdminSeedPassword];

		if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
		{
			return;
		}

		ApplicationUser? existingAdmin = await userManager.FindByEmailAsync(adminEmail);
		if (existingAdmin is not null)
		{
			return;
		}

		ApplicationUser adminUser = new()
		{
			UserName = adminEmail,
			Email = adminEmail,
			FirstName = configuration[ConfigurationVariables.AdminSeedFirstName],
			LastName = configuration[ConfigurationVariables.AdminSeedLastName],
		};

		IdentityResult result = await userManager.CreateAsync(adminUser, adminPassword);
		if (result.Succeeded)
		{
			await userManager.AddToRoleAsync(adminUser, AppRoles.Admin);
			await userManager.AddToRoleAsync(adminUser, AppRoles.User);
		}
	}
}
