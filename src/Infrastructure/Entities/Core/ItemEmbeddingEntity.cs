using Pgvector;

namespace Infrastructure.Entities.Core;

public class ItemEmbeddingEntity
{
	public Guid Id { get; set; }
	public string EntityType { get; set; } = string.Empty;
	public Guid EntityId { get; set; }
	public string EntityText { get; set; } = string.Empty;
	public Vector Embedding { get; set; } = null!;
	public string ModelVersion { get; set; } = string.Empty;
	public DateTimeOffset CreatedAt { get; set; }
}
