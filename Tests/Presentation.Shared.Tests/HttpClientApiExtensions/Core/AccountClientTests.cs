using Moq;
using Moq.Protected;
using SampleData.ViewModels.Core;
using Shared.HttpClientApiExtensions.Core;
using Shared.ViewModels.Core;
using System.Net;
using System.Net.Http.Json;

namespace Presentation.Shared.Tests.HttpClientApiExtensions.Core;

public class AccountClientTests
{
	private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
	private readonly HttpClient _httpClient;

	public AccountClientTests()
	{
		_httpMessageHandlerMock = new Mock<HttpMessageHandler>();
		_httpClient = new HttpClient(_httpMessageHandlerMock.Object) { BaseAddress = new Uri("http://localhost/api/") };
	}

	[Fact]
	public async Task CreateAccounts_ShouldReturnCreatedAccounts()
	{
		// Arrange
		List<AccountVM> accounts =
		[
			new AccountVM
			{
				AccountCode = "123456",
				Name = "Test Account",
				IsActive = true
			},
			new AccountVM
			{
				AccountCode = "654321",
				Name = "Test Account 2",
				IsActive = true
			}
		];

		List<AccountVM> createdAccounts = [
			new AccountVM
			{
				Id = Guid.NewGuid(),
				AccountCode = accounts[0].AccountCode,
				Name = accounts[0].Name,
				IsActive = accounts[0].IsActive
			},
			new AccountVM
			{
				Id = Guid.NewGuid(),
				AccountCode = accounts[1].AccountCode,
				Name = accounts[1].Name,
				IsActive = accounts[1].IsActive
			}
		];

		HttpResponseMessage responseMessage = new(HttpStatusCode.OK)
		{
			Content = JsonContent.Create(createdAccounts)
		};

		_httpMessageHandlerMock
			.Protected()
			.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
			.ReturnsAsync(responseMessage);

		// Act
		List<AccountVM>? result = await _httpClient.CreateAccountsAsync(accounts);

		// Assert
		Assert.NotNull(result);
		Assert.Equal(accounts.Count, result.Count);
		Assert.Equal(createdAccounts.Count, result.Count);
		Assert.All(result, r => Assert.Contains(createdAccounts, a => a.Id == r.Id));
		Assert.All(createdAccounts, a => Assert.Contains(result, r => r.Id == a.Id));
	}

	[Fact]
	public async Task GetAccountById_ShouldReturnAccount()
	{
		// Arrange
		AccountVM account = AccountVMGenerator.Generate();

		HttpResponseMessage responseMessage = new(HttpStatusCode.OK)
		{
			Content = JsonContent.Create(account)
		};

		_httpMessageHandlerMock
			.Protected()
			.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
			.ReturnsAsync(responseMessage);

		// Act
		AccountVM? result = await _httpClient.GetAccountByIdAsync(account.Id!.Value);

		// Assert
		Assert.NotNull(result);
		Assert.Equal(account, result);
	}

	[Theory]
	[InlineData(HttpStatusCode.Ambiguous)] // 3xx
	[InlineData(HttpStatusCode.BadRequest)] // 4xx
	[InlineData(HttpStatusCode.InternalServerError)] // 5xx
	public async Task GetAccountById_ShouldThrowHttpRequestExceptionOnNonSuccessStatusCode(HttpStatusCode statusCode)
	{
		// Arrange
		HttpResponseMessage responseMessage = new(statusCode);

		_httpMessageHandlerMock
			.Protected()
			.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
			.ReturnsAsync(responseMessage);

		// Act & Assert
		await Assert.ThrowsAsync<HttpRequestException>(() => _httpClient.GetAccountByIdAsync(Guid.NewGuid()));
	}

