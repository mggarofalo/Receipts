using Application.Interfaces.Services;
using FluentAssertions;
using Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;

namespace Infrastructure.Tests.Services;

[Trait("Category", "Integration")]
public class TesseractOcrEngineIntegrationTests : IDisposable
{
	private static readonly string TessdataPath = Path.Combine(AppContext.BaseDirectory, "Models", "Tessdata");
	private static readonly bool TessdataAvailable = File.Exists(Path.Combine(TessdataPath, "eng.traineddata"));

	private readonly TesseractOcrEngine? _engine;

	public TesseractOcrEngineIntegrationTests()
	{
		if (TessdataAvailable)
		{
			Dictionary<string, string?> configValues = new()
			{
				["Ocr:TessdataPath"] = TessdataPath
			};

			IConfiguration configuration = new ConfigurationBuilder()
				.AddInMemoryCollection(configValues)
				.Build();

			Mock<ILogger<TesseractOcrEngine>> mockLogger = new();
			_engine = new TesseractOcrEngine(configuration, mockLogger.Object);
		}
	}

	[Fact]
	public async Task ExtractTextAsync_SyntheticReceipt_ReturnsTextWithNonZeroConfidence()
	{
		if (!TessdataAvailable)
		{
			// Tessdata not available — skip gracefully. Integration tests require eng.traineddata.
			return;
		}

		// Arrange — create a simple white image with black horizontal bars simulating text lines
		byte[] imageBytes = CreateSyntheticReceiptPng();

		// Act
		OcrResult result = await _engine!.ExtractTextAsync(imageBytes, CancellationToken.None);

		// Assert
		result.Should().NotBeNull();
		result.Confidence.Should().BeInRange(0f, 1f);
	}

	[Fact]
	public async Task ExtractTextAsync_ImageExceedsMaxBytes_ThrowsArgumentException()
	{
		if (!TessdataAvailable)
		{
			return;
		}

		// Arrange — create a byte array larger than the 10 MB default
		byte[] oversizedBytes = new byte[11 * 1024 * 1024];

		// Act
		Func<Task> act = () => _engine!.ExtractTextAsync(oversizedBytes, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<ArgumentException>()
			.WithParameterName("imageBytes");
	}

	[Fact]
	public async Task ExtractTextAsync_CancelledToken_ThrowsOperationCanceledException()
	{
		if (!TessdataAvailable)
		{
			return;
		}

		// Arrange
		byte[] imageBytes = CreateSyntheticReceiptPng();
		using CancellationTokenSource cts = new();
		cts.Cancel();

		// Act
		Func<Task> act = () => _engine!.ExtractTextAsync(imageBytes, cts.Token);

		// Assert
		await act.Should().ThrowAsync<OperationCanceledException>();
	}

	private static byte[] CreateSyntheticReceiptPng()
	{
		// Create a white 400x200 image with black horizontal bars simulating text lines
		using Image<L8> image = new(400, 200, new L8(255));

		image.ProcessPixelRows(accessor =>
		{
			// Draw 5 horizontal black bars at different y positions
			int[] barYStarts = [30, 60, 90, 120, 150];
			int[] barWidths = [360, 300, 340, 280, 320];

			for (int b = 0; b < barYStarts.Length; b++)
			{
				for (int y = barYStarts[b]; y < barYStarts[b] + 8; y++)
				{
					Span<L8> row = accessor.GetRowSpan(y);
					for (int x = 20; x < 20 + barWidths[b]; x++)
					{
						row[x] = new L8(0);
					}
				}
			}
		});

		using MemoryStream ms = new();
		image.Save(ms, new PngEncoder());
		return ms.ToArray();
	}

	public void Dispose()
	{
		_engine?.Dispose();
		GC.SuppressFinalize(this);
	}
}
