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
		});

		// TODO: Add AutoMapper license key (see MGG-17)
		// Register at https://automapper.io for free Community Edition
		services.AddAutoMapper(cfg =>
		{
			// cfg.LicenseKey = configuration["AutoMapper:LicenseKey"];
		}, typeof(ApplicationService).Assembly);

		return services;
	}
}