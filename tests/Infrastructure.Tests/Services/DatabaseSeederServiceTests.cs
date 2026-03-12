using Common;
using FluentAssertions;
using Infrastructure.Entities;
using Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Infrastructure.Tests.Services;

public class DatabaseSeederServiceTests
{
	private readonly Mock<RoleManager<IdentityRole>> _mockRoleManager;
	private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
	private readonly IConfiguration _configuration;
	private readonly IServiceProvider _serviceProvider;

	public DatabaseSeederServiceTests()
	{
		Mock<IRoleStore<IdentityRole>> roleStore = new();
		_mockRoleManager = new Mock<RoleManager<IdentityRole>>(
			roleStore.Object, null!, null!, null!, null!);

		Mock<IUserStore<ApplicationUser>> userStore = new();
		_mockUserManager = new Mock<UserManager<ApplicationUser>>(
			userStore.Object, null!, null!, null!, null!, null!, null!, null!, null!);

		_configuration = new ConfigurationBuilder()
			.AddInMemoryCollection(new Dictionary<string, string?>
			{
				[ConfigurationVariables.AdminSeedEmail] = "admin@test.com",
				[ConfigurationVariables.AdminSeedPassword] = "Password123!",
				[ConfigurationVariables.AdminSeedFirstName] = "Admin",
				[ConfigurationVariables.AdminSeedLastName] = "User",
			})
			.Build();

		ServiceCollection services = new();
		services.AddSingleton(_mockRoleManager.Object);
		services.AddSingleton(_mockUserManager.Object);
		services.AddSingleton(_configuration);
		_serviceProvider = services.BuildServiceProvider();
	}

	private void SetupFullySeedableUser()
	{
		_mockUserManager.Setup(u => u.FindByEmailAsync("admin@test.com"))
			.ReturnsAsync((ApplicationUser?)null);
		_mockUserManager.Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
			.ReturnsAsync(IdentityResult.Success);
		_mockUserManager.Setup(u => u.IsInRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
			.ReturnsAsync(false);
		_mockUserManager.Setup(u => u.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
			.ReturnsAsync(IdentityResult.Success);
	}

	[Fact]
	public async Task SeedRolesAndAdminAsync_ThrowsWhenRoleCreationFails()
	{
		// Arrange
		_mockRoleManager.Setup(r => r.RoleExistsAsync(It.IsAny<string>()))
			.ReturnsAsync(false);

		IdentityError error = new() { Code = "DuplicateRoleName", Description = "Role already exists." };
		_mockRoleManager.Setup(r => r.CreateAsync(It.IsAny<IdentityRole>()))
			.ReturnsAsync(IdentityResult.Failed(error));

		// Act
		Func<Task> act = () => DatabaseSeederService.SeedRolesAndAdminAsync(_serviceProvider);

		// Assert
		await act.Should().ThrowAsync<InvalidOperationException>()
			.WithMessage("*Failed to create role*Role already exists.*");
	}

	[Fact]
	public async Task SeedRolesAndAdminAsync_CreatesAllRolesWhenNoneExist()
	{
		// Arrange
		_mockRoleManager.Setup(r => r.RoleExistsAsync(It.IsAny<string>()))
			.ReturnsAsync(false);
		_mockRoleManager.Setup(r => r.CreateAsync(It.IsAny<IdentityRole>()))
			.ReturnsAsync(IdentityResult.Success);

		ApplicationUser existingUser = new() { Email = "admin@test.com" };
		_mockUserManager.Setup(u => u.FindByEmailAsync("admin@test.com"))
			.ReturnsAsync(existingUser);
		_mockUserManager.Setup(u => u.IsInRoleAsync(existingUser, It.IsAny<string>()))
			.ReturnsAsync(true);

		// Act
		await DatabaseSeederService.SeedRolesAndAdminAsync(_serviceProvider);

		// Assert
		foreach (string role in AppRoles.All)
		{
			_mockRoleManager.Verify(r => r.CreateAsync(
				It.Is<IdentityRole>(ir => ir.Name == role)), Times.Once);
		}
	}

	[Fact]
	public async Task SeedRolesAndAdminAsync_SkipsExistingRoles()
	{
		// Arrange
		_mockRoleManager.Setup(r => r.RoleExistsAsync(It.IsAny<string>()))
			.ReturnsAsync(true);

		ApplicationUser existingUser = new() { Email = "admin@test.com" };
		_mockUserManager.Setup(u => u.FindByEmailAsync("admin@test.com"))
			.ReturnsAsync(existingUser);
		_mockUserManager.Setup(u => u.IsInRoleAsync(existingUser, It.IsAny<string>()))
			.ReturnsAsync(true);

		// Act
		await DatabaseSeederService.SeedRolesAndAdminAsync(_serviceProvider);

		// Assert
		_mockRoleManager.Verify(r => r.CreateAsync(It.IsAny<IdentityRole>()), Times.Never);
	}

	[Fact]
	public async Task SeedRolesAndAdminAsync_WhenUserCreationFails_ThrowsInvalidOperationException()
	{
		// Arrange
		_mockRoleManager.Setup(r => r.RoleExistsAsync(It.IsAny<string>()))
			.ReturnsAsync(true);

		_mockUserManager.Setup(u => u.FindByEmailAsync("admin@test.com"))
			.ReturnsAsync((ApplicationUser?)null);

		IdentityError error = new() { Code = "DuplicateEmail", Description = "Email already taken" };
		_mockUserManager.Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
			.ReturnsAsync(IdentityResult.Failed(error));

		// Act
		Func<Task> act = () => DatabaseSeederService.SeedRolesAndAdminAsync(_serviceProvider);

		// Assert
		await act.Should().ThrowAsync<InvalidOperationException>()
			.WithMessage("*Failed to create admin user*Email already taken*");
	}

	[Fact]
	public async Task SeedRolesAndAdminAsync_WhenAddToAdminRoleFails_ThrowsAndDeletesUser()
	{
		// Arrange
		_mockRoleManager.Setup(r => r.RoleExistsAsync(It.IsAny<string>()))
			.ReturnsAsync(true);

		_mockUserManager.Setup(u => u.FindByEmailAsync("admin@test.com"))
			.ReturnsAsync((ApplicationUser?)null);
		_mockUserManager.Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
			.ReturnsAsync(IdentityResult.Success);
		_mockUserManager.Setup(u => u.IsInRoleAsync(It.IsAny<ApplicationUser>(), AppRoles.Admin))
			.ReturnsAsync(false);
		_mockUserManager.Setup(u => u.IsInRoleAsync(It.IsAny<ApplicationUser>(), AppRoles.User))
			.ReturnsAsync(false);
		_mockUserManager.Setup(u => u.AddToRoleAsync(It.IsAny<ApplicationUser>(), AppRoles.Admin))
			.ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = "RoleFail", Description = "Admin role assignment failed" }));
		_mockUserManager.Setup(u => u.DeleteAsync(It.IsAny<ApplicationUser>()))
			.ReturnsAsync(IdentityResult.Success);

