using Shared.ViewModels.Core;
using System.Net.Http.Json;

namespace Shared.ApiClients;

public class TransactionClient(HttpClient? httpClient = default)
{
	private const string HttpClientBaseAddress = "http://localhost:5136/api/";
	private readonly HttpClient _httpClient = httpClient ?? new() { BaseAddress = new Uri(HttpClientBaseAddress) };

	public async Task<TransactionVM?> CreateTransactionAsync(TransactionVM model, CancellationToken cancellationToken = default)
	{
		HttpResponseMessage response = await _httpClient.PostAsJsonAsync("transactions", model, cancellationToken);
		response.EnsureSuccessStatusCode();
		return await response.Content.ReadFromJsonAsync<TransactionVM>(cancellationToken: cancellationToken);
	}

	public async Task<TransactionVM?> GetTransactionByIdAsync(Guid id, CancellationToken cancellationToken = default)
	{
		HttpResponseMessage response = await _httpClient.GetAsync($"transactions/{id}", cancellationToken);
		response.EnsureSuccessStatusCode();
		return await response.Content.ReadFromJsonAsync<TransactionVM>(cancellationToken: cancellationToken);
	}

	public async Task<List<TransactionVM>?> GetAllTransactionsAsync(CancellationToken cancellationToken = default)
	{
		HttpResponseMessage response = await _httpClient.GetAsync("transactions", cancellationToken);
		response.EnsureSuccessStatusCode();
		return await response.Content.ReadFromJsonAsync<List<TransactionVM>>(cancellationToken: cancellationToken);
	}

	public async Task<List<TransactionVM>?> GetTransactionsByReceiptIdAsync(Guid receiptId, CancellationToken cancellationToken = default)
	{
		HttpResponseMessage response = await _httpClient.GetAsync($"transactions/by-receipt-id/{receiptId}", cancellationToken);
		response.EnsureSuccessStatusCode();
		return await response.Content.ReadFromJsonAsync<List<TransactionVM>>(cancellationToken: cancellationToken);
	}

	public async Task<bool> UpdateTransactionAsync(TransactionVM model, CancellationToken cancellationToken = default)
	{
		HttpResponseMessage response = await _httpClient.PutAsJsonAsync("transactions", model, cancellationToken);
		response.EnsureSuccessStatusCode();
		return true;
	}

	public async Task<bool> DeleteTransactionsAsync(List<Guid> ids, CancellationToken cancellationToken = default)
	{
		HttpResponseMessage response = await _httpClient.PostAsJsonAsync("transactions/delete", ids, cancellationToken);
		response.EnsureSuccessStatusCode();
		return true;
	}
}