using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Infrastructure;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
	public ApplicationDbContext CreateDbContext(string[] args)
	{
		DbContextOptionsBuilder<ApplicationDbContext> builder = new();
		string? connectionString = Environment.GetEnvironmentVariable("POSTGRES_CONNECTION_STRING");

		builder.UseNpgsql(connectionString);

		return new ApplicationDbContext(builder.Options);
	}
}