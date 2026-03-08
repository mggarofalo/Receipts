namespace Application.Models.Dashboard;

public record SpendingOverTimeResult(List<SpendingBucketResult> Buckets);

public record SpendingBucketResult(string Period, decimal Amount);
