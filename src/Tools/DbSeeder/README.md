# DbSeeder

Seeds the database with initial data required for the application to function:

- Creates ASP.NET Identity roles (`Admin`, `User`)
- Creates the initial admin user account

The API does **not** self-seed. This tool must run after DbMigrator and before the API starts (handled automatically by Aspire and Docker Compose orchestration).

## Usage

```bash
dotnet run --project src/Tools/DbSeeder/DbSeeder.csproj
```

Requires database connection environment variables plus admin seed configuration: `AdminSeed__Email`, `AdminSeed__Password`, `AdminSeed__FirstName`, `AdminSeed__LastName`.
