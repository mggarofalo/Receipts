namespace Infrastructure.Interfaces;

public interface ISoftDeletable
{
	DateTimeOffset? DeletedAt { get; set; }
	string? DeletedByUserId { get; set; }
	Guid? DeletedByApiKeyId { get; set; }
}
