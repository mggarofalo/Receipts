namespace Application.Models.Reports;

public record ItemCostBucket(
	string Period,
	decimal Amount);

public record ItemCostOverTimeResult(
	List<ItemCostBucket> Buckets);
