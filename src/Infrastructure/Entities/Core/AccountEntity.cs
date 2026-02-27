using Infrastructure.Interfaces;

namespace Infrastructure.Entities.Core;

public class AccountEntity : ISoftDeletable
{
	public Guid Id { get; set; }
	public string AccountCode { get; set; } = string.Empty;
	public string Name { get; set; } = string.Empty;
	public bool IsActive { get; set; }
	public DateTimeOffset? DeletedAt { get; set; }
	public string? DeletedByUserId { get; set; }
	public Guid? DeletedByApiKeyId { get; set; }
}
