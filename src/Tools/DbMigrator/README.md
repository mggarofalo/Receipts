# DbMigrator

Applies EF Core database migrations to PostgreSQL.

The API does **not** self-migrate. This tool must run before the API starts (handled automatically by Aspire and Docker Compose orchestration).

## Usage

```bash
dotnet run --project src/Tools/DbMigrator/DbMigrator.csproj
```

Requires database connection environment variables: `POSTGRES_HOST`, `POSTGRES_PORT`, `POSTGRES_USER`, `POSTGRES_PASSWORD`, `POSTGRES_DB`.

## Creating New Migrations

```bash
dotnet ef migrations add MigrationName \
  --project src/Infrastructure/Infrastructure.csproj \
  --startup-project src/Tools/DbMigrator/DbMigrator.csproj
```
