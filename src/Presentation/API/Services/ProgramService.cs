using System.Reflection;

namespace API.Services;

public static class ProgramService
{
	public static IServiceCollection RegisterProgramServices(this IServiceCollection services)
	{
		services.AddControllers();
		services.AddEndpointsApiExplorer();
		services.AddSwaggerGen();
		// TODO: Add AutoMapper license key (see MGG-17)
		// Register at https://automapper.io for free Community Edition
		services.AddAutoMapper(cfg =>
		{
			// cfg.LicenseKey = configuration["AutoMapper:LicenseKey"];
		}, Assembly.GetExecutingAssembly());
		services.AddSignalR();

		return services;
	}
}
