using Application.Interfaces;
using Application.Interfaces.Repositories;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Services;

public static class InfrastructureService
{
	public static IServiceCollection RegisterInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
	{
		services.AddDbContext<ApplicationDbContext>(options =>
		{
			string? connectionString = Environment.GetEnvironmentVariable("POSTGRES_CONNECTION_STRING");
			options.UseNpgsql(connectionString, b =>
			{
				string? assemblyName = typeof(ApplicationDbContext).Assembly.FullName;
				b.MigrationsAssembly(assemblyName);
			});
		});

		services
			.AddScoped<IReceiptRepository, ReceiptRepository>()
			.AddScoped<IAccountRepository, AccountRepository>()
			.AddScoped<ITransactionRepository, TransactionRepository>()
			.AddScoped<IReceiptItemRepository, ReceiptItemRepository>()
			.AddScoped<IDatabaseMigrator, DatabaseMigrator>();

		services.AddAutoMapper(typeof(InfrastructureService).Assembly);

		return services;
	}
}