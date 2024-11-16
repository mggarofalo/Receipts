using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Tests.Repositories;

public static class DbContextHelpers
{
	public static IDbContextFactory<ApplicationDbContext> CreateInMemoryContextFactory()
	{
		DbContextOptions<ApplicationDbContext> options = CreateInMemoryContextOptions($"TestDatabase_{Guid.NewGuid()}");
		return new TestDbContextFactory(options);
	}

	public static DbContextOptions<ApplicationDbContext> CreateInMemoryContextOptions(string databaseName)
	{
		DbContextOptionsBuilder<ApplicationDbContext> optionsBuilder = new();
		optionsBuilder.UseInMemoryDatabase(databaseName: databaseName);
		return optionsBuilder.Options;
	}

	public static void ResetDatabase(this IDbContextFactory<ApplicationDbContext> factory)
	{
		using ApplicationDbContext context = factory.CreateDbContext();
		context.Database.EnsureDeleted();
		context.Database.EnsureCreated();
	}
}
