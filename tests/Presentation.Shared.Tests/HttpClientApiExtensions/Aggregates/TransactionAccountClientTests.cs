using FluentAssertions;
using Moq;
using Moq.Protected;
using SampleData.ViewModels.Aggregates;
using Shared.HttpClientApiExtensions.Aggregates;
using Shared.ViewModels.Aggregates;
using System.Net;
using System.Net.Http.Json;

namespace Presentation.Shared.Tests.HttpClientApiExtensions.Aggregates;

public class TransactionAccountClientTests
{
	private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
	private readonly HttpClient _httpClient;

	public TransactionAccountClientTests()
	{
		_httpMessageHandlerMock = new Mock<HttpMessageHandler>();
		_httpClient = new HttpClient(_httpMessageHandlerMock.Object) { BaseAddress = new Uri("http://localhost/api/") };
	}

	[Fact]
	public async Task GetTransactionAccountByTransactionId_ShouldReturnTransactionAccount()
	{
		// Arrange
		Guid transactionId = Guid.NewGuid();
		TransactionAccountVM transactionAccount = TransactionAccountVMGenerator.Generate();

		HttpResponseMessage responseMessage = new(HttpStatusCode.OK)
		{
			Content = JsonContent.Create(transactionAccount)
		};

		_httpMessageHandlerMock
			.Protected()
			.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
			.ReturnsAsync(responseMessage);

		// Act
		TransactionAccountVM? result = await _httpClient.GetTransactionAccountByTransactionIdAsync(transactionId);

		// Assert
		Assert.NotNull(result);
		result.Should().BeEquivalentTo(transactionAccount);
	}

	[Fact]
	public async Task GetTransactionAccountsByReceiptId_ShouldReturnListOfTransactionAccounts()
	{
		// Arrange
		Guid receiptId = Guid.NewGuid();
		List<TransactionAccountVM> transactionAccounts = TransactionAccountVMGenerator.GenerateList(3);

		HttpResponseMessage responseMessage = new(HttpStatusCode.OK)
		{
			Content = JsonContent.Create(transactionAccounts)
		};

		_httpMessageHandlerMock
			.Protected()
			.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
			.ReturnsAsync(responseMessage);

		// Act
		List<TransactionAccountVM>? result = await _httpClient.GetTransactionAccountsByReceiptIdAsync(receiptId);

		// Assert
		Assert.NotNull(result);
		Assert.Equal(transactionAccounts.Count, result.Count);
		result.Should().BeEquivalentTo(transactionAccounts);
	}

	[Theory]
	[InlineData(HttpStatusCode.Ambiguous)] // 3xx
	[InlineData(HttpStatusCode.BadRequest)] // 4xx
	[InlineData(HttpStatusCode.InternalServerError)] // 5xx
	public async Task GetTransactionAccountByTransactionId_ShouldThrowHttpRequestExceptionOnNonSuccessStatusCode(HttpStatusCode statusCode)
	{
		// Arrange
		Guid transactionId = Guid.NewGuid();
		HttpResponseMessage responseMessage = new(statusCode);

		_httpMessageHandlerMock
			.Protected()
			.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
			.ReturnsAsync(responseMessage);

		// Act & Assert
		await Assert.ThrowsAsync<HttpRequestException>(() => _httpClient.GetTransactionAccountByTransactionIdAsync(transactionId));
	}

	[Theory]
	[InlineData(HttpStatusCode.Ambiguous)] // 3xx
	[InlineData(HttpStatusCode.BadRequest)] // 4xx
	[InlineData(HttpStatusCode.InternalServerError)] // 5xx
	public async Task GetTransactionAccountsByReceiptId_ShouldThrowHttpRequestExceptionOnNonSuccessStatusCode(HttpStatusCode statusCode)
	{
		// Arrange
		Guid receiptId = Guid.NewGuid();
		HttpResponseMessage responseMessage = new(statusCode);

		_httpMessageHandlerMock
			.Protected()
			.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
			.ReturnsAsync(responseMessage);

		// Act & Assert
		await Assert.ThrowsAsync<HttpRequestException>(() => _httpClient.GetTransactionAccountsByReceiptIdAsync(receiptId));
	}
}
