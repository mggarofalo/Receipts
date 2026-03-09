using Common;
using Infrastructure.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Services;

public static class DatabaseSeederService
{
	public static async Task SeedRolesAndAdminAsync(IServiceProvider services)
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

		IList<ApplicationUser> existingAdmins = await userManager.GetUsersInRoleAsync(AppRoles.Admin);
		if (existingAdmins.Count > 0)
		{
			return;
		}

		ApplicationUser adminUser = new()
		{
			UserName = adminEmail,
			Email = adminEmail,
			FirstName = configuration[ConfigurationVariables.AdminSeedFirstName],
			LastName = configuration[ConfigurationVariables.AdminSeedLastName],
			MustResetPassword = true,
			CreatedAt = DateTimeOffset.UtcNow,
		};

		IdentityResult result = await userManager.CreateAsync(adminUser, adminPassword);
		if (result.Succeeded)
		{
			await userManager.AddToRoleAsync(adminUser, AppRoles.Admin);
			await userManager.AddToRoleAsync(adminUser, AppRoles.User);
		}
	}
}
