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
	/// <see cref="MaxPages"/> but only the first page is rendered. Failures (invalid
	/// bytes, password-protected, oversized, rasterization error) surface as
	/// <see cref="InvalidOperationException"/>.
	/// </summary>
	Task<byte[]> ConvertAsync(byte[] pdfBytes, CancellationToken ct);
}
