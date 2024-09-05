using Shared.ViewModels.Core;
using System.Net.Http.Json;

namespace Shared.ApiClients;

public class ReceiptItemClient(HttpClient? httpClient = default)
{
	private const string HttpClientBaseAddress = "http://localhost:5136/api/";
	private readonly HttpClient _httpClient = httpClient ?? new() { BaseAddress = new Uri(HttpClientBaseAddress) };

	public async Task<ReceiptItemVM?> CreateReceiptItemAsync(ReceiptItemVM model, CancellationToken cancellationToken = default)
	{
		HttpResponseMessage response = await _httpClient.PostAsJsonAsync("receiptitems", model, cancellationToken);
		response.EnsureSuccessStatusCode();
		return await response.Content.ReadFromJsonAsync<ReceiptItemVM>(cancellationToken: cancellationToken);
	}

	public async Task<ReceiptItemVM?> GetReceiptItemByIdAsync(Guid id, CancellationToken cancellationToken = default)
	{
		HttpResponseMessage response = await _httpClient.GetAsync($"receiptitems/{id}", cancellationToken);
		response.EnsureSuccessStatusCode();
		return await response.Content.ReadFromJsonAsync<ReceiptItemVM>(cancellationToken: cancellationToken);
	}

	public async Task<List<ReceiptItemVM>?> GetAllReceiptItemsAsync(CancellationToken cancellationToken = default)
	{
		HttpResponseMessage response = await _httpClient.GetAsync("receiptitems", cancellationToken);
		response.EnsureSuccessStatusCode();
		return await response.Content.ReadFromJsonAsync<List<ReceiptItemVM>>(cancellationToken: cancellationToken);
	}

	public async Task<List<ReceiptItemVM>?> GetReceiptItemsByReceiptIdAsync(Guid receiptId, CancellationToken cancellationToken = default)
	{
		HttpResponseMessage response = await _httpClient.GetAsync($"receiptitems/by-receipt-id/{receiptId}", cancellationToken);
		response.EnsureSuccessStatusCode();
		return await response.Content.ReadFromJsonAsync<List<ReceiptItemVM>>(cancellationToken: cancellationToken);
	}

	public async Task<bool> UpdateReceiptItemAsync(ReceiptItemVM model, CancellationToken cancellationToken = default)
	{
		HttpResponseMessage response = await _httpClient.PutAsJsonAsync("receiptitems", model, cancellationToken);
		response.EnsureSuccessStatusCode();
		return true;
	}

	public async Task<bool> DeleteReceiptItemsAsync(List<Guid> ids, CancellationToken cancellationToken = default)
	{
		HttpResponseMessage response = await _httpClient.PostAsJsonAsync("receiptitems/delete", ids, cancellationToken);
		response.EnsureSuccessStatusCode();
		return true;
	}
}