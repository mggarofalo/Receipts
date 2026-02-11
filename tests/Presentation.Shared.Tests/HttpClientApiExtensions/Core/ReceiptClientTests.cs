using FluentAssertions;
using Moq;
using Moq.Protected;
using SampleData.ViewModels.Core;
using Shared.HttpClientApiExtensions.Core;
using Shared.ViewModels.Core;
using System.Net;
using System.Net.Http.Json;

namespace Presentation.Shared.Tests.HttpClientApiExtensions.Core;

public class ReceiptClientTests
{
	private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
	private readonly HttpClient _httpClient;

	public ReceiptClientTests()
	{
		_httpMessageHandlerMock = new Mock<HttpMessageHandler>();
		_httpClient = new HttpClient(_httpMessageHandlerMock.Object) { BaseAddress = new Uri("http://localhost/api/") };
	}

	[Fact]
	public async Task CreateReceipts_ShouldReturnCreatedReceipts()
	{
		// Arrange
		List<ReceiptVM> receipts = ReceiptVMGenerator.GenerateList(2);

		HttpResponseMessage responseMessage = new(HttpStatusCode.OK)
		{
			Content = JsonContent.Create(receipts)
		};

		_httpMessageHandlerMock
			.Protected()
			.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
			.ReturnsAsync(responseMessage);

		// Act
		List<ReceiptVM>? result = await _httpClient.CreateReceiptsAsync(receipts);

		// Assert
		Assert.NotNull(result);
		Assert.Equal(receipts.Count, result.Count);
		result.Should().BeEquivalentTo(receipts);
	}

	[Theory]
	[InlineData(HttpStatusCode.Ambiguous)] // 3xx
	[InlineData(HttpStatusCode.BadRequest)] // 4xx
	[InlineData(HttpStatusCode.InternalServerError)] // 5xx
	public async Task CreateReceipts_ShouldThrowHttpRequestExceptionOnNonSuccessStatusCode(HttpStatusCode statusCode)
	{
		// Arrange
		List<ReceiptVM> receipts = ReceiptVMGenerator.GenerateList(2);
		HttpResponseMessage responseMessage = new(statusCode);

		_httpMessageHandlerMock
			.Protected()
			.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
			.ReturnsAsync(responseMessage);

		// Act & Assert
		await Assert.ThrowsAsync<HttpRequestException>(() => _httpClient.CreateReceiptsAsync(receipts));
	}

	[Fact]
	public async Task GetReceiptById_ShouldReturnReceipt()
	{
		// Arrange
		ReceiptVM receipt = ReceiptVMGenerator.Generate();

		HttpResponseMessage responseMessage = new(HttpStatusCode.OK)
		{
			Content = JsonContent.Create(receipt)
		};

		_httpMessageHandlerMock
			.Protected()
			.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
			.ReturnsAsync(responseMessage);

		// Act
		ReceiptVM? result = await _httpClient.GetReceiptByIdAsync(receipt.Id!.Value);

		// Assert
		Assert.NotNull(result);
		Assert.Equal(receipt.Id, result.Id);
	}

	[Theory]
	[InlineData(HttpStatusCode.Ambiguous)] // 3xx
	[InlineData(HttpStatusCode.BadRequest)] // 4xx
	[InlineData(HttpStatusCode.InternalServerError)] // 5xx
	public async Task GetReceiptById_ShouldThrowHttpRequestExceptionOnNonSuccessStatusCode(HttpStatusCode statusCode)
	{
		// Arrange
		HttpResponseMessage responseMessage = new(statusCode);

		_httpMessageHandlerMock
			.Protected()
			.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
			.ReturnsAsync(responseMessage);

		// Act & Assert
		await Assert.ThrowsAsync<HttpRequestException>(() => _httpClient.GetReceiptByIdAsync(Guid.NewGuid()));
	}

