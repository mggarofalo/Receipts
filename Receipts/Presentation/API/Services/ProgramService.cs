using System.Reflection;

namespace API.Services;

public static class ProgramService
{
	public static IServiceCollection RegisterProgramServices(this IServiceCollection services, Assembly assembly)
	{
		services.AddControllers();
		services.AddEndpointsApiExplorer();
		services.AddSwaggerGen();
		services.AddAutoMapper(assembly);
		services.AddSignalR();

		return services;
	}
}
