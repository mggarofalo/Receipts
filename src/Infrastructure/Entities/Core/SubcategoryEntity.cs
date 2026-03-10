using Infrastructure.Interfaces;

namespace Infrastructure.Entities.Core;

public class SubcategoryEntity : ISoftDeletable, IOwnedBy<CategoryEntity>
{
	public Guid Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public Guid CategoryId { get; set; }
	public string? Description { get; set; }
	public virtual CategoryEntity? Category { get; set; }
	public DateTimeOffset? DeletedAt { get; set; }
	public string? DeletedByUserId { get; set; }
	public Guid? DeletedByApiKeyId { get; set; }
}
