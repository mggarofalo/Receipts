namespace Application.Models.Ocr;

public record ParsedTaxLine(
	FieldConfidence<string> Label,
	FieldConfidence<decimal> Amount
);
