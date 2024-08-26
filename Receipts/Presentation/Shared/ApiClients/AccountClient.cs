using Shared.ViewModels;
using System.Net.Http.Json;

namespace Shared.ApiClients;

public static class AccountClient
{
	private static readonly HttpClient _httpClient = new() { BaseAddress = new Uri("http://localhost:5136/api/") };

	public static async Task<List<AccountVM>?> CreateAccounts(List<AccountVM> models)
	{
		HttpResponseMessage response = await _httpClient.PostAsJsonAsync("accounts", models);
		response.EnsureSuccessStatusCode();
		return await response.Content.ReadFromJsonAsync<List<AccountVM>?>();
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

	public static async Task<bool> UpdateAccounts(List<AccountVM> models)
	{
		HttpResponseMessage response = await _httpClient.PutAsJsonAsync("accounts", models);
		return response.IsSuccessStatusCode;
	}

	public static async Task<bool> DeleteAccounts(List<Guid> ids)
	{
		HttpResponseMessage response = await _httpClient.PostAsJsonAsync("accounts/delete", ids);
		return response.IsSuccessStatusCode;
	}
}