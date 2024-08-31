using Application.Common;
using Application.Queries;
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
		});

		services.AddAutoMapper(typeof(ApplicationService).Assembly);

		return services;
	}
}