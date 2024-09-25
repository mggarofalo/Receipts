using Moq;
using Moq.Protected;
using SampleData.ViewModels.Aggregates;
using Shared.ViewModels.Aggregates;
using System.Net;
using System.Net.Http.Json;
using Shared.HttpClientApiExtensions.Aggregates;

namespace Presentation.Shared.Tests.HttpClientApiExtensions.Aggregates;

public class TripClientTests
{
	private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
	private readonly HttpClient _httpClient;

	public TripClientTests()
	{
		_httpMessageHandlerMock = new Mock<HttpMessageHandler>();
		_httpClient = new HttpClient(_httpMessageHandlerMock.Object) { BaseAddress = new Uri("http://localhost/api/") };
	}

	[Fact]
	public async Task GetTripByReceiptId_ShouldReturnTrip()
	{
		// Arrange
		Guid receiptId = Guid.NewGuid();
		TripVM trip = TripVMGenerator.Generate();

		HttpResponseMessage responseMessage = new(HttpStatusCode.OK)
		{
			Content = JsonContent.Create(trip)
		};

		_httpMessageHandlerMock
			.Protected()
			.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
			.ReturnsAsync(responseMessage);

		// Act
		TripVM? result = await _httpClient.GetTripByReceiptIdAsync(receiptId);

		// Assert
		Assert.NotNull(result);
		Assert.Equal(trip.Receipt, result.Receipt);
		Assert.All(trip.Transactions!, t => Assert.Contains(result.Transactions!, r => r.Transaction == t.Transaction));
		Assert.All(result.Transactions!, r => Assert.Contains(trip.Transactions!, t => t.Transaction == r.Transaction));
	}

	[Theory]
	[InlineData(HttpStatusCode.Ambiguous)] // 3xx
	[InlineData(HttpStatusCode.BadRequest)] // 4xx
	[InlineData(HttpStatusCode.InternalServerError)] // 5xx
	public async Task GetTripByReceiptId_ShouldThrowHttpRequestExceptionOnNonSuccessStatusCode(HttpStatusCode statusCode)
	{
		// Arrange
		Guid receiptId = Guid.NewGuid();
		HttpResponseMessage responseMessage = new(statusCode);

		_httpMessageHandlerMock
			.Protected()
			.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
			.ReturnsAsync(responseMessage);

		// Act & Assert
		await Assert.ThrowsAsync<HttpRequestException>(() => _httpClient.GetTripByReceiptIdAsync(receiptId));
	}
}
