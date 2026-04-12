using Application.Commands.Receipt.Scan;
using Application.Exceptions;
using Application.Interfaces.Services;
using Application.Models.Ocr;
using FluentAssertions;
using Moq;

namespace Application.Tests.Commands.Receipt.Scan;

public class ScanReceiptCommandTests
{
	[Fact]
	public void Constructor_NullImageBytes_ThrowsArgumentNullException()
	{
		// Act
		Action act = () => new ScanReceiptCommand(null!, "image/jpeg");

		// Assert
		act.Should().Throw<ArgumentNullException>()
			.And.ParamName.Should().Be("imageBytes");
	}

	[Fact]
	public void Constructor_EmptyImageBytes_ThrowsArgumentException()
	{
		// Act
		Action act = () => new ScanReceiptCommand([], "image/jpeg");

		// Assert
		act.Should().Throw<ArgumentException>()
			.WithMessage($"*{ScanReceiptCommand.ImageBytesCannotBeEmpty}*");
	}

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData("   ")]
	public void Constructor_InvalidContentType_ThrowsArgumentException(string? contentType)
	{
		// Act
		Action act = () => new ScanReceiptCommand([0xFF], contentType!);

		// Assert
		act.Should().Throw<ArgumentException>()
			.WithMessage($"*{ScanReceiptCommand.ContentTypeCannotBeEmpty}*");
	}

	[Fact]
	public void Constructor_ValidArguments_SetsProperties()
	{
		// Arrange
		byte[] bytes = [0xFF, 0xD8];

		// Act
		ScanReceiptCommand actual = new(bytes, "image/jpeg");

		// Assert
		actual.ImageBytes.Should().BeSameAs(bytes);
		actual.ContentType.Should().Be("image/jpeg");
	}
}

public class ScanReceiptCommandHandlerTests
{
	private readonly Mock<IImageProcessingService> _mockImageProcessingService;
	private readonly Mock<IOcrEngine> _mockOcrEngine;
	private readonly Mock<IReceiptParsingService> _mockReceiptParsingService;
	private readonly Mock<IPdfConversionService> _mockPdfConversionService;
	private readonly ScanReceiptCommandHandler _handler;

	public ScanReceiptCommandHandlerTests()
	{
		_mockImageProcessingService = new Mock<IImageProcessingService>();
		_mockOcrEngine = new Mock<IOcrEngine>();
		_mockReceiptParsingService = new Mock<IReceiptParsingService>();
		_mockPdfConversionService = new Mock<IPdfConversionService>();
		_handler = new ScanReceiptCommandHandler(
			_mockImageProcessingService.Object,
			_mockOcrEngine.Object,
			_mockReceiptParsingService.Object,
			_mockPdfConversionService.Object);
	}

	[Fact]
	public async Task Handle_SuccessfulScan_ReturnsResultWithParsedReceipt()
	{
		// Arrange
		byte[] imageBytes = [0xFF, 0xD8, 0xFF, 0xE0];
		ScanReceiptCommand command = new(imageBytes, "image/jpeg");

		byte[] processedBytes = [0x89, 0x50, 0x4E, 0x47];
		ImageProcessingResult processingResult = new(processedBytes, 100, 200);
		_mockImageProcessingService
			.Setup(s => s.PreprocessAsync(imageBytes, "image/jpeg", It.IsAny<CancellationToken>()))
			.ReturnsAsync(processingResult);

		OcrResult ocrResult = new("STORE NAME\nItem 1  $5.00\nTOTAL $5.00", 0.85f);
		_mockOcrEngine
			.Setup(s => s.ExtractTextAsync(processedBytes, It.IsAny<CancellationToken>()))
			.ReturnsAsync(ocrResult);

		ParsedReceipt parsedReceipt = new(
			FieldConfidence<string>.High("STORE NAME"),
			FieldConfidence<DateOnly>.Low(DateOnly.FromDateTime(DateTime.Today)),
			[
				new ParsedReceiptItem(
					FieldConfidence<string?>.None(),
					FieldConfidence<string>.High("Item 1"),
					FieldConfidence<decimal>.Medium(1m),
					FieldConfidence<decimal>.Medium(5.00m),
					FieldConfidence<decimal>.High(5.00m))
			],
			FieldConfidence<decimal>.Low(0m),
			[],
			FieldConfidence<decimal>.High(5.00m),
			FieldConfidence<string?>.None()
		);
		_mockReceiptParsingService
			.Setup(s => s.Parse(ocrResult.Text))
			.Returns(parsedReceipt);

		// Act
		ScanReceiptResult actual = await _handler.Handle(command, CancellationToken.None);

		// Assert
		actual.ParsedReceipt.Should().Be(parsedReceipt);
		actual.RawOcrText.Should().Be(ocrResult.Text);
		actual.OcrConfidence.Should().Be(0.85f);
	}

