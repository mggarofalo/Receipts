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

	public Task<PdfConversionResult> ConvertAsync(byte[] pdfBytes, CancellationToken ct)
	{
		ct.ThrowIfCancellationRequested();

		PdfDocument document;
		try
		{
			document = PdfDocument.Open(pdfBytes);
		}
		catch (Exception ex)
		{
			throw new InvalidOperationException(
				"The uploaded file is not a valid PDF or is corrupted.", ex);
		}

		using (document)
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

			// Try to extract text from all pages; collect images from pages without text
			List<string> pageTexts = [];
			List<byte[]> pageImages = [];

			foreach (Page page in document.GetPages())
			{
				ct.ThrowIfCancellationRequested();

				string pageText = page.Text ?? string.Empty;

				if (pageText.Trim().Length >= MinTextLengthPerPage)
				{
					pageTexts.Add(pageText);
				}
				else
				{
					// Page lacks a text layer — try to extract embedded images for OCR
					IEnumerable<IPdfImage> images = page.GetImages();
					foreach (IPdfImage image in images)
					{
						ct.ThrowIfCancellationRequested();

						byte[]? imageBytes = TryExtractImageBytes(image);
						if (imageBytes is not null)
						{
							pageImages.Add(imageBytes);
						}
					}
				}
			}

			// Prefer text extracted directly from the PDF text layer
			if (pageTexts.Count > 0)
			{
				string combinedText = string.Join("\n\n", pageTexts);
				logger.LogInformation(
					"Extracted text from {PageCount} PDF pages ({TextLength} chars)",
					pageTexts.Count, combinedText.Length);

				return Task.FromResult(new PdfConversionResult(
					Array.Empty<byte[]>(), combinedText, metadata));
			}

			if (pageImages.Count > 0)
			{
				logger.LogInformation(
					"Extracted {ImageCount} images from PDF for OCR processing",
					pageImages.Count);

				return Task.FromResult(new PdfConversionResult(
					pageImages, null, metadata));
			}

			throw new InvalidOperationException(
				"The PDF document contains no readable text and no extractable images.");
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
}
