using Application.Interfaces.Services;
using Infrastructure.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Pgvector;

namespace Infrastructure.Services;

public class EmbeddingGenerationService(
	IServiceScopeFactory scopeFactory,
	ILogger<EmbeddingGenerationService> logger) : BackgroundService
{
	private const int BatchSize = 50;
	private static readonly TimeSpan Interval = TimeSpan.FromSeconds(30);
	private static readonly TimeSpan InitialDelay = TimeSpan.FromSeconds(10);

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		try
		{
			await Task.Delay(InitialDelay, stoppingToken);
		}
		catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
		{
			return;
		}

		while (!stoppingToken.IsCancellationRequested)
		{
			try
			{
				int processed = await ProcessPendingEmbeddingsAsync(stoppingToken);
				if (processed > 0)
				{
					logger.LogInformation("Generated embeddings for {Count} items", processed);
				}
			}
			catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
			{
				break;
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Error during embedding generation cycle");
			}

			try
			{
				await Task.Delay(Interval, stoppingToken);
			}
			catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
			{
				break;
			}
		}
	}

	private async Task<int> ProcessPendingEmbeddingsAsync(CancellationToken cancellationToken)
	{
		using IServiceScope scope = scopeFactory.CreateScope();
		IEmbeddingService embeddingService = scope.ServiceProvider.GetRequiredService<IEmbeddingService>();
		IDbContextFactory<ApplicationDbContext> contextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();

		if (!embeddingService.IsConfigured)
		{
			return 0;
		}

		using ApplicationDbContext context = contextFactory.CreateDbContext();

		List<PendingItem> pending = await GetPendingItemsAsync(context, cancellationToken);
		if (pending.Count == 0)
		{
			return 0;
		}

		List<string> texts = pending.Select(p => p.Text).ToList();
		List<float[]> embeddings = await embeddingService.GenerateEmbeddingsAsync(texts, cancellationToken);

		string modelVersion = OnnxEmbeddingService.ModelName;
		DateTimeOffset now = DateTimeOffset.UtcNow;

		List<Guid> entityIds = pending.Select(p => p.EntityId).ToList();
		Dictionary<(string EntityType, Guid EntityId), ItemEmbeddingEntity> existingMap = await context.ItemEmbeddings
			.Where(e => entityIds.Contains(e.EntityId))
			.ToDictionaryAsync(e => (e.EntityType, e.EntityId), cancellationToken);

		for (int i = 0; i < pending.Count; i++)
		{
			PendingItem item = pending[i];

			if (existingMap.TryGetValue((item.EntityType, item.EntityId), out ItemEmbeddingEntity? existing))
			{
				existing.EntityText = item.Text;
				existing.Embedding = new Vector(embeddings[i]);
				existing.ModelVersion = modelVersion;
				existing.CreatedAt = now;
			}
			else
			{
				context.ItemEmbeddings.Add(new ItemEmbeddingEntity
				{
					Id = Guid.NewGuid(),
					EntityType = item.EntityType,
					EntityId = item.EntityId,
					EntityText = item.Text,
					Embedding = new Vector(embeddings[i]),
					ModelVersion = modelVersion,
					CreatedAt = now,
				});
			}
		}

		await context.SaveChangesAsync(cancellationToken);
		return pending.Count;
	}

	private static async Task<List<PendingItem>> GetPendingItemsAsync(ApplicationDbContext context, CancellationToken cancellationToken)
	{
		// Find ItemTemplates without embeddings or with stale text
		List<PendingItem> templateItems = await context.ItemTemplates
			.IgnoreQueryFilters()
			.Where(t => t.DeletedAt == null && t.Name.Length >= 2)
			.GroupJoin(
				context.ItemEmbeddings.Where(e => e.EntityType == "ItemTemplate"),
				t => t.Id,
				e => e.EntityId,
				(t, embeddings) => new { Template = t, Embeddings = embeddings })
			.SelectMany(
				x => x.Embeddings.DefaultIfEmpty(),
				(x, e) => new { x.Template, Embedding = e })
			.Where(x => x.Embedding == null || x.Embedding.EntityText != x.Template.Name)
			.OrderBy(x => x.Template.Id)
			.Select(x => new PendingItem("ItemTemplate", x.Template.Id, x.Template.Name))
			.Take(BatchSize)
			.ToListAsync(cancellationToken);

		int remaining = BatchSize - templateItems.Count;
		if (remaining <= 0)
		{
			return templateItems;
		}

		// Find ReceiptItems without embeddings or with stale text
		List<PendingItem> receiptItems = await context.ReceiptItems
			.IgnoreQueryFilters()
			.Where(r => r.DeletedAt == null && r.Description.Length >= 2)
			.GroupJoin(
				context.ItemEmbeddings.Where(e => e.EntityType == "ReceiptItem"),
				r => r.Id,
				e => e.EntityId,
				(r, embeddings) => new { ReceiptItem = r, Embeddings = embeddings })
			.SelectMany(
				x => x.Embeddings.DefaultIfEmpty(),
				(x, e) => new { x.ReceiptItem, Embedding = e })
			.Where(x => x.Embedding == null || x.Embedding.EntityText != x.ReceiptItem.Description)
			.OrderBy(x => x.ReceiptItem.Id)
			.Select(x => new PendingItem("ReceiptItem", x.ReceiptItem.Id, x.ReceiptItem.Description))
			.Take(remaining)
			.ToListAsync(cancellationToken);

		templateItems.AddRange(receiptItems);
		return templateItems;
	}

	private sealed record PendingItem(string EntityType, Guid EntityId, string Text);
}
