using Application.Interfaces.Services;
using FluentAssertions;
using Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Moq;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Fonts.Standard14Fonts;
using UglyToad.PdfPig.Writer;

namespace Infrastructure.Tests.Services;

public class PdfConversionServiceTests
{
	private readonly PdfConversionService _service;

	public PdfConversionServiceTests()
	{
		Mock<ILogger<PdfConversionService>> mockLogger = new();
		_service = new PdfConversionService(mockLogger.Object);
	}

	[Fact]
	public async Task ConvertAsync_PdfWithTextLayer_ReturnsExtractedText()
	{
		// Arrange — PdfDocumentBuilder doesn't support newlines in AddText,
		// so we use a single line of sufficient length
		byte[] pdfBytes = CreateTextPdf("WALMART MILK 2% $3.49 TOTAL $3.74");

		// Act
		PdfConversionResult result = await _service.ConvertAsync(pdfBytes, CancellationToken.None);

		// Assert
		result.ExtractedText.Should().NotBeNullOrWhiteSpace();
		result.ExtractedText.Should().Contain("WALMART");
		result.PageImages.Should().BeEmpty();
	}

	[Fact]
	public async Task ConvertAsync_PdfWithTextLayer_ReturnsMetadata()
	{
		// Arrange
		byte[] pdfBytes = CreateTextPdfWithMetadata("Test Receipt", "Some text content here");

		// Act
		PdfConversionResult result = await _service.ConvertAsync(pdfBytes, CancellationToken.None);

		// Assert
		result.Metadata.Should().NotBeNull();
		result.Metadata!.Title.Should().Be("Test Receipt");
	}

	[Fact]
	public async Task ConvertAsync_MultiPagePdf_ConcatenatesText()
	{
		// Arrange — each page must have text >= MinTextLengthPerPage
		byte[] pdfBytes = CreateMultiPageTextPdf(
		[
			"Page one content from WALMART store",
			"Page two content with MILK for $3.49"
		]);

		// Act
		PdfConversionResult result = await _service.ConvertAsync(pdfBytes, CancellationToken.None);

		// Assert
		result.ExtractedText.Should().Contain("WALMART");
		result.ExtractedText.Should().Contain("MILK");
	}

	[Fact]
	public async Task ConvertAsync_InvalidPdfBytes_ThrowsInvalidOperationException()
	{
		// Arrange
		byte[] invalidBytes = [0x00, 0x01, 0x02, 0x03];

		// Act
		Func<Task> act = () => _service.ConvertAsync(invalidBytes, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<InvalidOperationException>()
			.WithMessage("*not a valid PDF*");
	}

	[Fact]
	public async Task ConvertAsync_BytesWithoutPdfMagicHeader_ThrowsInvalidOperationException()
	{
		// Arrange — a PNG header masquerading as a PDF upload
		byte[] pngHeader = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];

		// Act
		Func<Task> act = () => _service.ConvertAsync(pngHeader, CancellationToken.None);

		// Assert — rejected before PdfDocument.Open is called
		await act.Should().ThrowAsync<InvalidOperationException>()
			.WithMessage("*not a valid PDF*");
	}

	[Fact]
	public async Task ConvertAsync_EmptyByteArray_ThrowsInvalidOperationException()
	{
		// Arrange
		byte[] emptyBytes = [];

		// Act
		Func<Task> act = () => _service.ConvertAsync(emptyBytes, CancellationToken.None);

		// Assert — too short for magic byte check
		await act.Should().ThrowAsync<InvalidOperationException>()
			.WithMessage("*not a valid PDF*");
	}

	[Fact]
	public async Task ConvertAsync_EmptyPdf_ThrowsInvalidOperationException()
	{
		// Arrange
		byte[] pdfBytes = CreateEmptyPdf();

		// Act
		Func<Task> act = () => _service.ConvertAsync(pdfBytes, CancellationToken.None);

		// Assert — empty PDFs from PdfDocumentBuilder have no pages,
		// but the builder always creates at least the structure
		await act.Should().ThrowAsync<InvalidOperationException>();
	}

