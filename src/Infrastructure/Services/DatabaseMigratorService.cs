using Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class DatabaseMigratorService(IDbContextFactory<ApplicationDbContext> contextFactory) : IDatabaseMigratorService
{
	public async Task MigrateAsync()
	{
		using ApplicationDbContext dbContext = contextFactory.CreateDbContext();
		await dbContext.Database.MigrateAsync();
	}
}