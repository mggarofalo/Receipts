using API.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Presentation.API.Tests.Services;

public class LoggingServiceTests
{
	[Theory]
	[InlineData("Development")]
	[InlineData("Production")]
	public void AddLoggingService_RegistersLoggingService(string environmentName)
	{
		WebApplicationBuilder builder = WebApplication.CreateBuilder();
		builder.Environment.EnvironmentName = environmentName;
		builder.AddLoggingService();
		ServiceProvider serviceProvider = builder.Services.BuildServiceProvider();

		Assert.NotNull(Log.Logger);
		Assert.NotNull(serviceProvider.GetService<ILoggerFactory>());
		Assert.NotNull(serviceProvider.GetService<ILogger<LoggingServiceTests>>());
	}
}