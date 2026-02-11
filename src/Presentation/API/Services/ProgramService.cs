using API.Mapping.Aggregates;

namespace API.Services;

public static class ProgramService
{
	public static IServiceCollection RegisterProgramServices(this IServiceCollection services)
	{
		services.AddControllers();
		services.AddEndpointsApiExplorer();
		services.AddSwaggerGen();

		services
			.AddSingleton<API.Mapping.Core.AccountMapper>()
			.AddSingleton<API.Mapping.Core.ReceiptMapper>()
			.AddSingleton<API.Mapping.Core.ReceiptItemMapper>()
			.AddSingleton<API.Mapping.Core.TransactionMapper>()
			.AddSingleton<ReceiptWithItemsMapper>()
			.AddSingleton<TransactionAccountMapper>()
			.AddSingleton<TripMapper>();

		services.AddSignalR();

		return services;
	}
}
