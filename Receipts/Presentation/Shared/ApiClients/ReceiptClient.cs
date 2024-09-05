using Shared.ViewModels.Core;
using System.Net.Http.Json;

namespace Shared.ApiClients;

public class ReceiptClient(HttpClient? httpClient = default)
{
	private const string HttpClientBaseAddress = "http://localhost:5136/api/";
	private readonly HttpClient _httpClient = httpClient ?? new() { BaseAddress = new Uri(HttpClientBaseAddress) };

	public async Task<ReceiptVM?> CreateReceiptAsync(ReceiptVM model, CancellationToken cancellationToken = default)
	{
		HttpResponseMessage response = await _httpClient.PostAsJsonAsync("receipts", model, cancellationToken);
		response.EnsureSuccessStatusCode();
		return await response.Content.ReadFromJsonAsync<ReceiptVM>(cancellationToken: cancellationToken);
	}

	public async Task<ReceiptVM?> GetReceiptByIdAsync(Guid id, CancellationToken cancellationToken = default)
	{
		HttpResponseMessage response = await _httpClient.GetAsync($"receipts/{id}", cancellationToken);
		response.EnsureSuccessStatusCode();
		return await response.Content.ReadFromJsonAsync<ReceiptVM>(cancellationToken: cancellationToken);
	}

	public async Task<List<ReceiptVM>?> GetAllReceiptsAsync(CancellationToken cancellationToken = default)
	{
		HttpResponseMessage response = await _httpClient.GetAsync("receipts", cancellationToken);
		response.EnsureSuccessStatusCode();
		return await response.Content.ReadFromJsonAsync<List<ReceiptVM>>(cancellationToken: cancellationToken);
	}

	public async Task<bool> UpdateReceiptAsync(ReceiptVM model, CancellationToken cancellationToken = default)
	{
		HttpResponseMessage response = await _httpClient.PutAsJsonAsync("receipts", model, cancellationToken);
		response.EnsureSuccessStatusCode();
		return true;
	}

	public async Task<bool> DeleteReceiptsAsync(List<Guid> ids, CancellationToken cancellationToken = default)
	{
		HttpResponseMessage response = await _httpClient.PostAsJsonAsync("receipts/delete", ids, cancellationToken);
		response.EnsureSuccessStatusCode();
		return true;
	}
}