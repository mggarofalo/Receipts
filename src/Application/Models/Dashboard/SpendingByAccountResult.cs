namespace Application.Models.Dashboard;

public record SpendingByAccountResult(List<SpendingAccountItemResult> Items);

public record SpendingAccountItemResult(Guid AccountId, string AccountName, decimal Amount, decimal Percentage);
