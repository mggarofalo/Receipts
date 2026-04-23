using System.Text.Json.Serialization;

namespace Infrastructure.Services;

internal sealed class VlmReceiptPayload
{
	[JsonPropertyName("store")]
	public string? Store { get; set; }

	[JsonPropertyName("date")]
	public DateOnly? Date { get; set; }

	[JsonPropertyName("items")]
	public List<VlmReceiptItem>? Items { get; set; }

	[JsonPropertyName("subtotal")]
	public decimal? Subtotal { get; set; }

	[JsonPropertyName("taxLines")]
	public List<VlmTaxLine>? TaxLines { get; set; }

	[JsonPropertyName("total")]
	public decimal? Total { get; set; }

	[JsonPropertyName("paymentMethod")]
	public string? PaymentMethod { get; set; }
}

internal sealed class VlmReceiptItem
{
	[JsonPropertyName("code")]
	public string? Code { get; set; }

	[JsonPropertyName("description")]
	public string? Description { get; set; }

	[JsonPropertyName("quantity")]
	public decimal? Quantity { get; set; }

	[JsonPropertyName("unitPrice")]
	public decimal? UnitPrice { get; set; }

	[JsonPropertyName("totalPrice")]
	public decimal? TotalPrice { get; set; }
}

internal sealed class VlmTaxLine
{
	[JsonPropertyName("label")]
	public string? Label { get; set; }

	[JsonPropertyName("amount")]
	public decimal? Amount { get; set; }
}
