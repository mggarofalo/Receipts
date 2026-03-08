namespace Application.Models.Dashboard;

public record SpendingByCategoryResult(List<SpendingCategoryItemResult> Items);

public record SpendingCategoryItemResult(string CategoryName, decimal Amount, decimal Percentage);
