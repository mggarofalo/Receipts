using Shared.ViewModels.Core;

namespace Client.Interfaces.Services.Core;

public interface IReceiptItemService
{
	Task<List<ReceiptItemVM>?> CreateReceiptItemsAsync(List<ReceiptItemVM> models, CancellationToken cancellationToken = default);
	Task<ReceiptItemVM?> GetReceiptItemByIdAsync(Guid id, CancellationToken cancellationToken = default);
	Task<List<ReceiptItemVM>?> GetAllReceiptItemsAsync(CancellationToken cancellationToken = default);
	Task<List<ReceiptItemVM>?> GetReceiptItemsByReceiptIdAsync(Guid receiptId, CancellationToken cancellationToken = default);
	Task<bool> UpdateReceiptItemsAsync(List<ReceiptItemVM> models, CancellationToken cancellationToken = default);
	Task<bool> DeleteReceiptItemsAsync(List<Guid> ids, CancellationToken cancellationToken = default);
}
