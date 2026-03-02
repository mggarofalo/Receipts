using Application.Behaviors;
using Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Services;

public static class ApplicationService
{
	public static IServiceCollection RegisterApplicationServices(this IServiceCollection services, IConfiguration configuration)
	{
		services.AddMediatR(cfg =>
		{
			cfg.RegisterServicesFromAssembly(typeof(ICommand<>).Assembly);
			cfg.RegisterServicesFromAssembly(typeof(IQuery<>).Assembly);
			cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
		});

		return services;
	}
}