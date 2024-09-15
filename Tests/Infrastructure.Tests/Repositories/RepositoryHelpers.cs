using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Tests.Repositories;

public static class RepositoryHelpers
{
	public static IMapper CreateMapper<TProfile>() where TProfile : Profile, new()
	{
		MapperConfiguration configuration = new(cfg =>
		{
			cfg.AddProfile<TProfile>();
		});

		configuration.AssertConfigurationIsValid();

		return configuration.CreateMapper();
	}

	public static ApplicationDbContext CreateInMemoryContext()
	{
		DbContextOptionsBuilder<ApplicationDbContext> optionsBuilder = new();
		optionsBuilder.UseInMemoryDatabase(databaseName: $"TestDatabase_{Guid.NewGuid()}");
		DbContextOptions<ApplicationDbContext> options = optionsBuilder.Options;

		ApplicationDbContext context = new(options);
		context.Database.EnsureDeleted();
		context.Database.EnsureCreated();

		return context;
	}

	public static void ResetDatabase(this ApplicationDbContext context)
	{
		context.Database.EnsureDeleted();
		context.Database.EnsureCreated();
	}
}
