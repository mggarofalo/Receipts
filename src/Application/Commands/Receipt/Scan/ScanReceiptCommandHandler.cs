using Application.Exceptions;
using Application.Interfaces.Services;
using Application.Models.Ocr;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Commands.Receipt.Scan;

public class ScanReceiptCommandHandler(
	IReceiptExtractionService extractionService,
	IPdfConversionService pdfConversionService,
	ILogger<ScanReceiptCommandHandler> logger) : IRequestHandler<ScanReceiptCommand, ScanReceiptResult>
{
	internal const string PdfContentType = "application/pdf";
	internal const string PdfPageImageContentType = "image/png";

	public async Task<ScanReceiptResult> Handle(ScanReceiptCommand request, CancellationToken cancellationToken)
	{
		(byte[] imageBytes, string contentType) = await ResolveImageAsync(request, cancellationToken);

		ParsedReceipt parsed = await extractionService.ExtractAsync(imageBytes, contentType, cancellationToken);

		if (IsEmpty(parsed))
		{
			throw new OcrNoTextException("The receipt could not be extracted from the provided file.");
		}

		return new ScanReceiptResult(parsed);
	}

	private async Task<(byte[] ImageBytes, string ContentType)> ResolveImageAsync(
		ScanReceiptCommand request, CancellationToken cancellationToken)
	{
		if (!string.Equals(request.ContentType, PdfContentType, StringComparison.OrdinalIgnoreCase))
		{
			return (request.ImageBytes, request.ContentType);
		}

		PdfConversionResult conversion = await pdfConversionService.ConvertAsync(
			request.ImageBytes, cancellationToken);

		if (conversion.PageImages.Count == 0)
		{
			throw new OcrNoTextException(
				"The PDF document contains no extractable images for receipt scanning.");
		}

		if (conversion.PageImages.Count > 1)
		{
			logger.LogInformation(
				"PDF contains {PageCount} page images; extracting receipt from the first page only",
				conversion.PageImages.Count);
		}

		return (conversion.PageImages[0], PdfPageImageContentType);
	}

	private static bool IsEmpty(ParsedReceipt parsed)
	{
		return parsed.StoreName.Confidence == ConfidenceLevel.Low
			&& parsed.Date.Confidence == ConfidenceLevel.Low
			&& parsed.Subtotal.Confidence == ConfidenceLevel.Low
			&& parsed.Total.Confidence == ConfidenceLevel.Low
			&& parsed.PaymentMethod.Confidence == ConfidenceLevel.Low
			&& parsed.Items.Count == 0
			&& parsed.TaxLines.Count == 0;
	}
}
