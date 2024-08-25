using Shared.ViewModels;
using System.Net.Http.Json;

namespace Shared.ApiClients;

public static class ReceiptClient
{
	private static readonly HttpClient _httpClient = new() { BaseAddress = new Uri("http://localhost:5136/api/") };

	public static async Task<ReceiptVM?> CreateReceipt(ReceiptVM model)
	{
		HttpResponseMessage response = await _httpClient.PostAsJsonAsync("receipts", model);
		response.EnsureSuccessStatusCode();
		return await response.Content.ReadFromJsonAsync<ReceiptVM>();
	}

	public static async Task<ReceiptVM?> GetReceiptById(Guid id)
	{
		HttpResponseMessage response = await _httpClient.GetAsync($"receipts/{id}");
		response.EnsureSuccessStatusCode();
		return await response.Content.ReadFromJsonAsync<ReceiptVM>();
	}

	public static async Task<List<ReceiptVM>?> GetAllReceipts()
	{
		HttpResponseMessage response = await _httpClient.GetAsync("receipts");
		response.EnsureSuccessStatusCode();
		return await response.Content.ReadFromJsonAsync<List<ReceiptVM>>();
	}

	public static async Task<List<ReceiptVM>?> GetReceiptsByLocation(string location)
	{
		HttpResponseMessage response = await _httpClient.GetAsync($"receipts/by-location/{location}");
		response.EnsureSuccessStatusCode();
		return await response.Content.ReadFromJsonAsync<List<ReceiptVM>>();
	}

	public static async Task<List<ReceiptVM>?> GetReceiptsByDateRange(DateOnly startDate, DateOnly endDate)
	{
		HttpResponseMessage response = await _httpClient.GetAsync($"receipts/by-date?startDate={startDate:d}&endDate={endDate:d}");
		response.EnsureSuccessStatusCode();
		return await response.Content.ReadFromJsonAsync<List<ReceiptVM>>();
	}

	public static async Task<List<ReceiptVM>?> GetReceiptsByMoneyRange(decimal minAmount, decimal maxAmount)
	{
		minAmount = Math.Floor(minAmount * 100) / 100;
		maxAmount = Math.Ceiling(maxAmount * 100) / 100;

		HttpResponseMessage response = await _httpClient.GetAsync($"receipts/by-money?minAmount={minAmount}&maxAmount={maxAmount}");
		response.EnsureSuccessStatusCode();
		return await response.Content.ReadFromJsonAsync<List<ReceiptVM>>();
	}

	public static async Task<bool> UpdateReceipt(ReceiptVM model)
	{
		HttpResponseMessage response = await _httpClient.PutAsJsonAsync("receipts", model);
		return response.IsSuccessStatusCode;
	}

	public static async Task<bool> DeleteReceipts(List<Guid> ids)
	{
		HttpResponseMessage response = await _httpClient.PostAsJsonAsync("receipts/delete", ids);
		return response.IsSuccessStatusCode;
	}
}