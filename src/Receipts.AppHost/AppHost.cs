var builder = DistributedApplication.CreateBuilder(args);

IResourceBuilder<PostgresServerResource> postgres = builder.AddPostgres("postgres")
	.WithImage("pgvector/pgvector", "pg17")
	.WithDataVolume()
	.WithPgAdmin();

IResourceBuilder<PostgresDatabaseResource> db = postgres.AddDatabase("receiptsdb");

// DbMigrator: applies EF Core migrations, then exits
IResourceBuilder<ProjectResource> migrator = builder.AddProject<Projects.DbMigrator>("db-migrator")
	.WithReference(db)
	.WaitFor(db);

// DbSeeder: seeds roles, then exits
IResourceBuilder<ProjectResource> seeder = builder.AddProject<Projects.DbSeeder>("db-seeder")
	.WithReference(db)
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