	[Fact]
	public async Task GetAllAccounts_ShouldReturnAllAccounts()
	{
		// Arrange
		List<AccountVM> accounts = AccountVMGenerator.GenerateList(2);

		HttpResponseMessage responseMessage = new(HttpStatusCode.OK)
		{
			Content = JsonContent.Create(accounts)
		};

		_httpMessageHandlerMock
			.Protected()
			.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
			.ReturnsAsync(responseMessage);

		// Act
		List<AccountVM>? result = await _httpClient.GetAllAccountsAsync();

		// Assert
		Assert.NotNull(result);
		Assert.Equal(accounts.Count, result.Count);
		Assert.All(accounts, a => Assert.Contains(result, r => r.Id == a.Id));
		Assert.All(result, r => Assert.Contains(accounts, a => a.Id == r.Id));
	}

	[Theory]
	[InlineData(HttpStatusCode.Ambiguous)] // 3xx
	[InlineData(HttpStatusCode.BadRequest)] // 4xx
	[InlineData(HttpStatusCode.InternalServerError)] // 5xx
	public async Task GetAllAccounts_ShouldThrowHttpRequestExceptionOnNonSuccessStatusCode(HttpStatusCode statusCode)
	{
		// Arrange
		HttpResponseMessage responseMessage = new(statusCode);

		_httpMessageHandlerMock
			.Protected()
			.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
			.ReturnsAsync(responseMessage);

		// Act & Assert
		await Assert.ThrowsAsync<HttpRequestException>(() => _httpClient.GetAllAccountsAsync());
	}

	[Fact]
	public async Task UpdateAccounts_ShouldReturnTrueOnSuccess()
	{
		// Arrange
		List<AccountVM> accounts = AccountVMGenerator.GenerateList(2);

		HttpResponseMessage responseMessage = new(HttpStatusCode.OK);

		_httpMessageHandlerMock
			.Protected()
			.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
			.ReturnsAsync(responseMessage);

		// Act
		bool result = await _httpClient.UpdateAccountsAsync(accounts);

		// Assert
		Assert.True(result);
	}

	[Theory]
	[InlineData(HttpStatusCode.Ambiguous)] // 3xx
	[InlineData(HttpStatusCode.BadRequest)] // 4xx
	[InlineData(HttpStatusCode.InternalServerError)] // 5xx
	public async Task UpdateAccounts_ShouldThrowHttpRequestExceptionOnNonSuccessStatusCode(HttpStatusCode statusCode)
	{
		// Arrange
		List<AccountVM> accounts = AccountVMGenerator.GenerateList(2);
		HttpResponseMessage responseMessage = new(statusCode);

		_httpMessageHandlerMock
			.Protected()
			.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
			.ReturnsAsync(responseMessage);

		// Act & Assert
		await Assert.ThrowsAsync<HttpRequestException>(() => _httpClient.UpdateAccountsAsync(accounts));
	}

	[Fact]
	public async Task DeleteAccounts_ShouldReturnTrueOnSuccess()
	{
		// Arrange
		List<Guid> ids = AccountVMGenerator.GenerateList(3).Select(a => a.Id!.Value).ToList();

		HttpResponseMessage responseMessage = new(HttpStatusCode.OK);

		_httpMessageHandlerMock
			.Protected()
			.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
			.ReturnsAsync(responseMessage);

		// Act
		bool result = await _httpClient.DeleteAccountsAsync(ids);

		// Assert
		Assert.True(result);
	}

	[Theory]
	[InlineData(HttpStatusCode.Ambiguous)] // 3xx
	[InlineData(HttpStatusCode.BadRequest)] // 4xx
	[InlineData(HttpStatusCode.InternalServerError)] // 5xx
	public async Task DeleteAccounts_ShouldThrowHttpRequestExceptionOnNonSuccessStatusCode(HttpStatusCode statusCode)
	{
		// Arrange
		List<Guid> ids = AccountVMGenerator.GenerateList(3).Select(a => a.Id!.Value).ToList();
		HttpResponseMessage responseMessage = new(statusCode);

		_httpMessageHandlerMock
			.Protected()
			.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
			.ReturnsAsync(responseMessage);

		// Act & Assert
		await Assert.ThrowsAsync<HttpRequestException>(() => _httpClient.DeleteAccountsAsync(ids));
	}
}
