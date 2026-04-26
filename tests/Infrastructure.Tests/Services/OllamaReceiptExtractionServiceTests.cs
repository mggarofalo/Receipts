using System.Net;
using System.Text.Json;
using Application.Interfaces.Services;
using Application.Models.Ocr;
using Common;
using FluentAssertions;
using FluentAssertions.Specialized;
using Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Protected;
using Polly;

namespace Infrastructure.Tests.Services;

public class OllamaReceiptExtractionServiceTests
{
	private static readonly byte[] FakeImage = [0x89, 0x50, 0x4E, 0x47];

	private static OllamaReceiptExtractionService CreateService(
		HttpMessageHandler handler,
		VlmOcrOptions? options = null)
	{
		HttpClient httpClient = new(handler) { BaseAddress = new Uri("http://test-ollama/") };
		options ??= new VlmOcrOptions
		{
			OllamaUrl = "http://test-ollama",
			Model = "glm-ocr:q8_0",
			TimeoutSeconds = 30,
		};
		return new OllamaReceiptExtractionService(
			httpClient,
			options,
			NullLogger<OllamaReceiptExtractionService>.Instance);
	}

	private static HttpMessageHandler CreateHandler(string responseBody, HttpStatusCode statusCode = HttpStatusCode.OK)
	{
		Mock<HttpMessageHandler> handlerMock = new();
		handlerMock.Protected()
			.Setup<Task<HttpResponseMessage>>("SendAsync",
				ItExpr.IsAny<HttpRequestMessage>(),
				ItExpr.IsAny<CancellationToken>())
			.ReturnsAsync(new HttpResponseMessage
			{
				StatusCode = statusCode,
				Content = new StringContent(responseBody, System.Text.Encoding.UTF8, "application/json"),
			});
		return handlerMock.Object;
	}

	private static string WrapInOllamaEnvelope(string innerJson)
	{
		return JsonSerializer.Serialize(new
		{
			model = "glm-ocr:q8_0",
			response = innerJson,
			done = true,
		});
	}

	/// <summary>
	/// Runs <paramref name="action"/> against an <see cref="IReceiptExtractionService"/>
	/// resolved from a DI container configured exactly as the production code does
	/// (<see cref="InfrastructureService.RegisterReceiptExtractionService"/>), including
	/// the per-attempt Polly Timeout strategy. The container is disposed after the action
	/// completes so the underlying <see cref="IHttpClientFactory"/> and any other
	/// <see cref="IDisposable"/> singletons do not leak across tests.
	/// </summary>
	private static async Task RunWithPipelineServiceAsync(
		HttpMessageHandler primaryHandler,
		VlmOcrOptions options,
		Func<IReceiptExtractionService, Task> action)
	{
		ServiceCollection services = new();
		services.AddLogging();
		services.AddSingleton(options);
#pragma warning disable EXTEXP0001
		services.AddHttpClient<IReceiptExtractionService, OllamaReceiptExtractionService>(client =>
		{
			client.BaseAddress = new Uri(options.OllamaUrl!.TrimEnd('/') + "/");
			client.Timeout = Timeout.InfiniteTimeSpan;
		})
		.ConfigurePrimaryHttpMessageHandler(() => primaryHandler)
		.RemoveAllResilienceHandlers()
		.AddResilienceHandler("vlm-ocr-test", builder =>
			InfrastructureService.ConfigureVlmOcrResilience(builder, options));
#pragma warning restore EXTEXP0001

		await using ServiceProvider sp = services.BuildServiceProvider();
		IReceiptExtractionService service = sp.GetRequiredService<IReceiptExtractionService>();
		await action(service);
	}

