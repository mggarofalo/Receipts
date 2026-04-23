using API.Controllers.Core;
using API.Generated.Dtos;
using Application.Commands.Receipt.Scan;
using Application.Exceptions;
using Application.Models.Ocr;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using Moq;
using DtoConfidenceLevel = global::API.Generated.Dtos.ConfidenceLevel;

namespace Presentation.API.Tests.Controllers.Core;

public class ReceiptScanControllerTests
{
	private readonly Mock<IMediator> _mediatorMock;
	private readonly ReceiptScanController _controller;

	public ReceiptScanControllerTests()
	{
		_mediatorMock = new Mock<IMediator>();
		Mock<ILogger<ReceiptScanController>> loggerMock = ControllerTestHelpers.GetLoggerMock<ReceiptScanController>();
		_controller = new ReceiptScanController(_mediatorMock.Object, loggerMock.Object);
	}

	[Fact]
	public async Task ScanReceipt_NullFile_ReturnsBadRequest()
	{
		// Act
		Results<Ok<ProposedReceiptResponse>, BadRequest<string>, StatusCodeHttpResult, UnprocessableEntity<string>> actual = await _controller.ScanReceipt(null);

		// Assert
		actual.Result.Should().BeOfType<BadRequest<string>>()
			.Which.Value.Should().Be("No file was uploaded.");
	}

	[Fact]
	public async Task ScanReceipt_EmptyFile_ReturnsBadRequest()
	{
		// Arrange
		Mock<IFormFile> fileMock = new();
		fileMock.Setup(f => f.Length).Returns(0);

		// Act
		Results<Ok<ProposedReceiptResponse>, BadRequest<string>, StatusCodeHttpResult, UnprocessableEntity<string>> actual = await _controller.ScanReceipt(fileMock.Object);

		// Assert
		actual.Result.Should().BeOfType<BadRequest<string>>()
			.Which.Value.Should().Be("No file was uploaded.");
	}

	[Fact]
	public async Task ScanReceipt_OversizedFile_ReturnsBadRequest()
	{
		// Arrange
		Mock<IFormFile> fileMock = new();
		fileMock.Setup(f => f.Length).Returns(21 * 1024 * 1024); // 21 MB
		fileMock.Setup(f => f.ContentType).Returns("image/jpeg");

		// Act
		Results<Ok<ProposedReceiptResponse>, BadRequest<string>, StatusCodeHttpResult, UnprocessableEntity<string>> actual = await _controller.ScanReceipt(fileMock.Object);

		// Assert
		actual.Result.Should().BeOfType<BadRequest<string>>()
			.Which.Value.Should().Contain("maximum allowed size");
	}

