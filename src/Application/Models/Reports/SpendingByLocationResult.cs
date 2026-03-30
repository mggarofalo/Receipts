namespace Application.Models.Reports;

public record SpendingByLocationItem(
	string Location,
	int Visits,
	decimal Total,
	decimal AveragePerVisit);

public record SpendingByLocationResult(
	List<SpendingByLocationItem> Items,
	int TotalCount,
	decimal GrandTotal);