	[Fact]
	public async Task ExtractAsync_HappyPath_ReturnsAllFieldsWithHighConfidence()
	{
		// Arrange — exercises the full V2 shape: nested store, datetime, lineTotal,
		// nullable quantity/unitPrice (GRANULATED has neither printed), taxCode,
		// payments array, receipt/store/terminal identifiers.
		string innerJson = """
			{
			  "store": {
			    "name": "Walmart Supercenter",
			    "address": "9 BENTON RD, TRAVELERS REST SC 29690",
			    "phone": "864-834-7179"
			  },
			  "datetime": "2026-01-14T17:57:20",
			  "items": [
			    {
			      "description": "GRANULATED",
			      "code": "078742228030",
			      "lineTotal": 3.07,
			      "quantity": null,
			      "unitPrice": null,
			      "taxCode": "F"
			    },
			    {
			      "description": "BANANAS",
			      "code": "000000004011",
			      "lineTotal": 1.23,
			      "quantity": 2.46,
			      "unitPrice": 0.50,
			      "taxCode": "N"
			    }
			  ],
			  "subtotal": 69.68,
			  "taxLines": [{ "label": "TAX1 6.0000%", "amount": 0.75 }],
			  "total": 70.43,
			  "payments": [
			    { "method": "MASTERCARD", "amount": 70.43, "lastFour": "3409" }
			  ],
			  "receiptId": "7QKKG1XDWPD",
			  "storeNumber": "05487",
			  "terminalId": "54731105"
			}
			""";
		OllamaReceiptExtractionService service = CreateService(CreateHandler(WrapInOllamaEnvelope(innerJson)));

		// Act
		ParsedReceipt receipt = await service.ExtractAsync(FakeImage, "image/png", CancellationToken.None);

		// Assert
		receipt.StoreName.Value.Should().Be("Walmart Supercenter");
		receipt.StoreName.Confidence.Should().Be(ConfidenceLevel.High);
		receipt.Date.Value.Should().Be(new DateOnly(2026, 1, 14));
		receipt.Date.Confidence.Should().Be(ConfidenceLevel.High);

		receipt.StoreAddress.Value.Should().Be("9 BENTON RD, TRAVELERS REST SC 29690");
		receipt.StoreAddress.Confidence.Should().Be(ConfidenceLevel.High);
		receipt.StorePhone.Value.Should().Be("864-834-7179");
		receipt.StorePhone.Confidence.Should().Be(ConfidenceLevel.High);

		receipt.Items.Should().HaveCount(2);
		receipt.Items[0].Code.Value.Should().Be("078742228030");
		receipt.Items[0].Description.Value.Should().Be("GRANULATED");
		receipt.Items[0].Quantity.Confidence.Should().Be(ConfidenceLevel.None);
		receipt.Items[0].UnitPrice.Confidence.Should().Be(ConfidenceLevel.None);
		receipt.Items[0].TotalPrice.Value.Should().Be(3.07m);
		receipt.Items[0].TaxCode.Value.Should().Be("F");
		receipt.Items[0].TaxCode.Confidence.Should().Be(ConfidenceLevel.High);

		receipt.Items[1].Quantity.Value.Should().Be(2.46m);
		receipt.Items[1].UnitPrice.Value.Should().Be(0.50m);
		receipt.Items[1].TotalPrice.Value.Should().Be(1.23m);
		receipt.Items[1].TaxCode.Value.Should().Be("N");

		receipt.Subtotal.Value.Should().Be(69.68m);
		receipt.TaxLines.Should().HaveCount(1);
		receipt.TaxLines[0].Label.Value.Should().Be("TAX1 6.0000%");
		receipt.TaxLines[0].Amount.Value.Should().Be(0.75m);
		receipt.Total.Value.Should().Be(70.43m);
		receipt.PaymentMethod.Value.Should().Be("MASTERCARD");
		receipt.PaymentMethod.Confidence.Should().Be(ConfidenceLevel.High);

		receipt.Payments.Should().HaveCount(1);
		receipt.Payments[0].Method.Value.Should().Be("MASTERCARD");
		receipt.Payments[0].Amount.Value.Should().Be(70.43m);
		receipt.Payments[0].LastFour.Value.Should().Be("3409");
		receipt.Payments[0].LastFour.Confidence.Should().Be(ConfidenceLevel.High);

		receipt.ReceiptId.Value.Should().Be("7QKKG1XDWPD");
		receipt.ReceiptId.Confidence.Should().Be(ConfidenceLevel.High);
		receipt.StoreNumber.Value.Should().Be("05487");
		receipt.StoreNumber.Confidence.Should().Be(ConfidenceLevel.High);
		receipt.TerminalId.Value.Should().Be("54731105");
		receipt.TerminalId.Confidence.Should().Be(ConfidenceLevel.High);
	}

	[Theory]
	[InlineData("2026-04-01", 2026, 4, 1)]
	[InlineData("2026-01-14T17:57:20", 2026, 1, 14)]
	[InlineData("01/14/26 17:57:20", 2026, 1, 14)]
	[InlineData("01/14/26", 2026, 1, 14)]
	[InlineData("1/14/2026", 2026, 1, 14)]
	[InlineData("01/14/2026", 2026, 1, 14)]
	[InlineData("2026/04/01", 2026, 4, 1)]
	public async Task ExtractAsync_NonIsoDateFormats_ParsedWithHighConfidence(
		string dateString, int expectedYear, int expectedMonth, int expectedDay)
	{
		// Arrange — the VLM often returns dates in the format printed on the receipt
		// (e.g. "01/14/26") despite being asked for ISO-8601. We parse leniently.
		string innerJson = $$"""
			{
			  "store": { "name": "Walmart" },
			  "datetime": "{{dateString}}",
			  "total": 10.00
			}
			""";
		OllamaReceiptExtractionService service = CreateService(CreateHandler(WrapInOllamaEnvelope(innerJson)));

		// Act
		ParsedReceipt receipt = await service.ExtractAsync(FakeImage, "image/png", CancellationToken.None);

		// Assert
		receipt.Date.Value.Should().Be(new DateOnly(expectedYear, expectedMonth, expectedDay));
		receipt.Date.Confidence.Should().Be(ConfidenceLevel.High);
	}

	[Fact]
	public async Task ExtractAsync_UnparseableDate_YieldsNoneConfidenceWithoutThrowing()
	{
		// Arrange — a bad date string should not tank the entire extraction
		string innerJson = """
			{
			  "store": { "name": "Walmart" },
			  "datetime": "sometime last week",
			  "total": 10.00
			}
			""";
		OllamaReceiptExtractionService service = CreateService(CreateHandler(WrapInOllamaEnvelope(innerJson)));

		// Act
		ParsedReceipt receipt = await service.ExtractAsync(FakeImage, "image/png", CancellationToken.None);

		// Assert
		receipt.Date.Value.Should().Be(default(DateOnly));
		receipt.Date.Confidence.Should().Be(ConfidenceLevel.None);
		receipt.StoreName.Value.Should().Be("Walmart");
		receipt.Total.Value.Should().Be(10.00m);
	}

