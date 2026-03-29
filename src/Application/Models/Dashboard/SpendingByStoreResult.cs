namespace Application.Models.Dashboard;

public record SpendingByStoreResult(List<SpendingByStoreItemResult> Items);

public record SpendingByStoreItemResult(string Location, int VisitCount, decimal TotalAmount, decimal AveragePerVisit);
