using Application.Commands.Receipt.Scan;
using Application.Exceptions;
using Application.Interfaces.Services;
using Application.Models.Ocr;
using FluentAssertions;
using Microsoft.Extensions.Logging;
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
	private readonly Mock<IReceiptExtractionService> _mockExtractionService;
	private readonly Mock<IPdfConversionService> _mockPdfConversionService;
	private readonly Mock<ILogger<ScanReceiptCommandHandler>> _mockLogger;
	private readonly ScanReceiptCommandHandler _handler;

	public ScanReceiptCommandHandlerTests()
	{
		_mockExtractionService = new Mock<IReceiptExtractionService>();
		_mockPdfConversionService = new Mock<IPdfConversionService>();
		_mockLogger = new Mock<ILogger<ScanReceiptCommandHandler>>();
		_handler = new ScanReceiptCommandHandler(
			_mockExtractionService.Object,
			_mockPdfConversionService.Object,
			_mockLogger.Object);
	}

	private static ParsedReceipt BuildPopulatedReceipt(string storeName = "WALMART", decimal total = 3.74m)
	{
		return new ParsedReceipt(
			FieldConfidence<string>.High(storeName),
			FieldConfidence<DateOnly>.Medium(DateOnly.FromDateTime(DateTime.Today)),
			[
				new ParsedReceiptItem(
					FieldConfidence<string?>.None(),
					FieldConfidence<string>.High("MILK 2%"),
					FieldConfidence<decimal>.High(1m),
					FieldConfidence<decimal>.High(3.49m),
					FieldConfidence<decimal>.High(3.49m))
			],
			FieldConfidence<decimal>.High(3.49m),
			[],
			FieldConfidence<decimal>.High(total),
			FieldConfidence<string?>.None()
		);
	}

	private static ParsedReceipt BuildEmptyReceipt()
	{
		return new ParsedReceipt(
			FieldConfidence<string>.None(),
			FieldConfidence<DateOnly>.None(),
			[],
			FieldConfidence<decimal>.None(),
			[],
			FieldConfidence<decimal>.None(),
			FieldConfidence<string?>.None()
		);
	}

	[Fact]
	public async Task Handle_Image_CallsExtractionServiceWithBytesAndContentType()
	{
		// Arrange
		byte[] imageBytes = [0xFF, 0xD8, 0xFF, 0xE0];
		ScanReceiptCommand command = new(imageBytes, "image/jpeg");

		_mockExtractionService
			.Setup(s => s.ExtractAsync(imageBytes, "image/jpeg", It.IsAny<CancellationToken>()))
			.ReturnsAsync(BuildPopulatedReceipt());

		// Act
		await _handler.Handle(command, CancellationToken.None);

		// Assert
		_mockExtractionService.Verify(
			s => s.ExtractAsync(imageBytes, "image/jpeg", It.IsAny<CancellationToken>()),
			Times.Once);
		_mockPdfConversionService.Verify(
			s => s.ConvertAsync(It.IsAny<byte[]>(), It.IsAny<CancellationToken>()),
			Times.Never);
	}

	[Fact]
	public async Task Handle_Image_ReturnsResultWithParsedReceipt()
	{
		// Arrange
		byte[] imageBytes = [0xFF, 0xD8];
		ScanReceiptCommand command = new(imageBytes, "image/png");
		ParsedReceipt parsed = BuildPopulatedReceipt("COSTCO", 42.99m);

		_mockExtractionService
			.Setup(s => s.ExtractAsync(imageBytes, "image/png", It.IsAny<CancellationToken>()))
			.ReturnsAsync(parsed);

		// Act
		ScanReceiptResult actual = await _handler.Handle(command, CancellationToken.None);

		// Assert
		actual.ParsedReceipt.Should().BeSameAs(parsed);
	}

	[Fact]
	public async Task Handle_Pdf_ConvertsAndExtractsFromFirstPageImage()
	{
		// Arrange
		byte[] pdfBytes = [0x25, 0x50, 0x44, 0x46];
		ScanReceiptCommand command = new(pdfBytes, "application/pdf");

		byte[] firstPageImage = [0x89, 0x50, 0x4E, 0x47];
		PdfConversionResult conversion = new([firstPageImage], null, null);

		_mockPdfConversionService
			.Setup(s => s.ConvertAsync(pdfBytes, It.IsAny<CancellationToken>()))
			.ReturnsAsync(conversion);

		ParsedReceipt parsed = BuildPopulatedReceipt();
		_mockExtractionService
			.Setup(s => s.ExtractAsync(firstPageImage, "image/png", It.IsAny<CancellationToken>()))
			.ReturnsAsync(parsed);

		// Act
		ScanReceiptResult actual = await _handler.Handle(command, CancellationToken.None);

		// Assert
		actual.ParsedReceipt.Should().BeSameAs(parsed);
		_mockExtractionService.Verify(
			s => s.ExtractAsync(firstPageImage, "image/png", It.IsAny<CancellationToken>()),
			Times.Once);
	}

	[Fact]
	public async Task Handle_Pdf_IgnoresExtractedTextField()
	{
		// Arrange — even when the PDF has a text layer, the handler must still call
		// the VLM on the page image. The embedded-text shortcut was removed per
		// RECEIPTS-619 (Paperless OCR is mediocre, prefer re-OCR).
		byte[] pdfBytes = [0x25, 0x50, 0x44, 0x46];
		ScanReceiptCommand command = new(pdfBytes, "application/pdf");

		byte[] firstPageImage = [0x89, 0x50, 0x4E, 0x47];
		PdfConversionResult conversion = new(
			[firstPageImage],
			"WALMART\nMILK 2%  $3.49\nTOTAL  $3.74",
			new PdfMetadata("Receipt", null));

		_mockPdfConversionService
			.Setup(s => s.ConvertAsync(pdfBytes, It.IsAny<CancellationToken>()))
			.ReturnsAsync(conversion);

		_mockExtractionService
			.Setup(s => s.ExtractAsync(firstPageImage, "image/png", It.IsAny<CancellationToken>()))
			.ReturnsAsync(BuildPopulatedReceipt());

		// Act
		await _handler.Handle(command, CancellationToken.None);

		// Assert
		_mockExtractionService.Verify(
			s => s.ExtractAsync(firstPageImage, "image/png", It.IsAny<CancellationToken>()),
			Times.Once);
	}

	[Fact]
	public async Task Handle_PdfWithMultiplePages_UsesFirstPageOnly()
	{
		// Arrange
		byte[] pdfBytes = [0x25, 0x50, 0x44, 0x46];
		ScanReceiptCommand command = new(pdfBytes, "application/pdf");

		byte[] page1 = [0x89, 0x50, 0x4E, 0x47];
		byte[] page2 = [0x89, 0x50, 0x4E, 0x48];
		byte[] page3 = [0x89, 0x50, 0x4E, 0x49];
		PdfConversionResult conversion = new([page1, page2, page3], null, null);

		_mockPdfConversionService
			.Setup(s => s.ConvertAsync(pdfBytes, It.IsAny<CancellationToken>()))
			.ReturnsAsync(conversion);

		_mockExtractionService
			.Setup(s => s.ExtractAsync(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(BuildPopulatedReceipt());

		// Act
		await _handler.Handle(command, CancellationToken.None);

		// Assert
		_mockExtractionService.Verify(
			s => s.ExtractAsync(page1, "image/png", It.IsAny<CancellationToken>()),
			Times.Once);
		_mockExtractionService.Verify(
			s => s.ExtractAsync(page2, It.IsAny<string>(), It.IsAny<CancellationToken>()),
			Times.Never);
		_mockExtractionService.Verify(
			s => s.ExtractAsync(page3, It.IsAny<string>(), It.IsAny<CancellationToken>()),
			Times.Never);
	}

	[Fact]
	public async Task Handle_PdfWithNoImages_ThrowsOcrNoTextException()
	{
		// Arrange
		byte[] pdfBytes = [0x25, 0x50, 0x44, 0x46];
		ScanReceiptCommand command = new(pdfBytes, "application/pdf");

		PdfConversionResult conversion = new(
			Array.Empty<byte[]>(),
			"some text layer content",
			null);

		_mockPdfConversionService
			.Setup(s => s.ConvertAsync(pdfBytes, It.IsAny<CancellationToken>()))
			.ReturnsAsync(conversion);

		// Act
		Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<OcrNoTextException>()
			.WithMessage("*no extractable images*");

		_mockExtractionService.Verify(
			s => s.ExtractAsync(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
			Times.Never);
	}

	[Fact]
	public async Task Handle_ExtractionReturnsEmptyReceipt_ThrowsOcrNoTextException()
	{
		// Arrange
		byte[] imageBytes = [0xFF, 0xD8];
		ScanReceiptCommand command = new(imageBytes, "image/jpeg");

		_mockExtractionService
			.Setup(s => s.ExtractAsync(imageBytes, "image/jpeg", It.IsAny<CancellationToken>()))
			.ReturnsAsync(BuildEmptyReceipt());

		// Act
		Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<OcrNoTextException>()
			.WithMessage("*could not be extracted*");
	}

	[Fact]
	public async Task Handle_ExtractionServiceThrows_PropagatesException()
	{
		// Arrange
		byte[] imageBytes = [0xFF, 0xD8];
		ScanReceiptCommand command = new(imageBytes, "image/jpeg");

		_mockExtractionService
			.Setup(s => s.ExtractAsync(imageBytes, "image/jpeg", It.IsAny<CancellationToken>()))
			.ThrowsAsync(new InvalidOperationException("VLM returned unparseable JSON."));

		// Act
		Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<InvalidOperationException>()
			.WithMessage("*VLM returned unparseable JSON*");
	}

	[Fact]
	public async Task Handle_PdfConversionThrows_PropagatesException()
	{
		// Arrange
		byte[] pdfBytes = [0x25, 0x50, 0x44, 0x46];
		ScanReceiptCommand command = new(pdfBytes, "application/pdf");

		_mockPdfConversionService
			.Setup(s => s.ConvertAsync(pdfBytes, It.IsAny<CancellationToken>()))
			.ThrowsAsync(new InvalidOperationException("The uploaded file is not a valid PDF."));

		// Act
		Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<InvalidOperationException>()
			.WithMessage("*not a valid PDF*");

		_mockExtractionService.Verify(
			s => s.ExtractAsync(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
			Times.Never);
	}

	[Theory]
	[InlineData("image/jpeg")]
	[InlineData("image/png")]
	[InlineData("IMAGE/JPEG")]
	public async Task Handle_NonPdfContentType_SkipsPdfConversion(string contentType)
	{
		// Arrange
		byte[] imageBytes = [0xFF, 0xD8];
		ScanReceiptCommand command = new(imageBytes, contentType);

		_mockExtractionService
			.Setup(s => s.ExtractAsync(imageBytes, contentType, It.IsAny<CancellationToken>()))
			.ReturnsAsync(BuildPopulatedReceipt());

		// Act
		await _handler.Handle(command, CancellationToken.None);

		// Assert
		_mockPdfConversionService.Verify(
			s => s.ConvertAsync(It.IsAny<byte[]>(), It.IsAny<CancellationToken>()),
			Times.Never);
	}
}