	[Fact]
	public async Task ExtractAsync_WeightSubline_MergesIntoParent()
	{
		// Arrange — reproduces the VLM's output shape for a weighted item: parent row with
		// null quantity + null unitPrice, followed by a separate row whose description holds
		// the "X lb. @ $Y" pattern and carries the actual quantity/unitPrice.
		string innerJson = """
			{
			  "store": { "name": "Walmart" },
			  "items": [
			    { "description": "BANANAS", "code": "000000004011", "lineTotal": 1.23,
			      "quantity": null, "unitPrice": null, "taxCode": "N" },
			    { "description": "2.460 lb. @ 1 lb. /0.50", "code": null, "lineTotal": 1.23,
			      "quantity": 2.460, "unitPrice": 0.50, "taxCode": "N" }
			  ],
			  "total": 1.23
			}
			""";
		OllamaReceiptExtractionService service = CreateService(CreateHandler(WrapInOllamaEnvelope(innerJson)));

		// Act
		ParsedReceipt receipt = await service.ExtractAsync(FakeImage, "image/png", CancellationToken.None);

		// Assert — sub-line is absorbed; only the parent remains with the weight data
		receipt.Items.Should().HaveCount(1);
		receipt.Items[0].Description.Value.Should().Be("BANANAS");
		receipt.Items[0].Code.Value.Should().Be("000000004011");
		receipt.Items[0].TotalPrice.Value.Should().Be(1.23m);
		receipt.Items[0].Quantity.Value.Should().Be(2.460m);
		receipt.Items[0].UnitPrice.Value.Should().Be(0.50m);
	}

	[Fact]
	public async Task ExtractAsync_WeightSublineWithoutMatchingParent_PreservedAsItem()
	{
		// Arrange — defensive: if the parent's lineTotal doesn't match, we don't merge.
		// This keeps us from accidentally corrupting an unrelated prior item.
		string innerJson = """
			{
			  "store": { "name": "Walmart" },
			  "items": [
			    { "description": "BREAD", "code": "072250049190", "lineTotal": 3.76,
			      "quantity": null, "unitPrice": null, "taxCode": "N" },
			    { "description": "2.460 lb. @ 1 lb. /0.50", "code": null, "lineTotal": 1.23,
			      "quantity": 2.460, "unitPrice": 0.50, "taxCode": "N" }
			  ]
			}
			""";
		OllamaReceiptExtractionService service = CreateService(CreateHandler(WrapInOllamaEnvelope(innerJson)));

		// Act
		ParsedReceipt receipt = await service.ExtractAsync(FakeImage, "image/png", CancellationToken.None);

		// Assert — both rows kept; BREAD is untouched
		receipt.Items.Should().HaveCount(2);
		receipt.Items[0].Description.Value.Should().Be("BREAD");
		receipt.Items[0].Quantity.Confidence.Should().Be(ConfidenceLevel.None);
		receipt.Items[1].Description.Value.Should().Be("2.460 lb. @ 1 lb. /0.50");
		receipt.Items[1].Quantity.Value.Should().Be(2.460m);
	}

	[Fact]
	public async Task ExtractAsync_ParentAlreadyHasQuantity_DoesNotOverride()
	{
		// Arrange — if the VLM (or a future model) already populated the parent correctly,
		// the "sub-line" shouldn't clobber it. Treat it as a distinct item instead.
		string innerJson = """
			{
			  "items": [
			    { "description": "BANANAS", "code": "000000004011", "lineTotal": 1.23,
			      "quantity": 2.460, "unitPrice": 0.50, "taxCode": "N" },
			    { "description": "2.460 lb. @ 1 lb. /0.50", "code": null, "lineTotal": 1.23,
			      "quantity": 2.460, "unitPrice": 0.50, "taxCode": "N" }
			  ]
			}
			""";
		OllamaReceiptExtractionService service = CreateService(CreateHandler(WrapInOllamaEnvelope(innerJson)));

		// Act
		ParsedReceipt receipt = await service.ExtractAsync(FakeImage, "image/png", CancellationToken.None);

		// Assert — parent is preserved; second "item" is kept as a distinct row
		receipt.Items.Should().HaveCount(2);
		receipt.Items[0].Quantity.Value.Should().Be(2.460m);
	}

	[Fact]
	public void MergeWeightSublines_MultipleWeightedItems_MergesEach()
	{
		// Arrange — real Walmart case: two bananas weighed at different prices
		List<VlmReceiptItem> items =
		[
			new() { Description = "BANANAS", Code = "000000004011", LineTotal = 1.23m, Quantity = null, UnitPrice = null, TaxCode = "N" },
			new() { Description = "2.460 lb. @ 1 lb. /0.50", Code = null, LineTotal = 1.23m, Quantity = 2.460m, UnitPrice = 0.50m, TaxCode = "N" },
			new() { Description = "BANANAS", Code = "000000004011", LineTotal = 1.36m, Quantity = null, UnitPrice = null, TaxCode = "N" },
			new() { Description = "2.720 lb. @ 1 lb. /0.50", Code = null, LineTotal = 1.36m, Quantity = 2.720m, UnitPrice = 0.50m, TaxCode = "N" },
		];

		// Act
		List<VlmReceiptItem> merged = OllamaReceiptExtractionService.MergeWeightSublines(items);

		// Assert
		merged.Should().HaveCount(2);
		merged[0].Description.Should().Be("BANANAS");
		merged[0].Quantity.Should().Be(2.460m);
		merged[0].UnitPrice.Should().Be(0.50m);
		merged[0].TaxCode.Should().Be("N");
		merged[1].Description.Should().Be("BANANAS");
		merged[1].Quantity.Should().Be(2.720m);
		merged[1].UnitPrice.Should().Be(0.50m);
		merged[1].TaxCode.Should().Be("N");
	}