	[Fact]
	public async Task Handle_SuccessfulScan_CallsServicesInOrder()
	{
		// Arrange
		byte[] imageBytes = [0xFF, 0xD8];
		ScanReceiptCommand command = new(imageBytes, "image/png");

		byte[] processedBytes = [0x01, 0x02];
		ImageProcessingResult processingResult = new(processedBytes, 50, 50);
		_mockImageProcessingService
			.Setup(s => s.PreprocessAsync(imageBytes, "image/png", It.IsAny<CancellationToken>()))
			.ReturnsAsync(processingResult);

		OcrResult ocrResult = new("Some OCR text", 0.9f);
		_mockOcrEngine
			.Setup(s => s.ExtractTextAsync(processedBytes, It.IsAny<CancellationToken>()))
			.ReturnsAsync(ocrResult);

		ParsedReceipt parsedReceipt = new(
			FieldConfidence<string>.Medium("Store"),
			FieldConfidence<DateOnly>.Low(DateOnly.FromDateTime(DateTime.Today)),
			[],
			FieldConfidence<decimal>.Low(0m),
			[],
			FieldConfidence<decimal>.Low(0m),
			FieldConfidence<string?>.None()
		);
		_mockReceiptParsingService
			.Setup(s => s.Parse(ocrResult.Text))
			.Returns(parsedReceipt);

		// Act
		await _handler.Handle(command, CancellationToken.None);

		// Assert
		_mockImageProcessingService.Verify(
			s => s.PreprocessAsync(imageBytes, "image/png", It.IsAny<CancellationToken>()),
			Times.Once);
		_mockOcrEngine.Verify(
			s => s.ExtractTextAsync(processedBytes, It.IsAny<CancellationToken>()),
			Times.Once);
		_mockReceiptParsingService.Verify(
			s => s.Parse(ocrResult.Text),
			Times.Once);
	}

	[Fact]
	public async Task Handle_OcrReturnsEmptyText_ThrowsOcrNoTextException()
	{
		// Arrange
		byte[] imageBytes = [0xFF, 0xD8];
		ScanReceiptCommand command = new(imageBytes, "image/jpeg");

		ImageProcessingResult processingResult = new([0x01], 50, 50);
		_mockImageProcessingService
			.Setup(s => s.PreprocessAsync(imageBytes, "image/jpeg", It.IsAny<CancellationToken>()))
			.ReturnsAsync(processingResult);

		OcrResult ocrResult = new("", 0.0f);
		_mockOcrEngine
			.Setup(s => s.ExtractTextAsync(processingResult.ProcessedBytes, It.IsAny<CancellationToken>()))
			.ReturnsAsync(ocrResult);

		// Act
		Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<OcrNoTextException>()
			.WithMessage("*OCR returned no readable text*");

		_mockReceiptParsingService.Verify(
			s => s.Parse(It.IsAny<string>()),
			Times.Never);
	}

	[Fact]
	public async Task Handle_OcrReturnsWhitespaceText_ThrowsOcrNoTextException()
	{
		// Arrange
		byte[] imageBytes = [0xFF, 0xD8];
		ScanReceiptCommand command = new(imageBytes, "image/jpeg");

		ImageProcessingResult processingResult = new([0x01], 50, 50);
		_mockImageProcessingService
			.Setup(s => s.PreprocessAsync(imageBytes, "image/jpeg", It.IsAny<CancellationToken>()))
			.ReturnsAsync(processingResult);

		OcrResult ocrResult = new("   \n  \t  ", 0.0f);
		_mockOcrEngine
			.Setup(s => s.ExtractTextAsync(processingResult.ProcessedBytes, It.IsAny<CancellationToken>()))
			.ReturnsAsync(ocrResult);

		// Act
		Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<OcrNoTextException>()
			.WithMessage("*OCR returned no readable text*");
	}

