using Application.Interfaces.Services;
using MediatR;

namespace Application.Commands.Receipt.UploadImage;

public class UploadReceiptImageCommandHandler(
	IReceiptService receiptService,
	IImageStorageService imageStorageService,
	IImageProcessingService imageProcessingService) : IRequestHandler<UploadReceiptImageCommand, UploadReceiptImageResult>
{
	public async Task<UploadReceiptImageResult> Handle(UploadReceiptImageCommand request, CancellationToken cancellationToken)
	{
		bool exists = await receiptService.ExistsAsync(request.ReceiptId, cancellationToken);
		if (!exists)
		{
			throw new KeyNotFoundException($"Receipt {request.ReceiptId} not found.");
		}

		string originalPath = await imageStorageService.SaveOriginalAsync(
			request.ReceiptId, request.ImageBytes, request.FileExtension, cancellationToken);

		try
		{
			ImageProcessingResult processed = await imageProcessingService.PreprocessAsync(
				request.ImageBytes, request.ContentType, cancellationToken);

			string processedPath = await imageStorageService.SaveProcessedAsync(
				request.ReceiptId, processed.ProcessedBytes, cancellationToken);

			await receiptService.UpdateImagePathsAsync(
				request.ReceiptId, originalPath, processedPath, cancellationToken);

			return new UploadReceiptImageResult(originalPath, processedPath);
		}
		catch
		{
			await imageStorageService.DeleteReceiptImagesAsync(request.ReceiptId, cancellationToken);
			throw;
		}
	}
}
