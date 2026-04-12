namespace Application.Interfaces.Services;

/// <summary>
/// Metadata extracted from a PDF document.
/// </summary>
public record PdfMetadata(string? Title, DateOnly? CreationDate);

/// <summary>
/// Result of converting a PDF document for OCR processing.
/// </summary>
/// <param name="PageImages">
/// Images extracted from each page (for OCR when no text layer exists).
/// Empty if text was extracted directly.
/// </param>
/// <param name="ExtractedText">
/// Text extracted directly from the PDF text layer, if available.
/// Null when the PDF contains only images (scanned document).
/// </param>
/// <param name="Metadata">Optional PDF document metadata.</param>
public record PdfConversionResult(
	IReadOnlyList<byte[]> PageImages,
	string? ExtractedText,
	PdfMetadata? Metadata);

/// <summary>
/// Converts PDF documents to a format suitable for OCR processing.
/// </summary>
public interface IPdfConversionService
{
	/// <summary>
	/// Maximum number of pages allowed in a single PDF upload.
	/// </summary>
	const int MaxPages = 50;

	/// <summary>
	/// Converts a PDF to images and/or extracts text.
	/// If the PDF has a text layer, returns extracted text directly.
	/// If the PDF contains only images, extracts them for OCR processing.
	/// </summary>
	Task<PdfConversionResult> ConvertAsync(byte[] pdfBytes, CancellationToken ct);
}
