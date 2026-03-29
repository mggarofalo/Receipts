namespace Application.Models.Reports;

public record OutOfBalanceItem(
	Guid ReceiptId,
	string Location,
	DateOnly Date,
	decimal ItemSubtotal,
	decimal TaxAmount,
	decimal AdjustmentTotal,
	decimal ExpectedTotal,
	decimal TransactionTotal,
	decimal Difference);

public record OutOfBalanceResult(
	List<OutOfBalanceItem> Items,
	int TotalCount,
	decimal TotalDiscrepancy);
