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

// VLM OCR: Ollama container serving qwen2.5vl:3b for receipt extraction (RECEIPTS-616 epic).
// Named volume persists the model cache across restarts so the first-run ~3 GB pull happens once.
// Host port is left unset so Aspire picks a free one — Ollama's default 11434 is frequently
// already bound on developer machines running the native Ollama daemon, which would wedge Aspire
// startup since the API below does .WaitFor(vlmOcr).
//
// .WithHttpHealthCheck("/api/tags") gates WaitFor() on Ollama actually responding rather than
// just the container being up — without this, dependents (the pull sidecar, the API smoke test)
// would race the Ollama startup. /api/tags is the lightest Ollama endpoint that returns 200
// once the server is ready (RECEIPTS-636).
IResourceBuilder<ContainerResource> vlmOcr = builder.AddContainer("vlm-ocr", "ollama/ollama", "latest")
	.WithVolume("vlm-ocr-models", "/root/.ollama")
	.WithHttpEndpoint(targetPort: 11434, name: "http")
	.WithHttpHealthCheck("/api/tags");

// One-shot sidecar that pulls qwen2.5vl:3b if it is not already cached in the shared volume,
// then exits. Idempotent — subsequent runs find the model present and skip the download.
//
// The API and VlmEval gate on this sidecar via .WaitForCompletion (RECEIPTS-636), so a
// non-zero exit here permanently blocks dependents. Retry up to 5 times with backoff
// to tolerate transient network failures during the ~3 GB cold-start pull. Mirrors the
// docker-compose vlm-ocr-pull retry pattern.
const string vlmOcrPullCommand = """
	for i in 1 2 3 4 5; do
	  if ollama list | grep -q 'qwen2.5vl:3b'; then
	    echo "qwen2.5vl:3b already present; skipping pull"
	    exit 0
	  fi
	  echo "Pulling qwen2.5vl:3b (attempt $i/5)..."
	  if ollama pull qwen2.5vl:3b; then
	    exit 0
	  fi
	  echo "Pull failed; sleeping before retry"
	  sleep 10
	done
	echo "All pull attempts failed" >&2
	exit 1
	""";
IResourceBuilder<ContainerResource> vlmOcrPull = builder.AddContainer("vlm-ocr-pull", "ollama/ollama", "latest")
	.WithEntrypoint("/bin/sh")
	.WithArgs("-c", vlmOcrPullCommand)
	.WithEnvironment("OLLAMA_HOST", "http://vlm-ocr:11434")
	.WaitFor(vlmOcr);

// API: starts after seeder completes; Ollama URL injected for the smoke test and future extraction service.
// .WaitForCompletion(vlmOcrPull) ensures the model is fully pulled (cold first-run can be ~3 GB)
// before the API boots so the smoke test in InfrastructureService never catches Ollama mid-pull
// (RECEIPTS-636).
IResourceBuilder<ProjectResource> api = builder.AddProject<Projects.API>("api")
	.WithReference(db)
	.WithEnvironment("Ollama__BaseUrl", vlmOcr.GetEndpoint("http"))
	.WaitForCompletion(seeder)
	.WaitFor(vlmOcr)
	.WaitForCompletion(vlmOcrPull);

// VlmEval: dev-only sidecar that runs the local VLM receipt-extraction pipeline against a
// gitignored directory of real receipt fixtures and logs a scorecard. Parked on startup —
// trigger from the Aspire dashboard. See src/Tools/VlmEval/README.md.
string repoRoot = Path.GetFullPath(Path.Combine(builder.AppHostDirectory, "..", ".."));
string vlmEvalFixturesPath = Path.Combine(repoRoot, "fixtures", "vlm-eval");

builder.AddProject<Projects.VlmEval>("vlm-eval")
	.WithEnvironment("Ollama__BaseUrl", vlmOcr.GetEndpoint("http"))
	.WithEnvironment("VlmEval__FixturesPath", vlmEvalFixturesPath)
	.WaitFor(vlmOcr)
	.WaitForCompletion(vlmOcrPull)
	.WithExplicitStart();

builder.AddViteApp("frontend", "../client")
	.WithReference(api)
	.WithHttpEndpoint(port: 5173, name: "vite", env: "PORT")
	.WithExternalHttpEndpoints();

await builder.Build().RunAsync();
