using Shared.ViewModels.Core;
using System.Net.Http.Json;

namespace Shared.HttpClientApiExtensions.Core;

/// <summary>
/// Provides extension methods for HttpClient to interact with receipt-related API endpoints.
/// </summary>
public static class ReceiptClient
{
	/// <summary>
	/// Creates multiple receipts asynchronously.
	/// </summary>
	/// <param name="httpClient">The HttpClient instance.</param>
	/// <param name="models">A list of ReceiptVM objects to create.</param>
	/// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
	/// <returns>A task that represents the asynchronous operation. The task result contains a list of created ReceiptVM objects, or null.</returns>
	/// <exception cref="HttpRequestException">Thrown when the API request fails.</exception>
	public static async Task<List<ReceiptVM>?> CreateReceiptsAsync(this HttpClient httpClient, List<ReceiptVM> models, CancellationToken cancellationToken = default)
	{
		HttpResponseMessage response = await httpClient.PostAsJsonAsync("receipts", models, cancellationToken);
		response.EnsureSuccessStatusCode();
		return await response.Content.ReadFromJsonAsync<List<ReceiptVM>>(cancellationToken: cancellationToken);
	}

	/// <summary>
	/// Retrieves a receipt by its ID asynchronously.
	/// </summary>
	/// <param name="httpClient">The HttpClient instance.</param>
	/// <param name="id">The unique identifier of the receipt.</param>
	/// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
	/// <returns>A task that represents the asynchronous operation. The task result contains the retrieved ReceiptVM object, or null if not found.</returns>
	/// <exception cref="HttpRequestException">Thrown when the API request fails.</exception>
	public static async Task<ReceiptVM?> GetReceiptByIdAsync(this HttpClient httpClient, Guid id, CancellationToken cancellationToken = default)
	{
		HttpResponseMessage response = await httpClient.GetAsync($"receipts/{id}", cancellationToken);
		response.EnsureSuccessStatusCode();
		return await response.Content.ReadFromJsonAsync<ReceiptVM>(cancellationToken: cancellationToken);
	}

	/// <summary>
	/// Retrieves all receipts asynchronously.
	/// </summary>
	/// <param name="httpClient">The HttpClient instance.</param>
	/// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
	/// <returns>A task that represents the asynchronous operation. The task result contains a list of all ReceiptVM objects, or null.</returns>
	/// <exception cref="HttpRequestException">Thrown when the API request fails.</exception>
	public static async Task<List<ReceiptVM>?> GetAllReceiptsAsync(this HttpClient httpClient, CancellationToken cancellationToken = default)
	{
		HttpResponseMessage response = await httpClient.GetAsync("receipts", cancellationToken);
		response.EnsureSuccessStatusCode();
		return await response.Content.ReadFromJsonAsync<List<ReceiptVM>>(cancellationToken: cancellationToken);
	}

	/// <summary>
	/// Updates multiple receipts asynchronously.
	/// </summary>
	/// <param name="httpClient">The HttpClient instance.</param>
	/// <param name="models">A list of ReceiptVM objects to update.</param>
	/// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
	/// <returns>A task that represents the asynchronous operation. The task result is true if the update was successful.</returns>
	/// <exception cref="HttpRequestException">Thrown when the API request fails.</exception>
	public static async Task<bool> UpdateReceiptsAsync(this HttpClient httpClient, List<ReceiptVM> models, CancellationToken cancellationToken = default)
	{
		HttpResponseMessage response = await httpClient.PutAsJsonAsync("receipts", models, cancellationToken);
		response.EnsureSuccessStatusCode();
		return true;
	}

	/// <summary>
	/// Deletes multiple receipts asynchronously.
	/// </summary>
	/// <param name="httpClient">The HttpClient instance.</param>
	/// <param name="ids">A list of unique identifiers of the receipts to delete.</param>
	/// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
	/// <returns>A task that represents the asynchronous operation. The task result is true if the deletion was successful.</returns>
	/// <exception cref="HttpRequestException">Thrown when the API request fails.</exception>
	public static async Task<bool> DeleteReceiptsAsync(this HttpClient httpClient, List<Guid> ids, CancellationToken cancellationToken = default)
	{
		HttpResponseMessage response = await httpClient.PostAsJsonAsync("receipts/delete", ids, cancellationToken);
		response.EnsureSuccessStatusCode();
		return true;
	}
}