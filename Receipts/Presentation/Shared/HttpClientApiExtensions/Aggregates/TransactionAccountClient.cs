using Shared.ViewModels.Aggregates;
using System.Net.Http.Json;

namespace Shared.HttpClientApiExtensions.Aggregates;

/// <summary>
/// Provides static methods for retrieving transaction account information from an API.
/// </summary>
public static class TransactionAccountClient
{
	/// <summary>
	/// Retrieves a transaction account by its associated transaction ID.
	/// </summary>
	/// <param name="httpClient">The HttpClient instance to use for the API request.</param>
	/// <param name="transactionId">The unique identifier of the transaction.</param>
	/// <param name="cancellationToken">A token to cancel the asynchronous operation (optional).</param>
	/// <returns>A Task representing the asynchronous operation, containing the TransactionAccountVM if found, or null.</returns>
	/// <exception cref="HttpRequestException">Thrown when the API request fails.</exception>
	public static async Task<TransactionAccountVM?> GetTransactionAccountByTransactionIdAsync(this HttpClient httpClient, Guid transactionId, CancellationToken cancellationToken = default)
	{
		HttpResponseMessage response = await httpClient.GetAsync($"transactionaccount/by-transaction-id/{transactionId}", cancellationToken);
		response.EnsureSuccessStatusCode();
		return await response.Content.ReadFromJsonAsync<TransactionAccountVM>(cancellationToken: cancellationToken);
	}

	/// <summary>
	/// Retrieves a list of transaction accounts associated with a specific receipt ID.
	/// </summary>
	/// <param name="httpClient">The HttpClient instance to use for the API request.</param>
	/// <param name="receiptId">The unique identifier of the receipt.</param>
	/// <param name="cancellationToken">A token to cancel the asynchronous operation (optional).</param>
	/// <returns>A Task representing the asynchronous operation, containing a List of TransactionAccountVM if found, or null.</returns>
	/// <exception cref="HttpRequestException">Thrown when the API request fails.</exception>
	public static async Task<List<TransactionAccountVM>?> GetTransactionAccountsByReceiptIdAsync(this HttpClient httpClient, Guid receiptId, CancellationToken cancellationToken = default)
	{
		HttpResponseMessage response = await httpClient.GetAsync($"transactionaccount/by-receipt-id/{receiptId}", cancellationToken);
		response.EnsureSuccessStatusCode();
		return await response.Content.ReadFromJsonAsync<List<TransactionAccountVM>>(cancellationToken: cancellationToken);
	}
}
