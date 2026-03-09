using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Tests;

public class DesignTimeDbContextFactoryTests
{
	[Fact]
	public void CreateDbContext_ReturnsApplicationDbContext()
	{
		// Arrange
		DesignTimeDbContextFactory factory = new();
		string connectionString = "Host=localhost;Database=testdb;Username=testuser;Password=testpass";
		Environment.SetEnvironmentVariable("POSTGRES_CONNECTION_STRING", connectionString);

		// Act
		ApplicationDbContext context = factory.CreateDbContext([]);

		// Assert
		Assert.NotNull(context);
		Assert.IsType<ApplicationDbContext>(context);
	}

	[Fact]
	public void CreateDbContext_UsesNpgsqlProvider()
	{
		// Arrange
		DesignTimeDbContextFactory factory = new();
		string connectionString = "Host=localhost;Database=testdb;Username=testuser;Password=testpass";
		Environment.SetEnvironmentVariable("POSTGRES_CONNECTION_STRING", connectionString);

		// Act
		ApplicationDbContext context = factory.CreateDbContext([]);

		// Assert
		Assert.Contains("Npgsql", context.Database.ProviderName);
	}

	[Fact]
	public void CreateDbContext_UsesEnvironmentVariable()
	{
		// Arrange
		DesignTimeDbContextFactory factory = new();
		string connectionString = "Host=testhost;Database=testdb;Username=testuser;Password=testpass";
		Environment.SetEnvironmentVariable("POSTGRES_CONNECTION_STRING", connectionString);

		// Act
		ApplicationDbContext context = factory.CreateDbContext([]);

		// Assert — NpgsqlDataSource may normalize the connection string,
		// so verify key parameters are present rather than exact match.
		string? actualConnectionString = context.Database.GetConnectionString();
		Assert.NotNull(actualConnectionString);
		Assert.Contains("testhost", actualConnectionString);
		Assert.Contains("testdb", actualConnectionString);
		Assert.Contains("testuser", actualConnectionString);
	}
}