	[Fact]
	public async Task Handle_PreprocessingThrows_PropagatesException()
	{
		// Arrange
		byte[] imageBytes = [0xFF, 0xD8];
		ScanReceiptCommand command = new(imageBytes, "image/jpeg");

		_mockImageProcessingService
			.Setup(s => s.PreprocessAsync(imageBytes, "image/jpeg", It.IsAny<CancellationToken>()))
			.ThrowsAsync(new InvalidOperationException("The uploaded file is not a supported image format."));

		// Act
		Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<InvalidOperationException>()
			.WithMessage("*not a supported image format*");

		_mockOcrEngine.Verify(
			s => s.ExtractTextAsync(It.IsAny<byte[]>(), It.IsAny<CancellationToken>()),
			Times.Never);
		_mockReceiptParsingService.Verify(
			s => s.Parse(It.IsAny<string>()),
			Times.Never);
	}

	[Fact]
	public async Task Handle_OcrTextExceedsMaxLength_TruncatesBeforeParsing()
	{
		// Arrange
		byte[] imageBytes = [0xFF, 0xD8];
		ScanReceiptCommand command = new(imageBytes, "image/jpeg");

		ImageProcessingResult processingResult = new([0x01], 50, 50);
		_mockImageProcessingService
			.Setup(s => s.PreprocessAsync(imageBytes, "image/jpeg", It.IsAny<CancellationToken>()))
			.ReturnsAsync(processingResult);

		string longText = new('A', ScanReceiptCommandHandler.MaxOcrTextLength + 1000);
		OcrResult ocrResult = new(longText, 0.7f);
		_mockOcrEngine
			.Setup(s => s.ExtractTextAsync(processingResult.ProcessedBytes, It.IsAny<CancellationToken>()))
			.ReturnsAsync(ocrResult);

		ParsedReceipt parsedReceipt = new(
			FieldConfidence<string>.Low("Unknown"),
			FieldConfidence<DateOnly>.Low(DateOnly.FromDateTime(DateTime.Today)),
			[],
			FieldConfidence<decimal>.Low(0m),
			[],
			FieldConfidence<decimal>.Low(0m),
			FieldConfidence<string?>.None()
		);
		_mockReceiptParsingService
			.Setup(s => s.Parse(It.Is<string>(t => t.Length == ScanReceiptCommandHandler.MaxOcrTextLength)))
			.Returns(parsedReceipt);

		// Act
		ScanReceiptResult actual = await _handler.Handle(command, CancellationToken.None);

		// Assert
		actual.RawOcrText.Should().Be(longText);
		actual.ParsedReceipt.Should().Be(parsedReceipt);

		_mockReceiptParsingService.Verify(
			s => s.Parse(It.Is<string>(t => t.Length == ScanReceiptCommandHandler.MaxOcrTextLength)),
			Times.Once);
	}

	[Fact]
	public async Task Handle_OcrTextWithinMaxLength_PassesFullTextToParsing()
	{
		// Arrange
		byte[] imageBytes = [0xFF, 0xD8];
		ScanReceiptCommand command = new(imageBytes, "image/jpeg");

		ImageProcessingResult processingResult = new([0x01], 50, 50);
		_mockImageProcessingService
			.Setup(s => s.PreprocessAsync(imageBytes, "image/jpeg", It.IsAny<CancellationToken>()))
			.ReturnsAsync(processingResult);

		string normalText = "WALMART\nItem 1  $3.99\nTOTAL  $3.99";
		OcrResult ocrResult = new(normalText, 0.9f);
		_mockOcrEngine
			.Setup(s => s.ExtractTextAsync(processingResult.ProcessedBytes, It.IsAny<CancellationToken>()))
			.ReturnsAsync(ocrResult);

		ParsedReceipt parsedReceipt = new(
			FieldConfidence<string>.High("WALMART"),
			FieldConfidence<DateOnly>.Low(DateOnly.FromDateTime(DateTime.Today)),
			[],
			FieldConfidence<decimal>.Low(0m),
			[],
			FieldConfidence<decimal>.High(3.99m),
			FieldConfidence<string?>.None()
		);
		_mockReceiptParsingService
			.Setup(s => s.Parse(normalText))
			.Returns(parsedReceipt);

		// Act
		ScanReceiptResult actual = await _handler.Handle(command, CancellationToken.None);

		// Assert
		_mockReceiptParsingService.Verify(
			s => s.Parse(normalText),
			Times.Once);
	}

