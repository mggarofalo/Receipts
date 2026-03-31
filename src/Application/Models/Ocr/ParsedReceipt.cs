namespace Application.Models.Ocr;

public record ParsedReceipt(
	FieldConfidence<string> StoreName,
	FieldConfidence<DateOnly> Date,
	List<ParsedReceiptItem> Items,
	FieldConfidence<decimal> Subtotal,
	List<ParsedTaxLine> TaxLines,
	FieldConfidence<decimal> Total,
	FieldConfidence<string?> PaymentMethod
);
