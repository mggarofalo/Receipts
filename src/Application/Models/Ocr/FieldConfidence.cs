namespace Application.Models.Ocr;

public record FieldConfidence<T>(T? Value, ConfidenceLevel Confidence)
{
	public static FieldConfidence<T> High(T value) => new(value, ConfidenceLevel.High);
	public static FieldConfidence<T> Medium(T value) => new(value, ConfidenceLevel.Medium);
	public static FieldConfidence<T> Low(T value) => new(value, ConfidenceLevel.Low);
	public static FieldConfidence<T> None() => new(default, ConfidenceLevel.Low);
}
