using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Tests.Repositories;

internal class TestDbContextFactory(DbContextOptions<ApplicationDbContext> options) : IDbContextFactory<ApplicationDbContext>
{
	public ApplicationDbContext CreateDbContext()
	{
		return new ApplicationDbContext(options);
	}
}
