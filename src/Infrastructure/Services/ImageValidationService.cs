using Application.Interfaces.Services;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;

namespace Infrastructure.Services;

/// <summary>
/// Validates uploaded receipt images by checking magic-byte format (JPEG/PNG only)
/// and by rejecting oversized dimensions. Replaces the preprocessing-heavy
/// <c>ImageProcessingService</c> that the legacy OCR pipeline required — the VLM-based
/// extraction service ingests the original image bytes directly, so no pixel mutation
/// is needed here.
/// </summary>
public class ImageValidationService(ILogger<ImageValidationService> logger) : IImageValidationService
{
	private const int MaxPixelWidth = 10_000;
	private const int MaxPixelHeight = 10_000;

	private static readonly HashSet<Type> AllowedFormatTypes =
	[
		typeof(JpegFormat),
		typeof(PngFormat),
	];

	public async Task ValidateAsync(byte[] imageBytes, CancellationToken ct)
	{
		ct.ThrowIfCancellationRequested();

		// Validate actual image format via magic bytes, not Content-Type header.
		// ImageSharp throws `UnknownImageFormatException` on unrecognized payloads and
		// `ArgumentException`/`ArgumentNullException` on empty/null input — normalize
		// all three to our public "not a supported image format" contract.
		IImageFormat? detectedFormat;
		try
		{
			detectedFormat = Image.DetectFormat(imageBytes);
		}
		catch (UnknownImageFormatException)
		{
			detectedFormat = null;
		}
		catch (ArgumentException)
		{
			detectedFormat = null;
		}

		if (detectedFormat is null || !AllowedFormatTypes.Contains(detectedFormat.GetType()))
		{
			throw new InvalidOperationException(
				"The uploaded file is not a supported image format. Only JPEG and PNG are accepted.");
		}

		// Check image dimensions before full decode to reject oversized images cheaply
		ImageInfo info;
		try
		{
			info = Image.Identify(imageBytes);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Failed to identify image metadata during validation");
			throw new InvalidOperationException("The uploaded file is not a valid image or is corrupted.", ex);
		}

		if (info.Width > MaxPixelWidth || info.Height > MaxPixelHeight)
		{
			throw new InvalidOperationException(
				$"Image dimensions ({info.Width}x{info.Height}) exceed the maximum allowed ({MaxPixelWidth}x{MaxPixelHeight}).");
		}

		// Method is async so all exception paths surface as a faulted Task per TAP conventions,
		// not thrown synchronously at the call site. Today's body is entirely CPU-bound; the
		// no-op await keeps the signature honest without introducing a thread-pool hop.
		await Task.CompletedTask.ConfigureAwait(false);
	}
}