	[Fact]
	public async Task Handle_PdfWithTextLayer_UsesExtractedTextDirectly()
	{
		// Arrange
		byte[] pdfBytes = [0x25, 0x50, 0x44, 0x46]; // %PDF
		ScanReceiptCommand command = new(pdfBytes, "application/pdf");

		string extractedText = "WALMART\nMILK 2%  $3.49\nTOTAL  $3.74";
		PdfConversionResult conversionResult = new(
			Array.Empty<byte[]>(), extractedText, new PdfMetadata("Receipt", null));

		_mockPdfConversionService
			.Setup(s => s.ConvertAsync(pdfBytes, It.IsAny<CancellationToken>()))
			.ReturnsAsync(conversionResult);

		ParsedReceipt parsedReceipt = new(
			FieldConfidence<string>.High("WALMART"),
			FieldConfidence<DateOnly>.Low(DateOnly.FromDateTime(DateTime.Today)),
			[],
			FieldConfidence<decimal>.Low(0m),
			[],
			FieldConfidence<decimal>.High(3.74m),
			FieldConfidence<string?>.None()
		);
		_mockReceiptParsingService
			.Setup(s => s.Parse(extractedText))
			.Returns(parsedReceipt);

		// Act
		ScanReceiptResult actual = await _handler.Handle(command, CancellationToken.None);

		// Assert
		actual.ParsedReceipt.Should().Be(parsedReceipt);
		actual.RawOcrText.Should().Be(extractedText);
		actual.OcrConfidence.Should().Be(0.95f);

		// Should NOT call image processing or OCR engine for text-layer PDFs
		_mockImageProcessingService.Verify(
			s => s.PreprocessAsync(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
			Times.Never);
		_mockOcrEngine.Verify(
			s => s.ExtractTextAsync(It.IsAny<byte[]>(), It.IsAny<CancellationToken>()),
			Times.Never);
	}

	[Fact]
	public async Task Handle_PdfWithImages_RunsOcrOnExtractedImages()
	{
		// Arrange
		byte[] pdfBytes = [0x25, 0x50, 0x44, 0x46]; // %PDF
		ScanReceiptCommand command = new(pdfBytes, "application/pdf");

		byte[] pageImage1 = [0x89, 0x50, 0x4E, 0x47];
		byte[] pageImage2 = [0x89, 0x50, 0x4E, 0x48];
		PdfConversionResult conversionResult = new(
			[pageImage1, pageImage2], null, null);

		_mockPdfConversionService
			.Setup(s => s.ConvertAsync(pdfBytes, It.IsAny<CancellationToken>()))
			.ReturnsAsync(conversionResult);

		byte[] processedBytes1 = [0x01];
		byte[] processedBytes2 = [0x02];
		_mockImageProcessingService
			.Setup(s => s.PreprocessAsync(pageImage1, "image/png", It.IsAny<CancellationToken>()))
			.ReturnsAsync(new ImageProcessingResult(processedBytes1, 100, 100));
		_mockImageProcessingService
			.Setup(s => s.PreprocessAsync(pageImage2, "image/png", It.IsAny<CancellationToken>()))
			.ReturnsAsync(new ImageProcessingResult(processedBytes2, 100, 100));

		_mockOcrEngine
			.Setup(s => s.ExtractTextAsync(processedBytes1, It.IsAny<CancellationToken>()))
			.ReturnsAsync(new OcrResult("WALMART", 0.9f));
		_mockOcrEngine
			.Setup(s => s.ExtractTextAsync(processedBytes2, It.IsAny<CancellationToken>()))
			.ReturnsAsync(new OcrResult("TOTAL $3.74", 0.8f));

		ParsedReceipt parsedReceipt = new(
			FieldConfidence<string>.High("WALMART"),
			FieldConfidence<DateOnly>.Low(DateOnly.FromDateTime(DateTime.Today)),
			[],
			FieldConfidence<decimal>.Low(0m),
			[],
			FieldConfidence<decimal>.High(3.74m),
			FieldConfidence<string?>.None()
		);
		_mockReceiptParsingService
			.Setup(s => s.Parse(It.Is<string>(t => t.Contains("WALMART") && t.Contains("TOTAL"))))
			.Returns(parsedReceipt);

		// Act
		ScanReceiptResult actual = await _handler.Handle(command, CancellationToken.None);

		// Assert
		actual.ParsedReceipt.Should().Be(parsedReceipt);
		actual.RawOcrText.Should().Contain("WALMART");
		actual.RawOcrText.Should().Contain("TOTAL");
		actual.OcrConfidence.Should().BeApproximately(0.85f, 0.01f); // average of 0.9 and 0.8

		_mockImageProcessingService.Verify(
			s => s.PreprocessAsync(It.IsAny<byte[]>(), "image/png", It.IsAny<CancellationToken>()),
			Times.Exactly(2));
		_mockOcrEngine.Verify(
			s => s.ExtractTextAsync(It.IsAny<byte[]>(), It.IsAny<CancellationToken>()),
			Times.Exactly(2));
	}

	[Fact]
	public async Task Handle_PdfWithImagesButNoOcrText_ThrowsOcrNoTextException()
	{
		// Arrange
		byte[] pdfBytes = [0x25, 0x50, 0x44, 0x46];
		ScanReceiptCommand command = new(pdfBytes, "application/pdf");

		byte[] pageImage = [0x89, 0x50];
		PdfConversionResult conversionResult = new([pageImage], null, null);

		_mockPdfConversionService
			.Setup(s => s.ConvertAsync(pdfBytes, It.IsAny<CancellationToken>()))
			.ReturnsAsync(conversionResult);

		_mockImageProcessingService
			.Setup(s => s.PreprocessAsync(pageImage, "image/png", It.IsAny<CancellationToken>()))
			.ReturnsAsync(new ImageProcessingResult([0x01], 50, 50));

		_mockOcrEngine
			.Setup(s => s.ExtractTextAsync(It.IsAny<byte[]>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(new OcrResult("", 0f));

		// Act
		Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<OcrNoTextException>()
			.WithMessage("*OCR returned no readable text*");
	}

	[Fact]
	public async Task Handle_PdfContentType_DelegatesToPdfConversionService()
	{
		// Arrange
		byte[] pdfBytes = [0x25, 0x50, 0x44, 0x46];
		ScanReceiptCommand command = new(pdfBytes, "application/pdf");

		PdfConversionResult conversionResult = new(
			Array.Empty<byte[]>(), "Some receipt text here", null);

		_mockPdfConversionService
			.Setup(s => s.ConvertAsync(pdfBytes, It.IsAny<CancellationToken>()))
			.ReturnsAsync(conversionResult);

		ParsedReceipt parsedReceipt = new(
			FieldConfidence<string>.Low("Unknown"),
			FieldConfidence<DateOnly>.Low(DateOnly.FromDateTime(DateTime.Today)),
			[],
			FieldConfidence<decimal>.Low(0m),
			[],
			FieldConfidence<decimal>.Low(0m),
			FieldConfidence<string?>.None()
		);
		_mockReceiptParsingService
			.Setup(s => s.Parse(It.IsAny<string>()))
			.Returns(parsedReceipt);

		// Act
		await _handler.Handle(command, CancellationToken.None);

		// Assert
		_mockPdfConversionService.Verify(
			s => s.ConvertAsync(pdfBytes, It.IsAny<CancellationToken>()),
			Times.Once);
	}
}
