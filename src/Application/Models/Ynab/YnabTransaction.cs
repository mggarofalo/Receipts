namespace Application.Models.Ynab;

public record YnabTransaction(
	string Id,
	DateOnly Date,
	long Amount,
	string? Memo,
	string ClearedStatus,
	bool Approved,
	string AccountId,
	string? CategoryId,
	string? PayeeName,
	string? CategoryName = null,
	List<YnabSubTransactionRead>? SubTransactions = null);

/// <summary>
/// Read-side projection of a YNAB subtransaction.
/// Named distinctly from <see cref="YnabSubTransaction"/> (the write DTO in
/// <see cref="YnabCreateTransactionRequest"/>) to avoid ambiguity.
/// </summary>
public record YnabSubTransactionRead(
	string Id,
	long Amount,
	string? Memo,
	string? CategoryId,
	string? CategoryName);
