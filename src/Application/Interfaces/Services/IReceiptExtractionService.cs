using Application.Models.Ocr;

namespace Application.Interfaces.Services;

public interface IReceiptExtractionService
{
	Task<ParsedReceipt> ExtractAsync(byte[] imageBytes, string contentType, CancellationToken cancellationToken);
}
