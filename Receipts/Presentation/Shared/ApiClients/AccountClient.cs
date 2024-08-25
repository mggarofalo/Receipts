using Shared.ViewModels;
using System.Net.Http.Json;

namespace Shared.ApiClients;

public static class AccountClient
{
	private static readonly HttpClient _httpClient = new() { BaseAddress = new Uri("http://localhost:5136/api/") };

	public static async Task<AccountVM?> CreateAccount(AccountVM model)
	{
		HttpResponseMessage response = await _httpClient.PostAsJsonAsync("accounts", model);
		response.EnsureSuccessStatusCode();
		return await response.Content.ReadFromJsonAsync<AccountVM>();
	}

	public static async Task<AccountVM?> GetAccountById(Guid id)
	{
		HttpResponseMessage response = await _httpClient.GetAsync($"accounts/{id}");
		response.EnsureSuccessStatusCode();
		return await response.Content.ReadFromJsonAsync<AccountVM>();
	}

	public static async Task<List<AccountVM>?> GetAllAccounts()
	{
		HttpResponseMessage response = await _httpClient.GetAsync("accounts");
		response.EnsureSuccessStatusCode();
		return await response.Content.ReadFromJsonAsync<List<AccountVM>>();
	}

	public static async Task<List<AccountVM>?> GetAccountsByAccountCode(string accountCode)
	{
		HttpResponseMessage response = await _httpClient.GetAsync($"accounts/by-account-code/{accountCode}");
		response.EnsureSuccessStatusCode();
		return await response.Content.ReadFromJsonAsync<List<AccountVM>>();
	}

	public static async Task<List<AccountVM>?> GetAccountsByName(string name)
	{
		HttpResponseMessage response = await _httpClient.GetAsync($"accounts/by-name/{name}");
		response.EnsureSuccessStatusCode();
		return await response.Content.ReadFromJsonAsync<List<AccountVM>>();
	}

	public static async Task<bool> UpdateAccount(AccountVM model)
	{
		HttpResponseMessage response = await _httpClient.PutAsJsonAsync("accounts", model);
		return response.IsSuccessStatusCode;
	}

	public static async Task<bool> DeleteAccount(Guid id)
	{
		HttpResponseMessage response = await _httpClient.DeleteAsync($"accounts/{id}");
		return response.IsSuccessStatusCode;
	}
}