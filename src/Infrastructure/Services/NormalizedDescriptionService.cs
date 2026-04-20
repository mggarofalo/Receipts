using Application.Interfaces.Services;
using Domain.NormalizedDescriptions;
using Infrastructure.Entities.Core;
using Infrastructure.Mapping;
using Microsoft.EntityFrameworkCore;
using Pgvector;

namespace Infrastructure.Services;

public class NormalizedDescriptionService(
	IDbContextFactory<ApplicationDbContext> contextFactory,
	IEmbeddingService embeddingService,
	NormalizedDescriptionMapper mapper) : INormalizedDescriptionService
{
	// Threshold constants are hardcoded for RECEIPTS-577. A later issue (RECEIPTS-580) moves
	// them into a DB-backed settings row so they can be tuned without a redeploy. The values
	// below match the data-driven calibration baseline.
	public const double AutoAcceptThreshold = 0.81;
	public const double PendingReviewThreshold = 0.68;

	public const string ReceiptItemNotFound = "Receipt item not found.";

	private const string PostgreSQL = "Npgsql.EntityFrameworkCore.PostgreSQL";

	public async Task<NormalizedDescription> GetOrCreateAsync(string rawDescription, CancellationToken cancellationToken)
	{
		string normalized = (rawDescription ?? string.Empty).Trim();
		if (string.IsNullOrEmpty(normalized))
		{
			throw new ArgumentException(NormalizedDescription.CanonicalNameCannotBeEmpty, nameof(rawDescription));
		}

		using ApplicationDbContext context = contextFactory.CreateDbContext();

		// Step 1: exact case-insensitive match on existing canonical name.
		NormalizedDescriptionEntity? existing = await FindExactCaseInsensitiveAsync(context, normalized, cancellationToken);
		if (existing is not null)
		{
			return mapper.ToDomain(existing);
		}

		// Step 2: no embedding capability — create Active entry directly with no vector.
		if (!embeddingService.IsConfigured)
		{
			NormalizedDescriptionEntity created = await InsertAsync(context, normalized, NormalizedDescriptionStatus.Active, embedding: null, cancellationToken);
			return mapper.ToDomain(created);
		}

		// Step 3: generate embedding for the input.
		float[] embeddingData = await embeddingService.GenerateEmbeddingAsync(normalized, cancellationToken);
		Vector? embeddingVector = embeddingData.Length > 0 ? new Vector(embeddingData) : null;

		// Step 4: ANN top-1 search — only supported on Postgres. On other providers (InMemory tests)
		// the method is a no-op by default; tests can override AnnSearchTopOneAsync to simulate
		// specific top-1 matches and exercise each threshold band.
		double? topSimilarity = null;
		NormalizedDescriptionEntity? topMatch = null;
		if (embeddingVector is not null)
		{
			(topMatch, topSimilarity) = await AnnSearchTopOneAsync(context, embeddingVector, cancellationToken);
		}

		if (topMatch is not null && topSimilarity.HasValue)
		{
			if (topSimilarity.Value >= AutoAcceptThreshold)
			{
				return mapper.ToDomain(topMatch);
			}

			if (topSimilarity.Value >= PendingReviewThreshold)
			{
				NormalizedDescriptionEntity pending = await InsertAsync(context, normalized, NormalizedDescriptionStatus.PendingReview, embeddingVector, cancellationToken);
				return mapper.ToDomain(pending);
			}
		}

		NormalizedDescriptionEntity activeCreated = await InsertAsync(context, normalized, NormalizedDescriptionStatus.Active, embeddingVector, cancellationToken);
		return mapper.ToDomain(activeCreated);
	}

	public async Task<NormalizedDescription?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
	{
		using ApplicationDbContext context = contextFactory.CreateDbContext();
		NormalizedDescriptionEntity? entity = await context.NormalizedDescriptions
			.AsNoTracking()
			.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
		return entity is null ? null : mapper.ToDomain(entity);
	}

	public async Task<List<NormalizedDescription>> GetAllAsync(NormalizedDescriptionStatus? filter, CancellationToken cancellationToken)
	{
		using ApplicationDbContext context = contextFactory.CreateDbContext();
		IQueryable<NormalizedDescriptionEntity> query = context.NormalizedDescriptions.AsNoTracking();
		if (filter.HasValue)
		{
			query = query.Where(e => e.Status == filter.Value);
		}

		List<NormalizedDescriptionEntity> entities = await query
			.OrderBy(e => e.CanonicalName)
			.ToListAsync(cancellationToken);
		return [.. entities.Select(mapper.ToDomain)];
	}

	public async Task<int> MergeAsync(Guid keepId, Guid discardId, CancellationToken cancellationToken)
	{
		if (keepId == discardId)
		{
			return 0;
		}

		using ApplicationDbContext context = contextFactory.CreateDbContext();

		NormalizedDescriptionEntity? keep = await context.NormalizedDescriptions
			.FirstOrDefaultAsync(e => e.Id == keepId, cancellationToken);
		NormalizedDescriptionEntity? discard = await context.NormalizedDescriptions
			.FirstOrDefaultAsync(e => e.Id == discardId, cancellationToken);

		if (keep is null || discard is null)
		{
			return 0;
		}

		// Re-link every ReceiptItem currently pointing at discard to point at keep.
		List<ReceiptItemEntity> items = await context.ReceiptItems
			.IgnoreAutoIncludes()
			.IgnoreQueryFilters()
			.Where(r => r.NormalizedDescriptionId == discardId)
			.ToListAsync(cancellationToken);

		foreach (ReceiptItemEntity item in items)
		{
			item.NormalizedDescriptionId = keepId;
		}

		context.NormalizedDescriptions.Remove(discard);

		await context.SaveChangesAsync(cancellationToken);
		return items.Count;
	}

	public async Task<NormalizedDescription> SplitAsync(Guid receiptItemId, CancellationToken cancellationToken)
	{
		using ApplicationDbContext context = contextFactory.CreateDbContext();

		ReceiptItemEntity? item = await context.ReceiptItems
			.IgnoreAutoIncludes()
			.FirstOrDefaultAsync(r => r.Id == receiptItemId, cancellationToken);
		if (item is null)
		{
			throw new KeyNotFoundException(ReceiptItemNotFound);
		}

		// Generate an embedding for the split item's raw description if possible, so the
		// new entry is consistent with entries produced by GetOrCreateAsync.
		Vector? embeddingVector = null;
		if (embeddingService.IsConfigured)
		{
			float[] data = await embeddingService.GenerateEmbeddingAsync(item.Description, cancellationToken);
			if (data.Length > 0)
			{
				embeddingVector = new Vector(data);
			}
		}

		NormalizedDescriptionEntity created = await InsertAsync(
			context,
			item.Description,
			NormalizedDescriptionStatus.Active,
			embeddingVector,
			cancellationToken);

		item.NormalizedDescriptionId = created.Id;
		await context.SaveChangesAsync(cancellationToken);

		return mapper.ToDomain(created);
	}

	public async Task<bool> UpdateStatusAsync(Guid id, NormalizedDescriptionStatus status, CancellationToken cancellationToken)
	{
		using ApplicationDbContext context = contextFactory.CreateDbContext();
		NormalizedDescriptionEntity? entity = await context.NormalizedDescriptions
			.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
		if (entity is null || entity.Status == status)
		{
			return false;
		}

		entity.Status = status;
		await context.SaveChangesAsync(cancellationToken);
		return true;
	}

	private static async Task<NormalizedDescriptionEntity?> FindExactCaseInsensitiveAsync(
		ApplicationDbContext context, string canonicalName, CancellationToken cancellationToken)
	{
		// ToLower() translates to LOWER() on both PostgreSQL and InMemory. Paired with the
		// unique functional index on lower("CanonicalName") in the migration, this avoids
		// a sequential scan on Postgres while still working under InMemory for tests.
		string lowered = canonicalName.ToLowerInvariant();
		return await context.NormalizedDescriptions
			.FirstOrDefaultAsync(
				e => e.CanonicalName.ToLower() == lowered,
				cancellationToken);
	}

	private async Task<NormalizedDescriptionEntity> InsertAsync(
		ApplicationDbContext context,
		string canonicalName,
		NormalizedDescriptionStatus status,
		Vector? embedding,
		CancellationToken cancellationToken)
	{
		// Double-check for a race: between the caller's exact-match lookup and this insert,
		// another request may have created a row with the same canonical name. The DB has a
		// unique functional index on lower(CanonicalName), so a second lookup inside this
		// save path gives us a race-safe compromise without needing a distributed lock.
		NormalizedDescriptionEntity? preInsert = await FindExactCaseInsensitiveAsync(context, canonicalName, cancellationToken);
		if (preInsert is not null)
		{
			return preInsert;
		}

		NormalizedDescriptionEntity entity = new()
		{
			Id = Guid.NewGuid(),
			CanonicalName = canonicalName,
			Status = status,
			Embedding = embedding,
			EmbeddingModelVersion = embedding is null ? null : OnnxEmbeddingService.ModelName,
			CreatedAt = DateTimeOffset.UtcNow,
		};

		context.NormalizedDescriptions.Add(entity);
		try
		{
			await context.SaveChangesAsync(cancellationToken);
		}
		catch (DbUpdateException)
		{
			// Another writer raced us to the unique functional index on lower(CanonicalName).
			// Detach our losing insert, reload the winner, and return it.
			context.Entry(entity).State = EntityState.Detached;
			NormalizedDescriptionEntity? winner = await FindExactCaseInsensitiveAsync(context, canonicalName, cancellationToken);
			if (winner is null)
			{
				throw;
			}

			return winner;
		}

		return entity;
	}

	// Virtual so tests can simulate specific top-1 matches without a real Postgres. On providers
	// that don't support pgvector (e.g., InMemory) the default implementation is a no-op.
	protected virtual async Task<(NormalizedDescriptionEntity? Match, double? Similarity)> AnnSearchTopOneAsync(
		ApplicationDbContext context, Vector queryVector, CancellationToken cancellationToken)
	{
		if (context.Database.ProviderName != PostgreSQL)
		{
			return (null, null);
		}

		// pgvector's `<=>` operator returns cosine distance (1 - cosine_similarity).
		// The partial HNSW index covers the WHERE "Embedding" IS NOT NULL clause.
		string sql = """
			SELECT "Id" AS entity_id,
			       (1.0 - ("Embedding" <=> {0}::vector)) AS similarity
			FROM "NormalizedDescriptions"
			WHERE "Embedding" IS NOT NULL
			ORDER BY "Embedding" <=> {0}::vector
			LIMIT 1
			""";

		AnnSearchRow? row = await context.Database
			.SqlQueryRaw<AnnSearchRow>(sql, queryVector)
			.FirstOrDefaultAsync(cancellationToken);

		if (row is null)
		{
			return (null, null);
		}

		NormalizedDescriptionEntity? entity = await context.NormalizedDescriptions
			.FirstOrDefaultAsync(e => e.Id == row.entity_id, cancellationToken);
		return (entity, row.similarity);
	}

	private sealed class AnnSearchRow
	{
#pragma warning disable IDE1006 // Underscore naming matches raw-SQL column aliases.
		public Guid entity_id { get; set; }
		public double similarity { get; set; }
#pragma warning restore IDE1006
	}
}