	[Fact]
	public void MergeWeightSublines_ParentMissingTaxCode_AbsorbsFromSubline()
	{
		// Arrange — defensive: if the VLM echoes taxCode on the sub-line but not on the
		// parent, the merge must preserve it. Without this, the merged item drops the code
		// entirely now that MapItem reads ParsedReceiptItem.TaxCode.
		List<VlmReceiptItem> items =
		[
			new() { Description = "BANANAS", Code = "000000004011", LineTotal = 1.23m, Quantity = null, UnitPrice = null, TaxCode = null },
			new() { Description = "2.460 lb. @ 1 lb. /0.50", Code = null, LineTotal = 1.23m, Quantity = 2.460m, UnitPrice = 0.50m, TaxCode = "N" },
		];

		// Act
		List<VlmReceiptItem> merged = OllamaReceiptExtractionService.MergeWeightSublines(items);

		// Assert — sub-line is absorbed; parent inherits the tax code
		merged.Should().HaveCount(1);
		merged[0].TaxCode.Should().Be("N");
	}

	[Fact]
	public void MergeWeightSublines_ParentHasTaxCode_WinsOverSubline()
	{
		// Arrange — parent populated, sub-line disagrees. Parent wins because the tax-code
		// marker sits next to the parent line on the physical receipt.
		List<VlmReceiptItem> items =
		[
			new() { Description = "BANANAS", Code = "000000004011", LineTotal = 1.23m, Quantity = null, UnitPrice = null, TaxCode = "N" },
			new() { Description = "2.460 lb. @ 1 lb. /0.50", Code = null, LineTotal = 1.23m, Quantity = 2.460m, UnitPrice = 0.50m, TaxCode = "T" },
		];

		// Act
		List<VlmReceiptItem> merged = OllamaReceiptExtractionService.MergeWeightSublines(items);

		// Assert — parent's taxCode is preserved
		merged.Should().HaveCount(1);
		merged[0].TaxCode.Should().Be("N");
	}

	[Fact]
	public async Task ExtractAsync_MissingOptionalFields_ReturnsNoneConfidence()
	{
		// Arrange — no payments, no taxLines, items with unitPrice/quantity omitted (preferred per V2 prompt)
		string innerJson = """
			{
			  "store": { "name": "Walmart" },
			  "datetime": "2026-04-01",
			  "items": [],
			  "subtotal": 3.99,
			  "total": 4.29
			}
			""";
		OllamaReceiptExtractionService service = CreateService(CreateHandler(WrapInOllamaEnvelope(innerJson)));

		// Act
		ParsedReceipt receipt = await service.ExtractAsync(FakeImage, "image/png", CancellationToken.None);

		// Assert
		receipt.PaymentMethod.Value.Should().BeNull();
		receipt.PaymentMethod.Confidence.Should().Be(ConfidenceLevel.None);
		receipt.TaxLines.Should().BeEmpty();
		receipt.Items.Should().BeEmpty();
		receipt.StoreName.Confidence.Should().Be(ConfidenceLevel.High);
	}

	[Fact]
	public async Task ExtractAsync_MultiPayment_PreservesAllPayments()
	{
		// Arrange — split tender (gift card + card). The legacy PaymentMethod field picks
		// the first non-empty method for backward compatibility, but the Payments list must
		// carry every tender with its amount and last-four for downstream reconciliation.
		string innerJson = """
			{
			  "store": { "name": "Walmart" },
			  "total": 40.00,
			  "payments": [
			    { "method": "GIFT CARD", "amount": 10.00, "lastFour": null },
			    { "method": "VISA", "amount": 30.00, "lastFour": "1234" }
			  ]
			}
			""";
		OllamaReceiptExtractionService service = CreateService(CreateHandler(WrapInOllamaEnvelope(innerJson)));

		// Act
		ParsedReceipt receipt = await service.ExtractAsync(FakeImage, "image/png", CancellationToken.None);

		// Assert — legacy field keeps the first method
		receipt.PaymentMethod.Value.Should().Be("GIFT CARD");
		receipt.PaymentMethod.Confidence.Should().Be(ConfidenceLevel.High);

		// Assert — full Payments list is preserved in order
		receipt.Payments.Should().HaveCount(2);
		receipt.Payments[0].Method.Value.Should().Be("GIFT CARD");
		receipt.Payments[0].Amount.Value.Should().Be(10.00m);
		receipt.Payments[0].Amount.Confidence.Should().Be(ConfidenceLevel.High);
		receipt.Payments[0].LastFour.Value.Should().BeNull();
		receipt.Payments[0].LastFour.Confidence.Should().Be(ConfidenceLevel.None);
		receipt.Payments[1].Method.Value.Should().Be("VISA");
		receipt.Payments[1].Amount.Value.Should().Be(30.00m);
		receipt.Payments[1].LastFour.Value.Should().Be("1234");
		receipt.Payments[1].LastFour.Confidence.Should().Be(ConfidenceLevel.High);
	}