		// Act
		Func<Task> act = () => DatabaseSeederService.SeedRolesAndAdminAsync(_serviceProvider);

		// Assert
		await act.Should().ThrowAsync<InvalidOperationException>()
			.WithMessage("*Admin role*Admin role assignment failed*");
		_mockUserManager.Verify(u => u.DeleteAsync(It.IsAny<ApplicationUser>()), Times.Once);
	}

	[Fact]
	public async Task SeedRolesAndAdminAsync_WhenAddToUserRoleFails_ThrowsAndDeletesUser()
	{
		// Arrange
		_mockRoleManager.Setup(r => r.RoleExistsAsync(It.IsAny<string>()))
			.ReturnsAsync(true);

		_mockUserManager.Setup(u => u.FindByEmailAsync("admin@test.com"))
			.ReturnsAsync((ApplicationUser?)null);
		_mockUserManager.Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
			.ReturnsAsync(IdentityResult.Success);
		_mockUserManager.Setup(u => u.IsInRoleAsync(It.IsAny<ApplicationUser>(), AppRoles.Admin))
			.ReturnsAsync(false);
		_mockUserManager.Setup(u => u.IsInRoleAsync(It.IsAny<ApplicationUser>(), AppRoles.User))
			.ReturnsAsync(false);
		_mockUserManager.Setup(u => u.AddToRoleAsync(It.IsAny<ApplicationUser>(), AppRoles.Admin))
			.ReturnsAsync(IdentityResult.Success);
		_mockUserManager.Setup(u => u.AddToRoleAsync(It.IsAny<ApplicationUser>(), AppRoles.User))
			.ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = "RoleFail", Description = "User role assignment failed" }));
		_mockUserManager.Setup(u => u.DeleteAsync(It.IsAny<ApplicationUser>()))
			.ReturnsAsync(IdentityResult.Success);

		// Act
		Func<Task> act = () => DatabaseSeederService.SeedRolesAndAdminAsync(_serviceProvider);

		// Assert
		await act.Should().ThrowAsync<InvalidOperationException>()
			.WithMessage("*User role*User role assignment failed*");
		_mockUserManager.Verify(u => u.DeleteAsync(It.IsAny<ApplicationUser>()), Times.Once);
	}

	[Fact]
	public async Task SeedRolesAndAdminAsync_WhenBothRoleAssignmentsSucceed_DoesNotThrow()
	{
		// Arrange
		_mockRoleManager.Setup(r => r.RoleExistsAsync(It.IsAny<string>()))
			.ReturnsAsync(true);

		SetupFullySeedableUser();

		// Act
		Func<Task> act = () => DatabaseSeederService.SeedRolesAndAdminAsync(_serviceProvider);

		// Assert
		await act.Should().NotThrowAsync();

		_mockUserManager.Verify(u => u.AddToRoleAsync(It.IsAny<ApplicationUser>(), AppRoles.Admin), Times.Once);
		_mockUserManager.Verify(u => u.AddToRoleAsync(It.IsAny<ApplicationUser>(), AppRoles.User), Times.Once);
	}

	[Fact]
	public async Task SeedRolesAndAdminAsync_WhenExistingUserHasAllRoles_SkipsRoleAssignment()
	{
		// Arrange
		_mockRoleManager.Setup(r => r.RoleExistsAsync(It.IsAny<string>()))
			.ReturnsAsync(true);

		ApplicationUser existingUser = new() { Email = "admin@test.com" };
		_mockUserManager.Setup(u => u.FindByEmailAsync("admin@test.com"))
			.ReturnsAsync(existingUser);
		_mockUserManager.Setup(u => u.IsInRoleAsync(existingUser, It.IsAny<string>()))
			.ReturnsAsync(true);

		// Act
		await DatabaseSeederService.SeedRolesAndAdminAsync(_serviceProvider);

		// Assert
		_mockUserManager.Verify(u => u.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
		_mockUserManager.Verify(u => u.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
	}

	[Fact]
	public async Task SeedRolesAndAdminAsync_WhenExistingUserLacksRoles_AssignsRolesWithoutCreating()
	{
		// Arrange — simulates orphaned user from a previous partial failure
		_mockRoleManager.Setup(r => r.RoleExistsAsync(It.IsAny<string>()))
			.ReturnsAsync(true);

		ApplicationUser orphanedUser = new() { Email = "admin@test.com" };
		_mockUserManager.Setup(u => u.FindByEmailAsync("admin@test.com"))
			.ReturnsAsync(orphanedUser);
		_mockUserManager.Setup(u => u.IsInRoleAsync(orphanedUser, It.IsAny<string>()))
			.ReturnsAsync(false);
		_mockUserManager.Setup(u => u.AddToRoleAsync(orphanedUser, It.IsAny<string>()))
			.ReturnsAsync(IdentityResult.Success);

		// Act
		await DatabaseSeederService.SeedRolesAndAdminAsync(_serviceProvider);

		// Assert
		_mockUserManager.Verify(u => u.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
		_mockUserManager.Verify(u => u.AddToRoleAsync(orphanedUser, AppRoles.Admin), Times.Once);
		_mockUserManager.Verify(u => u.AddToRoleAsync(orphanedUser, AppRoles.User), Times.Once);
	}

	[Fact]
	public async Task SeedRolesAndAdminAsync_WhenRoleAssignmentFailsForOrphanedUser_DoesNotDeleteUser()
	{
		// Arrange — orphaned user from a previous run; role assignment fails again
		_mockRoleManager.Setup(r => r.RoleExistsAsync(It.IsAny<string>()))
			.ReturnsAsync(true);

		ApplicationUser orphanedUser = new() { Email = "admin@test.com" };
		_mockUserManager.Setup(u => u.FindByEmailAsync("admin@test.com"))
			.ReturnsAsync(orphanedUser);
		_mockUserManager.Setup(u => u.IsInRoleAsync(orphanedUser, AppRoles.Admin))
			.ReturnsAsync(false);
		_mockUserManager.Setup(u => u.AddToRoleAsync(orphanedUser, AppRoles.Admin))
			.ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = "RoleFail", Description = "Admin role assignment failed" }));

		// Act
		Func<Task> act = () => DatabaseSeederService.SeedRolesAndAdminAsync(_serviceProvider);

		// Assert — throws but does NOT delete the pre-existing user
		await act.Should().ThrowAsync<InvalidOperationException>()
			.WithMessage("*Admin role*Admin role assignment failed*");
		_mockUserManager.Verify(u => u.DeleteAsync(It.IsAny<ApplicationUser>()), Times.Never);
	}
}
