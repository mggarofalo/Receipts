using Application.Interfaces.Services;
using MediatR;

namespace Application.Commands.Receipt.Scan;

public class ScanReceiptCommandHandler(
	IImageProcessingService imageProcessingService,
	IOcrEngine ocrEngine,
	IReceiptParsingService receiptParsingService) : IRequestHandler<ScanReceiptCommand, ScanReceiptResult>
{
	/// <summary>
	/// Maximum OCR text length (100 KB) to prevent excessive memory usage during parsing.
	/// </summary>
	public const int MaxOcrTextLength = 100 * 1024;

	public async Task<ScanReceiptResult> Handle(ScanReceiptCommand request, CancellationToken cancellationToken)
	{
		ImageProcessingResult processed = await imageProcessingService.PreprocessAsync(
			request.ImageBytes, request.ContentType, cancellationToken);

		OcrResult ocrResult = await ocrEngine.ExtractTextAsync(
			processed.ProcessedBytes, cancellationToken);

		if (string.IsNullOrWhiteSpace(ocrResult.Text))
		{
			throw new InvalidOperationException("OCR returned no readable text from the image.");
		}

		string ocrText = ocrResult.Text.Length > MaxOcrTextLength
			? ocrResult.Text[..MaxOcrTextLength]
			: ocrResult.Text;

		Models.Ocr.ParsedReceipt parsed = receiptParsingService.Parse(ocrText);

		return new ScanReceiptResult(parsed, ocrResult.Text, ocrResult.Confidence);
	}
}
