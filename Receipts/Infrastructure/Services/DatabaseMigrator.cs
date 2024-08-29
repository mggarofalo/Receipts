using Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Services;

public class DatabaseMigrator(IServiceProvider serviceProvider) : IDatabaseMigrator
{
	private readonly IServiceProvider _serviceProvider = serviceProvider;

	public async Task MigrateAsync()
	{
		using IServiceScope scope = _serviceProvider.CreateScope();
		ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
		await dbContext.Database.MigrateAsync();
	}
}