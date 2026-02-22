using API.Generated.Dtos;

namespace API.Hubs;

public interface IReceiptsHubClient
{
	Task ReceiptCreated(ReceiptResponse receipt);
	Task ReceiptUpdated(ReceiptResponse receipt);
	Task ReceiptDeleted(Guid id);
}
