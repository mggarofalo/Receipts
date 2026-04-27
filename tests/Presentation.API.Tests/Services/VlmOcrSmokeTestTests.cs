using System.Net;
using API.Services;
using Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;

namespace Presentation.API.Tests.Services;

public class VlmOcrSmokeTestTests
{
	private const string BaseUrl = "http://vlm-ocr:11434";
	private const string ConfiguredModel = "glm-ocr:q8_0";

	private static readonly string TagsJsonWithExactModel = """
	{
	  "models": [
	    {"name": "glm-ocr:q8_0", "size": 987654321}
	  ]
	}
	""";

	private static readonly string TagsJsonWithoutModel = """
	{
	  "models": [
	    {"name": "llama3:8b", "size": 5000000000}
	  ]
	}
	""";

	// Critical regression case for RECEIPTS-635: a model whose name *contains* the configured
	// tag as a substring must NOT be treated as a match. Previously the smoke test ran
	// `body.Contains("glm-ocr")` which would have returned true here.
	private static readonly string TagsJsonWithLookalikeModel = """
	{
	  "models": [
	    {"name": "glm-ocr-experimental", "size": 1234567890}
	  ]
	}
	""";

	private static readonly string TagsJsonEmpty = """
	{ "models": [] }
	""";

	private static readonly string TagsJsonMalformed = "<<not json>>";

	[Fact]
	public async Task RunAsync_ReachableWithExactModel_LogsInformation()
	{
		Mock<ILogger> logger = new();
		HttpClient http = CreateHttpClient(HttpStatusCode.OK, TagsJsonWithExactModel);

		await VlmOcrSmokeTest.RunAsync(http, ConfiguredModel, logger.Object, CancellationToken.None);

		VerifyLogged(logger, LogLevel.Information, Times.Once());
		VerifyLogged(logger, LogLevel.Warning, Times.Never());
	}

	[Fact]
	public async Task RunAsync_ReachableButModelMissing_LogsWarning()
	{
		Mock<ILogger> logger = new();
		HttpClient http = CreateHttpClient(HttpStatusCode.OK, TagsJsonWithoutModel);

		await VlmOcrSmokeTest.RunAsync(http, ConfiguredModel, logger.Object, CancellationToken.None);

		VerifyLogged(logger, LogLevel.Warning, Times.Once(), "not in model list");
		VerifyLogged(logger, LogLevel.Information, Times.Never());
	}

	[Fact]
	public async Task RunAsync_LookalikeModelName_DoesNotMatch_LogsWarning()
	{
		// glm-ocr-experimental should NOT satisfy a check for glm-ocr:q8_0.
		Mock<ILogger> logger = new();
		HttpClient http = CreateHttpClient(HttpStatusCode.OK, TagsJsonWithLookalikeModel);

		await VlmOcrSmokeTest.RunAsync(http, ConfiguredModel, logger.Object, CancellationToken.None);

		VerifyLogged(logger, LogLevel.Warning, Times.Once(), "not in model list");
		VerifyLogged(logger, LogLevel.Information, Times.Never());
	}

	[Fact]
	public async Task RunAsync_EmptyModelsList_LogsWarning()
	{
		Mock<ILogger> logger = new();
		HttpClient http = CreateHttpClient(HttpStatusCode.OK, TagsJsonEmpty);

		await VlmOcrSmokeTest.RunAsync(http, ConfiguredModel, logger.Object, CancellationToken.None);

		VerifyLogged(logger, LogLevel.Warning, Times.Once(), "no models reported");
		VerifyLogged(logger, LogLevel.Information, Times.Never());
	}

	[Fact]
	public async Task RunAsync_MalformedJson_LogsWarningAndDoesNotThrow()
	{
		Mock<ILogger> logger = new();
		HttpClient http = CreateHttpClient(HttpStatusCode.OK, TagsJsonMalformed);

		await VlmOcrSmokeTest.RunAsync(http, ConfiguredModel, logger.Object, CancellationToken.None);

		VerifyLogged(logger, LogLevel.Warning, Times.Once(), "failed to parse");
		VerifyLogged(logger, LogLevel.Information, Times.Never());
	}

	[Fact]
	public async Task RunAsync_NonSuccessStatusCode_LogsWarning()
	{
		Mock<ILogger> logger = new();
		HttpClient http = CreateHttpClient(HttpStatusCode.ServiceUnavailable, "");

		await VlmOcrSmokeTest.RunAsync(http, ConfiguredModel, logger.Object, CancellationToken.None);

		VerifyLogged(logger, LogLevel.Warning, Times.Once(), "503");
		VerifyLogged(logger, LogLevel.Information, Times.Never());
	}

