using Microsoft.EntityFrameworkCore;
using Npgsql;
using Testcontainers.PostgreSql;

namespace Infrastructure.IntegrationTests.Fixtures;

public class PostgresFixture : IAsyncLifetime
{
	private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("pgvector/pgvector:pg17")
		.Build();

	private NpgsqlDataSource? _dataSource;

	public string ConnectionString => _container.GetConnectionString();

	public async Task InitializeAsync()
	{
		await _container.StartAsync();

		NpgsqlDataSourceBuilder dataSourceBuilder = new(ConnectionString);
		dataSourceBuilder.UseVector();
		_dataSource = dataSourceBuilder.Build();

		// Run migrations to create the schema. The first connection caches
		// npgsql's type info before the pgvector extension exists, so
		// UseVector()'s vector mapping can't resolve. Reload types after
		// migrations so the cache picks up the newly-created extension.
		await using (ApplicationDbContext context = CreateDbContext())
		{
			await context.Database.MigrateAsync();
		}

		await using NpgsqlConnection reloadConnection = await _dataSource.OpenConnectionAsync();
		await reloadConnection.ReloadTypesAsync();
	}

	public ApplicationDbContext CreateDbContext()
	{
		if (_dataSource is null)
		{
			throw new InvalidOperationException("Fixture has not been initialized. Call InitializeAsync() first.");
		}

		DbContextOptionsBuilder<ApplicationDbContext> builder = new();
		builder.UseNpgsql(_dataSource, b => b.UseVector());

		return new ApplicationDbContext(builder.Options);
	}

	public async Task DisposeAsync()
	{
		if (_dataSource is not null)
		{
			await _dataSource.DisposeAsync();
		}

		await _container.DisposeAsync();
		GC.SuppressFinalize(this);
	}
}
