using Asp.Versioning;

namespace API.Configuration;

public static class VersioningConfiguration
{
	public static IServiceCollection AddVersioningServices(this IServiceCollection services)
	{
		services.AddApiVersioning(options =>
		{
			options.DefaultApiVersion = new ApiVersion(1, 0);
			options.AssumeDefaultVersionWhenUnspecified = true;
			options.ReportApiVersions = true;
			options.ApiVersionReader = new HeaderApiVersionReader("api-version");
		})
		.AddMvc()
		.AddApiExplorer(options =>
		{
			options.GroupNameFormat = "'v'VVV";
		});

		return services;
	}
}
