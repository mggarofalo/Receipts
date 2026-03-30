using Application.Behaviors;
using Application.Interfaces;
using Application.Interfaces.Services;
using Application.Services.Parsing;
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
			cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
		});

		// Receipt parsing services (store-specific first, generic fallback last)
		services.AddSingleton<IReceiptParser, WalmartReceiptParser>();
		services.AddSingleton<IReceiptParser, CostcoReceiptParser>();
		services.AddSingleton<IReceiptParser, AldiReceiptParser>();
		services.AddSingleton<IReceiptParser, TargetReceiptParser>();
		services.AddSingleton<IReceiptParser, KrogerReceiptParser>();
		services.AddSingleton<IReceiptParser, GenericReceiptParser>();
		services.AddSingleton<IReceiptParsingService, ReceiptParsingService>();

		return services;
	}
}