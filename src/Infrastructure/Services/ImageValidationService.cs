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

	public Task ValidateAsync(byte[] imageBytes, CancellationToken ct)
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

		// Check image dimensions before full decode to reject oversized images cheaply.
		// Filter the catch to image-format problems only — `OperationCanceledException`,
		// `OutOfMemoryException`, and `StackOverflowException` must propagate so callers
		// can react appropriately (cancellation, process recycling, fatal error).
		ImageInfo info;
		try
		{
			info = Image.Identify(imageBytes);
		}
		catch (Exception ex) when (ex is UnknownImageFormatException or InvalidImageContentException or ArgumentException)
		{
			logger.LogError(ex, "Failed to identify image metadata during validation");
			throw new InvalidOperationException("The uploaded file is not a valid image or is corrupted.", ex);
		}

		if (info.Width > MaxPixelWidth || info.Height > MaxPixelHeight)
		{
			throw new InvalidOperationException(
				$"Image dimensions ({info.Width}x{info.Height}) exceed the maximum allowed ({MaxPixelWidth}x{MaxPixelHeight}).");
		}

		// Body is fully synchronous; return a completed Task so callers still observe
		// thrown exceptions as faulted Tasks per TAP conventions when invoking via the
		// interface, without the no-op `await Task.CompletedTask` thread-pool sham.
		return Task.CompletedTask;
	}
}
