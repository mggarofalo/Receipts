using System.Net;
using API.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;

namespace Presentation.API.Tests.Services;

public class VlmOcrSmokeTestTests
{
	private const string BaseUrl = "http://vlm-ocr:11434";
	private static readonly string TagsJsonWithModel = """
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

	[Fact]
	public async Task RunAsync_ReachableWithModel_LogsInformation()
	{
		Mock<ILogger> logger = new();
		HttpClient http = CreateHttpClient(HttpStatusCode.OK, TagsJsonWithModel);

		await VlmOcrSmokeTest.RunAsync(http, logger.Object, CancellationToken.None);

		VerifyLogged(logger, LogLevel.Information, Times.Once());
		VerifyLogged(logger, LogLevel.Warning, Times.Never());
	}

	[Fact]
	public async Task RunAsync_ReachableButModelMissing_LogsWarning()
	{
		Mock<ILogger> logger = new();
		HttpClient http = CreateHttpClient(HttpStatusCode.OK, TagsJsonWithoutModel);

		await VlmOcrSmokeTest.RunAsync(http, logger.Object, CancellationToken.None);

		VerifyLogged(logger, LogLevel.Warning, Times.Once(), "glm-ocr not in model list");
		VerifyLogged(logger, LogLevel.Information, Times.Never());
	}

	[Fact]
	public async Task RunAsync_NonSuccessStatusCode_LogsWarning()
	{
		Mock<ILogger> logger = new();
		HttpClient http = CreateHttpClient(HttpStatusCode.ServiceUnavailable, "");

		await VlmOcrSmokeTest.RunAsync(http, logger.Object, CancellationToken.None);

		VerifyLogged(logger, LogLevel.Warning, Times.Once(), "503");
		VerifyLogged(logger, LogLevel.Information, Times.Never());
	}

	[Fact]
	public async Task RunAsync_TransportFailure_LogsWarningAndSwallowsException()
	{
		Mock<ILogger> logger = new();
		HttpClient http = CreateThrowingHttpClient(new HttpRequestException("connection refused"));

		await VlmOcrSmokeTest.RunAsync(http, logger.Object, CancellationToken.None);

		VerifyLogged(logger, LogLevel.Warning, Times.Once(), "failed to reach");
		VerifyLogged(logger, LogLevel.Information, Times.Never());
	}

	[Fact]
	public async Task RunAsync_Timeout_LogsWarningAndSwallowsException()
	{
		Mock<ILogger> logger = new();
		HttpClient http = CreateThrowingHttpClient(new TaskCanceledException("timeout", new TimeoutException()));

		await VlmOcrSmokeTest.RunAsync(http, logger.Object, CancellationToken.None);

		VerifyLogged(logger, LogLevel.Warning, Times.Once(), "failed to reach");
		VerifyLogged(logger, LogLevel.Information, Times.Never());
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
}