	[Fact]
	public async Task ScanReceipt_ValidPdf_ReturnsOkWithProposal()
	{
		// Arrange
		IFormFile file = CreateMockFormFile("receipt.pdf", "application/pdf", 4096);

		ParsedReceipt parsedReceipt = new(
			FieldConfidence<string>.High("COSTCO"),
			FieldConfidence<DateOnly>.Medium(new DateOnly(2026, 4, 10)),
			[],
			FieldConfidence<decimal>.Low(0m),
			[],
			FieldConfidence<decimal>.High(42.99m),
			FieldConfidence<string?>.None()
		);

		ScanReceiptResult scanResult = new(parsedReceipt);

		_mediatorMock
			.Setup(m => m.Send(It.IsAny<ScanReceiptCommand>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(scanResult);

		// Act
		Results<Ok<ProposedReceiptResponse>, BadRequest<string>, StatusCodeHttpResult, UnprocessableEntity<string>> actual = await _controller.ScanReceipt(file);

		// Assert
		actual.Result.Should().BeOfType<Ok<ProposedReceiptResponse>>();
	}

	[Fact]
	public async Task ScanReceipt_PdfContentType_SendsCorrectContentTypeToMediator()
	{
		// Arrange
		IFormFile file = CreateMockFormFile("receipt.pdf", "application/pdf", 2048);

		ParsedReceipt parsedReceipt = new(
			FieldConfidence<string>.Low("Unknown"),
			FieldConfidence<DateOnly>.Low(DateOnly.FromDateTime(DateTime.Today)),
			[],
			FieldConfidence<decimal>.Low(0m),
			[],
			FieldConfidence<decimal>.Low(0m),
			FieldConfidence<string?>.None()
		);

		_mediatorMock
			.Setup(m => m.Send(It.IsAny<ScanReceiptCommand>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(new ScanReceiptResult(parsedReceipt));

		// Act
		await _controller.ScanReceipt(file);

		// Assert
		_mediatorMock.Verify(m => m.Send(
			It.Is<ScanReceiptCommand>(c =>
				c.ContentType == "application/pdf" &&
				c.ImageBytes.Length == 2048),
			It.IsAny<CancellationToken>()),
			Times.Once);
	}

	[Theory]
	[InlineData("image/gif")]
	[InlineData("image/bmp")]
	[InlineData("image/tiff")]
	[InlineData("application/octet-stream")]
	[InlineData("text/plain")]
	[InlineData("image/heic")]
	public async Task ScanReceipt_UnsupportedContentType_Returns415(string contentType)
	{
		// Arrange
		Mock<IFormFile> fileMock = new();
		fileMock.Setup(f => f.Length).Returns(1024);
		fileMock.Setup(f => f.ContentType).Returns(contentType);

		// Act
		Results<Ok<ProposedReceiptResponse>, BadRequest<string>, StatusCodeHttpResult, UnprocessableEntity<string>> actual = await _controller.ScanReceipt(fileMock.Object);

		// Assert
		actual.Result.Should().BeOfType<StatusCodeHttpResult>()
			.Which.StatusCode.Should().Be(StatusCodes.Status415UnsupportedMediaType);
	}

	[Fact]
	public async Task ScanReceipt_OcrReturnsNoText_Returns422()
	{
		// Arrange
		IFormFile file = CreateMockFormFile("receipt.jpg", "image/jpeg", 1024);

		_mediatorMock
			.Setup(m => m.Send(It.IsAny<ScanReceiptCommand>(), It.IsAny<CancellationToken>()))
			.ThrowsAsync(new OcrNoTextException("OCR returned no readable text from the image."));

		// Act
		Results<Ok<ProposedReceiptResponse>, BadRequest<string>, StatusCodeHttpResult, UnprocessableEntity<string>> actual = await _controller.ScanReceipt(file);

		// Assert
		actual.Result.Should().BeOfType<UnprocessableEntity<string>>()
			.Which.Value.Should().Contain("could not be read");
	}

	[Fact]
	public async Task ScanReceipt_ProcessingFails_Returns422()
	{
		// Arrange
		IFormFile file = CreateMockFormFile("receipt.jpg", "image/jpeg", 1024);

		_mediatorMock
			.Setup(m => m.Send(It.IsAny<ScanReceiptCommand>(), It.IsAny<CancellationToken>()))
			.ThrowsAsync(new InvalidOperationException("The uploaded file is not a supported image format."));

		// Act
		Results<Ok<ProposedReceiptResponse>, BadRequest<string>, StatusCodeHttpResult, UnprocessableEntity<string>> actual = await _controller.ScanReceipt(file);

		// Assert
		actual.Result.Should().BeOfType<UnprocessableEntity<string>>()
			.Which.Value.Should().Contain("not a supported image format");
	}

	[Fact]
	public async Task ScanReceipt_ValidJpeg_ReturnsOkWithProposal()
	{
		// Arrange
		IFormFile file = CreateMockFormFile("receipt.jpg", "image/jpeg", 2048);

		ParsedReceipt parsedReceipt = new(
			FieldConfidence<string>.High("WALMART"),
			FieldConfidence<DateOnly>.Medium(new DateOnly(2026, 3, 15)),
			[
				new ParsedReceiptItem(
					FieldConfidence<string?>.None(),
					FieldConfidence<string>.High("MILK 2%"),
					FieldConfidence<decimal>.High(1m),
					FieldConfidence<decimal>.High(3.49m),
					FieldConfidence<decimal>.High(3.49m))
			],
			FieldConfidence<decimal>.Medium(3.49m),
			[
				new ParsedTaxLine(
					FieldConfidence<string>.Medium("TAX"),
					FieldConfidence<decimal>.High(0.25m))
			],
			FieldConfidence<decimal>.High(3.74m),
			FieldConfidence<string?>.Low("VISA")
		);

		ScanReceiptResult scanResult = new(parsedReceipt);

		_mediatorMock
			.Setup(m => m.Send(It.IsAny<ScanReceiptCommand>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(scanResult);

		// Act
		Results<Ok<ProposedReceiptResponse>, BadRequest<string>, StatusCodeHttpResult, UnprocessableEntity<string>> actual = await _controller.ScanReceipt(file);

		// Assert
		Ok<ProposedReceiptResponse> okResult = actual.Result.Should().BeOfType<Ok<ProposedReceiptResponse>>().Subject;
		ProposedReceiptResponse response = okResult.Value!;

		response.StoreName.Should().Be("WALMART");
		response.StoreNameConfidence.Should().Be(DtoConfidenceLevel.High);
		response.Date.Should().Be(new DateOnly(2026, 3, 15));
		response.DateConfidence.Should().Be(DtoConfidenceLevel.Medium);
		response.Items.Should().HaveCount(1);
		response.Items.First().Description.Should().Be("MILK 2%");
		response.Items.First().TotalPrice.Should().Be(3.49d);
		response.Subtotal.Should().Be(3.49d);
		response.TaxLines.Should().HaveCount(1);
		response.TaxLines.First().Label.Should().Be("TAX");
		response.TaxLines.First().Amount.Should().Be(0.25d);
		response.Total.Should().Be(3.74d);
		response.TotalConfidence.Should().Be(DtoConfidenceLevel.High);
		response.PaymentMethod.Should().Be("VISA");
	}

	[Fact]
	public async Task ScanReceipt_ValidPng_ReturnsOkWithProposal()
	{
		// Arrange
		IFormFile file = CreateMockFormFile("receipt.png", "image/png", 4096);

		ParsedReceipt parsedReceipt = new(
			FieldConfidence<string>.Medium("STORE"),
			FieldConfidence<DateOnly>.Low(DateOnly.FromDateTime(DateTime.Today)),
			[],
			FieldConfidence<decimal>.Low(0m),
			[],
			FieldConfidence<decimal>.Low(0m),
			FieldConfidence<string?>.None()
		);

		ScanReceiptResult scanResult = new(parsedReceipt);

		_mediatorMock
			.Setup(m => m.Send(It.IsAny<ScanReceiptCommand>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(scanResult);

		// Act
		Results<Ok<ProposedReceiptResponse>, BadRequest<string>, StatusCodeHttpResult, UnprocessableEntity<string>> actual = await _controller.ScanReceipt(file);

		// Assert
		actual.Result.Should().BeOfType<Ok<ProposedReceiptResponse>>();
	}

	[Fact]
	public async Task ScanReceipt_FileSizeExactlyAtLimit_IsAccepted()
	{
		// Arrange
		long exactLimit = 20 * 1024 * 1024;
		IFormFile file = CreateMockFormFile("receipt.jpg", "image/jpeg", (int)exactLimit);

		ParsedReceipt parsedReceipt = new(
			FieldConfidence<string>.Low("Unknown"),
			FieldConfidence<DateOnly>.Low(DateOnly.FromDateTime(DateTime.Today)),
			[],
			FieldConfidence<decimal>.Low(0m),
			[],
			FieldConfidence<decimal>.Low(0m),
			FieldConfidence<string?>.None()
		);

		_mediatorMock
			.Setup(m => m.Send(It.IsAny<ScanReceiptCommand>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(new ScanReceiptResult(parsedReceipt));

		// Act
		Results<Ok<ProposedReceiptResponse>, BadRequest<string>, StatusCodeHttpResult, UnprocessableEntity<string>> actual = await _controller.ScanReceipt(file);

		// Assert
		actual.Result.Should().BeOfType<Ok<ProposedReceiptResponse>>();
	}

	[Fact]
	public async Task ScanReceipt_SendsCorrectCommandToMediator()
	{
		// Arrange
		IFormFile file = CreateMockFormFile("scan.jpg", "image/jpeg", 512);

		ParsedReceipt parsedReceipt = new(
			FieldConfidence<string>.Low("Unknown"),
			FieldConfidence<DateOnly>.Low(DateOnly.FromDateTime(DateTime.Today)),
			[],
			FieldConfidence<decimal>.Low(0m),
			[],
			FieldConfidence<decimal>.Low(0m),
			FieldConfidence<string?>.None()
		);

		_mediatorMock
			.Setup(m => m.Send(It.IsAny<ScanReceiptCommand>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(new ScanReceiptResult(parsedReceipt));

		// Act
		await _controller.ScanReceipt(file);

		// Assert
		_mediatorMock.Verify(m => m.Send(
			It.Is<ScanReceiptCommand>(c =>
				c.ContentType == "image/jpeg" &&
				c.ImageBytes.Length == 512),
			It.IsAny<CancellationToken>()),
			Times.Once);
	}

	private static IFormFile CreateMockFormFile(string fileName, string contentType, int size)
	{
		Mock<IFormFile> fileMock = new();
		fileMock.Setup(f => f.Length).Returns(size);
		fileMock.Setup(f => f.ContentType).Returns(contentType);
		fileMock.Setup(f => f.FileName).Returns(fileName);

		byte[] content = new byte[size];
		fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
			.Callback<Stream, CancellationToken>((stream, _) => stream.Write(content, 0, content.Length))
			.Returns(Task.CompletedTask);

		return fileMock.Object;
	}
}
