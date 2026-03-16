namespace Infrastructure.IntegrationTests.Fixtures;

[CollectionDefinition(Name)]
public class PostgresCollection : ICollectionFixture<PostgresFixture>
{
	public const string Name = "Postgres";
}