	[Fact]
	public async Task ExtractAsync_MissingStoreObject_YieldsNoneConfidenceStoreName()
	{
		// Arrange — the entire store object may be omitted on a hard-to-read receipt
		string innerJson = """
			{
			  "datetime": "2026-04-01",
			  "total": 10.00
			}
			""";
		OllamaReceiptExtractionService service = CreateService(CreateHandler(WrapInOllamaEnvelope(innerJson)));

		// Act
		ParsedReceipt receipt = await service.ExtractAsync(FakeImage, "image/png", CancellationToken.None);

		// Assert
		receipt.StoreName.Confidence.Should().Be(ConfidenceLevel.None);
		receipt.StoreAddress.Confidence.Should().Be(ConfidenceLevel.None);
		receipt.StoreAddress.Value.Should().BeNull();
		receipt.StorePhone.Confidence.Should().Be(ConfidenceLevel.None);
		receipt.StorePhone.Value.Should().BeNull();
		receipt.Date.Value.Should().Be(new DateOnly(2026, 4, 1));
		receipt.Total.Value.Should().Be(10.00m);
	}

	[Fact]
	public async Task ExtractAsync_MissingIdentifiersAndPayments_YieldNoneConfidenceAndEmptyList()
	{
		// Arrange — receiptId/storeNumber/terminalId are all commonly missing on smaller
		// independent-store receipts, and a receipt with no payments block should yield an
		// empty Payments list rather than throwing.
		string innerJson = """
			{
			  "store": { "name": "Corner Market" },
			  "datetime": "2026-04-01",
			  "total": 3.99
			}
			""";
		OllamaReceiptExtractionService service = CreateService(CreateHandler(WrapInOllamaEnvelope(innerJson)));

		// Act
		ParsedReceipt receipt = await service.ExtractAsync(FakeImage, "image/png", CancellationToken.None);

		// Assert
		receipt.ReceiptId.Value.Should().BeNull();
		receipt.ReceiptId.Confidence.Should().Be(ConfidenceLevel.None);
		receipt.StoreNumber.Value.Should().BeNull();
		receipt.StoreNumber.Confidence.Should().Be(ConfidenceLevel.None);
		receipt.TerminalId.Value.Should().BeNull();
		receipt.TerminalId.Confidence.Should().Be(ConfidenceLevel.None);
		receipt.Payments.Should().BeEmpty();
	}

	[Fact]
	public async Task ExtractAsync_HallucinatedLastFour_RejectedWithLowConfidence()
	{
		// Arrange — RECEIPTS-627: qwen2.5vl:3b sometimes lifts the APPR# (a 6+ digit reference
		// number printed elsewhere on the receipt) into lastFour instead of the true 4-digit
		// card tail. Post-processing rejects anything that does not match ^\d{4}$ so the UI
		// never surfaces a hallucinated value with high confidence.
		string innerJson = """
			{
			  "store": { "name": "Walmart" },
			  "total": 70.43,
			  "payments": [
			    { "method": "MCARD", "amount": 70.43, "lastFour": "014042" }
			  ]
			}
			""";
		OllamaReceiptExtractionService service = CreateService(CreateHandler(WrapInOllamaEnvelope(innerJson)));

		// Act
		ParsedReceipt receipt = await service.ExtractAsync(FakeImage, "image/png", CancellationToken.None);

		// Assert — value is nulled, confidence drops to Low. Other payment fields untouched.
		receipt.Payments.Should().HaveCount(1);
		receipt.Payments[0].LastFour.Value.Should().BeNull();
		receipt.Payments[0].LastFour.Confidence.Should().Be(ConfidenceLevel.Low);
		receipt.Payments[0].Method.Value.Should().Be("MCARD");
		receipt.Payments[0].Method.Confidence.Should().Be(ConfidenceLevel.High);
		receipt.Payments[0].Amount.Value.Should().Be(70.43m);
	}

	[Theory]
	// Hallucinated 6-digit value (the original RECEIPTS-627 repro)
	[InlineData("014042")]
	// Five digits — partial trim of an APPR#
	[InlineData("12345")]
	// Three digits — too short, not a valid full card tail
	[InlineData("340")]
	// Letters mixed in — OCR substituted alphanumerics
	[InlineData("34O9")]
	// Masked formats — strict pattern rejects, even though the trailing 4 digits are present
	[InlineData("****3409")]
	[InlineData("XX3409")]
	// Whitespace and separators
	[InlineData("3 409")]
	[InlineData("3-409")]
	// Non-ASCII Unicode digit sequences. .NET's default \d regex expands to the full
	// Unicode Decimal_Number category, so the pattern must use [0-9] explicitly to enforce
	// the documented "ASCII digits" contract. Without that, Arabic-Indic (٣٤٠٩) and
	// Devanagari (३४०९) digits would slip through with High confidence.
	[InlineData("\u0663\u0664\u0660\u0669")]
	[InlineData("\u096B\u096C\u0966\u0966")]
	public void ValidateLastFour_InvalidNonEmptyPatterns_YieldNullWithLowConfidence(string raw)
	{
		// Act — non-empty/non-whitespace input that fails the regex represents a hallucinated
		// or malformed value the VLM emitted. We retain Low confidence to signal "the model
		// said something but we rejected it" — distinct from None ("the model said nothing").
		FieldConfidence<string?> result = OllamaReceiptExtractionService.ValidateLastFour(raw);

		// Assert
		result.Value.Should().BeNull();
		result.Confidence.Should().Be(ConfidenceLevel.Low);
	}

	[Theory]
	[InlineData("")]
	[InlineData("   ")]
	[InlineData("\t")]
	public void ValidateLastFour_EmptyOrWhitespace_YieldsNullWithNoneConfidence(string raw)
	{
		// Act — an empty/whitespace lastFour is functionally equivalent to "the field was
		// absent", which we represent with ConfidenceLevel.None.
		FieldConfidence<string?> result = OllamaReceiptExtractionService.ValidateLastFour(raw);

		// Assert
		result.Value.Should().BeNull();
		result.Confidence.Should().Be(ConfidenceLevel.None);
	}

