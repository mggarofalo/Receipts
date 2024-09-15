using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Tests;

public static class DbContextHelpers
{
	public static ApplicationDbContext CreateInMemoryContext()
	{
		DbContextOptionsBuilder<ApplicationDbContext> optionsBuilder = new();
		optionsBuilder.UseInMemoryDatabase(databaseName: $"TestDatabase_{Guid.NewGuid()}");
		DbContextOptions<ApplicationDbContext> options = optionsBuilder.Options;

		ApplicationDbContext context = new(options);
		context.ResetDatabase();

		return context;
	}

	public static void ResetDatabase(this ApplicationDbContext context)
	{
		context.Database.EnsureDeleted();
		context.Database.EnsureCreated();
	}
}
