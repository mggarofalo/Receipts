using Application.Interfaces.Services;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Graphics.Colors;

namespace Infrastructure.Services;

public class PdfConversionService(ILogger<PdfConversionService> logger) : IPdfConversionService
{
	/// <summary>
	/// Minimum text length per page to consider the page as having a usable text layer.
	/// Pages with fewer characters are treated as image-only.
	/// </summary>
	internal const int MinTextLengthPerPage = 10;

	/// <summary>
	/// Maximum pixel dimension (width or height) for images created from raw pixel data.
	/// Matches the cap in <see cref="ImageProcessingService"/>.
	/// </summary>
	private const int MaxImageDimension = 10_000;

	/// <summary>
	/// Minimum pixel dimension (width and height) for an embedded image to be considered
	/// a candidate receipt scan. Decorative images on text-layer pages (logos, icons,
	/// background marks) are typically well under this threshold; Paperless-style scans
	/// and phone-camera photos are always far above it.
	/// </summary>
	internal const int MinReceiptImageDimension = 400;

	/// <summary>
	/// Maximum total accumulated image bytes (100 MB) before we stop extracting images.
	/// </summary>
	private const long MaxTotalImageBytes = 100 * 1024 * 1024;

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

		using (document)
		{
			try
			{
				return ProcessDocument(document, ct);
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
	}

	private Task<PdfConversionResult> ProcessDocument(PdfDocument document, CancellationToken ct)
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

		// Always collect both text and images from every page. Consumers (the scan command
		// handler) use the images for VLM-based extraction; the text layer is kept as an
		// optional informational field that is no longer consumed on the scan path.
		List<string> pageTexts = [];
		List<byte[]> pageImages = [];
		long totalImageBytes = 0;
		bool imageBytesCapReached = false;

		foreach (Page page in document.GetPages())
		{
			ct.ThrowIfCancellationRequested();

			string pageText = page.Text ?? string.Empty;
			if (pageText.Trim().Length >= MinTextLengthPerPage)
			{
				pageTexts.Add(pageText);
			}

			if (imageBytesCapReached)
			{
				continue;
			}

			foreach (IPdfImage image in page.GetImages())
			{
				ct.ThrowIfCancellationRequested();

				if (imageBytesCapReached)
				{
					break;
				}

				// Skip decorative images (logos, icons) on text-layer pages. A real
				// receipt scan is always at least ~400px on a side; logos rarely exceed
				// a couple hundred pixels.
				if (image.WidthInSamples < MinReceiptImageDimension
					|| image.HeightInSamples < MinReceiptImageDimension)
				{
					continue;
				}

				byte[]? imageBytes = TryExtractImageBytes(image);
				if (imageBytes is null)
				{
					continue;
				}

				totalImageBytes += imageBytes.Length;
				if (totalImageBytes > MaxTotalImageBytes)
				{
					logger.LogWarning(
						"Accumulated image bytes ({TotalBytes}) exceeded cap of {MaxBytes}; stopping image extraction",
						totalImageBytes, MaxTotalImageBytes);
					imageBytesCapReached = true;
					break;
				}

				pageImages.Add(imageBytes);
			}
		}

		if (pageImages.Count == 0 && pageTexts.Count == 0)
		{
			throw new InvalidOperationException(
				"The PDF document contains no readable text and no extractable images.");
		}

		string? combinedText = pageTexts.Count > 0 ? string.Join("\n\n", pageTexts) : null;

		if (pageImages.Count > 0)
		{
			logger.LogInformation(
				"Extracted {ImageCount} images from PDF for OCR processing",
				pageImages.Count);
		}
		if (combinedText is not null)
		{
			logger.LogInformation(
				"Extracted text from {PageCount} PDF pages ({TextLength} chars)",
				pageTexts.Count, combinedText.Length);
		}

		return Task.FromResult(new PdfConversionResult(pageImages, combinedText, metadata));
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

	private byte[]? TryExtractImageBytes(IPdfImage image)
	{
		try
		{
			// Try to get the raw image bytes
			if (!image.TryGetPng(out byte[]? pngBytes) || pngBytes is null)
			{
				// Fall back to raw bytes and try to interpret them
				byte[] rawBytes = image.RawBytes.ToArray();
				if (rawBytes.Length == 0)
				{
					return null;
				}

				// Try to load as an image to validate
				try
				{
					IImageFormat? format = Image.DetectFormat(rawBytes);
					if (format is not null)
					{
						return rawBytes;
					}
				}
				catch
				{
					// Not a valid image format
				}

				// For non-standard color spaces, try to create an image from raw pixel data
				if (image.WidthInSamples > 0 && image.HeightInSamples > 0 &&
					image.ColorSpaceDetails?.BaseType == ColorSpace.DeviceRGB)
				{
					return TryCreatePngFromRawPixels(
						rawBytes, (int)image.WidthInSamples, (int)image.HeightInSamples);
				}

				return null;
			}

			return pngBytes;
		}
		catch (Exception ex)
		{
			logger.LogDebug(ex, "Failed to extract image from PDF page");
			return null;
		}
	}

	private static byte[]? TryCreatePngFromRawPixels(byte[] rawBytes, int width, int height)
	{
		// Reject dimensions that would cause excessive memory allocation
		if (width > MaxImageDimension || height > MaxImageDimension)
		{
			return null;
		}

		int expectedLength = width * height * 3; // RGB
		if (rawBytes.Length < expectedLength)
		{
			return null;
		}

		try
		{
			using Image<Rgb24> image = new(width, height);
			image.ProcessPixelRows(accessor =>
			{
				for (int y = 0; y < height; y++)
				{
					Span<Rgb24> row = accessor.GetRowSpan(y);
					for (int x = 0; x < width; x++)
					{
						int offset = (y * width + x) * 3;
						row[x] = new Rgb24(rawBytes[offset], rawBytes[offset + 1], rawBytes[offset + 2]);
					}
				}
			});

			using MemoryStream ms = new();
			image.Save(ms, new PngEncoder());
			return ms.ToArray();
		}
		catch
		{
			return null;
		}
	}

	private static bool IsPasswordProtectedException(Exception ex)
	{
		string message = ex.Message;
		return message.Contains("encrypt", StringComparison.OrdinalIgnoreCase) ||
			   message.Contains("password", StringComparison.OrdinalIgnoreCase);
	}
}
