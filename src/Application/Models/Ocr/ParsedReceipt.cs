namespace Application.Models.Ocr;

public record ParsedReceipt(
	FieldConfidence<string> StoreName,
	FieldConfidence<DateOnly> Date,
	List<ParsedReceiptItem> Items,
	FieldConfidence<decimal> Subtotal,
	List<ParsedTaxLine> TaxLines,
	FieldConfidence<decimal> Total,
	FieldConfidence<string?> PaymentMethod)
{
	/// <summary>
	/// Store street address, as printed on the receipt. Extracted alongside the store name.
	/// </summary>
	public FieldConfidence<string?> StoreAddress { get; init; } = FieldConfidence<string?>.None();

	/// <summary>
	/// Store phone number, as printed on the receipt.
	/// </summary>
	public FieldConfidence<string?> StorePhone { get; init; } = FieldConfidence<string?>.None();

	/// <summary>
	/// All payment tenders applied to the receipt, in the order they appear. For split-tender
	/// receipts this preserves every payment — <see cref="PaymentMethod"/> only carries the
	/// first non-empty method string for backward compatibility with the V1 shape.
	/// </summary>
	public List<ParsedPayment> Payments { get; init; } = [];

	/// <summary>
	/// Receipt identifier / transaction number printed on the receipt (e.g. "7QKKG1XDWPD").
	/// </summary>
	public FieldConfidence<string?> ReceiptId { get; init; } = FieldConfidence<string?>.None();

	/// <summary>
	/// Store / branch number printed on the receipt (e.g. "ST# 05487").
	/// </summary>
	public FieldConfidence<string?> StoreNumber { get; init; } = FieldConfidence<string?>.None();

	/// <summary>
	/// Terminal / register / POS identifier printed on the receipt (e.g. "TE# 54 TR# 1105").
	/// </summary>
	public FieldConfidence<string?> TerminalId { get; init; } = FieldConfidence<string?>.None();
}
