using Application.Services;
using FluentAssertions;
using MediatR;
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

	[Fact]
	public void RegisterApplicationServices_WithLicenseKey_RegistersServices()
	{
		ServiceCollection serviceCollection = new();
		IConfiguration configuration = new ConfigurationBuilder()
			.AddInMemoryCollection(new Dictionary<string, string?>
			{
				["MediatR:LicenseKey"] = "test-license-key"
			})
			.Build();

		serviceCollection.RegisterApplicationServices(configuration);

		serviceCollection.Should().Contain(sd => sd.ServiceType == typeof(IMediator));
	}

	[Fact]
	public void RegisterApplicationServices_WithEmptyLicenseKey_RegistersServices()
	{
		ServiceCollection serviceCollection = new();
		IConfiguration configuration = new ConfigurationBuilder()
			.AddInMemoryCollection(new Dictionary<string, string?>
			{
				["MediatR:LicenseKey"] = ""
			})
			.Build();

		serviceCollection.RegisterApplicationServices(configuration);

		serviceCollection.Should().Contain(sd => sd.ServiceType == typeof(IMediator));
	}

	[Fact]
	public void RegisterApplicationServices_WithoutLicenseKey_RegistersServices()
	{
		ServiceCollection serviceCollection = new();
		IConfiguration configuration = new ConfigurationBuilder().Build();

		serviceCollection.RegisterApplicationServices(configuration);

		serviceCollection.Should().Contain(sd => sd.ServiceType == typeof(IMediator));
	}
}
