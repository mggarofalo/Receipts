using Application.Behaviors;
using Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Services;

public static class ApplicationService
{
	public static IServiceCollection RegisterApplicationServices(this IServiceCollection services, IConfiguration configuration)
	{
		services.AddMediator(opts =>
		{
			opts.ServiceLifetime = ServiceLifetime.Scoped;
			opts.Assemblies = [typeof(ICommand<>)];
			opts.PipelineBehaviors = [typeof(ValidationBehavior<,>)];
		});

		return services;
	}
}