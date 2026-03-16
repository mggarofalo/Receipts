using Microsoft.EntityFrameworkCore;
using Npgsql;
using Testcontainers.PostgreSql;

namespace Infrastructure.IntegrationTests.Fixtures;

public class PostgresFixture : IAsyncLifetime
{
	private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("pgvector/pgvector:pg17")
		.Build();

	public string ConnectionString => _container.GetConnectionString();

	public async Task InitializeAsync()
	{
		await _container.StartAsync();

		// Run migrations to create the schema
		using ApplicationDbContext context = CreateDbContext();
		await context.Database.MigrateAsync();
	}

	public ApplicationDbContext CreateDbContext()
	{
		NpgsqlDataSourceBuilder dataSourceBuilder = new(ConnectionString);
		dataSourceBuilder.UseVector();
		NpgsqlDataSource dataSource = dataSourceBuilder.Build();

		DbContextOptionsBuilder<ApplicationDbContext> builder = new();
		builder.UseNpgsql(dataSource, b => b.UseVector());

		return new ApplicationDbContext(builder.Options);
	}

	public async Task DisposeAsync()
	{
		await _container.DisposeAsync();
		GC.SuppressFinalize(this);
	}
}
