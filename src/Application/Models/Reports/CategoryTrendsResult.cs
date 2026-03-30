namespace Application.Models.Reports;

public record CategoryTrendsBucketResult(string Period, List<decimal> Amounts);

public record CategoryTrendsResult(List<string> Categories, List<CategoryTrendsBucketResult> Buckets);
