var builder = DistributedApplication.CreateBuilder(args);

IResourceBuilder<ProjectResource> api = builder.AddProject<Projects.API>("api")
	.WithHttpEndpoint(port: 5000, name: "http")
	.WithHttpsEndpoint(port: 5001, name: "https");

await builder.Build().RunAsync();
