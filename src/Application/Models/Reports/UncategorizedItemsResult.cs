namespace Application.Models.Reports;

public record UncategorizedItemRecord(
	Guid Id,
	Guid ReceiptId,
	string? ReceiptItemCode,
	string Description,
	decimal Quantity,
	decimal UnitPrice,
	decimal TotalAmount,
	string Category,
	string? Subcategory,
	string PricingMode);

public record UncategorizedItemsResult(
	List<UncategorizedItemRecord> Items,
	int TotalCount);
