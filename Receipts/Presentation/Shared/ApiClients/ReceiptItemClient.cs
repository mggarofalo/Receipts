using Shared.ViewModels.Core;
using System.Net.Http.Json;

namespace Shared.ApiClients;

public static class ReceiptItemClient
{
	private static readonly HttpClient _httpClient = new() { BaseAddress = new Uri("http://localhost:5136/api/") };

	public static async Task<ReceiptItemVM?> CreateReceiptItem(ReceiptItemVM model)
	{
		HttpResponseMessage response = await _httpClient.PostAsJsonAsync("receiptitems", model);
		response.EnsureSuccessStatusCode();
		return await response.Content.ReadFromJsonAsync<ReceiptItemVM>();
	}

	public static async Task<ReceiptItemVM?> GetReceiptItemById(Guid id)
	{
		HttpResponseMessage response = await _httpClient.GetAsync($"receiptitems/{id}");
		response.EnsureSuccessStatusCode();
		return await response.Content.ReadFromJsonAsync<ReceiptItemVM>();
	}

	public static async Task<List<ReceiptItemVM>?> GetAllReceiptItems()
	{
		HttpResponseMessage response = await _httpClient.GetAsync("receiptitems");
		response.EnsureSuccessStatusCode();
		return await response.Content.ReadFromJsonAsync<List<ReceiptItemVM>>();
	}

	public static async Task<List<ReceiptItemVM>?> GetReceiptItemsByReceiptId(Guid receiptId)
	{
		HttpResponseMessage response = await _httpClient.GetAsync($"receiptitems/by-receipt-id/{receiptId}");
		response.EnsureSuccessStatusCode();
		return await response.Content.ReadFromJsonAsync<List<ReceiptItemVM>>();
	}

	public static async Task<bool> UpdateReceiptItem(ReceiptItemVM model)
	{
		HttpResponseMessage response = await _httpClient.PutAsJsonAsync("receiptitems", model);
		return response.IsSuccessStatusCode;
	}

	public static async Task<bool> DeleteReceiptItems(List<Guid> ids)
	{
		HttpResponseMessage response = await _httpClient.PostAsJsonAsync("receiptitems/delete", ids);
		return response.IsSuccessStatusCode;
	}
}