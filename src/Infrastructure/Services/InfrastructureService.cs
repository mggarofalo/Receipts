using Application.Interfaces;
using Application.Interfaces.Services;
using Infrastructure.Interfaces.Repositories;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Services;

public static class InfrastructureService
{
	public static IServiceCollection RegisterInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
	{
		services.AddDbContextFactory<ApplicationDbContext>(options =>
		{
			string? connectionString = configuration["POSTGRES_CONNECTION_STRING"];
			options.UseNpgsql(connectionString, b =>
			{
				string? assemblyName = typeof(ApplicationDbContext).Assembly.FullName;
				b.MigrationsAssembly(assemblyName);
			});
		});

		services
			.AddScoped<IReceiptService, ReceiptService>()
			.AddScoped<IAccountService, AccountService>()
			.AddScoped<ITransactionService, TransactionService>()
			.AddScoped<IReceiptItemService, ReceiptItemService>()
			.AddScoped<IReceiptRepository, ReceiptRepository>()
			.AddScoped<IAccountRepository, AccountRepository>()
			.AddScoped<ITransactionRepository, TransactionRepository>()
			.AddScoped<IReceiptItemRepository, ReceiptItemRepository>()
			.AddScoped<IDatabaseMigratorService, DatabaseMigratorService>();

		services.AddAutoMapper(typeof(InfrastructureService).Assembly);

		return services;
	}
}