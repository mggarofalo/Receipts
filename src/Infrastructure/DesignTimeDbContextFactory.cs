using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Npgsql;

namespace Infrastructure;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
	public ApplicationDbContext CreateDbContext(string[] args)
	{
		NpgsqlDataSourceBuilder dataSourceBuilder = new(Environment.GetEnvironmentVariable("POSTGRES_CONNECTION_STRING"));
		dataSourceBuilder.UseVector();
		NpgsqlDataSource dataSource = dataSourceBuilder.Build();

		DbContextOptionsBuilder<ApplicationDbContext> builder = new();
		builder.UseNpgsql(dataSource, b => b.UseVector());
		return new ApplicationDbContext(builder.Options);
	}
}