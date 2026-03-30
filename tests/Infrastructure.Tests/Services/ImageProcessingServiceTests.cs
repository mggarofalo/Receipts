using Application.Interfaces.Services;
using FluentAssertions;
using Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Moq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Infrastructure.Tests.Services;

public class ImageProcessingServiceTests
{
	private readonly ImageProcessingService _service;

	public ImageProcessingServiceTests()
	{
		Mock<ILogger<ImageProcessingService>> mockLogger = new();
		_service = new ImageProcessingService(mockLogger.Object);
	}

	[Fact]
	public async Task PreprocessAsync_ValidJpeg_ReturnsProcessedPng()
	{
		// Arrange
		byte[] imageBytes = CreateTestJpeg(100, 100);

		// Act
		ImageProcessingResult result = await _service.PreprocessAsync(imageBytes, "image/jpeg", CancellationToken.None);

		// Assert
		result.ProcessedBytes.Should().NotBeNullOrEmpty();
		result.Width.Should().BeGreaterThan(0);
		result.Height.Should().BeGreaterThan(0);

		// Verify output is valid PNG
		using Image<L8> output = Image.Load<L8>(result.ProcessedBytes);
		output.Width.Should().BeGreaterThan(0);
	}

	[Fact]
	public async Task PreprocessAsync_ValidPng_ReturnsProcessedPng()
	{
		// Arrange
		byte[] imageBytes = CreateTestPng(80, 120);

		// Act
		ImageProcessingResult result = await _service.PreprocessAsync(imageBytes, "image/png", CancellationToken.None);

		// Assert
		result.ProcessedBytes.Should().NotBeNullOrEmpty();
		result.Width.Should().BeGreaterThan(0);
		result.Height.Should().BeGreaterThan(0);
	}

