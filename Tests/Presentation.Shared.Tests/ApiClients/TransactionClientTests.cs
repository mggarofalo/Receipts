using Moq;
using Moq.Protected;
using Shared.ViewModels.Core;
using Shared.ApiClients;
using System.Net;
using System.Net.Http.Json;
using SampleData.ViewModels.Core;

namespace Presentation.Shared.Tests.ApiClients;

public class TransactionClientTests
{
	private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
	private readonly HttpClient _httpClient;
	private readonly TransactionClient _transactionClient;

	public TransactionClientTests()
	{
		_httpMessageHandlerMock = new Mock<HttpMessageHandler>();
		_httpClient = new HttpClient(_httpMessageHandlerMock.Object) { BaseAddress = new Uri("http://localhost/api/") };
		_transactionClient = new TransactionClient(_httpClient);
	}

	[Fact]
	public void NoArgsConstructor_ShouldInitializeTransactionClient()
	{
		// Act
		TransactionClient transactionClient = new();

		// Assert
		Assert.NotNull(transactionClient);
	}

	[Fact]
	public async Task CreateTransaction_ShouldReturnCreatedTransaction()
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
		TransactionVM? result = await _transactionClient.CreateTransactionAsync(transaction);

		// Assert
		Assert.NotNull(result);
		Assert.Equal(transaction.Id, result.Id);
	}

	[Theory]
	[InlineData(HttpStatusCode.Ambiguous)] // 3xx
	[InlineData(HttpStatusCode.BadRequest)] // 4xx
	[InlineData(HttpStatusCode.InternalServerError)] // 5xx
	public async Task CreateTransaction_ShouldThrowHttpRequestExceptionOnNonSuccessStatusCode(HttpStatusCode statusCode)
	{
		// Arrange
		TransactionVM transaction = TransactionVMGenerator.Generate();
		HttpResponseMessage responseMessage = new(statusCode);

		_httpMessageHandlerMock
			.Protected()
			.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
			.ReturnsAsync(responseMessage);

		// Act & Assert
		await Assert.ThrowsAsync<HttpRequestException>(() => _transactionClient.CreateTransactionAsync(transaction));
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
		TransactionVM? result = await _transactionClient.GetTransactionByIdAsync(transaction.Id!.Value);

		// Assert
		Assert.NotNull(result);
		Assert.Equal(transaction, result);
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
		await Assert.ThrowsAsync<HttpRequestException>(() => _transactionClient.GetTransactionByIdAsync(Guid.NewGuid()));
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
		List<TransactionVM>? result = await _transactionClient.GetAllTransactionsAsync();

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
		await Assert.ThrowsAsync<HttpRequestException>(() => _transactionClient.GetAllTransactionsAsync());
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
		List<TransactionVM>? result = await _transactionClient.GetTransactionsByReceiptIdAsync(receiptId);

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
		await Assert.ThrowsAsync<HttpRequestException>(() => _transactionClient.GetTransactionsByReceiptIdAsync(receiptId));
	}

	[Fact]
	public async Task UpdateTransaction_ShouldReturnTrueOnSuccess()
	{
		// Arrange
		TransactionVM transaction = TransactionVMGenerator.Generate();

		HttpResponseMessage responseMessage = new(HttpStatusCode.OK);

		_httpMessageHandlerMock
			.Protected()
			.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
			.ReturnsAsync(responseMessage);

		// Act
		bool result = await _transactionClient.UpdateTransactionAsync(transaction);

		// Assert
		Assert.True(result);
	}

	[Theory]
	[InlineData(HttpStatusCode.Ambiguous)] // 3xx
	[InlineData(HttpStatusCode.BadRequest)] // 4xx
	[InlineData(HttpStatusCode.InternalServerError)] // 5xx
	public async Task UpdateTransaction_ShouldThrowHttpRequestExceptionOnNonSuccessStatusCode(HttpStatusCode statusCode)
	{
		// Arrange
		TransactionVM transaction = TransactionVMGenerator.Generate();
		HttpResponseMessage responseMessage = new(statusCode);

		_httpMessageHandlerMock
			.Protected()
			.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
			.ReturnsAsync(responseMessage);

		// Act & Assert
		await Assert.ThrowsAsync<HttpRequestException>(() => _transactionClient.UpdateTransactionAsync(transaction));
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
		bool result = await _transactionClient.DeleteTransactionsAsync(ids);

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
		await Assert.ThrowsAsync<HttpRequestException>(() => _transactionClient.DeleteTransactionsAsync(ids));
	}
}