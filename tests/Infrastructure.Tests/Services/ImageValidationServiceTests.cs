using FluentAssertions;
using Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Moq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;

namespace Infrastructure.Tests.Services;

public class ImageValidationServiceTests
{
	private readonly ImageValidationService _service;

	public ImageValidationServiceTests()
	{
		Mock<ILogger<ImageValidationService>> mockLogger = new();
		_service = new ImageValidationService(mockLogger.Object);
	}

	[Fact]
	public async Task ValidateAsync_ValidJpeg_DoesNotThrow()
	{
		// Arrange
		byte[] imageBytes = CreateTestJpeg(100, 100);

		// Act
		Func<Task> act = () => _service.ValidateAsync(imageBytes, CancellationToken.None);

		// Assert
		await act.Should().NotThrowAsync();
	}

	[Fact]
	public async Task ValidateAsync_ValidPng_DoesNotThrow()
	{
		// Arrange
		byte[] imageBytes = CreateTestPng(80, 120);

		// Act
		Func<Task> act = () => _service.ValidateAsync(imageBytes, CancellationToken.None);

		// Assert
		await act.Should().NotThrowAsync();
	}

	[Fact]
	public async Task ValidateAsync_CorruptData_ThrowsInvalidOperationException()
	{
		// Arrange - a handful of non-magic bytes
		byte[] corruptBytes = [0x00, 0x01, 0x02, 0x03, 0x04, 0x05];

		// Act
		Func<Task> act = () => _service.ValidateAsync(corruptBytes, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<InvalidOperationException>()
			.WithMessage("*not a supported image format*");
	}

	[Fact]
	public async Task ValidateAsync_GifBytes_ThrowsInvalidOperationException()
	{
		// Arrange - GIF magic bytes. ImageSharp detects it as GIF, but the validator only
		// accepts JPEG and PNG.
		byte[] gifBytes = [0x47, 0x49, 0x46, 0x38, 0x39, 0x61]; // "GIF89a"

		// Act
		Func<Task> act = () => _service.ValidateAsync(gifBytes, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<InvalidOperationException>()
			.WithMessage("*not a supported image format*");
	}

	[Fact]
	public async Task ValidateAsync_EmptyBytes_ThrowsInvalidOperationException()
	{
		// Arrange
		byte[] empty = [];

		// Act
		Func<Task> act = () => _service.ValidateAsync(empty, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<InvalidOperationException>()
			.WithMessage("*not a supported image format*");
	}

	[Fact]
	public async Task ValidateAsync_CancellationRequested_ThrowsOperationCanceledException()
	{
		// Arrange
		byte[] imageBytes = CreateTestJpeg(10, 10);
		using CancellationTokenSource cts = new();
		cts.Cancel();

		// Act
		Func<Task> act = () => _service.ValidateAsync(imageBytes, cts.Token);

		// Assert
		await act.Should().ThrowAsync<OperationCanceledException>();
	}

	[Fact]
	public async Task ValidateAsync_CancellationRequested_DoesNotWrapAsInvalidOperationException()
	{
		// Arrange — RECEIPTS-645: the previous catch (Exception ex) around Image.Identify
		// rewrapped OperationCanceledException as "the file is not a valid image or is
		// corrupted". The narrowed catch filter must let cancellation propagate intact so
		// callers can distinguish user cancellation from corrupt image content.
		byte[] imageBytes = CreateTestJpeg(10, 10);
		using CancellationTokenSource cts = new();
		cts.Cancel();

		// Act
		Func<Task> act = () => _service.ValidateAsync(imageBytes, cts.Token);

		// Assert
		(await act.Should().ThrowAsync<OperationCanceledException>())
			.Which.Should().NotBeOfType<InvalidOperationException>(
				"OperationCanceledException must propagate raw, not be wrapped as a corrupt-image error");
	}

	[Fact]
	public async Task ValidateAsync_TruncatedJpegContent_ThrowsInvalidOperationException()
	{
		// Arrange — RECEIPTS-645: format-error wrapping must still work with the narrower
		// catch filter. A valid JPEG SOI marker (FFD8FF) followed by garbage tricks
		// DetectFormat into returning JpegFormat, then Image.Identify fails with
		// InvalidImageContentException. The service must still translate that into the
		// public "not a valid image or is corrupted" InvalidOperationException.
		byte[] truncatedJpeg = [0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46, 0x00];

		// Act
		Func<Task> act = () => _service.ValidateAsync(truncatedJpeg, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<InvalidOperationException>()
			.WithMessage("*not a valid image or is corrupted*");
	}

	private static byte[] CreateTestJpeg(int width, int height)
	{
		using Image<Rgba32> image = new(width, height);
		using MemoryStream ms = new();
		image.Save(ms, new JpegEncoder());
		return ms.ToArray();
	}

	private static byte[] CreateTestPng(int width, int height)
	{
		using Image<Rgba32> image = new(width, height);
		using MemoryStream ms = new();
		image.Save(ms, new PngEncoder());
		return ms.ToArray();
	}
}
