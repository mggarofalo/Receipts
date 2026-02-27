# Paperless Integration Service - Implementation Plan

## Overview

This document outlines the implementation plan for integrating Paperless-ngx document management system with the Receipts application. The integration will automatically process receipts from Paperless, extract structured data using AI, and create receipt records in the system.

## Architecture

### High-Level Flow

```
Paperless-ngx → PaperlessIntegration Service → Claude API → Receipts API → PostgreSQL
                ↓
        Update Paperless Tags
```

1. **PaperlessIntegration Service** polls Paperless for documents tagged with `inbox` + `receipt`
2. Downloads document images/PDFs from Paperless
3. Sends images to **Claude API** for structured data extraction
4. Posts extracted data to **Receipts API** (`/api/receiptwithitems`)
5. Stores Paperless document ID mapping in database
6. Updates Paperless document tags to mark as processed

### Service Type

**Worker Service** - Long-running background service using .NET's `BackgroundService` base class
- Hosted in Docker container
- Scheduled job execution using **Hangfire** or similar
- Can be deployed alongside API container or standalone

## Project Structure

### New Projects

```
src/
├── Services/
│   └── PaperlessIntegration/
│       ├── PaperlessIntegration.csproj
│       ├── Program.cs
│       ├── Worker.cs
│       ├── Services/
│       │   ├── IPaperlessService.cs
│       │   ├── PaperlessService.cs
│       │   ├── IReceiptExtractionService.cs
│       │   ├── ClaudeReceiptExtractionService.cs
│       │   └── IReceiptsApiClient.cs
│       ├── Models/
│       │   ├── PaperlessDocument.cs
│       │   ├── ExtractedReceiptData.cs
│       │   └── ProcessingResult.cs
│       ├── Configuration/
│       │   ├── PaperlessSettings.cs
│       │   ├── ClaudeSettings.cs
│       │   └── SchedulingSettings.cs
│       └── appsettings.json
```

### Domain Changes

**New Entity: `PaperlessDocumentMapping`**

```csharp
// src/Domain/Entities/PaperlessDocumentMapping.cs
public class PaperlessDocumentMapping
{
    public int Id { get; set; }
    public int ReceiptId { get; set; }
    public Receipt Receipt { get; set; }
    public int PaperlessDocumentId { get; set; }
    public string PaperlessDocumentTitle { get; set; }
    public DateTime ProcessedAt { get; set; }
    public string ProcessingStatus { get; set; } // Success, Failed, Partial
    public string? ErrorMessage { get; set; }
}
```

**Receipt Entity Update**

```csharp
// Add navigation property to Receipt.cs
public ICollection<PaperlessDocumentMapping> PaperlessDocuments { get; set; }
```

## Implementation Details

### 1. Paperless API Integration

**IPaperlessService Interface**

```csharp
public interface IPaperlessService
{
    Task<IEnumerable<PaperlessDocument>> GetUnprocessedReceiptsAsync(CancellationToken ct);
    Task<byte[]> DownloadDocumentAsync(int documentId, CancellationToken ct);
    Task UpdateDocumentTagsAsync(int documentId, string[] tagsToAdd, string[] tagsToRemove, CancellationToken ct);
}
```

**Key Endpoints**
- `GET /api/documents/?tags__name__in=inbox,receipt` - Query documents
- `GET /api/documents/{id}/download/` - Download document
- `PATCH /api/documents/{id}/` - Update tags

**Authentication**
- API Token via `Authorization: Token {token}` header
- Store token in configuration (environment variable or secrets)

### 2. AI Receipt Extraction

**IReceiptExtractionService Interface**

```csharp
public interface IReceiptExtractionService
{
    Task<ExtractedReceiptData> ExtractReceiptDataAsync(
        byte[] imageData,
        string mimeType,
        CancellationToken ct);
}
```

**Claude Integration**

Use Anthropic SDK (`Anthropic` NuGet package):
- Model: `claude-sonnet-4-5-20250929` (multimodal)
- Structured output with JSON schema for receipt data
- Retry logic with exponential backoff
- Token usage logging for cost monitoring

