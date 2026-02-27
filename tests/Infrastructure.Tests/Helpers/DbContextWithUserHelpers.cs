using Application.Interfaces.Services;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Tests.Helpers;

public static class DbContextWithUserHelpers
{
	public static (IDbContextFactory<ApplicationDbContext>, MockCurrentUserAccessor) CreateInMemoryContextFactoryWithUser()
	{
		DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
			.UseInMemoryDatabase($"TestDatabase_{Guid.NewGuid()}")
			.Options;
		MockCurrentUserAccessor accessor = new();
		return (new TestDbContextFactoryWithUser(options, accessor), accessor);
	}
}

internal class TestDbContextFactoryWithUser(DbContextOptions<ApplicationDbContext> options, ICurrentUserAccessor currentUserAccessor) : IDbContextFactory<ApplicationDbContext>
{
	public ApplicationDbContext CreateDbContext() => new(options, currentUserAccessor);
}
