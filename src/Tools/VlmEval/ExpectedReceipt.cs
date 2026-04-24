using System.Text.Json.Serialization;

namespace VlmEval;

public sealed class ExpectedReceipt
{
	[JsonPropertyName("store")]
	public string? Store { get; set; }

	[JsonPropertyName("date")]
	public DateOnly? Date { get; set; }

	[JsonPropertyName("subtotal")]
	public decimal? Subtotal { get; set; }

	[JsonPropertyName("total")]
	public decimal? Total { get; set; }

	[JsonPropertyName("taxLines")]
	public List<ExpectedTaxLine>? TaxLines { get; set; }

	[JsonPropertyName("paymentMethod")]
	public string? PaymentMethod { get; set; }

	[JsonPropertyName("minItemCount")]
	public int? MinItemCount { get; set; }

	[JsonPropertyName("items")]
	public List<ExpectedItem>? Items { get; set; }

	[JsonPropertyName("notes")]
	public string? Notes { get; set; }
}

public sealed class ExpectedTaxLine
{
	[JsonPropertyName("label")]
	public string? Label { get; set; }

	[JsonPropertyName("amount")]
	public decimal? Amount { get; set; }
}

public sealed class ExpectedItem
{
	[JsonPropertyName("description")]
	public string? Description { get; set; }

	[JsonPropertyName("totalPrice")]
	public decimal? TotalPrice { get; set; }
}