**Extraction Prompt**

```text
Extract receipt information from this image and return ONLY valid JSON (no markdown):

{
  "description": "Store or merchant name",
  "location": "Store location/address if visible",
  "date": "YYYY-MM-DD format",
  "taxAmount": 0.00,
  "totalAmount": 0.00,
  "items": [
    {
      "description": "Item name/description",
      "amount": 0.00,
      "quantity": 1,
      "category": "Food/Supplies/etc"
    }
  ],
  "confidence": "high/medium/low",
  "notes": "Any unclear or ambiguous details"
}

If you cannot read the receipt clearly, set confidence to "low" and include notes.
Return ONLY the JSON object, no other text.
```

### 3. Receipts API Integration

**IReceiptsApiClient Interface**

```csharp
public interface IReceiptsApiClient
{
    Task<int> CreateReceiptWithItemsAsync(
        CreateReceiptWithItemsRequest request,
        CancellationToken ct);

    Task CreatePaperlessDocumentMappingAsync(
        CreatePaperlessDocumentMappingRequest request,
        CancellationToken ct);
}
```

**New API Endpoint**

Add to `ReceiptsController` or create new `PaperlessDocumentMappingsController`:

```csharp
// POST /api/paperless-mappings
[HttpPost]
public async Task<IActionResult> CreateMapping(
    [FromBody] CreatePaperlessDocumentMappingCommand command)
{
    var result = await _mediator.Send(command);
    return Ok(result);
}
```

### 4. Job Scheduling

**Option A: Hangfire (Recommended)**

Advantages:
- Persistent job storage in PostgreSQL
- Web dashboard for monitoring
- Recurring jobs with cron expressions
- Retry logic built-in
- Scalable (multiple workers)

```csharp
// Program.cs
builder.Services.AddHangfire(config =>
{
    config.UsePostgreSqlStorage(connectionString);
});
builder.Services.AddHangfireServer();

// Register recurring job
RecurringJob.AddOrUpdate<PaperlessIntegrationWorker>(
    "process-paperless-receipts",
    worker => worker.ProcessReceiptsAsync(CancellationToken.None),
    "*/15 * * * *"); // Every 15 minutes
```

**Option B: Native BackgroundService with Timer**

Simpler but less robust:
```csharp
public class PaperlessWorker : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(15));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await ProcessReceiptsAsync(stoppingToken);
        }
    }
}
```

**Option C: Docker Cron**

Scheduled Docker container execution via cron job on host or orchestrator.

**Recommendation**: Use **Hangfire** for production robustness and monitoring capabilities.

### 5. Worker Implementation

**Core Processing Logic**

