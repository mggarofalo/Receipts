namespace Application.Interfaces.Services;

/// <summary>
/// Metadata extracted from a PDF document.
/// </summary>
public record PdfMetadata(string? Title, DateOnly? CreationDate);

/// <summary>
/// Result of converting a PDF document for OCR processing.
/// </summary>
/// <param name="PageImages">
/// Embedded page images extracted from the PDF, independent of whether a text layer
/// exists. Small decorative images (logos, icons) are filtered out so that only
/// receipt-sized scans are returned. Empty when the PDF has no embedded bitmaps large
/// enough to be a receipt scan.
/// </param>
/// <param name="ExtractedText">
/// Text extracted directly from the PDF text layer, if available.
/// Null when the PDF contains no text layer (pure scanned document).
/// No longer consumed by the scan command path — retained for informational purposes
/// and may be removed in a future cleanup.
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
