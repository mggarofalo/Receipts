namespace Application.Interfaces.Services;

/// <summary>
/// Metadata extracted from a PDF document.
/// </summary>
public record PdfMetadata(string? Title, DateOnly? CreationDate);

/// <summary>
/// Result of converting a PDF document for OCR processing.
/// </summary>
/// <param name="PageImages">
/// The rasterized first page of the PDF as a PNG byte array. Always contains exactly one
/// entry when <see cref="IPdfConversionService.ConvertAsync"/> returns successfully —
/// the first page is rendered regardless of whether the PDF contains embedded raster
/// images, vector graphics, or only a text layer. The scan command handler consumes
/// only this first image.
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
	/// Rasterizes the first page of the PDF to a PNG and optionally extracts the text
	/// layer. The PNG is always produced (even for vector-only or text-only PDFs) so
	/// that downstream VLM/OCR processing always has a usable image.
	/// </summary>
	Task<PdfConversionResult> ConvertAsync(byte[] pdfBytes, CancellationToken ct);
}
