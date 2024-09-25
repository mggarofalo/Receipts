using Client.Interfaces.Services.Aggregates;
using Shared.HttpClientApiExtensions.Aggregates;
using Shared.ViewModels.Aggregates;

namespace Client.Services.Aggregates;

public class ReceiptWithItemsService(HttpClient httpClient) : IReceiptWithItemsService
{
	public async Task<ReceiptWithItemsVM?> GetReceiptWithItemsByReceiptIdAsync(Guid receiptId, CancellationToken cancellationToken = default)
	{
		return await httpClient
			.GetReceiptWithItemsByReceiptIdAsync(receiptId, cancellationToken);
	}
}