	[Fact]
	public async Task ConvertAsync_CancellationRequested_ThrowsOperationCanceledException()
	{
		// Arrange
		byte[] pdfBytes = CreateTextPdf("Some text");
		CancellationTokenSource cts = new();
		await cts.CancelAsync();

		// Act
		Func<Task> act = () => _service.ConvertAsync(pdfBytes, cts.Token);

		// Assert
		await act.Should().ThrowAsync<OperationCanceledException>();
	}

	[Fact]
	public async Task ConvertAsync_PdfWithNoTextAndNoImages_ThrowsInvalidOperationException()
	{
		// Arrange — create a PDF with a page that has very little text (below threshold)
		byte[] pdfBytes = CreateTextPdf("Hi");

		// Act
		Func<Task> act = () => _service.ConvertAsync(pdfBytes, CancellationToken.None);

		// Assert — "Hi" is only 2 chars, below MinTextLengthPerPage threshold
		await act.Should().ThrowAsync<InvalidOperationException>()
			.WithMessage("*no readable text*");
	}

	[Fact]
	public async Task ConvertAsync_MixedContentPdf_ReturnsTextFromTextPages()
	{
		// Arrange — page 1 has sufficient text, page 2 has very little (below threshold)
		// This tests the fix for BUG-001: text from text-layer pages should not be
		// discarded just because other pages lack a text layer.
		byte[] pdfBytes = CreateMultiPageTextPdf(
		[
			"Page one has a valid receipt text WALMART MILK $3.49 TOTAL $3.74",
			"x" // Below MinTextLengthPerPage threshold
		]);

		// Act
		PdfConversionResult result = await _service.ConvertAsync(pdfBytes, CancellationToken.None);

		// Assert — should return the text from page 1, not throw or return images
		result.ExtractedText.Should().NotBeNullOrWhiteSpace();
		result.ExtractedText.Should().Contain("WALMART");
		result.PageImages.Should().BeEmpty();
	}

	[Fact]
	public async Task ConvertAsync_PdfWithSufficientText_ReturnsTextDirectly()
	{
		// Arrange — text must be >= MinTextLengthPerPage chars
		string text = new('A', PdfConversionService.MinTextLengthPerPage + 10);
		byte[] pdfBytes = CreateTextPdf(text);

		// Act
		PdfConversionResult result = await _service.ConvertAsync(pdfBytes, CancellationToken.None);

		// Assert
		result.ExtractedText.Should().NotBeNullOrWhiteSpace();
		result.PageImages.Should().BeEmpty();
	}

	private static byte[] CreateTextPdf(string text)
	{
		PdfDocumentBuilder builder = new();
		PdfPageBuilder page = builder.AddPage(PageSize.Letter);
		PdfDocumentBuilder.AddedFont font = builder.AddStandard14Font(Standard14Font.Helvetica);
		page.AddText(text, 12, new UglyToad.PdfPig.Core.PdfPoint(72, 720), font);
		return builder.Build();
	}

	private static byte[] CreateTextPdfWithMetadata(string title, string text)
	{
		PdfDocumentBuilder builder = new();
		builder.DocumentInformation.Title = title;
		PdfPageBuilder page = builder.AddPage(PageSize.Letter);
		PdfDocumentBuilder.AddedFont font = builder.AddStandard14Font(Standard14Font.Helvetica);
		page.AddText(text, 12, new UglyToad.PdfPig.Core.PdfPoint(72, 720), font);
		return builder.Build();
	}

	private static byte[] CreateMultiPageTextPdf(string[] pageTexts)
	{
		PdfDocumentBuilder builder = new();
		PdfDocumentBuilder.AddedFont font = builder.AddStandard14Font(Standard14Font.Helvetica);
		foreach (string text in pageTexts)
		{
			PdfPageBuilder page = builder.AddPage(PageSize.Letter);
			page.AddText(text, 12, new UglyToad.PdfPig.Core.PdfPoint(72, 720), font);
		}
		return builder.Build();
	}

	private static byte[] CreateEmptyPdf()
	{
		PdfDocumentBuilder builder = new();
		return builder.Build();
	}
}
