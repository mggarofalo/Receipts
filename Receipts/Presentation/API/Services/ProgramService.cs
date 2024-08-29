namespace API.Services;

public static class ProgramService
{
	public static IServiceCollection RegisterProgramServices(this IServiceCollection services, IConfiguration configuration)
	{
		services.AddControllers();

		services
			.AddEndpointsApiExplorer()
			.AddSwaggerGen()
			.AddAutoMapper(typeof(Program).Assembly);

		return services;
	}
}
