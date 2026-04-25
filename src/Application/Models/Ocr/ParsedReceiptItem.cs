namespace Application.Models.Ocr;

public record ParsedReceiptItem(
	FieldConfidence<string?> Code,
	FieldConfidence<string> Description,
	FieldConfidence<decimal> Quantity,
	FieldConfidence<decimal> UnitPrice,
	FieldConfidence<decimal> TotalPrice)
{
	/// <summary>
	/// Per-item tax code as printed on the receipt (e.g. "N" = non-taxable, "T" = taxable,
	/// "F" = food). Store-specific; we preserve the raw string without interpretation.
	/// </summary>
	public FieldConfidence<string?> TaxCode { get; init; } = FieldConfidence<string?>.None();
}
