using Application.Exceptions;
using Application.Interfaces.Services;
using Application.Models.Ocr;
using MediatR;

namespace Application.Commands.Receipt.Scan;

public class ScanReceiptCommandHandler(
	IReceiptExtractionService extractionService,
	IPdfConversionService pdfConversionService) : IRequestHandler<ScanReceiptCommand, ScanReceiptResult>
{
	internal const string PdfContentType = "application/pdf";
	internal const string PdfPageImageContentType = "image/png";

	public async Task<ScanReceiptResult> Handle(ScanReceiptCommand request, CancellationToken cancellationToken)
	{
		(byte[] imageBytes, string contentType, int droppedPageCount) =
			await ResolveImageAsync(request, cancellationToken);

		ParsedReceipt parsed = await extractionService.ExtractAsync(imageBytes, contentType, cancellationToken);

		if (IsEmpty(parsed))
		{
			throw new OcrNoTextException("The receipt could not be extracted from the provided file.");
		}

		return new ScanReceiptResult(parsed, droppedPageCount);
	}

	private async Task<(byte[] ImageBytes, string ContentType, int DroppedPageCount)> ResolveImageAsync(
		ScanReceiptCommand request, CancellationToken cancellationToken)
	{
		if (!string.Equals(request.ContentType, PdfContentType, StringComparison.OrdinalIgnoreCase))
		{
			return (request.ImageBytes, request.ContentType, 0);
		}

		PdfConversionResult conversion = await pdfConversionService.ConvertAsync(
			request.ImageBytes, cancellationToken);

		// PdfConversionService guarantees TotalPageCount >= 1 (it rejects empty PDFs
		// with InvalidOperationException), so this subtraction is always >= 0.
		int droppedPageCount = conversion.TotalPageCount - 1;

		return (conversion.FirstPagePng, PdfPageImageContentType, droppedPageCount);
	}

	/// <summary>
	/// True when the extraction yielded no usable signal at all — every scalar field is
	/// <see cref="FieldConfidence{T}.None"/> and there are no items or tax lines. Note this
	/// must check for <see cref="ConfidenceLevel.None"/>, not <see cref="ConfidenceLevel.Low"/>:
	/// a low-confidence extracted value is still a real reading the user can review and edit,
	/// and rejecting it as "empty" would discard valid (if uncertain) data.
	/// </summary>
	private static bool IsEmpty(ParsedReceipt parsed)
	{
		return !parsed.StoreName.IsPresent
			&& !parsed.Date.IsPresent
			&& !parsed.Subtotal.IsPresent
			&& !parsed.Total.IsPresent
			&& !parsed.PaymentMethod.IsPresent
			&& parsed.Items.Count == 0
			&& parsed.TaxLines.Count == 0;
	}
}
