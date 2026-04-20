using Domain.NormalizedDescriptions;
using Pgvector;

namespace Infrastructure.Entities.Core;

public class NormalizedDescriptionEntity
{
	public Guid Id { get; set; }
	public string CanonicalName { get; set; } = string.Empty;
	public NormalizedDescriptionStatus Status { get; set; }
	public Vector? Embedding { get; set; }
	public string? EmbeddingModelVersion { get; set; }
	public DateTimeOffset CreatedAt { get; set; }
}
