using Application.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Commands.Receipt.UploadImage;

public class UploadReceiptImageCommandHandler(
	IReceiptService receiptService,
	IImageStorageService imageStorageService,
	IImageValidationService imageValidationService,
	ILogger<UploadReceiptImageCommandHandler> logger) : IRequestHandler<UploadReceiptImageCommand, UploadReceiptImageResult>
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
		catch (Exception ex) when (ex is not OperationCanceledException)
		{
			// If the path-update fails (e.g. DB offline) the just-saved blob would otherwise
			// be orphaned. Cleanup is best-effort:
			//
			// 1) Pass CancellationToken.None — if the caller's token was already canceled, we
			//    still want the orphan removed. Without this the cleanup itself silently aborts
			//    and the operator never sees the orphan.
			// 2) Catch-and-log any cleanup failure so the original exception (the actual
			//    root cause the operator needs to see) is the one that propagates. Without this
			//    a cleanup throw replaces the originating exception and we lose the root cause.
			// 3) Skip the destructive delete entirely on cancellation — a canceled upload should
			//    leave the saved blob in place rather than racing the user's cancel against the
			//    disk so a retry can re-attach it. The exception filter on the catch achieves this.
			//
			// See RECEIPTS-640.
			try
			{
				await imageStorageService.DeleteReceiptImagesAsync(
					request.ReceiptId, CancellationToken.None);
			}
			catch (Exception cleanupEx)
			{
				logger.LogError(
					cleanupEx,
					"Failed to clean up orphaned blob for receipt {ReceiptId} after path-update failure; original error: {OriginalError}",
					request.ReceiptId, ex.Message);
			}

			throw;
		}
	}
}
