namespace Infrastructure.Entities.Core;

public class ItemSimilarityEdgeEntity
{
	public string DescA { get; set; } = string.Empty;
	public string DescB { get; set; } = string.Empty;
	public double Score { get; set; }
	public DateTimeOffset ComputedAt { get; set; }
}
