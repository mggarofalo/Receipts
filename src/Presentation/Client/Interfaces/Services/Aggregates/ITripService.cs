using Shared.ViewModels.Aggregates;

namespace Client.Interfaces.Services.Aggregates;

public interface ITripService
{
	Task<TripVM?> GetTripByReceiptIdAsync(Guid receiptId, CancellationToken cancellationToken = default);
}
