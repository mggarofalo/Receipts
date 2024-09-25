using Client.Interfaces.Services.Aggregates;
using Shared.HttpClientApiExtensions.Aggregates;
using Shared.ViewModels.Aggregates;

namespace Client.Services.Aggregates;

public class TripService(HttpClient httpClient) : ITripService
{
	public async Task<TripVM?> GetTripByReceiptIdAsync(Guid receiptId, CancellationToken cancellationToken = default)
	{
		return await httpClient
			.GetTripByReceiptIdAsync(receiptId, cancellationToken);
	}
}
