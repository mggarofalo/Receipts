using Shared.ViewModels.Aggregates;
using System.Net.Http.Json;

namespace Shared.HttpClientApiExtensions.Aggregates;

/// <summary>
/// Provides methods for retrieving receipt data with associated items.
/// </summary>
public static class ReceiptWithItemsClient
{
	/// <summary>
	/// Retrieves a receipt with its associated items by the receipt ID.
	/// </summary>
	/// <param name="httpClient">The HttpClient instance to use for the request.</param>
	/// <param name="receiptId">The unique identifier of the receipt to retrieve.</param>
	/// <param name="cancellationToken">A token to cancel the asynchronous operation (optional).</param>
	/// <returns>A task that represents the asynchronous operation. The task result contains the ReceiptWithItemsVM if found, or null if not found.</returns>
	/// <exception cref="HttpRequestException">Thrown when the API request fails.</exception>
	public static async Task<ReceiptWithItemsVM?> GetReceiptWithItemsByReceiptIdAsync(this HttpClient httpClient, Guid receiptId, CancellationToken cancellationToken = default)
	{
		HttpResponseMessage response = await httpClient.GetAsync($"receiptwithitems/by-receipt-id/{receiptId}", cancellationToken);
		response.EnsureSuccessStatusCode();
		return await response.Content.ReadFromJsonAsync<ReceiptWithItemsVM>(cancellationToken: cancellationToken);
	}
}
