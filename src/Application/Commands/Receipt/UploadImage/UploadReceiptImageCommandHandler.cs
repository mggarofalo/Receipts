using Application.Interfaces.Services;
using MediatR;

namespace Application.Commands.Receipt.UploadImage;

public class UploadReceiptImageCommandHandler(
	IReceiptService receiptService,
	IImageStorageService imageStorageService,
	IImageValidationService imageValidationService) : IRequestHandler<UploadReceiptImageCommand, UploadReceiptImageResult>
{
	public async Task<UploadReceiptImageResult> Handle(UploadReceiptImageCommand request, CancellationToken cancellationToken)
	{
		bool exists = await receiptService.ExistsAsync(request.ReceiptId, cancellationToken);
		if (!exists)
		{
			throw new KeyNotFoundException($"Receipt {request.ReceiptId} not found.");
		}

		// Validate magic-byte format + dimensions before committing anything to disk.
		await imageValidationService.ValidateAsync(request.ImageBytes, cancellationToken);

		string originalPath = await imageStorageService.SaveOriginalAsync(
			request.ReceiptId, request.ImageBytes, request.FileExtension, cancellationToken);

		try
		{
			await receiptService.UpdateOriginalImagePathAsync(
				request.ReceiptId, originalPath, cancellationToken);

			return new UploadReceiptImageResult(originalPath);
		}
		catch
		{
			await imageStorageService.DeleteReceiptImagesAsync(request.ReceiptId, cancellationToken);
			throw;
		}
	}
}