	[Fact]
	public void ValidateLastFour_NullInput_YieldsNullWithNoneConfidence()
	{
		// Act — null is treated identically to an empty/absent value.
		FieldConfidence<string?> result = OllamaReceiptExtractionService.ValidateLastFour(null);

		// Assert
		result.Value.Should().BeNull();
		result.Confidence.Should().Be(ConfidenceLevel.None);
	}

	[Theory]
	[InlineData("3409")]
	[InlineData("0000")]
	[InlineData("1234")]
	public void ValidateLastFour_ExactlyFourDigits_PreservedWithHighConfidence(string raw)
	{
		// Act
		FieldConfidence<string?> result = OllamaReceiptExtractionService.ValidateLastFour(raw);

		// Assert
		result.Value.Should().Be(raw);
		result.Confidence.Should().Be(ConfidenceLevel.High);
	}

	[Fact]
	public void ValidateLastFour_FourDigitsWithSurroundingWhitespace_TrimmedAndAccepted()
	{
		// Act — defensive: VLMs sometimes emit "3409 " with a trailing space
		FieldConfidence<string?> result = OllamaReceiptExtractionService.ValidateLastFour(" 3409 ");

		// Assert
		result.Value.Should().Be("3409");
		result.Confidence.Should().Be(ConfidenceLevel.High);
	}

	[Fact]
	public async Task ExtractAsync_ValidLastFour_PreservedWithHighConfidence()
	{
		// Arrange — sanity check that the post-processing does not regress the happy path.
		string innerJson = """
			{
			  "store": { "name": "Walmart" },
			  "total": 70.43,
			  "payments": [
			    { "method": "MCARD", "amount": 70.43, "lastFour": "3409" }
			  ]
			}
			""";
		OllamaReceiptExtractionService service = CreateService(CreateHandler(WrapInOllamaEnvelope(innerJson)));

		// Act
		ParsedReceipt receipt = await service.ExtractAsync(FakeImage, "image/png", CancellationToken.None);

		// Assert
		receipt.Payments[0].LastFour.Value.Should().Be("3409");
		receipt.Payments[0].LastFour.Confidence.Should().Be(ConfidenceLevel.High);
	}

	[Fact]
	public async Task ExtractAsync_PaymentWithMissingMethod_YieldsNoneConfidenceOnThatPaymentMethod()
	{
		// Arrange — defensive: if a payment record omits the method but has an amount, we
		// still include the payment in the list but mark the method as None-confidence so
		// downstream code can distinguish "truly unknown tender" from legacy-mapper behavior.
		string innerJson = """
			{
			  "store": { "name": "Walmart" },
			  "total": 15.00,
			  "payments": [
			    { "method": null, "amount": 15.00, "lastFour": null }
			  ]
			}
			""";
		OllamaReceiptExtractionService service = CreateService(CreateHandler(WrapInOllamaEnvelope(innerJson)));

		// Act
		ParsedReceipt receipt = await service.ExtractAsync(FakeImage, "image/png", CancellationToken.None);

		// Assert — the payment is preserved with correct confidence
		receipt.Payments.Should().HaveCount(1);
		receipt.Payments[0].Method.Value.Should().BeNull();
		receipt.Payments[0].Method.Confidence.Should().Be(ConfidenceLevel.None);
		receipt.Payments[0].Amount.Value.Should().Be(15.00m);
		receipt.Payments[0].Amount.Confidence.Should().Be(ConfidenceLevel.High);

		// Assert — legacy PaymentMethod falls back to None since no method string was found
		receipt.PaymentMethod.Value.Should().BeNull();
		receipt.PaymentMethod.Confidence.Should().Be(ConfidenceLevel.None);
	}

	[Fact]
	public async Task ExtractAsync_MalformedInnerJson_Throws()
	{
		// Arrange — the response field is not valid JSON
		string envelope = WrapInOllamaEnvelope("{ this is not: valid JSON");
		OllamaReceiptExtractionService service = CreateService(CreateHandler(envelope));

		// Act
		Func<Task> act = () => service.ExtractAsync(FakeImage, "image/png", CancellationToken.None);

		// Assert
		ExceptionAssertions<InvalidOperationException> thrown = await act.Should().ThrowAsync<InvalidOperationException>();
		thrown.Which.InnerException.Should().BeOfType<JsonException>();
	}

