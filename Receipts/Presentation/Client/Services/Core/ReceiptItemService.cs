using Client.Interfaces.Services.Core;
using Shared.HttpClientApiExtensions.Core;
using Shared.ViewModels.Core;

namespace Client.Services.Core;

public class ReceiptItemService(HttpClient httpClient) : IReceiptItemService
{
	public async Task<List<ReceiptItemVM>?> CreateReceiptItemsAsync(List<ReceiptItemVM> models, CancellationToken cancellationToken = default)
	{
		return await httpClient
			.CreateReceiptItemsAsync(models, cancellationToken);
	}

	public async Task<ReceiptItemVM?> GetReceiptItemByIdAsync(Guid id, CancellationToken cancellationToken = default)
	{
		return await httpClient
			.GetReceiptItemByIdAsync(id, cancellationToken);
	}

	public async Task<List<ReceiptItemVM>?> GetAllReceiptItemsAsync(CancellationToken cancellationToken = default)
	{
		return await httpClient
			.GetAllReceiptItemsAsync(cancellationToken);
	}

	public async Task<List<ReceiptItemVM>?> GetReceiptItemsByReceiptIdAsync(Guid receiptId, CancellationToken cancellationToken = default)
	{
		return await httpClient
			.GetReceiptItemsByReceiptIdAsync(receiptId, cancellationToken);
	}

	public async Task<bool> UpdateReceiptItemsAsync(List<ReceiptItemVM> models, CancellationToken cancellationToken = default)
	{
		return await httpClient
			.UpdateReceiptItemsAsync(models, cancellationToken);
	}

	public async Task<bool> DeleteReceiptItemsAsync(List<Guid> ids, CancellationToken cancellationToken = default)
	{
		return await httpClient
			.DeleteReceiptItemsAsync(ids, cancellationToken);
	}
}
