using System.Text.Json.Serialization;

namespace Infrastructure.Ynab.Models;

/// <summary>
/// Response envelope for GET /v1/budgets/{budget_id}/transactions/{transaction_id}.
/// </summary>
internal sealed class YnabTransactionResponseEnvelope
{
	[JsonPropertyName("data")]
	public YnabTransactionResponseData Data { get; set; } = null!;
}

internal sealed class YnabTransactionResponseData
{
	[JsonPropertyName("transaction")]
	public YnabTransactionDto Transaction { get; set; } = null!;
}

internal sealed class YnabTransactionDto
{
	[JsonPropertyName("id")]
	public string Id { get; set; } = string.Empty;

	[JsonPropertyName("date")]
	public string Date { get; set; } = string.Empty;

	[JsonPropertyName("amount")]
	public long Amount { get; set; }

	[JsonPropertyName("memo")]
	public string? Memo { get; set; }

	[JsonPropertyName("cleared")]
	public string ClearedStatus { get; set; } = string.Empty;

	[JsonPropertyName("approved")]
	public bool Approved { get; set; }

	[JsonPropertyName("account_id")]
	public string AccountId { get; set; } = string.Empty;

	[JsonPropertyName("category_id")]
	public string? CategoryId { get; set; }

	[JsonPropertyName("payee_name")]
	public string? PayeeName { get; set; }

	[JsonPropertyName("deleted")]
	public bool Deleted { get; set; }
}
