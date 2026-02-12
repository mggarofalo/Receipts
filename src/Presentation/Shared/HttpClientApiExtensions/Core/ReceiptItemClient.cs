using Shared.ViewModels.Core;
using System.Net.Http.Json;

namespace Shared.HttpClientApiExtensions.Core;

/// <summary>
/// Provides static methods for interacting with receipt items through HTTP requests.
/// </summary>
public static class ReceiptItemClient
{
	/// <summary>
	/// Creates multiple receipt items asynchronously.
	/// </summary>
	/// <param name="httpClient">The HTTP client to use for the request.</param>
	/// <param name="models">The list of receipt item view models to create.</param>
	/// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
	/// <returns>A task that represents the asynchronous operation. The task result contains the list of created receipt items.</returns>
	/// <exception cref="HttpRequestException">Thrown when the API request fails.</exception>
	public static async Task<List<ReceiptItemVM>?> CreateReceiptItemsAsync(this HttpClient httpClient, List<ReceiptItemVM> models, CancellationToken cancellationToken = default)
	{
		HttpResponseMessage response = await httpClient.PostAsJsonAsync("receiptitems", models, cancellationToken);
		response.EnsureSuccessStatusCode();
		return await response.Content.ReadFromJsonAsync<List<ReceiptItemVM>>(cancellationToken: cancellationToken);
	}

	/// <summary>
	/// Retrieves a receipt item by its ID asynchronously.
	/// </summary>
	/// <param name="httpClient">The HTTP client to use for the request.</param>
	/// <param name="id">The ID of the receipt item to retrieve.</param>
	/// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
	/// <returns>A task that represents the asynchronous operation. The task result contains the retrieved receipt item.</returns>
	/// <exception cref="HttpRequestException">Thrown when the API request fails.</exception>
	public static async Task<ReceiptItemVM?> GetReceiptItemByIdAsync(this HttpClient httpClient, Guid id, CancellationToken cancellationToken = default)
	{
		HttpResponseMessage response = await httpClient.GetAsync($"receiptitems/{id}", cancellationToken);
		response.EnsureSuccessStatusCode();
		return await response.Content.ReadFromJsonAsync<ReceiptItemVM>(cancellationToken: cancellationToken);
	}

	/// <summary>
	/// Retrieves all receipt items asynchronously.
	/// </summary>
	/// <param name="httpClient">The HTTP client to use for the request.</param>
	/// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
	/// <returns>A task that represents the asynchronous operation. The task result contains the list of all receipt items.</returns>
	/// <exception cref="HttpRequestException">Thrown when the API request fails.</exception>
	public static async Task<List<ReceiptItemVM>?> GetAllReceiptItemsAsync(this HttpClient httpClient, CancellationToken cancellationToken = default)
	{
		HttpResponseMessage response = await httpClient.GetAsync("receiptitems", cancellationToken);
		response.EnsureSuccessStatusCode();
		return await response.Content.ReadFromJsonAsync<List<ReceiptItemVM>>(cancellationToken: cancellationToken);
	}

	/// <summary>
	/// Retrieves all receipt items for a specific receipt asynchronously.
	/// </summary>
	/// <param name="httpClient">The HTTP client to use for the request.</param>
	/// <param name="receiptId">The ID of the receipt to retrieve items for.</param>
	/// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
	/// <returns>A task that represents the asynchronous operation. The task result contains the list of receipt items for the specified receipt.</returns>
	/// <exception cref="HttpRequestException">Thrown when the API request fails.</exception>
	public static async Task<List<ReceiptItemVM>?> GetReceiptItemsByReceiptIdAsync(this HttpClient httpClient, Guid receiptId, CancellationToken cancellationToken = default)
	{
		HttpResponseMessage response = await httpClient.GetAsync($"receiptitems/by-receipt-id/{receiptId}", cancellationToken);
		response.EnsureSuccessStatusCode();
		return await response.Content.ReadFromJsonAsync<List<ReceiptItemVM>>(cancellationToken: cancellationToken);
	}

	/// <summary>
	/// Updates multiple receipt items asynchronously.
	/// </summary>
	/// <param name="httpClient">The HTTP client to use for the request.</param>
	/// <param name="models">The list of receipt item view models to update.</param>
	/// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
	/// <returns>A task that represents the asynchronous operation. The task result indicates whether the update was successful.</returns>
	/// <exception cref="HttpRequestException">Thrown when the API request fails.</exception>
	public static async Task<bool> UpdateReceiptItemsAsync(this HttpClient httpClient, List<ReceiptItemVM> models, CancellationToken cancellationToken = default)
	{
		HttpResponseMessage response = await httpClient.PutAsJsonAsync("receiptitems", models, cancellationToken);
		response.EnsureSuccessStatusCode();
		return true;
	}

	/// <summary>
	/// Deletes multiple receipt items asynchronously.
	/// </summary>
	/// <param name="httpClient">The HTTP client to use for the request.</param>
	/// <param name="ids">The list of IDs of the receipt items to delete.</param>
	/// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
	/// <returns>A task that represents the asynchronous operation. The task result indicates whether the deletion was successful.</returns>
	/// <exception cref="HttpRequestException">Thrown when the API request fails.</exception>
	public static async Task<bool> DeleteReceiptItemsAsync(this HttpClient httpClient, List<Guid> ids, CancellationToken cancellationToken = default)
	{
		HttpResponseMessage response = await httpClient.PostAsJsonAsync("receiptitems/delete", ids, cancellationToken);
		response.EnsureSuccessStatusCode();
		return true;
	}
}