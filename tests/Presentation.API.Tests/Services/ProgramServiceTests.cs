using API.Services;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;

namespace Presentation.API.Tests.Services;

public class ProgramServiceTests
{
	[Fact]
	public void RegisterProgramServices_RegistersRequiredServices()
	{
		ServiceCollection serviceCollection = new();
		serviceCollection.RegisterProgramServices();
		ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

		// Assume that the built-in services are registered
		AssertThatIMapperServiceIsNotNull(serviceProvider);
		AssertThatIMapperServiceHasValidConfiguration(serviceProvider);
	}

	private static void AssertThatIMapperServiceIsNotNull(ServiceProvider serviceProvider)
	{
		Assert.NotNull(serviceProvider.GetService<IMapper>());
	}

	private static void AssertThatIMapperServiceHasValidConfiguration(ServiceProvider serviceProvider)
	{
		IMapper mapper = serviceProvider.GetService<IMapper>()!;
		mapper.ConfigurationProvider.AssertConfigurationIsValid();
	}
}
