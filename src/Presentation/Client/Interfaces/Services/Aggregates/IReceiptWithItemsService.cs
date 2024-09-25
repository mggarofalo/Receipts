using Shared.ViewModels.Aggregates;

namespace Client.Interfaces.Services.Aggregates;

public interface IReceiptWithItemsService
{
	Task<ReceiptWithItemsVM?> GetReceiptWithItemsByReceiptIdAsync(Guid receiptId, CancellationToken cancellationToken = default);
}