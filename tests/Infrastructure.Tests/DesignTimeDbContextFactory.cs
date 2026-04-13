using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Tests;

/// <summary>
/// Serializes tests that mutate the process-global POSTGRES_CONNECTION_STRING
/// environment variable. Without this, xUnit's default parallelism lets sibling
/// tests race against each other: one test sets the variable to null while another
/// sets it to a connection string, and the "throws when not set" assertion flakes.
/// </summary>
[CollectionDefinition(Name, DisableParallelization = true)]
public class PostgresConnectionStringEnvCollection
{
	public const string Name = "PostgresConnectionStringEnv";
}

[Collection(PostgresConnectionStringEnvCollection.Name)]
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

	[Fact]
	public void CreateDbContext_ThrowsWhenEnvironmentVariableNotSet()
	{
		// Arrange
		DesignTimeDbContextFactory factory = new();
		Environment.SetEnvironmentVariable("POSTGRES_CONNECTION_STRING", null);

		// Act & Assert
		InvalidOperationException ex = Assert.Throws<InvalidOperationException>(
			() => factory.CreateDbContext([]));
		Assert.Contains("POSTGRES_CONNECTION_STRING", ex.Message);
	}
}
