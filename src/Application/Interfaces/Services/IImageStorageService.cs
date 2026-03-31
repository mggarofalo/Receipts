namespace Application.Interfaces.Services;

public interface IImageStorageService
{
	Task<string> SaveOriginalAsync(Guid receiptId, byte[] imageBytes, string extension, CancellationToken ct);
	Task<string> SaveProcessedAsync(Guid receiptId, byte[] processedBytes, CancellationToken ct);
	string GetImagePath(Guid receiptId, string fileName);
	Task DeleteReceiptImagesAsync(Guid receiptId, CancellationToken ct);
}
