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
		// Arrange
		string innerJson = """
			{
			  "store": "Walmart",
			  "date": "2026-04-01",
			  "items": [
			    { "code": "UPC-001", "description": "Milk", "quantity": 1, "unitPrice": 3.99, "totalPrice": 3.99 },
			    { "code": "UPC-002", "description": "Bread", "quantity": 2, "unitPrice": 2.50, "totalPrice": 5.00 }
			  ],
			  "subtotal": 8.99,
			  "taxLines": [{ "label": "Sales Tax", "amount": 0.72 }],
			  "total": 9.71,
			  "paymentMethod": "MASTERCARD"
			}
			""";
		OllamaReceiptExtractionService service = CreateService(CreateHandler(WrapInOllamaEnvelope(innerJson)));

		// Act
		ParsedReceipt receipt = await service.ExtractAsync(FakeImage, "image/png", CancellationToken.None);

		// Assert
		receipt.StoreName.Value.Should().Be("Walmart");
		receipt.StoreName.Confidence.Should().Be(ConfidenceLevel.High);
		receipt.Date.Value.Should().Be(new DateOnly(2026, 4, 1));
		receipt.Date.Confidence.Should().Be(ConfidenceLevel.High);

		receipt.Items.Should().HaveCount(2);
		receipt.Items[0].Code.Value.Should().Be("UPC-001");
		receipt.Items[0].Description.Value.Should().Be("Milk");
		receipt.Items[0].Quantity.Value.Should().Be(1m);
		receipt.Items[0].UnitPrice.Value.Should().Be(3.99m);
		receipt.Items[0].TotalPrice.Value.Should().Be(3.99m);
		receipt.Items[1].Quantity.Value.Should().Be(2m);
		receipt.Items[1].TotalPrice.Value.Should().Be(5.00m);

		receipt.Subtotal.Value.Should().Be(8.99m);
		receipt.TaxLines.Should().HaveCount(1);
		receipt.TaxLines[0].Label.Value.Should().Be("Sales Tax");
		receipt.TaxLines[0].Amount.Value.Should().Be(0.72m);
		receipt.Total.Value.Should().Be(9.71m);
		receipt.PaymentMethod.Value.Should().Be("MASTERCARD");
		receipt.PaymentMethod.Confidence.Should().Be(ConfidenceLevel.High);
	}

	[Fact]
	public async Task ExtractAsync_MissingOptionalFields_ReturnsNoneConfidence()
	{
		// Arrange — no paymentMethod, no taxLines
		string innerJson = """
			{
			  "store": "Walmart",
			  "date": "2026-04-01",
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
		string successBody = WrapInOllamaEnvelope("""{ "store": "Walmart", "total": 10.00 }""");

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
