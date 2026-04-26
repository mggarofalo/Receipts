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
	/// always has a usable image.
	/// </summary>
	/// <returns>
	/// A list containing exactly one entry — the rasterized first page as a PNG byte
	/// array — when the conversion succeeds. The scan command handler consumes only
	/// the first image; multi-page PDFs are still validated against
	/// <see cref="MaxPages"/> but only the first page is rendered.
	/// </returns>
	Task<IReadOnlyList<byte[]>> ConvertAsync(byte[] pdfBytes, CancellationToken ct);
}
