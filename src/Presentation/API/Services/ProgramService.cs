using System.Reflection;

namespace API.Services;

public static class ProgramService
{
	public static IServiceCollection RegisterProgramServices(this IServiceCollection services)
	{
		services.AddControllers();
		services.AddEndpointsApiExplorer();
		services.AddSwaggerGen();
		services.AddAutoMapper(cfg => { }, Assembly.GetExecutingAssembly());
		services.AddSignalR();

		return services;
	}
}
