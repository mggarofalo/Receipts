using Application.Models.Ocr;

namespace Application.Interfaces.Services;

public interface IReceiptExtractionService
{
	/// <summary>
	/// Extracts a structured <see cref="ParsedReceipt"/> from raw image bytes. The bytes are
	/// transmitted to the VLM verbatim — no MIME type is propagated downstream because Ollama's
	/// <c>/api/generate</c> accepts a base64 image with no per-image format hint (the model
	/// auto-detects PNG/JPEG/etc. from the bytes themselves). See RECEIPTS-640.
	/// </summary>
	Task<ParsedReceipt> ExtractAsync(byte[] imageBytes, CancellationToken cancellationToken);
}
