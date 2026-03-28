# DbSeeder

Seeds the database with initial data required for the application to function:

- Creates ASP.NET Identity roles (`Admin`, `User`)
- Optionally creates an initial admin user account (if admin seed config is provided)

The API does **not** self-seed. This tool must run after DbMigrator and before the API starts (handled automatically by Aspire and Docker Compose orchestration).

## Usage

```bash
dotnet run --project src/Tools/DbSeeder/DbSeeder.csproj
```

Requires database connection environment variables. Admin user creation is optional — if `AdminSeed__Email` and `AdminSeed__Password` are set, an admin user is created; otherwise only roles are seeded. Additional optional variables: `AdminSeed__FirstName`, `AdminSeed__LastName`.
