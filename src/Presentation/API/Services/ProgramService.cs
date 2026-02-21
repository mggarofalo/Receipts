using API.Mapping.Aggregates;
using Application.Interfaces.Services;

namespace API.Services;

public static class ProgramService
{
	public static IServiceCollection RegisterProgramServices(this IServiceCollection services)
	{
		services.AddControllers();
		services.AddHttpContextAccessor();
		services.AddScoped<ICurrentUserAccessor, CurrentUserAccessor>();

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
