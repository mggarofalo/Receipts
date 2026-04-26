namespace Application.Models.Ocr;

public record FieldConfidence<T>(T? Value, ConfidenceLevel Confidence)
{
	public static FieldConfidence<T> High(T value) => new(value, ConfidenceLevel.High);
	public static FieldConfidence<T> Medium(T value) => new(value, ConfidenceLevel.Medium);
	public static FieldConfidence<T> Low(T value) => new(value, ConfidenceLevel.Low);

	/// <summary>
	/// Constructs a "field absent" sentinel: the source did not provide this field at all.
	/// Distinct from <see cref="Low(T)"/>, which carries a real (but uncertain) extracted value.
	/// </summary>
	public static FieldConfidence<T> None() => new(default, ConfidenceLevel.None);

	/// <summary>
	/// True when the source provided a value for this field (at any confidence level).
	/// False only for the <see cref="None()"/> sentinel.
	/// </summary>
	public bool IsPresent => Confidence != ConfidenceLevel.None;
}