```csharp
public class PaperlessIntegrationWorker
{
    private readonly IPaperlessService _paperlessService;
    private readonly IReceiptExtractionService _extractionService;
    private readonly IReceiptsApiClient _receiptsApi;
    private readonly ILogger<PaperlessIntegrationWorker> _logger;

    public async Task ProcessReceiptsAsync(CancellationToken ct)
    {
        _logger.LogInformation("Starting Paperless receipt processing");

        // 1. Get unprocessed documents
        var documents = await _paperlessService.GetUnprocessedReceiptsAsync(ct);
        _logger.LogInformation("Found {Count} documents to process", documents.Count());

        foreach (var doc in documents)
        {
            try
            {
                // 2. Download document
                var imageData = await _paperlessService.DownloadDocumentAsync(doc.Id, ct);

                // 3. Extract receipt data
                var extractedData = await _extractionService.ExtractReceiptDataAsync(
                    imageData,
                    doc.MimeType,
                    ct);

                // 4. Create receipt in API
                var receiptId = await _receiptsApi.CreateReceiptWithItemsAsync(
                    new CreateReceiptWithItemsRequest
                    {
                        Receipt = new CreateReceiptRequest
                        {
                            Description = extractedData.Description,
                            Location = extractedData.Location,
                            Date = extractedData.Date,
                            TaxAmount = extractedData.TaxAmount
                        },
                        Items = extractedData.Items.Select(i => new CreateReceiptItemRequest
                        {
                            Description = i.Description,
                            Amount = i.Amount
                        }).ToList()
                    },
                    ct);

                // 5. Create mapping record
                await _receiptsApi.CreatePaperlessDocumentMappingAsync(
                    new CreatePaperlessDocumentMappingRequest
                    {
                        ReceiptId = receiptId,
                        PaperlessDocumentId = doc.Id,
                        PaperlessDocumentTitle = doc.Title,
                        ProcessingStatus = extractedData.Confidence == "high"
                            ? "Success"
                            : "Partial",
                        ErrorMessage = extractedData.Confidence == "low"
                            ? $"Low confidence: {extractedData.Notes}"
                            : null
                    },
                    ct);

                // 6. Update Paperless tags
                await _paperlessService.UpdateDocumentTagsAsync(
                    doc.Id,
                    tagsToAdd: new[] { "processed", "receipts-imported" },
                    tagsToRemove: new[] { "inbox" },
                    ct);

                _logger.LogInformation(
                    "Successfully processed document {DocId} -> Receipt {ReceiptId}",
                    doc.Id,
                    receiptId);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to process document {DocId}: {Error}",
                    doc.Id,
                    ex.Message);

                // Optionally: Add "failed" tag to document in Paperless
                await _paperlessService.UpdateDocumentTagsAsync(
                    doc.Id,
                    tagsToAdd: new[] { "import-failed" },
                    tagsToRemove: new[] { "inbox" },
                    ct);
            }
        }

        _logger.LogInformation("Completed Paperless receipt processing");
    }
}
```

### 6. Configuration

**appsettings.json**

```json
{
  "Paperless": {
    "BaseUrl": "https://paperless.yourdomain.com",
    "ApiToken": "", // Set via environment variable
    "InboxTag": "inbox",
    "ReceiptTag": "receipt",
    "ProcessedTag": "processed",
    "FailedTag": "import-failed"
  },
  "Claude": {
    "ApiKey": "", // Set via environment variable
    "Model": "claude-sonnet-4-5-20250929",
    "MaxTokens": 2048,
    "Temperature": 0.0
  },
  "ReceiptsApi": {
    "BaseUrl": "http://localhost:5000",
    "ApiKey": "" // Optional, if API auth is added
  },
  "Scheduling": {
    "IntervalMinutes": 15,
    "EnableScheduling": true
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "PaperlessIntegration": "Debug"
    }
  }
}
```

**Environment Variables (Docker)**

```bash
PAPERLESS__APITOKEN=<paperless-token>
CLAUDE__APIKEY=<anthropic-api-key>
RECEIPTSAPI__BASEURL=http://receipts-api:5000
ConnectionStrings__DefaultConnection=<postgres-connection>
```

### 7. Error Handling & Resilience

**Retry Policies (Polly)**

```csharp
// Transient HTTP errors
services.AddHttpClient<IPaperlessService, PaperlessService>()
    .AddTransientHttpErrorPolicy(builder =>
        builder.WaitAndRetryAsync(3, retryAttempt =>
            TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))));

// Claude API rate limiting
services.AddHttpClient<IReceiptExtractionService, ClaudeReceiptExtractionService>()
    .AddTransientHttpErrorPolicy(builder =>
        builder.WaitAndRetryAsync(
            3,
            retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
            (outcome, timespan, retryCount, context) =>
            {
                logger.LogWarning(
                    "Claude API retry {RetryCount} after {Delay}s",
                    retryCount,
                    timespan.TotalSeconds);
            }));
```

**Dead Letter Queue**

For documents that fail repeatedly:
- Mark with `import-failed` tag in Paperless
- Log detailed error information
- Consider manual review queue/dashboard

**Health Checks**

```csharp
services.AddHealthChecks()
    .AddCheck<PaperlessHealthCheck>("paperless")
    .AddCheck<ClaudeHealthCheck>("claude")
    .AddCheck<ReceiptsApiHealthCheck>("receipts-api");
```

### 8. Testing Strategy

