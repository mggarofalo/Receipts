using System.IO.Compression;
using System.Security.Claims;
using System.Text.Json.Serialization;
using API.Filters;
using API.Hubs;
using API.Middleware;
using API.Services;
using API.Validators;
using FluentValidation;
using Microsoft.AspNetCore.ResponseCompression;
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
				options.Filters.Add<ResourceIdResultFilter>();
			})
			.AddJsonOptions(options =>
			{
				options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
				options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
			});

		services.AddResponseCompression(options =>
		{
			options.EnableForHttps = true;
			options.Providers.Add<BrotliCompressionProvider>();
			options.Providers.Add<GzipCompressionProvider>();
		});
		services.Configure<BrotliCompressionProviderOptions>(options =>
			options.Level = CompressionLevel.Fastest);
		services.Configure<GzipCompressionProviderOptions>(options =>
			options.Level = CompressionLevel.SmallestSize);

		return services;
	}

	public static WebApplication UseApplicationServices(this WebApplication app)
	{
		app.UseResponseCompression();
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
		if (app.Environment.IsDevelopment())
		{
			app.UseHttpsRedirection();
		}

		app.UseRouting();

		return app;
	}
}