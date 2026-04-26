using Application.Interfaces.Services;
using Microsoft.Extensions.Logging;
using PDFtoImage;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Exceptions;

namespace Infrastructure.Services;

public class PdfConversionService(ILogger<PdfConversionService> logger) : IPdfConversionService
{
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
		using (document)
		{
			try
			{
				pageCount = ValidateDocument(document);
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

		return Task.FromResult(new PdfConversionResult(firstPagePng, pageCount));
	}

	private static int ValidateDocument(PdfDocument document)
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

		return document.NumberOfPages;
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
		catch (Exception ex) when (IsPdfiumEncryptionException(ex))
		{
			// PdfPig usually catches password-protected PDFs earlier, but PDFium may detect
			// it here for files that opened cleanly in PdfPig but encrypt their content
			// streams. PDFium has no typed equivalent of PdfDocumentEncryptedException —
			// detection is by message string. Match both "password" and "encrypt" here
			// because PDFium error messages for encrypted-content-stream rasterization
			// failures may use either keyword. The broader "encrypt" match is safe in this
			// scope: TLS / runtime crypto errors don't surface from `Conversion.SavePng`.
			throw new InvalidOperationException(
				"Password-protected PDFs are not supported.", ex);
		}
		catch (Exception ex)
		{
			throw new InvalidOperationException(
				"Failed to rasterize the first page of the PDF document.", ex);
		}
	}

	/// <summary>
	/// Recognizes a PdfPig-thrown encrypted-document exception. Used for the
	/// <c>PdfDocument.Open</c> path where PdfPig has a dedicated typed exception.
	/// Pure type check — no message-substring matching, so unrelated runtime errors
	/// that happen to mention "encrypt" (TLS, crypto subsystem) are not misclassified.
	/// Walks <see cref="Exception.InnerException"/> defensively in case a higher-level
	/// layer wraps the typed exception.
	/// </summary>
	internal static bool IsPasswordProtectedException(Exception ex)
	{
		return ex is PdfDocumentEncryptedException ||
			   ex.InnerException is PdfDocumentEncryptedException;
	}

	/// <summary>
	/// Recognizes a PDFium / PDFtoImage-thrown exception that signals an encryption
	/// or password-required failure. Used for the <c>Conversion.SavePng</c>
	/// rasterization path. PDFium exposes no typed equivalent of
	/// <see cref="PdfDocumentEncryptedException"/>, so detection is by message
	/// substring. Match both "encrypt" and "password" — PDFium's messages for
	/// content-stream encryption failures use either keyword. The broader match is
	/// safe in this scope because <c>Conversion.SavePng</c> does not surface unrelated
	/// runtime/TLS errors that the prior unscoped substring check could misclassify.
	/// Also covers the typed PdfPig case so callers can use either path uniformly.
	/// </summary>
	internal static bool IsPdfiumEncryptionException(Exception ex)
	{
		if (IsPasswordProtectedException(ex))
		{
			return true;
		}

		string message = ex.Message;
		return message.Contains("password", StringComparison.OrdinalIgnoreCase) ||
			   message.Contains("encrypt", StringComparison.OrdinalIgnoreCase);
	}
}