	[Fact]
	public async Task RunAsync_TransportFailure_LogsWarningAndSwallowsException()
	{
		Mock<ILogger> logger = new();
		HttpClient http = CreateThrowingHttpClient(new HttpRequestException("connection refused"));

		await VlmOcrSmokeTest.RunAsync(http, ConfiguredModel, logger.Object, CancellationToken.None);

		VerifyLogged(logger, LogLevel.Warning, Times.Once(), "failed to reach");
		VerifyLogged(logger, LogLevel.Information, Times.Never());
	}

	[Fact]
	public async Task RunAsync_Timeout_LogsWarningAndSwallowsException()
	{
		Mock<ILogger> logger = new();
		HttpClient http = CreateThrowingHttpClient(new TaskCanceledException("timeout", new TimeoutException()));

		await VlmOcrSmokeTest.RunAsync(http, ConfiguredModel, logger.Object, CancellationToken.None);

		VerifyLogged(logger, LogLevel.Warning, Times.Once(), "failed to reach");
		VerifyLogged(logger, LogLevel.Information, Times.Never());
	}

	[Fact]
	public async Task InstanceRunAsync_UsesIHttpClientFactory_AndConfiguredModel()
	{
		// Verify the public instance API resolves the named "vlm-smoke" client from the factory
		// and uses the model from IOptions<VlmOcrOptions>.
		Mock<ILogger<VlmOcrSmokeTest>> logger = new();
		HttpClient http = CreateHttpClient(HttpStatusCode.OK, TagsJsonWithExactModel);

		Mock<IHttpClientFactory> factory = new();
		factory.Setup(f => f.CreateClient(VlmOcrSmokeTest.HttpClientName)).Returns(http);

		IOptions<VlmOcrOptions> options = Options.Create(new VlmOcrOptions
		{
			OllamaUrl = BaseUrl,
			Model = ConfiguredModel,
		});

		VlmOcrSmokeTest smokeTest = new(factory.Object, options, logger.Object);

		await smokeTest.RunAsync(CancellationToken.None);

		factory.Verify(f => f.CreateClient(VlmOcrSmokeTest.HttpClientName), Times.Once());
		VerifyLogged(logger, LogLevel.Information, Times.Once());
		VerifyLogged(logger, LogLevel.Warning, Times.Never());
	}

	private static HttpClient CreateHttpClient(HttpStatusCode status, string body)
	{
		Mock<HttpMessageHandler> handler = new();
		handler.Protected()
			.Setup<Task<HttpResponseMessage>>("SendAsync",
				ItExpr.IsAny<HttpRequestMessage>(),
				ItExpr.IsAny<CancellationToken>())
			.ReturnsAsync(new HttpResponseMessage
			{
				StatusCode = status,
				Content = new StringContent(body, System.Text.Encoding.UTF8, "application/json"),
			});

		return new HttpClient(handler.Object) { BaseAddress = new Uri(BaseUrl) };
	}

	private static HttpClient CreateThrowingHttpClient(Exception exception)
	{
		Mock<HttpMessageHandler> handler = new();
		handler.Protected()
			.Setup<Task<HttpResponseMessage>>("SendAsync",
				ItExpr.IsAny<HttpRequestMessage>(),
				ItExpr.IsAny<CancellationToken>())
			.ThrowsAsync(exception);

		return new HttpClient(handler.Object) { BaseAddress = new Uri(BaseUrl) };
	}

	private static void VerifyLogged(Mock<ILogger> logger, LogLevel level, Times times, string? containing = null)
	{
		logger.Verify(
			x => x.Log(
				level,
				It.IsAny<EventId>(),
				It.Is<It.IsAnyType>((state, _) => containing == null || state.ToString()!.Contains(containing)),
				It.IsAny<Exception?>(),
				It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
			times);
	}

	private static void VerifyLogged<T>(Mock<ILogger<T>> logger, LogLevel level, Times times, string? containing = null)
	{
		logger.Verify(
			x => x.Log(
				level,
				It.IsAny<EventId>(),
				It.Is<It.IsAnyType>((state, _) => containing == null || state.ToString()!.Contains(containing)),
				It.IsAny<Exception?>(),
				It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
			times);
	}
}