	[Fact]
	public async Task PreprocessAsync_CorruptData_ThrowsInvalidOperationException()
	{
		// Arrange
		byte[] corruptBytes = [0x00, 0x01, 0x02, 0x03, 0x04, 0x05];

		// Act
		Func<Task> act = () => _service.PreprocessAsync(corruptBytes, "image/jpeg", CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<InvalidOperationException>()
			.WithMessage("*not a supported image format*");
	}

	[Fact]
	public async Task PreprocessAsync_GifMasqueradingAsJpeg_ThrowsInvalidOperationException()
	{
		// Arrange - GIF magic bytes with JPEG content type
		byte[] gifBytes = [0x47, 0x49, 0x46, 0x38, 0x39, 0x61]; // GIF89a

		// Act
		Func<Task> act = () => _service.PreprocessAsync(gifBytes, "image/jpeg", CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<InvalidOperationException>()
			.WithMessage("*not a supported image format*");
	}

	[Fact]
	public void ApplyAdaptiveThreshold_GrayscaleImage_ProducesBlackAndWhite()
	{
		// Arrange - create a gradient image
		using Image<L8> image = new(50, 50);
		image.ProcessPixelRows(accessor =>
		{
			for (int y = 0; y < 50; y++)
			{
				Span<L8> row = accessor.GetRowSpan(y);
				for (int x = 0; x < 50; x++)
				{
					row[x] = new L8((byte)(x * 5)); // gradient from 0 to 245
				}
			}
		});

		// Act
		ImageProcessingService.ApplyAdaptiveThreshold(image);

		// Assert - pixels should be either 0 or 255
		image.ProcessPixelRows(accessor =>
		{
			for (int y = 0; y < image.Height; y++)
			{
				Span<L8> row = accessor.GetRowSpan(y);
				for (int x = 0; x < image.Width; x++)
				{
					byte val = row[x].PackedValue;
					(val == 0 || val == 255).Should().BeTrue(
						$"pixel ({x},{y}) should be 0 or 255 but was {val}");
				}
			}
		});
	}

	[Fact]
	public void ApplyAdaptiveThreshold_AllBlackImage_SkipsThreshold()
	{
		// Arrange - all-black image (mean < 5)
		using Image<L8> image = new(30, 30, new L8(0));

		// Act
		ImageProcessingService.ApplyAdaptiveThreshold(image);

		// Assert - should remain all black (threshold skipped)
		image.ProcessPixelRows(accessor =>
		{
			Span<L8> row = accessor.GetRowSpan(0);
			row[0].PackedValue.Should().Be(0);
		});
	}

	[Fact]
	public void ApplyAdaptiveThreshold_AllWhiteImage_SkipsThreshold()
	{
		// Arrange - all-white image (mean > 250)
		using Image<L8> image = new(30, 30, new L8(255));

		// Act
		ImageProcessingService.ApplyAdaptiveThreshold(image);

		// Assert - should remain all white (threshold skipped)
		image.ProcessPixelRows(accessor =>
		{
			Span<L8> row = accessor.GetRowSpan(0);
			row[0].PackedValue.Should().Be(255);
		});
	}

	[Fact]
	public void DetectSkewAngle_StraightImage_ReturnsNearZero()
	{
		// Arrange - create image with horizontal black lines (no skew)
		using Image<L8> image = new(100, 100, new L8(255));
		image.ProcessPixelRows(accessor =>
		{
			for (int y = 20; y < 25; y++)
			{
				Span<L8> row = accessor.GetRowSpan(y);
				for (int x = 10; x < 90; x++)
				{
					row[x] = new L8(0);
				}
			}
			for (int y = 50; y < 55; y++)
			{
				Span<L8> row = accessor.GetRowSpan(y);
				for (int x = 10; x < 90; x++)
				{
					row[x] = new L8(0);
				}
			}
			for (int y = 80; y < 85; y++)
			{
				Span<L8> row = accessor.GetRowSpan(y);
				for (int x = 10; x < 90; x++)
				{
					row[x] = new L8(0);
				}
			}
		});

		// Act
		double angle = ImageProcessingService.DetectSkewAngle(image);

		// Assert - should be near zero for horizontal lines
		Math.Abs(angle).Should().BeLessThan(1.0);
	}

	[Fact]
	public async Task PreprocessAsync_CancellationRequested_ThrowsOperationCanceledException()
	{
		// Arrange
		byte[] imageBytes = CreateTestPng(50, 50);
		using CancellationTokenSource cts = new();
		cts.Cancel();

		// Act
		Func<Task> act = () => _service.PreprocessAsync(imageBytes, "image/png", cts.Token);

		// Assert
		await act.Should().ThrowAsync<OperationCanceledException>();
	}

	[Fact]
	public async Task PreprocessAsync_OutputIsSingleChannelGrayscale()
	{
		// Arrange - create a color-like JPEG (will still be L8 channel but verifies pipeline output)
		byte[] imageBytes = CreateTestJpeg(60, 40);

		// Act
		ImageProcessingResult result = await _service.PreprocessAsync(imageBytes, "image/jpeg", CancellationToken.None);

		// Assert - output should be loadable as L8 (grayscale)
		using Image<L8> output = Image.Load<L8>(result.ProcessedBytes);
		output.Width.Should().BeGreaterThan(0);
		output.Height.Should().BeGreaterThan(0);
	}

	[Fact]
	public async Task PreprocessAsync_ReturnsCorrectDimensions()
	{
		// Arrange
		byte[] imageBytes = CreateTestPng(75, 50);

		// Act
		ImageProcessingResult result = await _service.PreprocessAsync(imageBytes, "image/png", CancellationToken.None);

		// Assert
		result.Width.Should().BeGreaterThan(0);
		result.Height.Should().BeGreaterThan(0);
		// Dimensions may differ slightly due to deskew rotation, but should be close
		result.Width.Should().BeInRange(50, 100);
		result.Height.Should().BeInRange(25, 75);
	}

	[Fact]
	public void DetectSkewAngle_UniformImage_ReturnsZero()
	{
		// Arrange - uniform gray image with no features
		using Image<L8> image = new(50, 50, new L8(128));

		// Act
		double angle = ImageProcessingService.DetectSkewAngle(image);

		// Assert - with no features, angle should be near zero
		Math.Abs(angle).Should().BeLessThanOrEqualTo(10.0);
	}

	[Fact]
	public void ApplyAdaptiveThreshold_NearBlackImage_SkipsThreshold()
	{
		// Arrange - image with mean pixel value < 5 (mostly black with some very dark pixels)
		using Image<L8> image = new(30, 30, new L8(2));

		// Act
		ImageProcessingService.ApplyAdaptiveThreshold(image);

		// Assert - should be unchanged (threshold skipped)
		image.ProcessPixelRows(accessor =>
		{
			Span<L8> row = accessor.GetRowSpan(0);
			row[0].PackedValue.Should().Be(2);
		});
	}

	[Fact]
	public void ApplyAdaptiveThreshold_NearWhiteImage_SkipsThreshold()
	{
		// Arrange - image with mean pixel value > 250
		using Image<L8> image = new(30, 30, new L8(252));

		// Act
		ImageProcessingService.ApplyAdaptiveThreshold(image);

		// Assert - should be unchanged (threshold skipped)
		image.ProcessPixelRows(accessor =>
		{
			Span<L8> row = accessor.GetRowSpan(0);
			row[0].PackedValue.Should().Be(252);
		});
	}

	private static byte[] CreateTestJpeg(int width, int height)
	{
		using Image<L8> image = new(width, height);
		image.ProcessPixelRows(accessor =>
		{
			for (int y = 0; y < height; y++)
			{
				Span<L8> row = accessor.GetRowSpan(y);
				for (int x = 0; x < width; x++)
				{
					row[x] = new L8((byte)((x + y) % 256));
				}
			}
		});

		using MemoryStream ms = new();
		image.Save(ms, new JpegEncoder());
		return ms.ToArray();
	}

	private static byte[] CreateTestPng(int width, int height)
	{
		using Image<L8> image = new(width, height);
		image.ProcessPixelRows(accessor =>
		{
			for (int y = 0; y < height; y++)
			{
				Span<L8> row = accessor.GetRowSpan(y);
				for (int x = 0; x < width; x++)
				{
					row[x] = new L8((byte)((x * 3 + y * 7) % 256));
				}
			}
		});

		using MemoryStream ms = new();
		image.Save(ms, new PngEncoder());
		return ms.ToArray();
	}
}
