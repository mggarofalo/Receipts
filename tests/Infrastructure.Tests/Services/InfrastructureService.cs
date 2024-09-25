using Application.Interfaces;
using Application.Interfaces.Repositories;
using AutoMapper;
using Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Infrastructure.Tests.Services;

public class InfrastructureServiceTests
{
	[Fact]
	public void RegisterInfrastructureServices_RegistersRequiredServices()
	{
		// Arrange
		ServiceCollection services = new();
		Mock<IConfiguration> mockConfiguration = new();

		// Act
		services.RegisterInfrastructureServices(mockConfiguration.Object);
		ServiceProvider serviceProvider = services.BuildServiceProvider();

		// Assert
		AssertThatDbContextIsRegistered(serviceProvider);
		AssertThatRepositoriesAreRegistered(serviceProvider);
		AssertThatDatabaseMigratorIsRegistered(serviceProvider);
		AssertThatAutoMapperIsRegistered(serviceProvider);
	}

	private static void AssertThatDbContextIsRegistered(ServiceProvider serviceProvider)
	{
		Assert.NotNull(serviceProvider.GetService<ApplicationDbContext>());
	}

	private static void AssertThatRepositoriesAreRegistered(ServiceProvider serviceProvider)
	{
		Assert.NotNull(serviceProvider.GetService<IReceiptRepository>());
		Assert.NotNull(serviceProvider.GetService<IAccountRepository>());
		Assert.NotNull(serviceProvider.GetService<ITransactionRepository>());
		Assert.NotNull(serviceProvider.GetService<IReceiptItemRepository>());
	}

	private static void AssertThatDatabaseMigratorIsRegistered(ServiceProvider serviceProvider)
	{
		Assert.NotNull(serviceProvider.GetService<IDatabaseMigrator>());
	}

	private static void AssertThatAutoMapperIsRegistered(ServiceProvider serviceProvider)
	{
		Assert.NotNull(serviceProvider.GetService<IMapper>());
	}
}