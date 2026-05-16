using Application.Services;
using FluentAssertions;
using Mediator;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Tests.Services;

public class ApplicationServiceTests
{
	[Fact]
	public void RegisterApplicationServices_RegistersRequiredServices()
	{
		ServiceCollection serviceCollection = new();
		IConfiguration configuration = new ConfigurationBuilder().Build();
		serviceCollection.RegisterApplicationServices(configuration);

		serviceCollection.Should().Contain(sd => sd.ServiceType == typeof(IMediator));
	}
}
