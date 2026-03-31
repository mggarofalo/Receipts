namespace Application.Models.Ocr;

public record ParsedReceiptItem(
	FieldConfidence<string?> Code,
	FieldConfidence<string> Description,
	FieldConfidence<decimal> Quantity,
	FieldConfidence<decimal> UnitPrice,
	FieldConfidence<decimal> TotalPrice
);
