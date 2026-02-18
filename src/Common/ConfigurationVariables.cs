namespace Common;

public static class ConfigurationVariables
{
	public const string PostgresHost = "POSTGRES_HOST";
	public const string PostgresPort = "POSTGRES_PORT";
	public const string PostgresUser = "POSTGRES_USER";
	public const string PostgresPassword = "POSTGRES_PASSWORD";
	public const string PostgresDb = "POSTGRES_DB";

	// Aspire-injected connection string name (set via WithReference(db) in AppHost)
	public const string AspireConnectionStringName = "receiptsdb";
}

