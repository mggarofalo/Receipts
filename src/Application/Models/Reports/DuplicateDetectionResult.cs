namespace Application.Models.Reports;

public record DuplicateReceiptSummary(
	Guid ReceiptId,
	string Location,
	DateOnly Date,
	decimal TransactionTotal);

public record DuplicateGroup(
	string MatchKey,
	List<DuplicateReceiptSummary> Receipts);

public record DuplicateDetectionResult(
	List<DuplicateGroup> Groups,
	int GroupCount,
	int TotalDuplicateReceipts);