**Unit Tests**
- `PaperlessService` - Mock HTTP responses
- `ClaudeReceiptExtractionService` - Test prompt engineering and JSON parsing
- `PaperlessIntegrationWorker` - Mock all dependencies, test orchestration logic

**Integration Tests**
- End-to-end test with test Paperless instance
- Mock Claude API with sample responses
- Verify database records created correctly

**Test Data**
- Sample receipt images (clear and poor quality)
- Sample Paperless API responses
- Sample Claude extraction outputs

### 9. Monitoring & Observability

**Metrics to Track**
- Documents processed per run
- Success/failure rates
- Average processing time per document
- Claude API token usage and cost
- Extraction confidence distribution

**Logging**
- Structured logging with Serilog
- Include correlation IDs for tracing document through pipeline
- Log levels:
  - `Information`: Processing start/complete, document counts
  - `Warning`: Retries, low confidence extractions
  - `Error`: Failures with full exception details

**Hangfire Dashboard**
- Monitor job execution history
- View failed jobs and retry
- Track processing duration trends

## Database Migration

### New Table

```sql
CREATE TABLE paperless_document_mappings (
    id SERIAL PRIMARY KEY,
    receipt_id INTEGER NOT NULL REFERENCES receipts(id) ON DELETE CASCADE,
    paperless_document_id INTEGER NOT NULL,
    paperless_document_title VARCHAR(500),
    processed_at TIMESTAMP NOT NULL DEFAULT NOW(),
    processing_status VARCHAR(50) NOT NULL,
    error_message TEXT,
    CONSTRAINT uq_paperless_document UNIQUE (paperless_document_id)
);

CREATE INDEX idx_paperless_mappings_receipt_id
    ON paperless_document_mappings(receipt_id);
CREATE INDEX idx_paperless_mappings_document_id
    ON paperless_document_mappings(paperless_document_id);
```

### EF Core Migration

```bash
cd src/Infrastructure
dotnet ef migrations add AddPaperlessDocumentMapping \
    --startup-project ../Presentation/API/API.csproj \
    --output-dir Migrations
```

## Deployment

### Docker Compose Setup

```yaml
services:
  receipts-api:
    build: ./src/Presentation/API
    ports:
      - "5000:8080"
    environment:
      - ConnectionStrings__DefaultConnection=${DB_CONNECTION}
    depends_on:
      - postgres

  paperless-integration:
    build: ./src/Services/PaperlessIntegration
    environment:
      - ConnectionStrings__DefaultConnection=${DB_CONNECTION}
      - Paperless__BaseUrl=${PAPERLESS_URL}
      - Paperless__ApiToken=${PAPERLESS_TOKEN}
      - Claude__ApiKey=${ANTHROPIC_API_KEY}
      - ReceiptsApi__BaseUrl=http://receipts-api:8080
    depends_on:
      - receipts-api
      - postgres

  postgres:
    image: postgres:16
    environment:
      POSTGRES_DB: receipts
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: postgres
    volumes:
      - postgres-data:/var/lib/postgresql/data

volumes:
  postgres-data:
```

### Dockerfile

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/Services/PaperlessIntegration/PaperlessIntegration.csproj", "Services/PaperlessIntegration/"]
COPY ["src/Domain/Domain.csproj", "Domain/"]
COPY ["src/Application/Application.csproj", "Application/"]
COPY ["Directory.Packages.props", "."]

RUN dotnet restore "Services/PaperlessIntegration/PaperlessIntegration.csproj"

