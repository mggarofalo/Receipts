using FluentAssertions;
using Moq;
using Moq.Protected;
using Shared.ViewModels.Core;
using System.Net;
using System.Net.Http.Json;
using SampleData.ViewModels.Core;
using Shared.HttpClientApiExtensions.Core;

namespace Presentation.Shared.Tests.HttpClientApiExtensions.Core;

public class ReceiptItemClientTests
{
	private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
	private readonly HttpClient _httpClient;

	public ReceiptItemClientTests()
	{
		_httpMessageHandlerMock = new Mock<HttpMessageHandler>();
		_httpClient = new HttpClient(_httpMessageHandlerMock.Object) { BaseAddress = new Uri("http://localhost/api/") };
	}

	[Fact]
	public async Task CreateReceiptItems_ShouldReturnCreatedReceiptItems()
	{
		// Arrange
		List<ReceiptItemVM> receiptItems = ReceiptItemVMGenerator.GenerateList(2);

		HttpResponseMessage responseMessage = new(HttpStatusCode.OK)
		{
			Content = JsonContent.Create(receiptItems)
		};

		_httpMessageHandlerMock
			.Protected()
			.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
			.ReturnsAsync(responseMessage);

		// Act
		List<ReceiptItemVM>? result = await _httpClient.CreateReceiptItemsAsync(receiptItems);

		// Assert
		Assert.NotNull(result);
		Assert.Equal(receiptItems.Count, result.Count);
		result.Should().BeEquivalentTo(receiptItems);
	}

	[Theory]
	[InlineData(HttpStatusCode.Ambiguous)] // 3xx
	[InlineData(HttpStatusCode.BadRequest)] // 4xx
	[InlineData(HttpStatusCode.InternalServerError)] // 5xx
	public async Task CreateReceiptItems_ShouldThrowHttpRequestExceptionOnNonSuccessStatusCode(HttpStatusCode statusCode)
	{
		// Arrange
		List<ReceiptItemVM> receiptItems = ReceiptItemVMGenerator.GenerateList(2);
		HttpResponseMessage responseMessage = new(statusCode);

		_httpMessageHandlerMock
			.Protected()
			.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
			.ReturnsAsync(responseMessage);

		// Act & Assert
		await Assert.ThrowsAsync<HttpRequestException>(() => _httpClient.CreateReceiptItemsAsync(receiptItems));
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
		ReceiptItemVM? result = await _httpClient.GetReceiptItemByIdAsync(receiptItem.Id!.Value);

		// Assert
		Assert.NotNull(result);
		Assert.Equal(receiptItem.Id, result.Id);
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
		await Assert.ThrowsAsync<HttpRequestException>(() => _httpClient.GetReceiptItemByIdAsync(Guid.NewGuid()));
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
		List<ReceiptItemVM>? result = await _httpClient.GetAllReceiptItemsAsync();

		// Assert
		Assert.NotNull(result);
		Assert.Equal(receiptItems.Count, result.Count);
		result.Should().BeEquivalentTo(receiptItems);
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
		await Assert.ThrowsAsync<HttpRequestException>(() => _httpClient.GetAllReceiptItemsAsync());
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
		List<ReceiptItemVM>? result = await _httpClient.GetReceiptItemsByReceiptIdAsync(receiptId);

		// Assert
		Assert.NotNull(result);
		Assert.Equal(receiptItems.Count, result.Count);
		result.Should().BeEquivalentTo(receiptItems);
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
		await Assert.ThrowsAsync<HttpRequestException>(() => _httpClient.GetReceiptItemsByReceiptIdAsync(receiptId));
	}

	[Fact]
	public async Task UpdateReceiptItems_ShouldReturnTrueOnSuccess()
	{
		// Arrange
		List<ReceiptItemVM> receiptItems = ReceiptItemVMGenerator.GenerateList(2);

		HttpResponseMessage responseMessage = new(HttpStatusCode.OK);

		_httpMessageHandlerMock
			.Protected()
			.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
			.ReturnsAsync(responseMessage);

		// Act
		bool result = await _httpClient.UpdateReceiptItemsAsync(receiptItems);

		// Assert
		Assert.True(result);
	}

	[Theory]
	[InlineData(HttpStatusCode.Ambiguous)] // 3xx
	[InlineData(HttpStatusCode.BadRequest)] // 4xx
	[InlineData(HttpStatusCode.InternalServerError)] // 5xx
	public async Task UpdateReceiptItems_ShouldThrowHttpRequestExceptionOnNonSuccessStatusCode(HttpStatusCode statusCode)
	{
		// Arrange
		List<ReceiptItemVM> receiptItems = ReceiptItemVMGenerator.GenerateList(2);
		HttpResponseMessage responseMessage = new(statusCode);

		_httpMessageHandlerMock
			.Protected()
			.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
			.ReturnsAsync(responseMessage);

		// Act & Assert
		await Assert.ThrowsAsync<HttpRequestException>(() => _httpClient.UpdateReceiptItemsAsync(receiptItems));
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
		bool result = await _httpClient.DeleteReceiptItemsAsync(ids);

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
		await Assert.ThrowsAsync<HttpRequestException>(() => _httpClient.DeleteReceiptItemsAsync(ids));
	}
}