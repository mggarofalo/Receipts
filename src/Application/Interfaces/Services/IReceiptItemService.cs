using Domain.Core;

namespace Application.Interfaces.Services;

public interface IReceiptItemService : IService<ReceiptItem>
{
	Task<List<ReceiptItem>?> GetByReceiptIdAsync(Guid receiptId, CancellationToken cancellationToken);
	Task<List<ReceiptItem>> CreateAsync(List<ReceiptItem> models, Guid receiptId, CancellationToken cancellationToken);
	Task UpdateAsync(List<ReceiptItem> models, Guid receiptId, CancellationToken cancellationToken);
}
