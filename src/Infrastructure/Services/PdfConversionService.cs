using Application.Interfaces.Services;
using Microsoft.Extensions.Logging;
using PDFtoImage;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Exceptions;

namespace Infrastructure.Services;

public class PdfConversionService(ILogger<PdfConversionService> logger) : IPdfConversionService
{
	/// <summary>
	/// Minimum text length per page to consider the page as having a usable text layer.
	/// Pages with fewer characters are treated as image-only.
	/// </summary>
	internal const int MinTextLengthPerPage = 10;

	/// <summary>
	/// DPI used when rasterizing the first PDF page for VLM/OCR consumption. 200 DPI is the
	/// baseline recommended by the rasterization issue (RECEIPTS-624) — high enough for
	/// legible receipt text, low enough to keep memory/bandwidth reasonable. Tune via
	/// fixture set if extraction quality regresses.
	/// </summary>
	internal const int RasterizationDpi = 200;

	/// <summary>
	/// Zero-based index of the PDF page to rasterize for receipt extraction. The scan
	/// command handler only consumes the first page image, so only page 0 is rendered.
	/// </summary>
	private const int RasterizePageIndex = 0;

	/// <summary>
	/// PDF magic bytes: <c>%PDF</c> (0x25 0x50 0x44 0x46).
	/// </summary>
	private static readonly byte[] PdfMagicBytes = [0x25, 0x50, 0x44, 0x46];

	public Task<PdfConversionResult> ConvertAsync(byte[] pdfBytes, CancellationToken ct)
	{
		ct.ThrowIfCancellationRequested();

		// Validate PDF magic bytes before attempting to parse
		if (pdfBytes.Length < PdfMagicBytes.Length ||
			!pdfBytes.AsSpan(0, PdfMagicBytes.Length).SequenceEqual(PdfMagicBytes))
		{
			throw new InvalidOperationException(
				"The uploaded file is not a valid PDF or is corrupted.");
		}

		PdfDocument document;
		try
		{
			document = PdfDocument.Open(pdfBytes);
		}
		catch (Exception ex) when (IsPasswordProtectedException(ex))
		{
			throw new InvalidOperationException(
				"Password-protected PDFs are not supported.", ex);
		}
		catch (Exception ex)
		{
			throw new InvalidOperationException(
				"The uploaded file is not a valid PDF or is corrupted.", ex);
		}

		int pageCount;
		PdfMetadata? metadata;
		string? extractedText;
		using (document)
		{
			try
			{
				(pageCount, metadata, extractedText) = ProcessDocumentMetadataAndText(document, ct);
			}
			catch (InvalidOperationException)
			{
				throw; // Our own validation exceptions — let them propagate
			}
			catch (OperationCanceledException)
			{
				throw; // Cancellation — let it propagate
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException(
					"An error occurred while processing the PDF document.", ex);
			}
		}

		ct.ThrowIfCancellationRequested();

		// Always rasterize the first page so the VLM always gets a usable image, even for
		// vector-only or text-only PDFs (emailed POS receipts, scanner-app exports,
		// invoicing-tool PDFs) that have no embedded raster images. This replaces the old
		// embedded-image extraction path, which failed for vector PDFs with
		// "no extractable images" and produced a 422 at the scan endpoint.
		byte[] firstPagePng = RasterizeFirstPage(pdfBytes);

		logger.LogInformation(
			"Rasterized first PDF page at {Dpi} DPI ({PngSize} bytes) for VLM extraction (document has {PageCount} page(s))",
			RasterizationDpi, firstPagePng.Length, pageCount);

		if (extractedText is not null)
		{
			logger.LogDebug(
				"Extracted text layer from PDF ({TextLength} chars); retained as informational only",
				extractedText.Length);
		}

		return Task.FromResult(new PdfConversionResult([firstPagePng], extractedText, metadata));
	}

