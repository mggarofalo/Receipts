using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Tests;

public static class DbContextHelpers
{
	public static ApplicationDbContext CreateInMemoryContext()
	{
		DbContextOptionsBuilder<ApplicationDbContext> optionsBuilder = new();
		optionsBuilder.UseInMemoryDatabase(databaseName: $"TestDatabase_{Guid.NewGuid()}");

		ApplicationDbContext context = new(optionsBuilder.Options);
		context.ResetDatabase();

		return context;
	}

	public static void ResetDatabase(this ApplicationDbContext context)
	{
		context.Database.EnsureDeleted();
		context.Database.EnsureCreated();
	}
}
