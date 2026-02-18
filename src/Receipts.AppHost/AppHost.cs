var builder = DistributedApplication.CreateBuilder(args);

IResourceBuilder<PostgresServerResource> postgres = builder.AddPostgres("postgres")
	.WithDataVolume()
	.WithPgAdmin();

IResourceBuilder<PostgresDatabaseResource> db = postgres.AddDatabase("receiptsdb");

IResourceBuilder<ProjectResource> api = builder.AddProject<Projects.API>("api")
	.WithReference(db)
	.WaitFor(db);

await builder.Build().RunAsync();
