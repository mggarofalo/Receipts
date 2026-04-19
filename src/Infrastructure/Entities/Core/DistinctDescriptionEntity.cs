namespace Infrastructure.Entities.Core;

public class DistinctDescriptionEntity
{
	public string Description { get; set; } = string.Empty;
	public DateTimeOffset? ProcessedAt { get; set; }
}
