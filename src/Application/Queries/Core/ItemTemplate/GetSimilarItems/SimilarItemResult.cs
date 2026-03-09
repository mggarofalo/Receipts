namespace Application.Queries.Core.ItemTemplate.GetSimilarItems;

public class SimilarItemResult
{
	public required string Name { get; set; }
	public double Similarity { get; set; }
	public double? SemanticSimilarity { get; set; }
	public double CombinedScore { get; set; }
	public required string Source { get; set; }
	public string? DefaultCategory { get; set; }
	public string? DefaultSubcategory { get; set; }
	public decimal? DefaultUnitPrice { get; set; }
	public string? DefaultPricingMode { get; set; }
	public string? DefaultItemCode { get; set; }
}

public class CategoryRecommendation
{
	public required string Category { get; set; }
	public string? Subcategory { get; set; }
	public double Confidence { get; set; }
	public int OccurrenceCount { get; set; }
}
