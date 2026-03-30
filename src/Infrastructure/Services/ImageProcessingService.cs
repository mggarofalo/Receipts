using Application.Interfaces.Services;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Infrastructure.Services;

public class ImageProcessingService(ILogger<ImageProcessingService> logger) : IImageProcessingService
{
	private const int ThresholdKernelSize = 15;
	private const double DeskewMaxAngle = 10.0;
	private const double DeskewStepDegrees = 0.5;
	private const double DeskewMinAngle = 0.5;

	public Task<ImageProcessingResult> PreprocessAsync(byte[] imageBytes, string contentType, CancellationToken ct)
	{
		ct.ThrowIfCancellationRequested();

		Image<L8> image;
		try
		{
			image = Image.Load<L8>(imageBytes);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Failed to load image for preprocessing");
			throw new InvalidOperationException("The uploaded file is not a valid image or is corrupted.", ex);
		}

		using (image)
		{
			// Step 1: Already grayscale (loaded as L8)

			// Step 2: Adaptive threshold (15x15 local mean)
			ApplyAdaptiveThreshold(image);

			ct.ThrowIfCancellationRequested();

			// Step 3: Deskew via projection profile
			double skewAngle = DetectSkewAngle(image);
			if (Math.Abs(skewAngle) > DeskewMinAngle)
			{
				image.Mutate(ctx => ctx.Rotate((float)-skewAngle));
				logger.LogInformation("Deskewed image by {Angle:F1} degrees", -skewAngle);
			}

			ct.ThrowIfCancellationRequested();

			// Step 4: Encode as PNG
			using MemoryStream ms = new();
			image.Save(ms, new PngEncoder());

			return Task.FromResult(new ImageProcessingResult(
				ms.ToArray(), image.Width, image.Height));
		}
	}

	internal static void ApplyAdaptiveThreshold(Image<L8> image)
	{
		int width = image.Width;
		int height = image.Height;
		int halfKernel = ThresholdKernelSize / 2;

		// Build integral image for fast local mean computation
		long[,] integral = new long[height + 1, width + 1];

		image.ProcessPixelRows(accessor =>
		{
			for (int y = 0; y < height; y++)
			{
				Span<L8> row = accessor.GetRowSpan(y);
				long rowSum = 0;
				for (int x = 0; x < width; x++)
				{
					rowSum += row[x].PackedValue;
					integral[y + 1, x + 1] = integral[y, x + 1] + rowSum;
				}
			}
		});

		// Compute mean pixel value to detect all-black/white images
		long totalSum = integral[height, width];
		double meanPixel = (double)totalSum / (width * height);

		// Skip thresholding if image is nearly all-black or all-white
		if (meanPixel < 5 || meanPixel > 250)
		{
			return;
		}

		// Apply threshold using integral image
		image.ProcessPixelRows(accessor =>
		{
			for (int y = 0; y < height; y++)
			{
				Span<L8> row = accessor.GetRowSpan(y);
				for (int x = 0; x < width; x++)
				{
					int y1 = Math.Max(0, y - halfKernel);
					int y2 = Math.Min(height - 1, y + halfKernel);
					int x1 = Math.Max(0, x - halfKernel);
					int x2 = Math.Min(width - 1, x + halfKernel);

					int area = (y2 - y1 + 1) * (x2 - x1 + 1);
					long sum = integral[y2 + 1, x2 + 1]
						- integral[y1, x2 + 1]
						- integral[y2 + 1, x1]
						+ integral[y1, x1];

					double localMean = (double)sum / area;
					row[x] = new L8(row[x].PackedValue >= localMean ? (byte)255 : (byte)0);
				}
			}
		});
	}

	internal static double DetectSkewAngle(Image<L8> image)
	{
		int width = image.Width;
		int height = image.Height;

		// Extract pixel data for projection computation
		byte[] pixels = new byte[width * height];
		image.ProcessPixelRows(accessor =>
		{
			for (int y = 0; y < height; y++)
			{
				Span<L8> row = accessor.GetRowSpan(y);
				for (int x = 0; x < width; x++)
				{
					pixels[y * width + x] = row[x].PackedValue;
				}
			}
		});

		double bestAngle = 0;
		double bestVariance = -1;

		int steps = (int)(2 * DeskewMaxAngle / DeskewStepDegrees) + 1;
		for (int i = 0; i < steps; i++)
		{
			double angle = -DeskewMaxAngle + i * DeskewStepDegrees;
			double radians = angle * Math.PI / 180.0;
			double sinA = Math.Sin(radians);
			double cosA = Math.Cos(radians);

			// Compute horizontal projection profile
			int[] projection = new int[height];
			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					if (pixels[y * width + x] == 0)
					{
						// Rotate point and accumulate at projected row
						int projY = (int)Math.Round(-x * sinA + y * cosA);
						if (projY >= 0 && projY < height)
						{
							projection[projY]++;
						}
					}
				}
			}

			// Compute variance of projection
			double mean = 0;
			for (int y = 0; y < height; y++)
			{
				mean += projection[y];
			}
			mean /= height;

			double variance = 0;
			for (int y = 0; y < height; y++)
			{
				double diff = projection[y] - mean;
				variance += diff * diff;
			}
			variance /= height;

			if (variance > bestVariance)
			{
				bestVariance = variance;
				bestAngle = angle;
			}
		}

		return bestAngle;
	}
}
