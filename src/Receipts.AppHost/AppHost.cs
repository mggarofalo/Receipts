var builder = DistributedApplication.CreateBuilder(args);

IResourceBuilder<PostgresServerResource> postgres = builder.AddPostgres("postgres")
	.WithDataVolume()
	.WithPgAdmin();

IResourceBuilder<PostgresDatabaseResource> db = postgres.AddDatabase("receiptsdb");

IResourceBuilder<ProjectResource> api = builder.AddProject<Projects.API>("api")
	.WithReference(db)
	.WaitFor(db)
	.WithHttpEndpoint(port: 5000, name: "http")
	.WithHttpsEndpoint(port: 5001, name: "https");

await builder.Build().RunAsync();
