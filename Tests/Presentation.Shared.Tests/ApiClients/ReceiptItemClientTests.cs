using Moq;
using Moq.Protected;
using Shared.ViewModels.Core;
using Shared.ApiClients;
using System.Net;
using System.Net.Http.Json;
using SampleData.ViewModels.Core;

namespace Presentation.Shared.Tests.ApiClients;

public class ReceiptItemClientTests
{
	private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
	private readonly HttpClient _httpClient;
	private readonly ReceiptItemClient _receiptItemClient;

	public ReceiptItemClientTests()
	{
		_httpMessageHandlerMock = new Mock<HttpMessageHandler>();
		_httpClient = new HttpClient(_httpMessageHandlerMock.Object) { BaseAddress = new Uri("http://localhost/api/") };
		_receiptItemClient = new ReceiptItemClient(_httpClient);
	}

	[Fact]
	public void NoArgsConstructor_ShouldInitializeReceiptItemClient()
	{
		// Act
		ReceiptItemClient receiptItemClient = new();

		// Assert
		Assert.NotNull(receiptItemClient);
	}

	[Fact]
	public async Task CreateReceiptItem_ShouldReturnCreatedReceiptItem()
	{
		// Arrange
		ReceiptItemVM receiptItem = ReceiptItemVMGenerator.Generate();

		HttpResponseMessage responseMessage = new(HttpStatusCode.OK)
		{
			Content = JsonContent.Create(receiptItem)
		};

		_httpMessageHandlerMock
			.Protected()
			.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
			.ReturnsAsync(responseMessage);

		// Act
		ReceiptItemVM? result = await _receiptItemClient.CreateReceiptItemAsync(receiptItem);

		// Assert
		Assert.NotNull(result);
		Assert.Equal(receiptItem.Id, result.Id);
	}

	[Theory]
	[InlineData(HttpStatusCode.Ambiguous)] // 3xx
	[InlineData(HttpStatusCode.BadRequest)] // 4xx
	[InlineData(HttpStatusCode.InternalServerError)] // 5xx
	public async Task CreateReceiptItem_ShouldThrowHttpRequestExceptionOnNonSuccessStatusCode(HttpStatusCode statusCode)
	{
		// Arrange
		ReceiptItemVM receiptItem = ReceiptItemVMGenerator.Generate();
		HttpResponseMessage responseMessage = new(statusCode);

		_httpMessageHandlerMock
			.Protected()
			.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
			.ReturnsAsync(responseMessage);

		// Act & Assert
		await Assert.ThrowsAsync<HttpRequestException>(() => _receiptItemClient.CreateReceiptItemAsync(receiptItem));
	}

	[Fact]
	public async Task GetReceiptItemById_ShouldReturnReceiptItem()
	{
		// Arrange
		ReceiptItemVM receiptItem = ReceiptItemVMGenerator.Generate();

		HttpResponseMessage responseMessage = new(HttpStatusCode.OK)
		{
			Content = JsonContent.Create(receiptItem)
		};

		_httpMessageHandlerMock
			.Protected()
			.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
			.ReturnsAsync(responseMessage);

		// Act
		ReceiptItemVM? result = await _receiptItemClient.GetReceiptItemByIdAsync(receiptItem.Id!.Value);

		// Assert
		Assert.NotNull(result);
		Assert.Equal(receiptItem, result);
	}

	[Theory]
	[InlineData(HttpStatusCode.Ambiguous)] // 3xx
	[InlineData(HttpStatusCode.BadRequest)] // 4xx
	[InlineData(HttpStatusCode.InternalServerError)] // 5xx
	public async Task GetReceiptItemById_ShouldThrowHttpRequestExceptionOnNonSuccessStatusCode(HttpStatusCode statusCode)
	{
		// Arrange
		HttpResponseMessage responseMessage = new(statusCode);

		_httpMessageHandlerMock
			.Protected()
			.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
			.ReturnsAsync(responseMessage);

		// Act & Assert
		await Assert.ThrowsAsync<HttpRequestException>(() => _receiptItemClient.GetReceiptItemByIdAsync(Guid.NewGuid()));
	}

	[Fact]
	public async Task GetAllReceiptItems_ShouldReturnAllReceiptItems()
	{
		// Arrange
		List<ReceiptItemVM> receiptItems = ReceiptItemVMGenerator.GenerateList(3);

		HttpResponseMessage responseMessage = new(HttpStatusCode.OK)
		{
			Content = JsonContent.Create(receiptItems)
		};

		_httpMessageHandlerMock
			.Protected()
			.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
			.ReturnsAsync(responseMessage);

		// Act
		List<ReceiptItemVM>? result = await _receiptItemClient.GetAllReceiptItemsAsync();

		// Assert
		Assert.NotNull(result);
		Assert.Equal(receiptItems.Count, result.Count);
		Assert.All(receiptItems, r => Assert.Contains(result, res => res.Id == r.Id));
		Assert.All(result, res => Assert.Contains(receiptItems, r => r.Id == res.Id));
	}

	[Theory]
	[InlineData(HttpStatusCode.Ambiguous)] // 3xx
	[InlineData(HttpStatusCode.BadRequest)] // 4xx
	[InlineData(HttpStatusCode.InternalServerError)] // 5xx
	public async Task GetAllReceiptItems_ShouldThrowHttpRequestExceptionOnNonSuccessStatusCode(HttpStatusCode statusCode)
	{
		// Arrange
		HttpResponseMessage responseMessage = new(statusCode);

		_httpMessageHandlerMock
			.Protected()
			.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
			.ReturnsAsync(responseMessage);

		// Act & Assert
		await Assert.ThrowsAsync<HttpRequestException>(() => _receiptItemClient.GetAllReceiptItemsAsync());
	}

