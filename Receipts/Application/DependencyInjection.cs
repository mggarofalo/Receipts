using Application.Common;
using Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class DependencyInjection
{
	public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration, IInfrastructureService infrastructureService)
	{
		services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ICommand<>).Assembly));

		// Register infrastructure services through the interface
		infrastructureService.AddInfrastructureServices(services, configuration);

		return services;
	}
}