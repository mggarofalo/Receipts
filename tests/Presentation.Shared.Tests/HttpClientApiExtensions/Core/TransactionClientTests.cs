using Moq;
using Moq.Protected;
using Shared.ViewModels.Core;
using System.Net;
using System.Net.Http.Json;
using SampleData.ViewModels.Core;
using Shared.HttpClientApiExtensions.Core;

namespace Presentation.Shared.Tests.HttpClientApiExtensions.Core;

public class TransactionClientTests
{
	private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
	private readonly HttpClient _httpClient;

	public TransactionClientTests()
	{
		_httpMessageHandlerMock = new Mock<HttpMessageHandler>();
		_httpClient = new HttpClient(_httpMessageHandlerMock.Object) { BaseAddress = new Uri("http://localhost/api/") };
	}

	[Fact]
	public async Task CreateTransactions_ShouldReturnCreatedTransactions()
	{
		// Arrange
		List<TransactionVM> transactions = TransactionVMGenerator.GenerateList(2);

		HttpResponseMessage responseMessage = new(HttpStatusCode.OK)
		{
			Content = JsonContent.Create(transactions)
		};

		_httpMessageHandlerMock
			.Protected()
			.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
			.ReturnsAsync(responseMessage);

		// Act
		List<TransactionVM>? result = await _httpClient.CreateTransactionsAsync(transactions);

		// Assert
		Assert.NotNull(result);
		Assert.Equal(transactions.Count, result.Count);
		Assert.All(transactions, t => Assert.Contains(result, res => res.Id == t.Id));
	}

	[Theory]
	[InlineData(HttpStatusCode.Ambiguous)] // 3xx
	[InlineData(HttpStatusCode.BadRequest)] // 4xx
	[InlineData(HttpStatusCode.InternalServerError)] // 5xx
	public async Task CreateTransactions_ShouldThrowHttpRequestExceptionOnNonSuccessStatusCode(HttpStatusCode statusCode)
	{
		// Arrange
		List<TransactionVM> transactions = TransactionVMGenerator.GenerateList(2);
		HttpResponseMessage responseMessage = new(statusCode);

		_httpMessageHandlerMock
			.Protected()
			.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
			.ReturnsAsync(responseMessage);

		// Act & Assert
		await Assert.ThrowsAsync<HttpRequestException>(() => _httpClient.CreateTransactionsAsync(transactions));
	}

	[Fact]
	public async Task GetTransactionById_ShouldReturnTransaction()
	{
		// Arrange
		TransactionVM transaction = TransactionVMGenerator.Generate();

		HttpResponseMessage responseMessage = new(HttpStatusCode.OK)
		{
			Content = JsonContent.Create(transaction)
		};

		_httpMessageHandlerMock
			.Protected()
			.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
			.ReturnsAsync(responseMessage);

		// Act
		TransactionVM? result = await _httpClient.GetTransactionByIdAsync(transaction.Id!.Value);

		// Assert
		Assert.NotNull(result);
		Assert.Equal(transaction.Id, result.Id);
	}

	[Theory]
	[InlineData(HttpStatusCode.Ambiguous)] // 3xx
	[InlineData(HttpStatusCode.BadRequest)] // 4xx
	[InlineData(HttpStatusCode.InternalServerError)] // 5xx
	public async Task GetTransactionById_ShouldThrowHttpRequestExceptionOnNonSuccessStatusCode(HttpStatusCode statusCode)
	{
		// Arrange
		HttpResponseMessage responseMessage = new(statusCode);

		_httpMessageHandlerMock
			.Protected()
			.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
			.ReturnsAsync(responseMessage);

		// Act & Assert
		await Assert.ThrowsAsync<HttpRequestException>(() => _httpClient.GetTransactionByIdAsync(Guid.NewGuid()));
	}

	[Fact]
	public async Task GetAllTransactions_ShouldReturnAllTransactions()
	{
		// Arrange
		List<TransactionVM> transactions = TransactionVMGenerator.GenerateList(3);

		HttpResponseMessage responseMessage = new(HttpStatusCode.OK)
		{
			Content = JsonContent.Create(transactions)
		};

		_httpMessageHandlerMock
			.Protected()
			.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
			.ReturnsAsync(responseMessage);

		// Act
		List<TransactionVM>? result = await _httpClient.GetAllTransactionsAsync();

		// Assert
		Assert.NotNull(result);
		Assert.Equal(transactions.Count, result.Count);
		Assert.All(transactions, t => Assert.Contains(result, res => res.Id == t.Id));
		Assert.All(result, res => Assert.Contains(transactions, t => t.Id == res.Id));
	}

