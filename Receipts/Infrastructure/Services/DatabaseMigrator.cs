using Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Services;

public class DatabaseMigrator(IServiceProvider serviceProvider) : IDatabaseMigrator
{
	public async Task MigrateAsync()
	{
		using IServiceScope scope = serviceProvider.CreateScope();
		ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
		await dbContext.Database.MigrateAsync();
	}
}