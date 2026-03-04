using System.Security.Claims;
using API.Filters;
using API.Hubs;
using API.Middleware;
using API.Services;
using API.Validators;
using FluentValidation;
using Serilog;

namespace API.Configuration;

public static class ApplicationConfiguration
{
	public static WebApplicationBuilder AddApplicationConfiguration(this WebApplicationBuilder builder)
	{
		if (builder.Environment.IsDevelopment())
		{
			builder.Configuration
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile($"appsettings.{Environments.Development}.json", optional: true, reloadOnChange: true)
				.AddUserSecrets<Program>(optional: true);
		}

		builder.Configuration.AddEnvironmentVariables();
		builder.AddLoggingService();

		return builder;
	}

	public static IServiceCollection AddApplicationServices(this IServiceCollection services)
	{
		services.AddValidatorsFromAssemblyContaining<CreateReceiptRequestValidator>();

		services.AddControllers(options =>
			{
				options.Filters.Add<FluentValidationActionFilter>();
			})
			.AddJsonOptions(options =>
			{
				options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
			});

		return services;
	}

	public static WebApplication UseApplicationServices(this WebApplication app)
	{
		app.UseSerilogRequestLogging(options =>
		{
			options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
			{
				diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
				diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.ToString());
				string? userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
				if (userId is not null)
				{
					diagnosticContext.Set("UserId", userId);
				}
			};
		});
		app.UseMiddleware<ValidationExceptionMiddleware>();
		app.UseHttpsRedirection();
		app.UseRouting();

		return app;
	}
}