	private (int PageCount, PdfMetadata? Metadata, string? ExtractedText) ProcessDocumentMetadataAndText(
		PdfDocument document, CancellationToken ct)
	{
		if (document.NumberOfPages == 0)
		{
			throw new InvalidOperationException("The PDF document contains no pages.");
		}

		if (document.NumberOfPages > IPdfConversionService.MaxPages)
		{
			throw new InvalidOperationException(
				$"The PDF document has {document.NumberOfPages} pages, which exceeds the maximum of {IPdfConversionService.MaxPages}.");
		}

		PdfMetadata? metadata = ExtractMetadata(document);

		// Collect text from every page for informational purposes. The scan command path
		// no longer consumes this — rasterized pixels are the source of truth — but the
		// text layer is retained for logging and potential future use.
		List<string> pageTexts = [];
		foreach (Page page in document.GetPages())
		{
			ct.ThrowIfCancellationRequested();

			string pageText = page.Text ?? string.Empty;
			if (pageText.Trim().Length >= MinTextLengthPerPage)
			{
				pageTexts.Add(pageText);
			}
		}

		string? combinedText = pageTexts.Count > 0 ? string.Join("\n\n", pageTexts) : null;
		return (document.NumberOfPages, metadata, combinedText);
	}

	// PDFtoImage targets Android/iOS/Linux/macCatalyst/macOS/Windows — all platforms we
	// deploy to (Docker/Linux, macOS/Windows dev) or target on mobile. The CA1416
	// analyzer can't prove at the call site that we'll only run on these platforms, so
	// suppress the warning for this helper.
	[System.Diagnostics.CodeAnalysis.SuppressMessage(
		"Interoperability",
		"CA1416:Validate platform compatibility",
		Justification = "PDFtoImage supports all platforms this application runs on (Linux, macOS, Windows).")]
	private byte[] RasterizeFirstPage(byte[] pdfBytes)
	{
		try
		{
			using MemoryStream ms = new();
			Conversion.SavePng(
				ms,
				pdfBytes,
				page: RasterizePageIndex,
				password: null,
				options: new RenderOptions(Dpi: RasterizationDpi));
			return ms.ToArray();
		}
		catch (Exception ex) when (IsPasswordProtectedException(ex))
		{
			// PdfPig usually catches password-protected PDFs earlier, but PDFium may detect
			// it here for files that opened cleanly in PdfPig but encrypt their content
			// streams. Surface the same error either way.
			throw new InvalidOperationException(
				"Password-protected PDFs are not supported.", ex);
		}
		catch (Exception ex)
		{
			throw new InvalidOperationException(
				"Failed to rasterize the first page of the PDF document.", ex);
		}
	}

	private PdfMetadata? ExtractMetadata(PdfDocument document)
	{
		try
		{
			DocumentInformation info = document.Information;
			string? title = string.IsNullOrWhiteSpace(info.Title) ? null : info.Title;

			DateOnly? creationDate = null;
			DateTimeOffset? createdDateTimeOffset = info.GetCreatedDateTimeOffset();
			if (createdDateTimeOffset.HasValue)
			{
				creationDate = DateOnly.FromDateTime(createdDateTimeOffset.Value.DateTime);
			}

			if (title is null && creationDate is null)
			{
				return null;
			}

			return new PdfMetadata(title, creationDate);
		}
		catch (Exception ex)
		{
			logger.LogDebug(ex, "Failed to extract PDF metadata");
			return null;
		}
	}

	internal static bool IsPasswordProtectedException(Exception ex)
	{
		// Primary check: PdfPig surfaces encrypted documents with a typed exception that
		// can also appear as the InnerException when wrapped by higher-level errors.
		// Substring matching on `Message` is fragile — localized .NET runtime strings,
		// future PdfPig message changes, and unrelated errors mentioning "encrypt"
		// (e.g., TLS) all misclassify.
		if (ex is PdfDocumentEncryptedException ||
			ex.InnerException is PdfDocumentEncryptedException)
		{
			return true;
		}

		// Fallback for the PDFium (PDFtoImage) rasterization path: PDFium has no typed
		// equivalent and surfaces password failures only via the message string. Match
		// "password" specifically — narrower than the prior "encrypt" check which would
		// match unrelated runtime/TLS errors.
		return ex.Message.Contains("password", StringComparison.OrdinalIgnoreCase);
	}
}
