using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Infrastructure;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
	public ApplicationDbContext CreateDbContext(string[] args)
	{
		DbContextOptionsBuilder<ApplicationDbContext> builder = new();
		string? connectionString = "Host=localhost;Port=5432;Database=receipts;Username=postgres;Password=admin;";

		builder.UseNpgsql(connectionString);

		return new ApplicationDbContext(builder.Options);
	}
}