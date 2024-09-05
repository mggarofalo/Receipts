using Moq;
using Moq.Protected;
using SampleData.ViewModels.Core;
using Shared.ApiClients;
using Shared.ViewModels.Core;
using System.Net;
using System.Net.Http.Json;

namespace Presentation.Shared.Tests.ApiClients;

public class ReceiptClientTests
{
	private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
	private readonly HttpClient _httpClient;
	private readonly ReceiptClient _receiptClient;

	public ReceiptClientTests()
	{
		_httpMessageHandlerMock = new Mock<HttpMessageHandler>();
		_httpClient = new HttpClient(_httpMessageHandlerMock.Object) { BaseAddress = new Uri("http://localhost/api/") };
		_receiptClient = new ReceiptClient(_httpClient);
	}

	[Fact]
	public void NoArgsConstructor_ShouldInitializeReceiptClient()
	{
		// Act
		ReceiptClient receiptClient = new();

		// Assert
		Assert.NotNull(receiptClient);
	}

	[Fact]
	public async Task CreateReceipt_ShouldReturnCreatedReceipt()
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
		ReceiptVM? result = await _receiptClient.CreateReceiptAsync(receipt);

		// Assert
		Assert.NotNull(result);
		Assert.Equal(receipt.Id, result.Id);
	}

	[Theory]
	[InlineData(HttpStatusCode.Ambiguous)] // 3xx
	[InlineData(HttpStatusCode.BadRequest)] // 4xx
	[InlineData(HttpStatusCode.InternalServerError)] // 5xx
	public async Task CreateReceipt_ShouldThrowHttpRequestExceptionOnNonSuccessStatusCode(HttpStatusCode statusCode)
	{
		// Arrange
		ReceiptVM receipt = ReceiptVMGenerator.Generate();
		HttpResponseMessage responseMessage = new(statusCode);

		_httpMessageHandlerMock
			.Protected()
			.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
			.ReturnsAsync(responseMessage);

		// Act & Assert
		await Assert.ThrowsAsync<HttpRequestException>(() => _receiptClient.CreateReceiptAsync(receipt));
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
		ReceiptVM? result = await _receiptClient.GetReceiptByIdAsync(receipt.Id!.Value);

		// Assert
		Assert.NotNull(result);
		Assert.Equal(receipt, result);
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
		await Assert.ThrowsAsync<HttpRequestException>(() => _receiptClient.GetReceiptByIdAsync(Guid.NewGuid()));
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
		List<ReceiptVM>? result = await _receiptClient.GetAllReceiptsAsync();

		// Assert
		Assert.NotNull(result);
		Assert.Equal(receipts.Count, result.Count);
		Assert.All(receipts, r => Assert.Contains(result, res => res.Id == r.Id));
		Assert.All(result, res => Assert.Contains(receipts, r => r.Id == res.Id));
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
		await Assert.ThrowsAsync<HttpRequestException>(() => _receiptClient.GetAllReceiptsAsync());
	}

	[Fact]
	public async Task UpdateReceipt_ShouldReturnTrueOnSuccess()
	{
		// Arrange
		ReceiptVM receipt = ReceiptVMGenerator.Generate();

		HttpResponseMessage responseMessage = new(HttpStatusCode.OK);

		_httpMessageHandlerMock
			.Protected()
			.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
			.ReturnsAsync(responseMessage);

		// Act
		bool result = await _receiptClient.UpdateReceiptAsync(receipt);

		// Assert
		Assert.True(result);
	}

	[Theory]
	[InlineData(HttpStatusCode.Ambiguous)] // 3xx
	[InlineData(HttpStatusCode.BadRequest)] // 4xx
	[InlineData(HttpStatusCode.InternalServerError)] // 5xx
	public async Task UpdateReceipt_ShouldThrowHttpRequestExceptionOnNonSuccessStatusCode(HttpStatusCode statusCode)
	{
		// Arrange
		ReceiptVM receipt = ReceiptVMGenerator.Generate();
		HttpResponseMessage responseMessage = new(statusCode);

		_httpMessageHandlerMock
			.Protected()
			.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
			.ReturnsAsync(responseMessage);

		// Act & Assert
		await Assert.ThrowsAsync<HttpRequestException>(() => _receiptClient.UpdateReceiptAsync(receipt));
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
		bool result = await _receiptClient.DeleteReceiptsAsync(ids);

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
		await Assert.ThrowsAsync<HttpRequestException>(() => _receiptClient.DeleteReceiptsAsync(ids));
	}
}