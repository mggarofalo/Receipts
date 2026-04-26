using Application.Interfaces.Services;
using FluentAssertions;
using Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Moq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Core;
using UglyToad.PdfPig.Exceptions;
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
	public async Task ConvertAsync_PdfWithTextLayer_RasterizesFirstPageAndExtractsText()
	{
		// Arrange — PdfDocumentBuilder doesn't support newlines in AddText,
		// so we use a single line of sufficient length. This is the vector-only case:
		// no embedded raster images, only text glyphs as vector outlines. Before
		// RECEIPTS-624 this produced an empty PageImages list and /api/receipts/scan
		// returned 422. Now the first page is always rasterized to a PNG.
		byte[] pdfBytes = CreateTextPdf("WALMART MILK 2% $3.49 TOTAL $3.74");

		// Act
		PdfConversionResult result = await _service.ConvertAsync(pdfBytes, CancellationToken.None);

		// Assert
		result.ExtractedText.Should().NotBeNullOrWhiteSpace();
		result.ExtractedText.Should().Contain("WALMART");
		AssertContainsValidPng(result.PageImages);
	}

	[Fact]
	public async Task ConvertAsync_VectorOnlyPdf_ProducesRasterizedFirstPageImage()
	{
		// Arrange — acceptance criterion for RECEIPTS-624: vector-only PDFs (emailed POS
		// receipts, scanner-app exports) must succeed. We build a text-only PDF because
		// PdfDocumentBuilder cannot emit arbitrary vector paths, but the rasterization
		// code path is the same: PDFium renders glyph outlines to pixels regardless of
		// whether the page contains raster images.
		byte[] pdfBytes = CreateTextPdf("Vector PDF receipt content with more than ten characters of text");

		// Act
		PdfConversionResult result = await _service.ConvertAsync(pdfBytes, CancellationToken.None);

		// Assert
		result.PageImages.Should().ContainSingle(
			"rasterization always produces exactly one first-page image");
		AssertPngHasDimensions(result.PageImages[0], minWidth: 100, minHeight: 100);
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
	public async Task ConvertAsync_MultiPagePdf_RasterizesFirstPageOnly()
	{
		// Arrange — each page must have text >= MinTextLengthPerPage. The scan command
		// handler only consumes the first page image, so the converter only rasterizes
		// page 0 even when the PDF has multiple pages. Text from all pages is still
		// concatenated and returned as an informational text layer.
		byte[] pdfBytes = CreateMultiPageTextPdf(
		[
			"Page one content from WALMART store",
			"Page two content with MILK for $3.49"
		]);

		// Act
		PdfConversionResult result = await _service.ConvertAsync(pdfBytes, CancellationToken.None);

		// Assert
		result.PageImages.Should().ContainSingle(
			"only the first page is rasterized regardless of total page count");
		result.ExtractedText.Should().Contain("WALMART");
		result.ExtractedText.Should().Contain("MILK");
		AssertPngHasDimensions(result.PageImages[0], minWidth: 100, minHeight: 100);
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
		byte[] pdfBytes = CreateTextPdf("Some text that is sufficiently long to be indexed");
		CancellationTokenSource cts = new();
		await cts.CancelAsync();

		// Act
		Func<Task> act = () => _service.ConvertAsync(pdfBytes, cts.Token);

		// Assert
		await act.Should().ThrowAsync<OperationCanceledException>();
	}

	[Fact]
	public async Task ConvertAsync_PdfWithBelowThresholdText_StillRasterizes()
	{
		// Arrange — a PDF whose text layer is below MinTextLengthPerPage. Before
		// rasterization, this threw "no readable text" because the text was too short
		// and no embedded raster images existed. Now the rasterized first page is the
		// usable output regardless of the text layer length — the VLM will read pixels
		// directly.
		byte[] pdfBytes = CreateTextPdf("Hi");

		// Act
		PdfConversionResult result = await _service.ConvertAsync(pdfBytes, CancellationToken.None);

		// Assert
		result.PageImages.Should().ContainSingle();
		result.ExtractedText.Should().BeNull(
			"\"Hi\" is below the MinTextLengthPerPage threshold so the text layer is discarded");
		AssertPngHasDimensions(result.PageImages[0], minWidth: 100, minHeight: 100);
	}

	[Fact]
	public async Task ConvertAsync_PdfWithSufficientText_RasterizesAndReturnsText()
	{
		// Arrange — text must be >= MinTextLengthPerPage chars
		string text = new('A', PdfConversionService.MinTextLengthPerPage + 10);
		byte[] pdfBytes = CreateTextPdf(text);

		// Act
		PdfConversionResult result = await _service.ConvertAsync(pdfBytes, CancellationToken.None);

		// Assert
		result.ExtractedText.Should().NotBeNullOrWhiteSpace();
		result.PageImages.Should().ContainSingle();
	}

	[Fact]
	public async Task ConvertAsync_PdfWithTextLayerAndLargeImage_RasterizesFirstPage()
	{
		// Arrange — a Paperless-style PDF: text layer (OCR) + large embedded scan image.
		// Previously the embedded image was extracted directly; now the full rasterized
		// first page (which includes the embedded image as rendered on the page) is used.
		// The scan handler gets a PNG with at least the raster image's content plus any
		// surrounding text. This keeps Paperless-style flows working while unifying the
		// code path.
		byte[] pdfBytes = CreateTextPdfWithPng(
			text: "WALMART SUPERCENTER TOTAL $3.74",
			imageWidth: 600,
			imageHeight: 800);

		// Act
		PdfConversionResult result = await _service.ConvertAsync(pdfBytes, CancellationToken.None);

		// Assert — text layer is still extracted (informational), and the rasterized
		// first page is what the scan handler will use.
		result.PageImages.Should().ContainSingle();
		result.ExtractedText.Should().NotBeNullOrWhiteSpace();
		result.ExtractedText.Should().Contain("WALMART");
		AssertPngHasDimensions(result.PageImages[0], minWidth: 100, minHeight: 100);
	}

	[Fact]
	public async Task ConvertAsync_PdfWithSmallLogoAndText_StillRasterizes()
	{
		// Arrange — previously a text+small-logo PDF produced empty PageImages because
		// the logo was below the 400x400 embedded-image size filter. That caused the scan
		// handler to throw OcrNoTextException for emailed POS receipts with a tiny store
		// logo. Now the rasterized first page carries the whole document including the
		// logo and surrounding text, so the VLM can extract the receipt.
		byte[] pdfBytes = CreateTextPdfWithPng(
			text: "WALMART SUPERCENTER TOTAL $3.74",
			imageWidth: 64,
			imageHeight: 64);

		// Act
		PdfConversionResult result = await _service.ConvertAsync(pdfBytes, CancellationToken.None);

		// Assert
		result.PageImages.Should().ContainSingle();
		result.ExtractedText.Should().Contain("WALMART");
	}

	[Fact]
	public async Task ConvertAsync_OversizedPdf_ThrowsBeforeRasterization()
	{
		// Arrange — build a PDF with one more page than IPdfConversionService.MaxPages
		// allows. The converter must reject these cleanly before invoking PDFtoImage,
		// otherwise a hostile or accidental upload could spend rasterization budget on
		// work we reject anyway.
		string[] pages = Enumerable
			.Range(1, IPdfConversionService.MaxPages + 1)
			.Select(i => $"Page {i} content with enough text to satisfy the threshold")
			.ToArray();
		byte[] pdfBytes = CreateMultiPageTextPdf(pages);

		// Act
		Func<Task> act = () => _service.ConvertAsync(pdfBytes, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<InvalidOperationException>()
			.WithMessage("*exceeds the maximum*");
	}

	[Fact]
	public async Task ConvertAsync_EncryptedPdf_ThrowsPasswordProtectedInvalidOperationException()
	{
		// Arrange — RECEIPTS-629 acceptance: encrypted PDFs must be rejected with a
		// clear "Password-protected" error before the rasterizer is invoked. PdfPig's
		// PdfDocument.Open detects the trailer's /Encrypt entry and raises
		// PdfDocumentEncryptedException; the service translates that to the user-facing
		// InvalidOperationException via the typed predicate (no message-substring match).
		// The fixture is a hand-crafted minimal PDF whose trailer references a Standard
		// encryption dictionary (V=1, R=2). It opens enough to detect encryption but
		// exposes no real password — exactly the typed-exception path we need to verify.
		byte[] pdfBytes = CreateEncryptedPdf();

		// Act
		Func<Task> act = () => _service.ConvertAsync(pdfBytes, CancellationToken.None);

		// Assert
		FluentAssertions.Specialized.ExceptionAssertions<InvalidOperationException> thrown =
			await act.Should().ThrowAsync<InvalidOperationException>()
				.WithMessage("*Password-protected*");
		thrown.Which.InnerException.Should().BeOfType<PdfDocumentEncryptedException>(
			"the typed PdfPig encryption exception is the trigger that the service translates");
	}

	[Fact]
	public void IsPasswordProtectedException_PdfDocumentEncryptedException_ReturnsTrue()
	{
		// Arrange — the typed exception PdfPig raises when PdfDocument.Open hits an
		// encrypted document. The service helper should recognize it directly without
		// resorting to message-substring matching.
		PdfDocumentEncryptedException ex = new("Cannot read encrypted document");

		// Act
		bool result = PdfConversionService.IsPasswordProtectedException(ex);

		// Assert
		result.Should().BeTrue();
	}

	[Fact]
	public void IsPasswordProtectedException_WrappedPdfDocumentEncryptedException_ReturnsTrue()
	{
		// Arrange — defensive: if a higher-level layer wraps the typed exception, the
		// helper should still classify it as password-protected via InnerException.
		PdfDocumentEncryptedException inner = new("PDF is encrypted");
		InvalidOperationException wrapper = new("Wrapped failure", inner);

		// Act
		bool result = PdfConversionService.IsPasswordProtectedException(wrapper);

		// Assert
		result.Should().BeTrue();
	}

	[Fact]
	public void IsPasswordProtectedException_AnyPasswordMessageWithoutTypedException_ReturnsFalse()
	{
		// Arrange — the PdfPig path uses the typed check ONLY. A non-typed exception
		// whose message merely mentions "password" must not be classified at this site;
		// the rasterization path has its own broader predicate.
		Exception pdfiumException = new("This PDF requires a password to open.");

		// Act
		bool result = PdfConversionService.IsPasswordProtectedException(pdfiumException);

		// Assert
		result.Should().BeFalse(
			"the PdfPig-path predicate must rely solely on the typed exception");
	}

	[Fact]
	public void IsPasswordProtectedException_UnrelatedEncryptionError_ReturnsFalse()
	{
		// Arrange — the previous implementation used `message.Contains("encrypt")`,
		// which classified unrelated runtime errors (TLS, crypto subsystem) as
		// password-protected and produced a misleading user-facing error. The new
		// helper must NOT match these.
		Exception tlsException = new("Authentication failed: TLS handshake encryption error.");

		// Act
		bool result = PdfConversionService.IsPasswordProtectedException(tlsException);

		// Assert
		result.Should().BeFalse(
			"the new typed-exception check must not regress to substring matching on \"encrypt\"");
	}

	[Fact]
	public void IsPasswordProtectedException_GenericException_ReturnsFalse()
	{
		// Arrange — a plain exception with no encryption-related signal must not be
		// classified as password-protected.
		Exception genericException = new("Something else went wrong");

		// Act
		bool result = PdfConversionService.IsPasswordProtectedException(genericException);

		// Assert
		result.Should().BeFalse();
	}

	[Fact]
	public void IsPdfiumEncryptionException_PasswordInMessage_ReturnsTrue()
	{
		// Arrange — PDFium (used by PDFtoImage during rasterization) has no typed
		// equivalent and surfaces password failures only via the message string.
		// The rasterization-path predicate must classify these correctly.
		Exception pdfiumException = new("This PDF requires a password to open.");

		// Act
		bool result = PdfConversionService.IsPdfiumEncryptionException(pdfiumException);

		// Assert
		result.Should().BeTrue();
	}

	[Fact]
	public void IsPdfiumEncryptionException_EncryptInMessage_ReturnsTrue()
	{
		// Arrange — RECEIPTS-645 bug-finder follow-up: PDFium errors for content-stream
		// encryption failures may contain "encrypt" without the word "password" (e.g.,
		// "Cannot decode encrypted content stream"). The rasterization predicate must
		// catch this so users see the actionable "Password-protected PDFs are not
		// supported" message rather than a generic rasterization failure.
		Exception pdfiumException = new("Cannot decode encrypted content stream.");

		// Act
		bool result = PdfConversionService.IsPdfiumEncryptionException(pdfiumException);

		// Assert
		result.Should().BeTrue(
			"rasterization-path encryption errors without \"password\" must still classify as password-protected");
	}

	[Fact]
	public void IsPdfiumEncryptionException_TypedPdfPigException_ReturnsTrue()
	{
		// Arrange — the rasterization-path predicate is a superset of the PdfPig-path
		// predicate; the typed exception must still classify even if the surrounding
		// catch is the PDFium one (defensive for ordering or library changes).
		PdfDocumentEncryptedException ex = new("Cannot read encrypted document");

		// Act
		bool result = PdfConversionService.IsPdfiumEncryptionException(ex);

		// Assert
		result.Should().BeTrue();
	}

	[Fact]
	public void IsPdfiumEncryptionException_GenericRasterizationError_ReturnsFalse()
	{
		// Arrange — non-encryption rasterization failures must NOT be misclassified as
		// password-protected. The user should see the generic "Failed to rasterize"
		// error in those cases.
		Exception genericException = new("Failed to render page: invalid graphics state.");

		// Act
		bool result = PdfConversionService.IsPdfiumEncryptionException(genericException);

		// Assert
		result.Should().BeFalse();
	}

	private static byte[] CreateTextPdf(string text)
	{
		PdfDocumentBuilder builder = new();
		PdfPageBuilder page = builder.AddPage(PageSize.Letter);
		PdfDocumentBuilder.AddedFont font = builder.AddStandard14Font(Standard14Font.Helvetica);
		page.AddText(text, 12, new PdfPoint(72, 720), font);
		return builder.Build();
	}

	private static byte[] CreateTextPdfWithPng(string text, int imageWidth, int imageHeight)
	{
		PdfDocumentBuilder builder = new();
		PdfPageBuilder page = builder.AddPage(PageSize.Letter);
		PdfDocumentBuilder.AddedFont font = builder.AddStandard14Font(Standard14Font.Helvetica);
		page.AddText(text, 12, new PdfPoint(72, 720), font);

		byte[] pngBytes = CreateSolidPng(imageWidth, imageHeight);
		page.AddPng(pngBytes, new PdfRectangle(72, 200, 72 + imageWidth, 200 + imageHeight));

		return builder.Build();
	}

	private static byte[] CreateSolidPng(int width, int height)
	{
		using Image<Rgb24> image = new(width, height, new Rgb24(200, 200, 200));
		using MemoryStream ms = new();
		image.Save(ms, new PngEncoder());
		return ms.ToArray();
	}

	private static byte[] CreateTextPdfWithMetadata(string title, string text)
	{
		PdfDocumentBuilder builder = new();
		builder.DocumentInformation.Title = title;
		PdfPageBuilder page = builder.AddPage(PageSize.Letter);
		PdfDocumentBuilder.AddedFont font = builder.AddStandard14Font(Standard14Font.Helvetica);
		page.AddText(text, 12, new PdfPoint(72, 720), font);
		return builder.Build();
	}

	private static byte[] CreateMultiPageTextPdf(string[] pageTexts)
	{
		PdfDocumentBuilder builder = new();
		PdfDocumentBuilder.AddedFont font = builder.AddStandard14Font(Standard14Font.Helvetica);
		foreach (string text in pageTexts)
		{
			PdfPageBuilder page = builder.AddPage(PageSize.Letter);
			page.AddText(text, 12, new PdfPoint(72, 720), font);
		}
		return builder.Build();
	}

	private static byte[] CreateEmptyPdf()
	{
		PdfDocumentBuilder builder = new();
		return builder.Build();
	}

	/// <summary>
	/// Builds a minimal encrypted PDF byte stream by hand. PdfPig's writer
	/// (<see cref="PdfDocumentBuilder"/>) does not support emitting encrypted PDFs, so
	/// the only way to exercise the encrypted-document path end-to-end without shipping
	/// a binary fixture is to hand-craft the bytes.
	/// <para>
	/// The PDF below contains a Standard encryption dictionary (V=1, R=2, 40-bit RC4)
	/// referenced from the trailer's <c>/Encrypt</c> key. PdfPig's <c>PdfDocument.Open</c>
	/// detects the trailer entry and immediately raises
	/// <see cref="PdfDocumentEncryptedException"/> when no password is supplied — exactly
	/// the path the service-level translation must catch via the typed predicate.
	/// </para>
	/// <para>
	/// The xref byte offsets and <c>startxref</c> value below match the exact Latin-1
	/// byte positions of each object in the produced file. Hand-counted offsets are
	/// fragile, so future edits to the object literals must update these values too —
	/// otherwise PdfPig falls back to scanning recovery and the test passes for the
	/// wrong reason (or breaks on a PdfPig upgrade).
	/// </para>
	/// </summary>
	private static byte[] CreateEncryptedPdf()
	{
		// Byte offsets are exact Latin-1 positions of each object marker in the
		// concatenated string. Verified by counting the bytes — see RECEIPTS-629
		// PR #496 review trail for the calculation.
		const string Pdf =
			"%PDF-1.4\n" +                                                                            //   0..  8 (9 bytes)
			"%\u00e2\u00e3\u00cf\u00d3\n" +                                                           //   9.. 14 (6 bytes)
			"1 0 obj\n<</Type /Catalog /Pages 2 0 R>>\nendobj\n" +                                    //  15.. 61 (47 bytes)
			"2 0 obj\n<</Type /Pages /Kids [3 0 R] /Count 1>>\nendobj\n" +                            //  62..116 (55 bytes)
			"3 0 obj\n<</Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Contents 4 0 R>>\nendobj\n" + // 117..201 (85 bytes)
			"4 0 obj\n<</Length 0>>\nstream\nendstream\nendobj\n" +                                   // 202..247 (46 bytes)
			"5 0 obj\n<</Filter /Standard /V 1 /R 2 /P -4 /Length 40 " +
			"/O <0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF> " +
			"/U <FEDCBA9876543210FEDCBA9876543210FEDCBA9876543210FEDCBA9876543210>>>\nendobj\n" +     // 248..451 (204 bytes)
			"xref\n" +                                                                                // 452 — startxref target
			"0 6\n" +
			"0000000000 65535 f \n" +
			"0000000015 00000 n \n" +
			"0000000062 00000 n \n" +
			"0000000117 00000 n \n" +
			"0000000202 00000 n \n" +
			"0000000248 00000 n \n" +
			"trailer\n" +
			"<</Size 6 /Root 1 0 R /Encrypt 5 0 R " +
			"/ID [<00000000000000000000000000000000> <00000000000000000000000000000000>]>>\n" +
			"startxref\n" +
			"452\n" +
			"%%EOF\n";
		// ASCII-safe encoding — PDF's binary marker comment uses Latin-1 high bytes,
		// but the rest is plain ASCII. Use Latin-1 to preserve the marker bytes faithfully.
		return System.Text.Encoding.Latin1.GetBytes(Pdf);
	}

	private static void AssertContainsValidPng(IReadOnlyList<byte[]> images)
	{
		images.Should().NotBeEmpty();
		foreach (byte[] png in images)
		{
			png.Should().NotBeEmpty();
			png.Length.Should().BeGreaterThan(8);
			// PNG magic: 89 50 4E 47 0D 0A 1A 0A
			png[0].Should().Be(0x89);
			png[1].Should().Be(0x50);
			png[2].Should().Be(0x4E);
			png[3].Should().Be(0x47);
		}
	}

	private static void AssertPngHasDimensions(byte[] png, int minWidth, int minHeight)
	{
		using Image<Rgba32> img = Image.Load<Rgba32>(png);
		img.Width.Should().BeGreaterThanOrEqualTo(minWidth);
		img.Height.Should().BeGreaterThanOrEqualTo(minHeight);
	}
}
