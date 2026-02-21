var builder = DistributedApplication.CreateBuilder(args);

IResourceBuilder<PostgresServerResource> postgres = builder.AddPostgres("postgres")
	.WithDataVolume()
	.WithPgAdmin();

IResourceBuilder<PostgresDatabaseResource> db = postgres.AddDatabase("receiptsdb");

IResourceBuilder<ProjectResource> api = builder.AddProject<Projects.API>("api")
	.WithReference(db)
	.WaitFor(db);

builder.AddViteApp("frontend", "../Presentation/client")
	.WithReference(api)
	.WithHttpEndpoint(port: 5173, env: "PORT")
	.WithExternalHttpEndpoints();

await builder.Build().RunAsync();
