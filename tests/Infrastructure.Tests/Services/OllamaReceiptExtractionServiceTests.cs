using System.Net;
using System.Text.Json;
using Application.Interfaces.Services;
using Application.Models.Ocr;
using FluentAssertions;
using FluentAssertions.Specialized;
using Infrastructure.Services;
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

		receipt.Items.Should().HaveCount(2);
		receipt.Items[0].Code.Value.Should().Be("078742228030");
		receipt.Items[0].Description.Value.Should().Be("GRANULATED");
		receipt.Items[0].Quantity.Confidence.Should().Be(ConfidenceLevel.Low);
		receipt.Items[0].UnitPrice.Confidence.Should().Be(ConfidenceLevel.Low);
		receipt.Items[0].TotalPrice.Value.Should().Be(3.07m);

		receipt.Items[1].Quantity.Value.Should().Be(2.46m);
		receipt.Items[1].UnitPrice.Value.Should().Be(0.50m);
		receipt.Items[1].TotalPrice.Value.Should().Be(1.23m);

		receipt.Subtotal.Value.Should().Be(69.68m);
		receipt.TaxLines.Should().HaveCount(1);
		receipt.TaxLines[0].Label.Value.Should().Be("TAX1 6.0000%");
		receipt.TaxLines[0].Amount.Value.Should().Be(0.75m);
		receipt.Total.Value.Should().Be(70.43m);
		receipt.PaymentMethod.Value.Should().Be("MASTERCARD");
		receipt.PaymentMethod.Confidence.Should().Be(ConfidenceLevel.High);
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
		receipt.Date.Confidence.Should().Be(ConfidenceLevel.Low);
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
		receipt.Items[0].Quantity.Confidence.Should().Be(ConfidenceLevel.Low);
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
		merged[1].Description.Should().Be("BANANAS");
		merged[1].Quantity.Should().Be(2.720m);
		merged[1].UnitPrice.Should().Be(0.50m);
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
		receipt.PaymentMethod.Confidence.Should().Be(ConfidenceLevel.Low);
		receipt.TaxLines.Should().BeEmpty();
		receipt.Items.Should().BeEmpty();
		receipt.StoreName.Confidence.Should().Be(ConfidenceLevel.High);
	}

	[Fact]
	public async Task ExtractAsync_MultiPayment_TakesFirstMethod()
	{
		// Arrange — split tender (gift card + card). V1 ParsedReceipt can only carry one method;
		// we pick the first that has a non-empty method string.
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

		// Assert
		receipt.PaymentMethod.Value.Should().Be("GIFT CARD");
		receipt.PaymentMethod.Confidence.Should().Be(ConfidenceLevel.High);
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
		receipt.StoreName.Confidence.Should().Be(ConfidenceLevel.Low);
		receipt.Date.Value.Should().Be(new DateOnly(2026, 4, 1));
		receipt.Total.Value.Should().Be(10.00m);
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
		// Arrange — handler delays longer than TimeoutSeconds
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
		OllamaReceiptExtractionService service = CreateService(handlerMock.Object, options);

		// Act
		Func<Task> act = () => service.ExtractAsync(FakeImage, "image/png", CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<TimeoutException>()
			.WithMessage("*timed out after 1s*");
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
