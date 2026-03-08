namespace Application.Models.Dashboard;

public record DashboardSummaryResult(
	int TotalReceipts,
	decimal TotalSpent,
	decimal AverageTripAmount,
	NameCountResult MostUsedAccount,
	NameCountResult MostUsedCategory);

public record NameCountResult(string? Name, int Count);