	[Fact]
	public async Task ExtractAsync_EmptyResponse_Throws()
	{
		// Arrange — envelope present but response field is empty string
		string envelope = WrapInOllamaEnvelope(string.Empty);
		OllamaReceiptExtractionService service = CreateService(CreateHandler(envelope));

		// Act
		Func<Task> act = () => service.ExtractAsync(FakeImage, "image/png", CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<InvalidOperationException>()
			.WithMessage("*empty response*");
	}

	[Fact]
	public async Task ExtractAsync_OperationCanceled_Propagates()
	{
		// Arrange — handler blocks until canceled
		Mock<HttpMessageHandler> handlerMock = new();
		handlerMock.Protected()
			.Setup<Task<HttpResponseMessage>>("SendAsync",
				ItExpr.IsAny<HttpRequestMessage>(),
				ItExpr.IsAny<CancellationToken>())
			.Returns(async (HttpRequestMessage _, CancellationToken ct) =>
			{
				await Task.Delay(Timeout.Infinite, ct);
				return new HttpResponseMessage(HttpStatusCode.OK);
			});
		OllamaReceiptExtractionService service = CreateService(handlerMock.Object);
		using CancellationTokenSource cts = new();
		cts.CancelAfter(TimeSpan.FromMilliseconds(50));

		// Act
		Func<Task> act = () => service.ExtractAsync(FakeImage, "image/png", cts.Token);

		// Assert — caller-initiated cancellation surfaces as OperationCanceledException
		await act.Should().ThrowAsync<OperationCanceledException>();
	}

	[Fact]
	public async Task ExtractAsync_Timeout_ThrowsTimeoutException()
	{
		// Arrange — handler delays longer than the per-attempt timeout. The Polly Timeout
		// strategy registered by RegisterReceiptExtractionService aborts the attempt and
		// surfaces TimeoutRejectedException, which the service translates to TimeoutException.
		Mock<HttpMessageHandler> handlerMock = new();
		handlerMock.Protected()
			.Setup<Task<HttpResponseMessage>>("SendAsync",
				ItExpr.IsAny<HttpRequestMessage>(),
				ItExpr.IsAny<CancellationToken>())
			.Returns(async (HttpRequestMessage _, CancellationToken ct) =>
			{
				await Task.Delay(TimeSpan.FromSeconds(5), ct);
				return new HttpResponseMessage(HttpStatusCode.OK);
			});
		VlmOcrOptions options = new()
		{
			OllamaUrl = "http://test-ollama",
			Model = "glm-ocr:q8_0",
			TimeoutSeconds = 1,
		};

		await RunWithPipelineServiceAsync(handlerMock.Object, options, async service =>
		{
			// Act
			Func<Task> act = () => service.ExtractAsync(FakeImage, "image/png", CancellationToken.None);

			// Assert
			await act.Should().ThrowAsync<TimeoutException>()
				.WithMessage("*timed out after 1s*");
		});
	}

	[Fact]
	public async Task ExtractAsync_Request_IncludesModelAndBase64AndJsonFormat()
	{
		// Arrange
		HttpRequestMessage? capturedRequest = null;
		string? capturedBody = null;
		Mock<HttpMessageHandler> handlerMock = new();
		handlerMock.Protected()
			.Setup<Task<HttpResponseMessage>>("SendAsync",
				ItExpr.IsAny<HttpRequestMessage>(),
				ItExpr.IsAny<CancellationToken>())
			.Returns(async (HttpRequestMessage req, CancellationToken _) =>
			{
				capturedRequest = req;
				capturedBody = req.Content is not null ? await req.Content.ReadAsStringAsync() : null;
				return new HttpResponseMessage
				{
					StatusCode = HttpStatusCode.OK,
					Content = new StringContent(WrapInOllamaEnvelope("{}"), System.Text.Encoding.UTF8, "application/json"),
				};
			});
		OllamaReceiptExtractionService service = CreateService(handlerMock.Object);

		// Act
		await service.ExtractAsync(FakeImage, "image/png", CancellationToken.None);

		// Assert
		capturedRequest.Should().NotBeNull();
		capturedRequest!.Method.Should().Be(HttpMethod.Post);
		capturedRequest.RequestUri!.AbsolutePath.Should().EndWith("/api/generate");
		capturedBody.Should().NotBeNullOrEmpty();

		using JsonDocument doc = JsonDocument.Parse(capturedBody!);
		doc.RootElement.GetProperty("model").GetString().Should().Be("glm-ocr:q8_0");
		doc.RootElement.GetProperty("format").GetString().Should().Be("json");
		doc.RootElement.GetProperty("stream").GetBoolean().Should().BeFalse();
		doc.RootElement.GetProperty("images")[0].GetString().Should().Be(Convert.ToBase64String(FakeImage));
		doc.RootElement.GetProperty("prompt").GetString().Should().Contain("receipt");
	}

	[Fact]
	public async Task RegisterReceiptExtractionService_SlowResponse_NotCancelledByServiceDefaultsStandardHandler()
	{
		// Regression for RECEIPTS-630. The Receipts.ServiceDefaults registration applies
		// AddStandardResilienceHandler globally via ConfigureHttpClientDefaults (30s per
		// attempt / 90s total). Real glm-ocr inferences routinely exceed 30s, so the
		// vlm-ocr typed client MUST opt out of that handler — otherwise every slow VLM
		// call surfaces as a Polly TimeoutRejectedException long before the documented
		// VlmOcrOptions.TimeoutSeconds (120s) budget applies.
		//
		// This test simulates the production composition: ServiceDefaults registers the
		// standard handler with an aggressive 200ms attempt timeout, THEN the application
		// calls RegisterReceiptExtractionService. A handler that delays 1s — well past the
		// 200ms standard-handler ceiling but well under the test's per-attempt VLM timeout
		// (5s) — must complete successfully. If the standard handler were not removed by
		// our registration, the call would die at ~200ms with a TimeoutRejectedException.
		Mock<HttpMessageHandler> handlerMock = new();
		string successBody = WrapInOllamaEnvelope("""{ "store": { "name": "Walmart" }, "total": 10.00 }""");
		handlerMock.Protected()
			.Setup<Task<HttpResponseMessage>>("SendAsync",
				ItExpr.IsAny<HttpRequestMessage>(),
				ItExpr.IsAny<CancellationToken>())
			.Returns(async (HttpRequestMessage _, CancellationToken ct) =>
			{
				await Task.Delay(TimeSpan.FromSeconds(1), ct);
				return new HttpResponseMessage(HttpStatusCode.OK)
				{
					Content = new StringContent(successBody, System.Text.Encoding.UTF8, "application/json"),
				};
			});

		ServiceCollection services = new();
		services.AddLogging();

		// Replicate Receipts.ServiceDefaults: register an aggressive standard handler
		// for every HttpClient via ConfigureHttpClientDefaults. If RegisterReceiptExtractionService
		// fails to remove this handler, the 200ms attempt timeout will cancel any call
		// that takes longer than 200ms.
		services.ConfigureHttpClientDefaults(http =>
		{
			http.AddStandardResilienceHandler(opts =>
			{
				opts.AttemptTimeout.Timeout = TimeSpan.FromMilliseconds(200);
				opts.TotalRequestTimeout.Timeout = TimeSpan.FromMilliseconds(600);
				opts.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(2);
				opts.Retry.ShouldHandle = _ => ValueTask.FromResult(false);
			});
		});

		// Configuration mirrors the production VLM section: a real OllamaUrl plus a
		// TimeoutSeconds well above the 1s simulated work — so success depends solely
		// on whether the global standard handler was successfully removed.
		IConfiguration configuration = new ConfigurationBuilder()
			.AddInMemoryCollection(new Dictionary<string, string?>
			{
				[$"{ConfigurationVariables.OcrVlmSection}:{nameof(VlmOcrOptions.OllamaUrl)}"] = "http://test-ollama",
				[$"{ConfigurationVariables.OcrVlmSection}:{nameof(VlmOcrOptions.Model)}"] = "glm-ocr:q8_0",
				[$"{ConfigurationVariables.OcrVlmSection}:{nameof(VlmOcrOptions.TimeoutSeconds)}"] = "5",
			})
			.Build();

		InfrastructureService.RegisterReceiptExtractionService(services, configuration);

		// Override the primary handler so we don't actually hit Ollama. ConfigurePrimaryHttpMessageHandler
		// is a separate registration that targets the same named client.
		services.AddHttpClient<IReceiptExtractionService, OllamaReceiptExtractionService>()
			.ConfigurePrimaryHttpMessageHandler(() => handlerMock.Object);

		using ServiceProvider sp = services.BuildServiceProvider();
		IReceiptExtractionService service = sp.GetRequiredService<IReceiptExtractionService>();

		// Act
		ParsedReceipt receipt = await service.ExtractAsync(FakeImage, "image/png", CancellationToken.None);

		// Assert — call completed without being cancelled by ServiceDefaults' 200ms standard
		// handler. With the bug present, this throws TimeoutRejectedException at ~200ms.
		receipt.StoreName.Value.Should().Be("Walmart");
		receipt.Total.Value.Should().Be(10.00m);
	}

	[Fact]
	public async Task ExtractAsync_RetryThenSuccess_ReturnsReceipt()
	{
		// Arrange — build a DI pipeline with retry. Fail twice with 503, then succeed.
		int callCount = 0;
		string successBody = WrapInOllamaEnvelope("""{ "store": { "name": "Walmart" }, "total": 10.00 }""");

		Mock<HttpMessageHandler> handlerMock = new();
		handlerMock.Protected()
			.Setup<Task<HttpResponseMessage>>("SendAsync",
				ItExpr.IsAny<HttpRequestMessage>(),
				ItExpr.IsAny<CancellationToken>())
			.Returns(() =>
			{
				int call = Interlocked.Increment(ref callCount);
				HttpResponseMessage message = call <= 2
					? new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
					{
						Content = new StringContent("{}"),
					}
					: new HttpResponseMessage(HttpStatusCode.OK)
					{
						Content = new StringContent(successBody, System.Text.Encoding.UTF8, "application/json"),
					};
				return Task.FromResult(message);
			});

		ServiceCollection services = new();
		services.AddLogging();
		services.AddSingleton(new VlmOcrOptions
		{
			OllamaUrl = "http://test-ollama",
			Model = "glm-ocr:q8_0",
			TimeoutSeconds = 30,
		});
		services.AddHttpClient<IReceiptExtractionService, OllamaReceiptExtractionService>(client =>
		{
			client.BaseAddress = new Uri("http://test-ollama/");
			client.Timeout = Timeout.InfiniteTimeSpan;
		})
		.ConfigurePrimaryHttpMessageHandler(() => handlerMock.Object)
		.AddResilienceHandler("vlm-ocr-test", builder =>
		{
			builder.AddRetry(new HttpRetryStrategyOptions
			{
				MaxRetryAttempts = 3,
				Delay = TimeSpan.FromMilliseconds(1),
				BackoffType = DelayBackoffType.Constant,
				ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
					.Handle<HttpRequestException>()
					.HandleResult(r => r.StatusCode is HttpStatusCode.ServiceUnavailable
						or HttpStatusCode.TooManyRequests
						or HttpStatusCode.GatewayTimeout),
			});
		});

		using ServiceProvider sp = services.BuildServiceProvider();
		IReceiptExtractionService service = sp.GetRequiredService<IReceiptExtractionService>();

		// Act
		ParsedReceipt receipt = await service.ExtractAsync(FakeImage, "image/png", CancellationToken.None);

		// Assert
		receipt.StoreName.Value.Should().Be("Walmart");
		receipt.Total.Value.Should().Be(10.00m);
		callCount.Should().Be(3); // 2 transient failures + 1 success
	}
}
