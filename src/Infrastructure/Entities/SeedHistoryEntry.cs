namespace Infrastructure.Entities;

public class SeedHistoryEntry
{
	public string SeedId { get; set; } = string.Empty;
	public DateTimeOffset AppliedAt { get; set; }
}
