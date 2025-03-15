using Application.Interfaces;
using Application.Interfaces.Services;
using AutoMapper;
using Common;
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
		Mock<IConfiguration> mockConfiguration = SetupMockConfiguration();

		// Act
		services.RegisterInfrastructureServices(mockConfiguration.Object);
		ServiceProvider serviceProvider = services.BuildServiceProvider();

		// Assert
		AssertThatDbContextFactoryIsRegistered(serviceProvider);
		AssertThatRepositoriesAreRegistered(serviceProvider);
		AssertThatDatabaseMigratorIsRegistered(serviceProvider);
		AssertThatAutoMapperIsRegistered(serviceProvider);
	}

	private static Mock<IConfiguration> SetupMockConfiguration()
	{
		Mock<IConfiguration> mockConfiguration = new();

		mockConfiguration.SetupGet(c => c[ConfigurationVariables.PostgresHost]).Returns("localhost");
		mockConfiguration.SetupGet(c => c[ConfigurationVariables.PostgresPort]).Returns("5432");
		mockConfiguration.SetupGet(c => c[ConfigurationVariables.PostgresUser]).Returns("user");
		mockConfiguration.SetupGet(c => c[ConfigurationVariables.PostgresPassword]).Returns("password");
		mockConfiguration.SetupGet(c => c[ConfigurationVariables.PostgresDb]).Returns("database");

		return mockConfiguration;
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