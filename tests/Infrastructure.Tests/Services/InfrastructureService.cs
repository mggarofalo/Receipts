using Application.Interfaces;
using Application.Interfaces.Services;
using AutoMapper;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
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
		AssertThatDbContextFactoryIsRegistered(serviceProvider);
		AssertThatRepositoriesAreRegistered(serviceProvider);
		AssertThatDatabaseMigratorIsRegistered(serviceProvider);
		AssertThatAutoMapperIsRegistered(serviceProvider);
	}

	private static void AssertThatDbContextFactoryIsRegistered(ServiceProvider serviceProvider)
	{
		Assert.NotNull(serviceProvider.GetService<IDbContextFactory<ApplicationDbContext>>());
	}

	private static void AssertThatRepositoriesAreRegistered(ServiceProvider serviceProvider)
	{
		Assert.NotNull(serviceProvider.GetService<IReceiptService>());
		Assert.NotNull(serviceProvider.GetService<IAccountService>());
		Assert.NotNull(serviceProvider.GetService<ITransactionService>());
		Assert.NotNull(serviceProvider.GetService<IReceiptItemService>());
	}

	private static void AssertThatDatabaseMigratorIsRegistered(ServiceProvider serviceProvider)
	{
		Assert.NotNull(serviceProvider.GetService<IDatabaseMigratorService>());
	}

	private static void AssertThatAutoMapperIsRegistered(ServiceProvider serviceProvider)
	{
		Assert.NotNull(serviceProvider.GetService<IMapper>());
	}
}