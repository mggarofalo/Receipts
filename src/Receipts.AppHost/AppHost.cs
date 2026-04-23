var builder = DistributedApplication.CreateBuilder(args);

IResourceBuilder<PostgresServerResource> postgres = builder.AddPostgres("postgres")
	.WithImage("pgvector/pgvector", "pg17")
	.WithDataVolume()
	.WithPgAdmin(pgAdmin => pgAdmin.WithImageTag("9.13"));

IResourceBuilder<PostgresDatabaseResource> db = postgres.AddDatabase("receiptsdb");

// DbMigrator: applies EF Core migrations, then exits
IResourceBuilder<ProjectResource> migrator = builder.AddProject<Projects.DbMigrator>("db-migrator")
	.WithReference(db)
	.WaitFor(db);

// DbSeeder: seeds roles and admin user, then exits
IResourceBuilder<ProjectResource> seeder = builder.AddProject<Projects.DbSeeder>("db-seeder")
	.WithReference(db)
	.WaitForCompletion(migrator)
	// These override DbSeeder/appsettings.Development.json when running under Aspire.
	// Keep both in sync, or remove appsettings.Development.json AdminSeed section if
	// all local dev runs go through Aspire.
	.WithEnvironment("AdminSeed__Email", "admin@receipts.local")
	.WithEnvironment("AdminSeed__Password", "Admin123!@#")
	.WithEnvironment("AdminSeed__FirstName", "Admin")
	.WithEnvironment("AdminSeed__LastName", "User");

// VLM OCR: Ollama container serving glm-ocr:q8_0 for receipt extraction (RECEIPTS-616 epic).
// Named volume persists the model cache across restarts so the first-run ~1 GB pull happens once.
// Host port is left unset so Aspire picks a free one — Ollama's default 11434 is frequently
// already bound on developer machines running the native Ollama daemon, which would wedge Aspire
// startup since the API below does .WaitFor(vlmOcr).
IResourceBuilder<ContainerResource> vlmOcr = builder.AddContainer("vlm-ocr", "ollama/ollama", "latest")
	.WithVolume("vlm-ocr-models", "/root/.ollama")
	.WithHttpEndpoint(targetPort: 11434, name: "http");

// One-shot sidecar that pulls glm-ocr:q8_0 if it is not already cached in the shared volume,
// then exits. Idempotent — subsequent runs find the model present and skip the download.
builder.AddContainer("vlm-ocr-pull", "ollama/ollama", "latest")
	.WithEntrypoint("/bin/sh")
	.WithArgs("-c", "ollama list | grep -q 'glm-ocr:q8_0' || ollama pull glm-ocr:q8_0")
	.WithEnvironment("OLLAMA_HOST", "http://vlm-ocr:11434")
	.WaitFor(vlmOcr);

// API: starts after seeder completes; Ollama URL injected for the smoke test and future extraction service
IResourceBuilder<ProjectResource> api = builder.AddProject<Projects.API>("api")
	.WithReference(db)
	.WithEnvironment("Ollama__BaseUrl", vlmOcr.GetEndpoint("http"))
	.WaitForCompletion(seeder)
	.WaitFor(vlmOcr);

builder.AddViteApp("frontend", "../client")
	.WithReference(api)
	.WithHttpEndpoint(port: 5173, name: "vite", env: "PORT")
	.WithExternalHttpEndpoints();

await builder.Build().RunAsync();
