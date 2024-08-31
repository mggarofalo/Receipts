using Application.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Application.Tests.Services;

public class ApplicationServiceTests
{
	[Fact]
	public void RegisterApplicationServices_RegistersExpectedServices()
	{
		// Arrange
		Mock<IServiceCollection> services = new();
		Mock<IConfiguration> configuration = new();

		// Act
		services.Object.RegisterApplicationServices(configuration.Object);

		// Assert
		services.Verify(s => s.Add(It.Is<ServiceDescriptor>(
			d => d.ServiceType == typeof(MediatR.IMediator))), Times.Once);

		services.Verify(s => s.Add(It.Is<ServiceDescriptor>(
			d => d.ServiceType == typeof(AutoMapper.IMapper))), Times.Once);
	}

	[Fact]
	public void RegisterApplicationServices_DoesNotRegisterUnexpectedServices()
	{
		// Arrange
		ServiceCollection services = new();
		Mock<IConfiguration> configuration = new();
		int initialServiceCount = services.Count;

		// Act
		services.RegisterApplicationServices(configuration.Object);

		// Assert
		List<ServiceDescriptor> addedServices = services.Skip(initialServiceCount).ToList();

		// Verify MediatR services
		Assert.Contains(addedServices, s => s.ServiceType == typeof(MediatR.IMediator));

		// Verify AutoMapper services
		Assert.Contains(addedServices, s => s.ServiceType == typeof(AutoMapper.IMapper));

		// Verify no other services were added
		Assert.Equal(2, addedServices.Count);
	}
}