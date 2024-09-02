using API.Services;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Presentation.API.Tests.Services;

public class ProgramServiceTests
{
	[Fact]
	public void RegisterProgramServices_RegistersRequiredServices()
	{
		ServiceCollection serviceCollection = new();
		serviceCollection.RegisterProgramServices(Assembly.GetExecutingAssembly());
		ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

		// Assume that the built-in services are registered
		AssertThatIMapperServiceIsNotNull(serviceProvider);
	}

	private static void AssertThatIMapperServiceIsNotNull(ServiceProvider serviceProvider)
	{
		Assert.NotNull(serviceProvider.GetService<IMapper>());
	}
}