	[Fact]
	public async Task GetAllReceipts_ShouldReturnAllReceipts()
	{
		// Arrange
		List<ReceiptVM> receipts = ReceiptVMGenerator.GenerateList(3);

		HttpResponseMessage responseMessage = new(HttpStatusCode.OK)
		{
			Content = JsonContent.Create(receipts)
		};

		_httpMessageHandlerMock
			.Protected()
			.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
			.ReturnsAsync(responseMessage);

		// Act
		List<ReceiptVM>? result = await _httpClient.GetAllReceiptsAsync();

		// Assert
		Assert.NotNull(result);
		Assert.Equal(receipts.Count, result.Count);
		result.Should().BeEquivalentTo(receipts);
	}

	[Theory]
	[InlineData(HttpStatusCode.Ambiguous)] // 3xx
	[InlineData(HttpStatusCode.BadRequest)] // 4xx
	[InlineData(HttpStatusCode.InternalServerError)] // 5xx
	public async Task GetAllReceipts_ShouldThrowHttpRequestExceptionOnNonSuccessStatusCode(HttpStatusCode statusCode)
	{
		// Arrange
		HttpResponseMessage responseMessage = new(statusCode);

		_httpMessageHandlerMock
			.Protected()
			.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
			.ReturnsAsync(responseMessage);

		// Act & Assert
		await Assert.ThrowsAsync<HttpRequestException>(() => _httpClient.GetAllReceiptsAsync());
	}

	[Fact]
	public async Task UpdateReceipts_ShouldReturnTrueOnSuccess()
	{
		// Arrange
		List<ReceiptVM> receipts = ReceiptVMGenerator.GenerateList(2);

		HttpResponseMessage responseMessage = new(HttpStatusCode.OK);

		_httpMessageHandlerMock
			.Protected()
			.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
			.ReturnsAsync(responseMessage);

		// Act
		bool result = await _httpClient.UpdateReceiptsAsync(receipts);

		// Assert
		Assert.True(result);
	}

	[Theory]
	[InlineData(HttpStatusCode.Ambiguous)] // 3xx
	[InlineData(HttpStatusCode.BadRequest)] // 4xx
	[InlineData(HttpStatusCode.InternalServerError)] // 5xx
	public async Task UpdateReceipts_ShouldThrowHttpRequestExceptionOnNonSuccessStatusCode(HttpStatusCode statusCode)
	{
		// Arrange
		List<ReceiptVM> receipts = ReceiptVMGenerator.GenerateList(2);
		HttpResponseMessage responseMessage = new(statusCode);

		_httpMessageHandlerMock
			.Protected()
			.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
			.ReturnsAsync(responseMessage);

		// Act & Assert
		await Assert.ThrowsAsync<HttpRequestException>(() => _httpClient.UpdateReceiptsAsync(receipts));
	}

	[Fact]
	public async Task DeleteReceipts_ShouldReturnTrueOnSuccess()
	{
		// Arrange
		List<Guid> ids = ReceiptVMGenerator.GenerateList(3).Select(r => r.Id!.Value).ToList();

		HttpResponseMessage responseMessage = new(HttpStatusCode.OK);

		_httpMessageHandlerMock
			.Protected()
			.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
			.ReturnsAsync(responseMessage);

		// Act
		bool result = await _httpClient.DeleteReceiptsAsync(ids);

		// Assert
		Assert.True(result);
	}

	[Theory]
	[InlineData(HttpStatusCode.Ambiguous)] // 3xx
	[InlineData(HttpStatusCode.BadRequest)] // 4xx
	[InlineData(HttpStatusCode.InternalServerError)] // 5xx
	public async Task DeleteReceipts_ShouldThrowHttpRequestExceptionOnNonSuccessStatusCode(HttpStatusCode statusCode)
	{
		// Arrange
		List<Guid> ids = ReceiptVMGenerator.GenerateList(3).Select(r => r.Id!.Value).ToList();
		HttpResponseMessage responseMessage = new(statusCode);

		_httpMessageHandlerMock
			.Protected()
			.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
			.ReturnsAsync(responseMessage);

		// Act & Assert
		await Assert.ThrowsAsync<HttpRequestException>(() => _httpClient.DeleteReceiptsAsync(ids));
	}
}
