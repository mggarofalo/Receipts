using Infrastructure.Interfaces;

namespace Infrastructure.Entities.Core;

public class CategoryEntity : ISoftDeletable
{
	public Guid Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public string? Description { get; set; }
	public DateTimeOffset? DeletedAt { get; set; }
	public string? DeletedByUserId { get; set; }
	public Guid? DeletedByApiKeyId { get; set; }
}
