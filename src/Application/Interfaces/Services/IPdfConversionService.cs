namespace Application.Interfaces.Services;

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
	/// Rasterizes the first page of the PDF to a PNG. The PNG is always produced
	/// (even for vector-only or text-only PDFs) so that downstream VLM/OCR processing
	/// always has a usable image. Multi-page PDFs are validated against
	/// <see cref="MaxPages"/> but only the first page is rendered. The total page
	/// count is reported back so callers can warn users about silently dropped
	/// pages (RECEIPTS-637). Failures (invalid bytes, password-protected, oversized,
	/// rasterization error) surface as <see cref="InvalidOperationException"/>.
	/// </summary>
	Task<PdfConversionResult> ConvertAsync(byte[] pdfBytes, CancellationToken ct);
}

/// <summary>
/// Result of converting a PDF to a single rasterized page image.
/// </summary>
/// <param name="FirstPagePng">PNG bytes of the rasterized first page.</param>
/// <param name="TotalPageCount">
/// Total page count of the source PDF. Always &gt;= 1 (the conversion service rejects
/// empty PDFs with <see cref="InvalidOperationException"/>). Used by the scan handler
/// to report the number of dropped pages (<c>TotalPageCount - 1</c>) when more than
/// one page was present.
/// </param>
public record PdfConversionResult(byte[] FirstPagePng, int TotalPageCount);
