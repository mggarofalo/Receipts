namespace Application.Exceptions;

/// <summary>
/// Thrown when OCR processing completes but returns no readable text from the image.
/// </summary>
public class OcrNoTextException : InvalidOperationException
{
	public OcrNoTextException(string message) : base(message) { }

	public OcrNoTextException(string message, Exception innerException) : base(message, innerException) { }
}