COPY src/ .
RUN dotnet build "Services/PaperlessIntegration/PaperlessIntegration.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Services/PaperlessIntegration/PaperlessIntegration.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PaperlessIntegration.dll"]
```

## Security Considerations

### Paperless User Setup

1. Create dedicated API user in Paperless
2. Assign minimal permissions:
   - Read documents
   - Read tags
   - Update tags (only for marking processed)
3. Generate API token
4. Rotate token periodically (every 90 days)

### Secrets Management

- Use Docker secrets or environment variables
- **Never commit** API tokens or keys to repository
- Use Azure Key Vault / AWS Secrets Manager in production
- Encrypt sensitive data at rest in database

### API Security

**Option 1: API Key Authentication**
- Add API key middleware to Receipts API
- Generate dedicated key for PaperlessIntegration service
- Store in configuration

**Option 2: Mutual TLS**
- For production deployments
- Certificate-based authentication between services

**Option 3: Internal Network Only**
- If both services in same Docker network, no external access needed
- Use internal DNS resolution

## Implementation Phases

### Phase 1: Core Infrastructure (Week 1)
- [ ] Create PaperlessIntegration project structure
- [ ] Add Domain entity `PaperlessDocumentMapping`
- [ ] Create EF Core migration
- [ ] Implement `PaperlessService` with basic API integration
- [ ] Add unit tests for PaperlessService

### Phase 2: AI Integration (Week 1-2)
- [ ] Implement `ClaudeReceiptExtractionService`
- [ ] Design and test extraction prompt
- [ ] Add structured JSON validation
- [ ] Test with sample receipt images
- [ ] Add unit tests with mock Claude responses

### Phase 3: API Integration (Week 2)
- [ ] Implement `ReceiptsApiClient`
- [ ] Create `PaperlessDocumentMappingsController` in API
- [ ] Add CQRS commands/queries for mappings
- [ ] Add unit tests

### Phase 4: Worker Implementation (Week 2-3)
- [ ] Implement `PaperlessIntegrationWorker`
- [ ] Add Hangfire scheduling
- [ ] Implement error handling and retry logic
- [ ] Add structured logging
- [ ] Integration tests

### Phase 5: Docker & Deployment (Week 3)
- [ ] Create Dockerfile
- [ ] Update docker-compose.yml
- [ ] Test Docker deployment locally
- [ ] Add health checks
- [ ] Documentation for deployment

### Phase 6: Monitoring & Polish (Week 3-4)
- [ ] Hangfire dashboard setup
- [ ] Add metrics/telemetry
- [ ] Performance testing with high volume
- [ ] Create operator documentation
- [ ] Security review

## Success Criteria

- ✅ Service successfully polls Paperless every 15 minutes
- ✅ Receipts are extracted with >80% accuracy on clear images
- ✅ Data is correctly posted to Receipts API
- ✅ Paperless documents are tagged appropriately
- ✅ Failed documents are logged and marked for manual review
- ✅ Service handles API rate limits and transient failures gracefully
- ✅ Hangfire dashboard shows job execution history
- ✅ All unit and integration tests pass
- ✅ Service runs reliably in Docker environment

## Future Enhancements

1. **Manual Review UI**
   - Web interface for reviewing low-confidence extractions
   - Ability to correct and approve

2. **Machine Learning Feedback Loop**
   - Store corrections to improve extraction over time
   - Fine-tune prompts based on common errors

3. **Multi-Document Support**
   - Handle receipts split across multiple pages
   - Merge related documents

4. **Advanced Classification**
   - Automatic category assignment for items
   - Merchant/vendor matching

5. **Webhook Integration**
   - Real-time processing instead of polling
   - Immediate notification of new receipts

6. **Duplicate Detection**
   - Prevent duplicate receipts from being imported
   - Fuzzy matching on date, location, amount

7. **Batch Processing Mode**
   - Process backlog of historical documents
   - Bulk import with progress tracking

## References

- [Paperless-ngx API Documentation](https://docs.paperless-ngx.com/api/)
- [Anthropic Claude API Documentation](https://docs.anthropic.com/)
- [Hangfire Documentation](https://docs.hangfire.io/)
- [Polly Resilience Patterns](https://github.com/App-vNext/Polly)

## Questions & Decisions

- [ ] **Scheduling interval**: 15 minutes or configurable?
- [ ] **Claude model**: Sonnet 4.5 (balance) or Haiku 4.5 (cost)?
- [ ] **Hangfire dashboard**: Expose externally or internal only?
- [ ] **Retry strategy**: How many retries before marking as failed?
- [ ] **Manual review**: Build in Phase 1 or later enhancement?
- [ ] **API authentication**: Add now or defer to security hardening phase?