	[Fact]
	public async Task GetReceiptItemsByReceiptId_ShouldReturnReceiptItems()
	{
		// Arrange
		List<ReceiptItemVM> receiptItems = ReceiptItemVMGenerator.GenerateList(3);
		Guid receiptId = Guid.NewGuid();

		HttpResponseMessage responseMessage = new(HttpStatusCode.OK)
		{
			Content = JsonContent.Create(receiptItems)
		};

		_httpMessageHandlerMock
			.Protected()
			.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
			.ReturnsAsync(responseMessage);

		// Act
		List<ReceiptItemVM>? result = await _receiptItemClient.GetReceiptItemsByReceiptIdAsync(receiptId);

		// Assert
		Assert.NotNull(result);
		Assert.Equal(receiptItems.Count, result.Count);
		Assert.All(receiptItems, r => Assert.Contains(result, res => res.Id == r.Id));
		Assert.All(result, res => Assert.Contains(receiptItems, r => r.Id == res.Id));
	}

	[Theory]
	[InlineData(HttpStatusCode.Ambiguous)] // 3xx
	[InlineData(HttpStatusCode.BadRequest)] // 4xx
	[InlineData(HttpStatusCode.InternalServerError)] // 5xx
	public async Task GetReceiptItemsByReceiptId_ShouldThrowHttpRequestExceptionOnNonSuccessStatusCode(HttpStatusCode statusCode)
	{
		// Arrange
		Guid receiptId = Guid.NewGuid();
		HttpResponseMessage responseMessage = new(statusCode);

		_httpMessageHandlerMock
			.Protected()
			.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
			.ReturnsAsync(responseMessage);

		// Act & Assert
		await Assert.ThrowsAsync<HttpRequestException>(() => _receiptItemClient.GetReceiptItemsByReceiptIdAsync(receiptId));
	}

	[Fact]
	public async Task UpdateReceiptItem_ShouldReturnTrueOnSuccess()
	{
		// Arrange
		ReceiptItemVM receiptItem = ReceiptItemVMGenerator.Generate();

		HttpResponseMessage responseMessage = new(HttpStatusCode.OK);

		_httpMessageHandlerMock
			.Protected()
			.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
			.ReturnsAsync(responseMessage);

		// Act
		bool result = await _receiptItemClient.UpdateReceiptItemAsync(receiptItem);

		// Assert
		Assert.True(result);
	}

	[Theory]
	[InlineData(HttpStatusCode.Ambiguous)] // 3xx
	[InlineData(HttpStatusCode.BadRequest)] // 4xx
	[InlineData(HttpStatusCode.InternalServerError)] // 5xx
	public async Task UpdateReceiptItem_ShouldThrowHttpRequestExceptionOnNonSuccessStatusCode(HttpStatusCode statusCode)
	{
		// Arrange
		ReceiptItemVM receiptItem = ReceiptItemVMGenerator.Generate();
		HttpResponseMessage responseMessage = new(statusCode);

		_httpMessageHandlerMock
			.Protected()
			.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
			.ReturnsAsync(responseMessage);

		// Act & Assert
		await Assert.ThrowsAsync<HttpRequestException>(() => _receiptItemClient.UpdateReceiptItemAsync(receiptItem));
	}

	[Fact]
	public async Task DeleteReceiptItems_ShouldReturnTrueOnSuccess()
	{
		// Arrange
		List<Guid> ids = ReceiptItemVMGenerator.GenerateList(3).Select(r => r.Id!.Value).ToList();

		HttpResponseMessage responseMessage = new(HttpStatusCode.OK);

		_httpMessageHandlerMock
			.Protected()
			.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
			.ReturnsAsync(responseMessage);

		// Act
		bool result = await _receiptItemClient.DeleteReceiptItemsAsync(ids);

		// Assert
		Assert.True(result);
	}

	[Theory]
	[InlineData(HttpStatusCode.Ambiguous)] // 3xx
	[InlineData(HttpStatusCode.BadRequest)] // 4xx
	[InlineData(HttpStatusCode.InternalServerError)] // 5xx
	public async Task DeleteReceiptItems_ShouldThrowHttpRequestExceptionOnNonSuccessStatusCode(HttpStatusCode statusCode)
	{
		// Arrange
		List<Guid> ids = ReceiptItemVMGenerator.GenerateList(3).Select(r => r.Id!.Value).ToList();
		HttpResponseMessage responseMessage = new(statusCode);

		_httpMessageHandlerMock
			.Protected()
			.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
			.ReturnsAsync(responseMessage);

		// Act & Assert
		await Assert.ThrowsAsync<HttpRequestException>(() => _receiptItemClient.DeleteReceiptItemsAsync(ids));
	}

	[Theory]
	[InlineData(HttpStatusCode.Ambiguous)] // 3xx
	[InlineData(HttpStatusCode.BadRequest)] // 4xx
	[InlineData(HttpStatusCode.InternalServerError)] // 5xx
	public async Task ReceiptItemClient_ShouldThrowHttpRequestExceptionOnNonSuccessStatusCode(HttpStatusCode statusCode)
	{
		// Arrange
		HttpResponseMessage responseMessage = new(statusCode);

		_httpMessageHandlerMock
			.Protected()
			.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
			.ReturnsAsync(responseMessage);

		// Act & Assert
		await Assert.ThrowsAsync<HttpRequestException>(() => _receiptItemClient.GetAllReceiptItemsAsync());
	}
}