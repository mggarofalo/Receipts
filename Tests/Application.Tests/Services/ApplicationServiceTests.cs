using Application.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MediatR;
using AutoMapper;

namespace Application.Tests.Services;

public class ApplicationServiceTests
{
	[Fact]
	public void RegisterApplicationServices_RegistersRequiredServices()
	{
		ServiceCollection serviceCollection = new();
		serviceCollection.RegisterApplicationServices(new Mock<IConfiguration>().Object);
		ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();
		AssertThatIMediatorServiceIsNotNull(serviceProvider);
		AssertThatIMapperServiceIsNotNull(serviceProvider);
	}

	private static void AssertThatIMediatorServiceIsNotNull(ServiceProvider serviceProvider)
	{
		Assert.NotNull(serviceProvider.GetService<IMediator>());
	}

	private static void AssertThatIMapperServiceIsNotNull(ServiceProvider serviceProvider)
	{
		Assert.NotNull(serviceProvider.GetService<IMapper>());
	}
}