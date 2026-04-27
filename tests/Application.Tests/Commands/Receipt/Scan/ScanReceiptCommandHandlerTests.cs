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
	private readonly Mock<IReceiptExtractionService> _mockExtractionService;
	private readonly Mock<IPdfConversionService> _mockPdfConversionService;
	private readonly ScanReceiptCommandHandler _handler;

	public ScanReceiptCommandHandlerTests()
	{
		_mockExtractionService = new Mock<IReceiptExtractionService>();
		_mockPdfConversionService = new Mock<IPdfConversionService>();
		_handler = new ScanReceiptCommandHandler(
			_mockExtractionService.Object,
			_mockPdfConversionService.Object);
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

		_mockPdfConversionService
			.Setup(s => s.ConvertAsync(pdfBytes, It.IsAny<CancellationToken>()))
			.ReturnsAsync(new PdfConversionResult(firstPageImage, TotalPageCount: 1));

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
	public async Task Handle_PdfWithMultiplePages_ReportsDroppedPageCount()
	{
		// Arrange — RECEIPTS-637: a 3-page PDF must surface DroppedPageCount = 2 so
		// the caller can warn the user that pages 2..N were silently ignored.
		byte[] pdfBytes = [0x25, 0x50, 0x44, 0x46];
		ScanReceiptCommand command = new(pdfBytes, "application/pdf");

		byte[] firstPageImage = [0x89, 0x50, 0x4E, 0x47];

		_mockPdfConversionService
			.Setup(s => s.ConvertAsync(pdfBytes, It.IsAny<CancellationToken>()))
			.ReturnsAsync(new PdfConversionResult(firstPageImage, TotalPageCount: 3));

		_mockExtractionService
			.Setup(s => s.ExtractAsync(firstPageImage, "image/png", It.IsAny<CancellationToken>()))
			.ReturnsAsync(BuildPopulatedReceipt());

		// Act
		ScanReceiptResult actual = await _handler.Handle(command, CancellationToken.None);

		// Assert
		actual.DroppedPageCount.Should().Be(2);
	}

	[Fact]
	public async Task Handle_PdfWithSinglePage_ReportsZeroDroppedPages()
	{
		// Arrange — single-page PDF: the count must be 0 (nothing was dropped).
		byte[] pdfBytes = [0x25, 0x50, 0x44, 0x46];
		ScanReceiptCommand command = new(pdfBytes, "application/pdf");

		byte[] firstPageImage = [0x89, 0x50, 0x4E, 0x47];

		_mockPdfConversionService
			.Setup(s => s.ConvertAsync(pdfBytes, It.IsAny<CancellationToken>()))
			.ReturnsAsync(new PdfConversionResult(firstPageImage, TotalPageCount: 1));

		_mockExtractionService
			.Setup(s => s.ExtractAsync(firstPageImage, "image/png", It.IsAny<CancellationToken>()))
			.ReturnsAsync(BuildPopulatedReceipt());

		// Act
		ScanReceiptResult actual = await _handler.Handle(command, CancellationToken.None);

		// Assert
		actual.DroppedPageCount.Should().Be(0);
	}

	[Fact]
	public async Task Handle_Image_ReportsZeroDroppedPages()
	{
		// Arrange — image input: nothing to drop, count must be 0.
		byte[] imageBytes = [0xFF, 0xD8];
		ScanReceiptCommand command = new(imageBytes, "image/jpeg");

		_mockExtractionService
			.Setup(s => s.ExtractAsync(imageBytes, "image/jpeg", It.IsAny<CancellationToken>()))
			.ReturnsAsync(BuildPopulatedReceipt());

		// Act
		ScanReceiptResult actual = await _handler.Handle(command, CancellationToken.None);

		// Assert
		actual.DroppedPageCount.Should().Be(0);
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
	public async Task Handle_ExtractionReturnsLowConfidenceStoreNameOnly_ReturnsResult()
	{
		// Arrange — RECEIPTS-631: a receipt where every field except StoreName is None must
		// NOT be treated as empty. A low-confidence extracted value (even one as minimal as
		// the store name) is still a real reading the user can review and edit. The previous
		// IsEmpty check rejected this case as "empty" because Low and None were conflated.
		byte[] imageBytes = [0xFF, 0xD8];
		ScanReceiptCommand command = new(imageBytes, "image/jpeg");

		ParsedReceipt parsed = new(
			FieldConfidence<string>.Low("Walmart"),
			FieldConfidence<DateOnly>.None(),
			[],
			FieldConfidence<decimal>.None(),
			[],
			FieldConfidence<decimal>.None(),
			FieldConfidence<string?>.None()
		);

		_mockExtractionService
			.Setup(s => s.ExtractAsync(imageBytes, "image/jpeg", It.IsAny<CancellationToken>()))
			.ReturnsAsync(parsed);

		// Act
		ScanReceiptResult actual = await _handler.Handle(command, CancellationToken.None);

		// Assert
		actual.ParsedReceipt.Should().BeSameAs(parsed);
		actual.ParsedReceipt.StoreName.Value.Should().Be("Walmart");
		actual.ParsedReceipt.StoreName.Confidence.Should().Be(ConfidenceLevel.Low);
	}

	[Fact]
	public async Task Handle_ExtractionReturnsLowConfidenceZeroTotalOnly_ReturnsResult()
	{
		// Arrange — RECEIPTS-631 specifically: distinguishes Low(0m) from None() for value
		// types. A receipt where the VLM extracted "$0.00" with low confidence (e.g. an
		// illegible total) is still a present field and must not trigger the empty-receipt
		// short-circuit.
		byte[] imageBytes = [0xFF, 0xD8];
		ScanReceiptCommand command = new(imageBytes, "image/jpeg");

		ParsedReceipt parsed = new(
			FieldConfidence<string>.None(),
			FieldConfidence<DateOnly>.None(),
			[],
			FieldConfidence<decimal>.None(),
			[],
			FieldConfidence<decimal>.Low(0m),
			FieldConfidence<string?>.None()
		);

		_mockExtractionService
			.Setup(s => s.ExtractAsync(imageBytes, "image/jpeg", It.IsAny<CancellationToken>()))
			.ReturnsAsync(parsed);

		// Act
		ScanReceiptResult actual = await _handler.Handle(command, CancellationToken.None);

		// Assert
		actual.ParsedReceipt.Should().BeSameAs(parsed);
		actual.ParsedReceipt.Total.Value.Should().Be(0m);
		actual.ParsedReceipt.Total.Confidence.Should().Be(ConfidenceLevel.Low);
	}

	[Fact]
	public async Task Handle_ExtractionReturnsItemsButNoScalarFields_ReturnsResult()
	{
		// Arrange — a receipt that yielded zero header/total fields but at least one line
		// item is still useful data. The handler must not reject it as empty.
		byte[] imageBytes = [0xFF, 0xD8];
		ScanReceiptCommand command = new(imageBytes, "image/jpeg");

		ParsedReceipt parsed = new(
			FieldConfidence<string>.None(),
			FieldConfidence<DateOnly>.None(),
			[
				new ParsedReceiptItem(
					FieldConfidence<string?>.None(),
					FieldConfidence<string>.High("MILK"),
					FieldConfidence<decimal>.None(),
					FieldConfidence<decimal>.None(),
					FieldConfidence<decimal>.High(3.49m))
			],
			FieldConfidence<decimal>.None(),
			[],
			FieldConfidence<decimal>.None(),
			FieldConfidence<string?>.None()
		);

		_mockExtractionService
			.Setup(s => s.ExtractAsync(imageBytes, "image/jpeg", It.IsAny<CancellationToken>()))
			.ReturnsAsync(parsed);

		// Act
		ScanReceiptResult actual = await _handler.Handle(command, CancellationToken.None);

		// Assert
		actual.ParsedReceipt.Should().BeSameAs(parsed);
		actual.ParsedReceipt.Items.Should().HaveCount(1);
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

	[Fact]
	public async Task Handle_Image_PropagatesCallerCancellationTokenToExtractionService()
	{
		// Arrange — RECEIPTS-647: a refactor that accidentally substitutes
		// CancellationToken.None for the caller-provided cancellationToken would
		// silently break /scan request cancellation. This test asserts the
		// EXACT token instance reaches the extraction service so any drop-the-
		// token regression fails loudly.
		byte[] imageBytes = [0xFF, 0xD8];
		ScanReceiptCommand command = new(imageBytes, "image/jpeg");
		using CancellationTokenSource cts = new();
		CancellationToken expected = cts.Token;

		_mockExtractionService
			.Setup(s => s.ExtractAsync(imageBytes, "image/jpeg", It.Is<CancellationToken>(t => t == expected)))
			.ReturnsAsync(BuildPopulatedReceipt());

		// Act
		await _handler.Handle(command, expected);

		// Assert
		_mockExtractionService.Verify(
			s => s.ExtractAsync(imageBytes, "image/jpeg", It.Is<CancellationToken>(t => t == expected)),
			Times.Once);
	}

	[Fact]
	public async Task Handle_Pdf_PropagatesCallerCancellationTokenToBothServices()
	{
		// Arrange — RECEIPTS-647: the PDF path fans out across two service calls
		// (PdfConversionService.ConvertAsync, then IReceiptExtractionService
		// .ExtractAsync). Both must receive the caller's exact token.
		byte[] pdfBytes = [0x25, 0x50, 0x44, 0x46];
		ScanReceiptCommand command = new(pdfBytes, "application/pdf");
		byte[] firstPageImage = [0x89, 0x50, 0x4E, 0x47];
		using CancellationTokenSource cts = new();
		CancellationToken expected = cts.Token;

		_mockPdfConversionService
			.Setup(s => s.ConvertAsync(pdfBytes, It.Is<CancellationToken>(t => t == expected)))
			.ReturnsAsync(new PdfConversionResult(firstPageImage, TotalPageCount: 1));

		_mockExtractionService
			.Setup(s => s.ExtractAsync(firstPageImage, "image/png", It.Is<CancellationToken>(t => t == expected)))
			.ReturnsAsync(BuildPopulatedReceipt());

		// Act
		await _handler.Handle(command, expected);

		// Assert — both services received the exact caller token
		_mockPdfConversionService.Verify(
			s => s.ConvertAsync(pdfBytes, It.Is<CancellationToken>(t => t == expected)),
			Times.Once);
		_mockExtractionService.Verify(
			s => s.ExtractAsync(firstPageImage, "image/png", It.Is<CancellationToken>(t => t == expected)),
			Times.Once);
	}
}
