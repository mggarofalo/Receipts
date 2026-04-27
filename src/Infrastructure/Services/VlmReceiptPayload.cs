using System.Text.Json.Serialization;

namespace Infrastructure.Services;

internal sealed class VlmReceiptPayload
{
	/// <summary>
	/// The schema version the prompt was authored against. The host validates this against
	/// <see cref="CurrentSchemaVersion"/> after deserialization and throws on mismatch — this
	/// prevents an old VLM model that emits a stale shape from passing through silently after
	/// a future schema bump. See RECEIPTS-639.
	/// </summary>
	public const int CurrentSchemaVersion = 1;

	[JsonPropertyName("schema_version")]
	public int? SchemaVersion { get; set; }

	[JsonPropertyName("store")]
	public VlmStore? Store { get; set; }

	[JsonPropertyName("datetime")]
	public string? Datetime { get; set; }

	[JsonPropertyName("items")]
	public List<VlmReceiptItem>? Items { get; set; }

	[JsonPropertyName("subtotal")]
	public decimal? Subtotal { get; set; }

	[JsonPropertyName("taxLines")]
	public List<VlmTaxLine>? TaxLines { get; set; }

	[JsonPropertyName("total")]
	public decimal? Total { get; set; }

	[JsonPropertyName("payments")]
	public List<VlmPayment>? Payments { get; set; }

	[JsonPropertyName("receiptId")]
	public string? ReceiptId { get; set; }

	[JsonPropertyName("storeNumber")]
	public string? StoreNumber { get; set; }

	[JsonPropertyName("terminalId")]
	public string? TerminalId { get; set; }
}

internal sealed class VlmStore
{
	[JsonPropertyName("name")]
	public string? Name { get; set; }

	[JsonPropertyName("address")]
	public string? Address { get; set; }

	[JsonPropertyName("phone")]
	public string? Phone { get; set; }
}

internal sealed class VlmReceiptItem
{
	[JsonPropertyName("description")]
	public string? Description { get; set; }

	[JsonPropertyName("code")]
	public string? Code { get; set; }

	[JsonPropertyName("lineTotal")]
	public decimal? LineTotal { get; set; }

	[JsonPropertyName("quantity")]
	public decimal? Quantity { get; set; }

	[JsonPropertyName("unitPrice")]
	public decimal? UnitPrice { get; set; }

	[JsonPropertyName("taxCode")]
	public string? TaxCode { get; set; }
}

internal sealed class VlmTaxLine
{
	[JsonPropertyName("label")]
	public string? Label { get; set; }

	[JsonPropertyName("amount")]
	public decimal? Amount { get; set; }
}

internal sealed class VlmPayment
{
	[JsonPropertyName("method")]
	public string? Method { get; set; }

	[JsonPropertyName("amount")]
	public decimal? Amount { get; set; }

	[JsonPropertyName("lastFour")]
	public string? LastFour { get; set; }
}
