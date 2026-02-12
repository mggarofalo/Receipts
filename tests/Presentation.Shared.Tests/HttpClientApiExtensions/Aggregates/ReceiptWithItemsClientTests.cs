using FluentAssertions;
using Moq;
using Moq.Protected;
using SampleData.ViewModels.Aggregates;
using Shared.HttpClientApiExtensions.Aggregates;
using Shared.ViewModels.Aggregates;
using System.Net;
using System.Net.Http.Json;

namespace Presentation.Shared.Tests.HttpClientApiExtensions.Aggregates;

public class ReceiptWithItemsClientTests
{
	private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
	private readonly HttpClient _httpClient;

	public ReceiptWithItemsClientTests()
	{
		_httpMessageHandlerMock = new Mock<HttpMessageHandler>();
		_httpClient = new HttpClient(_httpMessageHandlerMock.Object) { BaseAddress = new Uri("http://localhost/api/") };
	}

	[Fact]
	public async Task GetReceiptWithItemsByReceiptId_ShouldReturnReceiptWithItems()
	{
		// Arrange
		Guid receiptId = Guid.NewGuid();
		ReceiptWithItemsVM receiptWithItems = ReceiptWithItemsVMGenerator.Generate();

		HttpResponseMessage responseMessage = new(HttpStatusCode.OK)
		{
			Content = JsonContent.Create(receiptWithItems)
		};

		_httpMessageHandlerMock
			.Protected()
			.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
			.ReturnsAsync(responseMessage);

		// Act
		ReceiptWithItemsVM? result = await _httpClient.GetReceiptWithItemsByReceiptIdAsync(receiptId);

		// Assert
		Assert.NotNull(result);
		result.Should().BeEquivalentTo(receiptWithItems);
	}

	[Theory]
	[InlineData(HttpStatusCode.Ambiguous)] // 3xx
	[InlineData(HttpStatusCode.BadRequest)] // 4xx
	[InlineData(HttpStatusCode.InternalServerError)] // 5xx
	public async Task GetReceiptWithItemsByReceiptId_ShouldThrowHttpRequestExceptionOnNonSuccessStatusCode(HttpStatusCode statusCode)
	{
		// Arrange
		Guid receiptId = Guid.NewGuid();
		HttpResponseMessage responseMessage = new(statusCode);

		_httpMessageHandlerMock
			.Protected()
			.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
			.ReturnsAsync(responseMessage);

		// Act & Assert
		await Assert.ThrowsAsync<HttpRequestException>(() => _httpClient.GetReceiptWithItemsByReceiptIdAsync(receiptId));
	}
}
