namespace Application.Interfaces.Services;

public interface IImageStorageService
{
	Task<string> SaveOriginalAsync(Guid receiptId, byte[] imageBytes, string extension, CancellationToken ct);
	string GetImagePath(Guid receiptId, string fileName);
	Task DeleteReceiptImagesAsync(Guid receiptId, CancellationToken ct);
}
