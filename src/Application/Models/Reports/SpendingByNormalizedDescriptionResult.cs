namespace Application.Models.Reports;

public record SpendingByNormalizedDescriptionItem(
	string CanonicalName,
	decimal TotalAmount,
	string Currency,
	int ItemCount,
	DateTimeOffset? FirstSeen,
	DateTimeOffset? LastSeen);

public record SpendingByNormalizedDescriptionResult(
	List<SpendingByNormalizedDescriptionItem> Items,
	DateTimeOffset? FromDate,
	DateTimeOffset? ToDate);
