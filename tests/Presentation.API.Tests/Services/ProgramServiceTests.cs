using API.Mapping.Aggregates;
using API.Mapping.Core;
using API.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Presentation.API.Tests.Services;

public class ProgramServiceTests
{
	[Fact]
	public void RegisterProgramServices_RegistersRequiredServices()
	{
		ServiceCollection serviceCollection = new();
		serviceCollection.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
		serviceCollection.RegisterProgramServices();
		ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

		// Verify that all Mapperly mappers are registered
		AssertThatMappersAreRegistered(serviceProvider);
	}

	private static void AssertThatMappersAreRegistered(ServiceProvider serviceProvider)
	{
		// Core mappers
		Assert.NotNull(serviceProvider.GetService<AccountMapper>());
		Assert.NotNull(serviceProvider.GetService<ReceiptMapper>());
		Assert.NotNull(serviceProvider.GetService<ReceiptItemMapper>());
		Assert.NotNull(serviceProvider.GetService<TransactionMapper>());

		// Aggregate mappers
		Assert.NotNull(serviceProvider.GetService<ReceiptWithItemsMapper>());
		Assert.NotNull(serviceProvider.GetService<TransactionAccountMapper>());
		Assert.NotNull(serviceProvider.GetService<TripMapper>());
	}
}
