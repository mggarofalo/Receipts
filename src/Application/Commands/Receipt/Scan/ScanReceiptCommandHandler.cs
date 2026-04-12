using Application.Exceptions;
using Application.Interfaces.Services;
using MediatR;

namespace Application.Commands.Receipt.Scan;

public class ScanReceiptCommandHandler(
	IImageProcessingService imageProcessingService,
	IOcrEngine ocrEngine,
	IReceiptParsingService receiptParsingService,
	IPdfConversionService pdfConversionService) : IRequestHandler<ScanReceiptCommand, ScanReceiptResult>
{
	/// <summary>
	/// Maximum OCR text length (100 KB) to prevent excessive memory usage during parsing.
	/// </summary>
	public const int MaxOcrTextLength = 100 * 1024;

	internal const string PdfContentType = "application/pdf";

	public async Task<ScanReceiptResult> Handle(ScanReceiptCommand request, CancellationToken cancellationToken)
	{
		if (string.Equals(request.ContentType, PdfContentType, StringComparison.OrdinalIgnoreCase))
		{
			return await HandlePdfAsync(request, cancellationToken);
		}

		return await HandleImageAsync(request, cancellationToken);
	}

	private async Task<ScanReceiptResult> HandleImageAsync(ScanReceiptCommand request, CancellationToken cancellationToken)
	{
		ImageProcessingResult processed = await imageProcessingService.PreprocessAsync(
			request.ImageBytes, request.ContentType, cancellationToken);

		OcrResult ocrResult = await ocrEngine.ExtractTextAsync(
			processed.ProcessedBytes, cancellationToken);

		return BuildResult(ocrResult.Text, ocrResult.Confidence);
	}

	private async Task<ScanReceiptResult> HandlePdfAsync(ScanReceiptCommand request, CancellationToken cancellationToken)
	{
		PdfConversionResult conversion = await pdfConversionService.ConvertAsync(
			request.ImageBytes, cancellationToken);

		// If the PDF had a text layer, use the extracted text directly
		if (!string.IsNullOrWhiteSpace(conversion.ExtractedText))
		{
			// Use a high confidence score since text was extracted directly, not via OCR
			return BuildResult(conversion.ExtractedText, 0.95f);
		}

		// Otherwise, run OCR on extracted page images
		List<string> pageTexts = [];
		float totalConfidence = 0;

		foreach (byte[] pageImage in conversion.PageImages)
		{
			cancellationToken.ThrowIfCancellationRequested();

			ImageProcessingResult processed = await imageProcessingService.PreprocessAsync(
				pageImage, "image/png", cancellationToken);

			OcrResult ocrResult = await ocrEngine.ExtractTextAsync(
				processed.ProcessedBytes, cancellationToken);

			if (!string.IsNullOrWhiteSpace(ocrResult.Text))
			{
				pageTexts.Add(ocrResult.Text);
				totalConfidence += ocrResult.Confidence;
			}
		}

		string combinedText = string.Join("\n\n", pageTexts);
		float averageConfidence = pageTexts.Count > 0 ? totalConfidence / pageTexts.Count : 0f;

		return BuildResult(combinedText, averageConfidence);
	}

	private ScanReceiptResult BuildResult(string ocrText, float confidence)
	{
		if (string.IsNullOrWhiteSpace(ocrText))
		{
			throw new OcrNoTextException("OCR returned no readable text from the image.");
		}

		string truncatedText = ocrText.Length > MaxOcrTextLength
			? ocrText[..MaxOcrTextLength]
			: ocrText;

		Models.Ocr.ParsedReceipt parsed = receiptParsingService.Parse(truncatedText);

		return new ScanReceiptResult(parsed, ocrText, confidence);
	}
}
