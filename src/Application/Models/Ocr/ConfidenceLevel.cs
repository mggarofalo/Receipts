namespace Application.Models.Ocr;

/// <summary>
/// Per-field confidence reported by the receipt extraction pipeline. Ordered from
/// "absent" through ascending information content. Placing <see cref="None"/> first
/// makes <c>default(ConfidenceLevel)</c> resolve to <see cref="None"/>, which is the
/// safest default — a freshly-constructed <c>FieldConfidence&lt;T&gt;</c> reads as
/// "no value" rather than as a spurious low-confidence reading.
/// </summary>
public enum ConfidenceLevel { None, Low, Medium, High }
