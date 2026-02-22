using API.Generated.Dtos;

namespace API.Hubs;

public interface IReceiptsHubClient
{
	Task ReceiptCreated(ReceiptResponse receipt, CancellationToken cancellationToken = default);
	Task ReceiptUpdated(ReceiptResponse receipt, CancellationToken cancellationToken = default);
	Task ReceiptDeleted(Guid id, CancellationToken cancellationToken = default);
}