	[Theory]
	[InlineData(HttpStatusCode.Ambiguous)] // 3xx
	[InlineData(HttpStatusCode.BadRequest)] // 4xx
	[InlineData(HttpStatusCode.InternalServerError)] // 5xx
	public async Task GetAllTransactions_ShouldThrowHttpRequestExceptionOnNonSuccessStatusCode(HttpStatusCode statusCode)
	{
		// Arrange
		HttpResponseMessage responseMessage = new(statusCode);

		_httpMessageHandlerMock
			.Protected()
			.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
			.ReturnsAsync(responseMessage);

		// Act & Assert
		await Assert.ThrowsAsync<HttpRequestException>(() => _httpClient.GetAllTransactionsAsync());
	}

	[Fact]
	public async Task GetTransactionsByReceiptId_ShouldReturnTransactions()
	{
		// Arrange
		List<TransactionVM> transactions = TransactionVMGenerator.GenerateList(3);
		Guid receiptId = Guid.NewGuid();

		HttpResponseMessage responseMessage = new(HttpStatusCode.OK)
		{
			Content = JsonContent.Create(transactions)
		};

		_httpMessageHandlerMock
			.Protected()
			.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
			.ReturnsAsync(responseMessage);

		// Act
		List<TransactionVM>? result = await _httpClient.GetTransactionsByReceiptIdAsync(receiptId);

		// Assert
		Assert.NotNull(result);
		Assert.Equal(transactions.Count, result.Count);
		Assert.All(transactions, t => Assert.Contains(result, res => res.Id == t.Id));
		Assert.All(result, res => Assert.Contains(transactions, t => t.Id == res.Id));
	}

	[Theory]
	[InlineData(HttpStatusCode.Ambiguous)] // 3xx
	[InlineData(HttpStatusCode.BadRequest)] // 4xx
	[InlineData(HttpStatusCode.InternalServerError)] // 5xx
	public async Task GetTransactionsByReceiptId_ShouldThrowHttpRequestExceptionOnNonSuccessStatusCode(HttpStatusCode statusCode)
	{
		// Arrange
		Guid receiptId = Guid.NewGuid();
		HttpResponseMessage responseMessage = new(statusCode);

		_httpMessageHandlerMock
			.Protected()
			.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
			.ReturnsAsync(responseMessage);

		// Act & Assert
		await Assert.ThrowsAsync<HttpRequestException>(() => _httpClient.GetTransactionsByReceiptIdAsync(receiptId));
	}

	[Fact]
	public async Task UpdateTransactions_ShouldReturnTrueOnSuccess()
	{
		// Arrange
		List<TransactionVM> transactions = TransactionVMGenerator.GenerateList(2);

		HttpResponseMessage responseMessage = new(HttpStatusCode.OK);

		_httpMessageHandlerMock
			.Protected()
			.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
			.ReturnsAsync(responseMessage);

		// Act
		bool result = await _httpClient.UpdateTransactionsAsync(transactions);

		// Assert
		Assert.True(result);
	}

	[Theory]
	[InlineData(HttpStatusCode.Ambiguous)] // 3xx
	[InlineData(HttpStatusCode.BadRequest)] // 4xx
	[InlineData(HttpStatusCode.InternalServerError)] // 5xx
	public async Task UpdateTransactions_ShouldThrowHttpRequestExceptionOnNonSuccessStatusCode(HttpStatusCode statusCode)
	{
		// Arrange
		List<TransactionVM> transactions = TransactionVMGenerator.GenerateList(2);
		HttpResponseMessage responseMessage = new(statusCode);

		_httpMessageHandlerMock
			.Protected()
			.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
			.ReturnsAsync(responseMessage);

		// Act & Assert
		await Assert.ThrowsAsync<HttpRequestException>(() => _httpClient.UpdateTransactionsAsync(transactions));
	}

	[Fact]
	public async Task DeleteTransactions_ShouldReturnTrueOnSuccess()
	{
		// Arrange
		List<Guid> ids = TransactionVMGenerator.GenerateList(3).Select(t => t.Id!.Value).ToList();

		HttpResponseMessage responseMessage = new(HttpStatusCode.OK);

		_httpMessageHandlerMock
			.Protected()
			.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
			.ReturnsAsync(responseMessage);

		// Act
		bool result = await _httpClient.DeleteTransactionsAsync(ids);

		// Assert
		Assert.True(result);
	}

	[Theory]
	[InlineData(HttpStatusCode.Ambiguous)] // 3xx
	[InlineData(HttpStatusCode.BadRequest)] // 4xx
	[InlineData(HttpStatusCode.InternalServerError)] // 5xx
	public async Task DeleteTransactions_ShouldThrowHttpRequestExceptionOnNonSuccessStatusCode(HttpStatusCode statusCode)
	{
		// Arrange
		List<Guid> ids = TransactionVMGenerator.GenerateList(3).Select(t => t.Id!.Value).ToList();
		HttpResponseMessage responseMessage = new(statusCode);

		_httpMessageHandlerMock
			.Protected()
			.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
			.ReturnsAsync(responseMessage);

		// Act & Assert
		await Assert.ThrowsAsync<HttpRequestException>(() => _httpClient.DeleteTransactionsAsync(ids));
	}
}