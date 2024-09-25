using Shared.ViewModels.Core;
using System.Net.Http.Json;

namespace Shared.HttpClientApiExtensions.Core;

/// <summary>
/// Provides static methods for interacting with transaction-related API endpoints.
/// </summary>
public static class TransactionClient
{
	/// <summary>
	/// Creates multiple transactions asynchronously.
	/// </summary>
	/// <param name="httpClient">The HttpClient instance to use for the request.</param>
	/// <param name="models">A list of TransactionVM objects representing the transactions to create.</param>
	/// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
	/// <returns>A task that represents the asynchronous operation. The task result contains a list of created TransactionVM objects, or null if the operation fails.</returns>
	/// <exception cref="HttpRequestException">Thrown when the API request fails.</exception>
	public static async Task<List<TransactionVM>?> CreateTransactionsAsync(this HttpClient httpClient, List<TransactionVM> models, CancellationToken cancellationToken = default)
	{
		HttpResponseMessage response = await httpClient.PostAsJsonAsync("transactions", models, cancellationToken);
		response.EnsureSuccessStatusCode();
		return await response.Content.ReadFromJsonAsync<List<TransactionVM>>(cancellationToken: cancellationToken);
	}

	/// <summary>
	/// Retrieves a specific transaction by its ID asynchronously.
	/// </summary>
	/// <param name="httpClient">The HttpClient instance to use for the request.</param>
	/// <param name="id">The unique identifier of the transaction to retrieve.</param>
	/// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
	/// <returns>A task that represents the asynchronous operation. The task result contains the retrieved TransactionVM object, or null if not found.</returns>
	/// <exception cref="HttpRequestException">Thrown when the API request fails.</exception>
	public static async Task<TransactionVM?> GetTransactionByIdAsync(this HttpClient httpClient, Guid id, CancellationToken cancellationToken = default)
	{
		HttpResponseMessage response = await httpClient.GetAsync($"transactions/{id}", cancellationToken);
		response.EnsureSuccessStatusCode();
		return await response.Content.ReadFromJsonAsync<TransactionVM>(cancellationToken: cancellationToken);
	}

	/// <summary>
	/// Retrieves all transactions asynchronously.
	/// </summary>
	/// <param name="httpClient">The HttpClient instance to use for the request.</param>
	/// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
	/// <returns>A task that represents the asynchronous operation. The task result contains a list of all TransactionVM objects, or null if the operation fails.</returns>
	/// <exception cref="HttpRequestException">Thrown when the API request fails.</exception>
	public static async Task<List<TransactionVM>?> GetAllTransactionsAsync(this HttpClient httpClient, CancellationToken cancellationToken = default)
	{
		HttpResponseMessage response = await httpClient.GetAsync("transactions", cancellationToken);
		response.EnsureSuccessStatusCode();
		return await response.Content.ReadFromJsonAsync<List<TransactionVM>>(cancellationToken: cancellationToken);
	}

	/// <summary>
	/// Retrieves all transactions associated with a specific receipt ID asynchronously.
	/// </summary>
	/// <param name="httpClient">The HttpClient instance to use for the request.</param>
	/// <param name="receiptId">The unique identifier of the receipt whose transactions are to be retrieved.</param>
	/// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
	/// <returns>A task that represents the asynchronous operation. The task result contains a list of TransactionVM objects associated with the specified receipt, or null if the operation fails.</returns>
	/// <exception cref="HttpRequestException">Thrown when the API request fails.</exception>
	public static async Task<List<TransactionVM>?> GetTransactionsByReceiptIdAsync(this HttpClient httpClient, Guid receiptId, CancellationToken cancellationToken = default)
	{
		HttpResponseMessage response = await httpClient.GetAsync($"transactions/by-receipt-id/{receiptId}", cancellationToken);
		response.EnsureSuccessStatusCode();
		return await response.Content.ReadFromJsonAsync<List<TransactionVM>>(cancellationToken: cancellationToken);
	}

	/// <summary>
	/// Updates multiple transactions asynchronously.
	/// </summary>
	/// <param name="httpClient">The HttpClient instance to use for the request.</param>
	/// <param name="models">A list of TransactionVM objects representing the updated transactions.</param>
	/// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
	/// <returns>A task that represents the asynchronous operation. The task result is true if the update was successful, otherwise false.</returns>
	/// <exception cref="HttpRequestException">Thrown when the API request fails.</exception>
	public static async Task<bool> UpdateTransactionsAsync(this HttpClient httpClient, List<TransactionVM> models, CancellationToken cancellationToken = default)
	{
		HttpResponseMessage response = await httpClient.PutAsJsonAsync("transactions", models, cancellationToken);
		response.EnsureSuccessStatusCode();
		return true;
	}

	/// <summary>
	/// Deletes multiple transactions asynchronously.
	/// </summary>
	/// <param name="httpClient">The HttpClient instance to use for the request.</param>
	/// <param name="ids">A list of unique identifiers of the transactions to delete.</param>
	/// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
	/// <returns>A task that represents the asynchronous operation. The task result is true if the deletion was successful, otherwise false.</returns>
	/// <exception cref="HttpRequestException">Thrown when the API request fails.</exception>
	public static async Task<bool> DeleteTransactionsAsync(this HttpClient httpClient, List<Guid> ids, CancellationToken cancellationToken = default)
	{
		HttpResponseMessage response = await httpClient.PostAsJsonAsync("transactions/delete", ids, cancellationToken);
		response.EnsureSuccessStatusCode();
		return true;
	}
}