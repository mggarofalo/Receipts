using Application.Services;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Application.Tests.Services;

public class ApplicationServiceTests
{
	[Fact]
	public void RegisterApplicationServices_RegistersRequiredServices()
	{
		ServiceCollection serviceCollection = new();
		serviceCollection.RegisterApplicationServices(new Mock<IConfiguration>().Object);

		serviceCollection.Should().Contain(sd => sd.ServiceType == typeof(IMediator));
	}
}