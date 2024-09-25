using Shared.ViewModels.Aggregates;
using System.Net.Http.Json;

namespace Shared.HttpClientApiExtensions.Aggregates;

/// <summary>
/// Provides methods for retrieving trip data from an API.
/// </summary>
public static class TripClient
{
	/// <summary>
	/// Retrieves a trip by its associated receipt ID.
	/// </summary>
	/// <param name="httpClient">The HttpClient instance to use for the request.</param>
	/// <param name="receiptId">The unique identifier of the receipt associated with the trip.</param>
	/// <param name="cancellationToken">A token to cancel the asynchronous operation (optional).</param>
	/// <returns>A task that represents the asynchronous operation. The task result contains the TripVM if found, or null if not found.</returns>
	/// <exception cref="HttpRequestException">Thrown when the API request fails.</exception>
	public static async Task<TripVM?> GetTripByReceiptIdAsync(this HttpClient httpClient, Guid receiptId, CancellationToken cancellationToken = default)
	{
		HttpResponseMessage response = await httpClient.GetAsync($"trip/by-receipt-id/{receiptId}", cancellationToken);
		response.EnsureSuccessStatusCode();
		return await response.Content.ReadFromJsonAsync<TripVM>(cancellationToken: cancellationToken);
	}
}
