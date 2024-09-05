using Shared.ViewModels.Core;
using System.Net.Http.Json;

namespace Shared.ApiClients;

public class AccountClient(HttpClient? httpClient = default)
{
	private const string HttpClientBaseAddress = "http://localhost:5136/api/";
	private readonly HttpClient _httpClient = httpClient ?? new() { BaseAddress = new Uri(HttpClientBaseAddress) };

	public async Task<List<AccountVM>?> CreateAccountsAsync(List<AccountVM> models, CancellationToken cancellationToken = default)
	{
		HttpResponseMessage response = await _httpClient.PostAsJsonAsync("accounts", models, cancellationToken);
		response.EnsureSuccessStatusCode();
		return await response.Content.ReadFromJsonAsync<List<AccountVM>>(cancellationToken: cancellationToken);
	}

	public async Task<AccountVM?> GetAccountByIdAsync(Guid id, CancellationToken cancellationToken = default)
	{
		HttpResponseMessage response = await _httpClient.GetAsync($"accounts/{id}", cancellationToken);
		response.EnsureSuccessStatusCode();
		return await response.Content.ReadFromJsonAsync<AccountVM>(cancellationToken: cancellationToken);
	}

	public async Task<List<AccountVM>?> GetAllAccountsAsync(CancellationToken cancellationToken = default)
	{
		HttpResponseMessage response = await _httpClient.GetAsync("accounts", cancellationToken);
		response.EnsureSuccessStatusCode();
		return await response.Content.ReadFromJsonAsync<List<AccountVM>>(cancellationToken: cancellationToken);
	}

	public async Task<bool> UpdateAccountsAsync(List<AccountVM> models, CancellationToken cancellationToken = default)
	{
		HttpResponseMessage response = await _httpClient.PutAsJsonAsync("accounts", models, cancellationToken);
		response.EnsureSuccessStatusCode();
		return true;
	}

	public async Task<bool> DeleteAccountsAsync(List<Guid> ids, CancellationToken cancellationToken = default)
	{
		HttpResponseMessage response = await _httpClient.PostAsJsonAsync("accounts/delete", ids, cancellationToken);
		response.EnsureSuccessStatusCode();
		return true;
	}
}