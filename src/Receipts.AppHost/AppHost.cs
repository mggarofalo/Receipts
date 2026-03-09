var builder = DistributedApplication.CreateBuilder(args);

IResourceBuilder<PostgresServerResource> postgres = builder.AddPostgres("postgres")
	.WithImage("pgvector/pgvector", "pg17")
	.WithDataVolume()
	.WithPgAdmin();

IResourceBuilder<PostgresDatabaseResource> db = postgres.AddDatabase("receiptsdb");

// Admin seed parameters (resolved from AppHost user secrets or Aspire dashboard)
IResourceBuilder<ParameterResource> adminEmail = builder.AddParameter("admin-email");
IResourceBuilder<ParameterResource> adminPassword = builder.AddParameter("admin-password", secret: true);
IResourceBuilder<ParameterResource> adminFirstName = builder.AddParameter("admin-first-name");
IResourceBuilder<ParameterResource> adminLastName = builder.AddParameter("admin-last-name");
// DbMigrator: applies EF Core migrations, then exits
IResourceBuilder<ProjectResource> migrator = builder.AddProject<Projects.DbMigrator>("db-migrator")
	.WithReference(db)
	.WaitFor(db);

// DbSeeder: seeds roles and admin user, then exits
IResourceBuilder<ProjectResource> seeder = builder.AddProject<Projects.DbSeeder>("db-seeder")
	.WithReference(db)
	.WithEnvironment("AdminSeed__Email", adminEmail)
	.WithEnvironment("AdminSeed__Password", adminPassword)
	.WithEnvironment("AdminSeed__FirstName", adminFirstName)
	.WithEnvironment("AdminSeed__LastName", adminLastName)
	.WaitForCompletion(migrator);

// API: starts after seeder completes
IResourceBuilder<ProjectResource> api = builder.AddProject<Projects.API>("api")
	.WithReference(db)
	.WaitForCompletion(seeder);

builder.AddViteApp("frontend", "../client")
	.WithReference(api)
	.WithHttpEndpoint(port: 5173, name: "vite", env: "PORT")
	.WithExternalHttpEndpoints();

await builder.Build().RunAsync();
