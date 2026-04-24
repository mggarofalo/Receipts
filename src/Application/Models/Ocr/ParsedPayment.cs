namespace Application.Models.Ocr;

/// <summary>
/// A single payment tender extracted from a receipt. A receipt may include multiple payments
/// (split tender, gift card + card, etc.) — prefer <see cref="ParsedReceipt.Payments"/> over
/// the legacy <see cref="ParsedReceipt.PaymentMethod"/> when you need per-payment detail.
/// </summary>
public record ParsedPayment(
	FieldConfidence<string?> Method,
	FieldConfidence<decimal?> Amount,
	FieldConfidence<string?> LastFour
);
