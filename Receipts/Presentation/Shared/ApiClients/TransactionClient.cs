using Shared.ViewModels;
using System.Net.Http.Json;

namespace Shared.ApiClients;

public static class TransactionClient
{
	private static readonly HttpClient _httpClient = new() { BaseAddress = new Uri("http://localhost:5136/api/") };

	public static async Task<TransactionVM?> CreateTransaction(TransactionVM model)
	{
		HttpResponseMessage response = await _httpClient.PostAsJsonAsync("transactions", model);
		response.EnsureSuccessStatusCode();
		return await response.Content.ReadFromJsonAsync<TransactionVM>();
	}

	public static async Task<TransactionVM?> GetTransactionById(Guid id)
	{
		HttpResponseMessage response = await _httpClient.GetAsync($"transactions/{id}");
		response.EnsureSuccessStatusCode();
		return await response.Content.ReadFromJsonAsync<TransactionVM>();
	}

	public static async Task<List<TransactionVM>?> GetAllTransactions()
	{
		HttpResponseMessage response = await _httpClient.GetAsync("transactions");
		response.EnsureSuccessStatusCode();
		return await response.Content.ReadFromJsonAsync<List<TransactionVM>>();
	}

	public static async Task<List<TransactionVM>?> GetTransactionsByDateRange(DateOnly startDate, DateOnly endDate)
	{
		HttpResponseMessage response = await _httpClient.GetAsync($"transactions/by-date?startDate={startDate:d}&endDate={endDate:d}");
		response.EnsureSuccessStatusCode();
		return await response.Content.ReadFromJsonAsync<List<TransactionVM>>();
	}

	public static async Task<List<TransactionVM>?> GetTransactionsByMoneyRange(decimal minAmount, decimal maxAmount)
	{
		minAmount = Math.Floor(minAmount * 100) / 100;
		maxAmount = Math.Ceiling(maxAmount * 100) / 100;

		HttpResponseMessage response = await _httpClient.GetAsync($"transactions/by-money?minAmount={minAmount}&maxAmount={maxAmount}");
		response.EnsureSuccessStatusCode();
		return await response.Content.ReadFromJsonAsync<List<TransactionVM>>();
	}

	public static async Task<bool> UpdateTransaction(TransactionVM model)
	{
		HttpResponseMessage response = await _httpClient.PutAsJsonAsync("transactions", model);
		return response.IsSuccessStatusCode;
	}

	public static async Task<bool> DeleteTransactions(List<Guid> ids)
	{
		HttpResponseMessage response = await _httpClient.PostAsJsonAsync("transactions/delete", ids);
		return response.IsSuccessStatusCode;
	}
}