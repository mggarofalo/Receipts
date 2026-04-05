namespace Application.Models.Ynab;

public record YnabCreateTransactionRequest(string AccountId, DateOnly Date, long Amount, string? Memo, string? PayeeName, string? CategoryId, bool Approved);
