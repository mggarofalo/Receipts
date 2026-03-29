namespace Application.Models.Reports;

public record ItemSimilarityGroup(
	string CanonicalName,
	List<string> Variants,
	List<Guid> ItemIds,
	int Occurrences,
	double MaxSimilarity);

public record ItemSimilarityResult(
	List<ItemSimilarityGroup> Groups,
	int TotalCount);
